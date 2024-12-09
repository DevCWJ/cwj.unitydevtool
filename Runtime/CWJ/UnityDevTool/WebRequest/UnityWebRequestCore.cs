#if UNITY_2017_1_OR_NEWER || UNITY_BUILD

//
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;

namespace CWJ
{
    /// <summary>
    /// CWJ 최적화 완료 240907
    /// </summary>
    public abstract class UnityWebRequestCore<TChild, TWS, TWR, TRS, TRR> : MonoBehaviour
        where TChild : MonoBehaviour where TWS : struct where TWR : class where TRS : struct where TRR : class
    {
        private static readonly object _lock = new object();
        public static TChild Instance
        {
            get
            {
                if (!_instance)
                {
                    lock (_lock)
                    {
                        var newGo  = new GameObject(typeof(TChild).Name, typeof(TChild));
                        var newIns = newGo.GetComponent<TChild>();
                        _instance = newIns;
                        GameObject.DontDestroyOnLoad(newIns.gameObject);
                    }
                }
                return _instance;
            }
        }
        protected static TChild _instance;



        protected abstract string baseURL { get; }

        /// <summary>
        /// password 필요없으면 null3
        /// </summary>
        protected abstract string propertyName_PasswordKey { get; }
        /// <summary>
        /// password 필요없으면 null
        /// </summary>
        /// <returns></returns>
        protected abstract string GetPassword();

        static Dictionary<Type, FieldInfo[]> typeFieldInfoCache = new Dictionary<Type, FieldInfo[]>();

        public string GetUrlWithQuery<T>(T data)
        {
            if (data == null) return baseURL;

            Type type = typeof(T);

            if (!typeFieldInfoCache.TryGetValue(type, out var fieldInfos))
            {
                fieldInfos = type.GetFields(BindingFlags.Instance | BindingFlags.Public);
                typeFieldInfoCache.Add(type, fieldInfos);
            }
            var properties = fieldInfos.Where(f => f.GetValue(data) != null)
                            .Select(f => $"{f.Name}={Uri.EscapeUriString(f.GetValue(data).ToString())}").ToArray();
            int propLength = properties.Length;

            string uQuery = null;
            if (propLength > 0)
                uQuery = "?" + (propLength == 1 ? properties[0] : string.Join("&", properties));

            string password = GetPassword();
            if (!string.IsNullOrEmpty(password))
            {
                uQuery += uQuery == null ? "?" : "&";
                uQuery += "password=" + Uri.EscapeUriString(password);
            }

            return baseURL + uQuery;
        }

        protected virtual void Awake()
        {
            //singleton
            if (_instance != null && _instance != this)
            {
                Destroy(this.gameObject);
            }
            else
            {
                _instance = this as TChild;
            }
        }

        [VisualizeProperty] public static bool IsWriting_Any => _WriteNormalTaskCnt > 0 || _WriteLazyTaskCnt > 0;
        [VisualizeProperty] public static bool IsWriting_OnlyNormalTrack => _WriteNormalTaskCnt > 0;
        [VisualizeProperty] public static int _WriteFastTaskCnt { get; private set; } = 0;
        [VisualizeProperty] public static int _WriteNormalTaskCnt { get; private set; } = 0;
        [VisualizeProperty] public static int _WriteLazyTaskCnt { get; private set; } = 0;


        public class ObjectPool<T> where T : new()
        {
            private readonly Stack<T> _stack = new Stack<T>();

            public T Get()
            {
                return _stack.Count > 0 ? _stack.Pop() : new T();
            }

            public void Release(T item)
            {
                _stack.Push(item);
            }
        }

        public struct WriteQueContainer
        {
            public JObject jObj;
            public Action<Exception> excepCallback;
            public Action<TWR> callback;
            public int interval;

            public void Initialize(JObject jObj, Action<Exception> excepCallback, Action<TWR> callback, int interval)
            {
                this.jObj = jObj;
                this.excepCallback = excepCallback;
                this.callback = callback;
                this.interval = interval;
            }
        }
        private static readonly ObjectPool<WriteQueContainer> _WriteQueContainerPool = new ObjectPool<WriteQueContainer>();
        private static readonly ObjectPool<ReadQueContainer> _ReadQueContainerPool = new ObjectPool<ReadQueContainer>();

        protected static readonly Queue<WriteQueContainer> _WriteFastTrackWaiting = new();
        protected static readonly Queue<WriteQueContainer> _WriteNormalTrackWaiting = new();

        protected static readonly Queue<(JObject jObj, Action<Exception> excepCallback, Action<TWR> callback)> _WriteLazyTrackWaiting
            = new Queue<(JObject jObj, Action<Exception> excepCallback, Action<TWR> callback)>();

        protected static CancellationTokenSource cts_write = new CancellationTokenSource();
        protected readonly static List<UniTask> activeTasks_write = new List<UniTask>();


        [VisualizeProperty] public static bool IsReading_Any => _ReadNormalTaskCnt > 0 || _ReadLazyTaskCnt > 0;
        [VisualizeProperty] public static bool IsReading_OnlyNormalTrack => _ReadNormalTaskCnt > 0;

        [VisualizeProperty] public static int _ReadFastTaskCnt { get; private set; } = 0;
        [VisualizeProperty] public static int _ReadNormalTaskCnt { get; private set; } = 0;
        [VisualizeProperty] public static int _ReadLazyTaskCnt { get; private set; } = 0;



        public struct ReadQueContainer
        {
            public TRS readMdl;
            public Action<Exception> excepCallback;
            public Action<TRR> callback;
            public int interval;

            public void Initialize(TRS readMdl, Action<Exception> excepCallback, Action<TRR> callback, int interval)
            {
                this.readMdl = readMdl;
                this.excepCallback = excepCallback;
                this.callback = callback;
                this.interval = interval;
            }
        }

        protected readonly static Queue<ReadQueContainer> _ReadFastTrackWaiting = new();

        protected readonly static Queue<ReadQueContainer> _ReadNormalTrackWaiting = new();

        protected readonly static
            Queue<(string readURL, Action<Exception> excepCallback, Action<string> onResponseText)>
            _ReadLazyTrackWaiting = new();
        protected static CancellationTokenSource cts_read = new CancellationTokenSource();
        protected readonly static List<UniTask> activeTasks_read = new List<UniTask>();


        protected static int _WriteBatchSize { get; } = 5;
        protected static int _ReadBatchSize { get; } = 5;

        static int writeProcessed = 0;
        static int readProcessed = 0;
        private static bool CanWriteProcess => writeProcessed < _WriteBatchSize;
        private static bool CanReadProcess => readProcessed < _ReadBatchSize;

        protected void Update()
        {
            writeProcessed = 0;
            while (CanWriteProcess &&_WriteFastTrackWaiting.Count > 0)
            {
                var writeItem = _WriteFastTrackWaiting.Dequeue();
                RunPost(writeItem.jObj, writeItem.excepCallback, writeItem.callback, 2);
                _WriteQueContainerPool.Release(writeItem);
                ++writeProcessed;
            }

            readProcessed = 0;
            while (CanReadProcess &&_ReadFastTrackWaiting.Count > 0)
            {
                var readItem = _ReadFastTrackWaiting.Dequeue();
                RunGet(GetReadURLByReadMdl(readItem.readMdl), readItem.excepCallback, null, 2, readItem.callback);
                _ReadQueContainerPool.Release(readItem);
                ++readProcessed;
            }
        }

        const int maxInterval = 60;
        int t = 0;

        protected void LateUpdate()
        {
            if (++t > maxInterval)
            {
                t = 1;
            }

            bool hasWriteNormalTrack = true;

            if (CanWriteProcess)
            {
                if (hasWriteNormalTrack = _WriteNormalTrackWaiting.TryDequeue(out var writeItem))
                {
                    if (writeItem.interval == 0 || t % writeItem.interval == 0)
                    {
                        RunPost(writeItem.jObj, writeItem.excepCallback, writeItem.callback, 1);
                        _WriteQueContainerPool.Release(writeItem);
                        ++writeProcessed;
                    }
                    else
                    {
                        hasWriteNormalTrack = false;
                        _WriteNormalTrackWaiting.Enqueue(writeItem);
                    }
                }
            }

            bool hasReadNormalTrack = true;

            if (CanReadProcess)
            {
                if (hasReadNormalTrack = _ReadNormalTrackWaiting.TryDequeue(out var readItem))
                {
                    if (readItem.interval == 0 || t % readItem.interval == 0)
                    {
                        RunGet(GetReadURLByReadMdl(readItem.readMdl), readItem.excepCallback, null, 1, readItem.callback);
                        _ReadQueContainerPool.Release(readItem);
                        ++readProcessed;
                    }
                    else
                    {
                        hasReadNormalTrack = false;
                        _ReadNormalTrackWaiting.Enqueue(readItem);
                    }
                }
            }


            if (!hasWriteNormalTrack && CanWriteProcess)
            {
                if (_WriteLazyTaskCnt == 0 && _WriteLazyTrackWaiting.TryDequeue(out var writeWaiting))
                {
                    RunPost(writeWaiting.jObj, writeWaiting.excepCallback, writeWaiting.callback, 0);
                }
            }
            if (!hasReadNormalTrack && CanReadProcess)
            {
                if (_ReadLazyTaskCnt == 0 && _ReadLazyTrackWaiting.TryDequeue(out var readWaiting))
                {
                    RunGet(readWaiting.readURL, readWaiting.excepCallback, readWaiting.onResponseText, 0, null);
                }
            }
        }

        public virtual void WriteFastTrack(JObject jobj, Action<Exception> errResponse,
                                           Action<TWR> callback)
        {
            if (!MonoBehaviourEventHelper.IsMainThread)
            {
                ThreadDispatcher.Enqueue(() =>
                {
                    Debug.LogError("WriteFastTrack MainThread 아니라서 ThreadDispatcher 사용함");
                    _WriteFastTrack(jobj, errResponse, callback);
                });
                return;
            }

            _WriteFastTrack(jobj, errResponse, callback);
        }

        public void WriteFastTrack(TWS writeReqMdl, Action<Exception> errResponse, Action<TWR> callback)
        {
            WriteFastTrack(GetWriteJObjByWriteMdl(writeReqMdl), errResponse, callback);
        }

        private void _WriteFastTrack(JObject jObj, Action<Exception> errResponse, Action<TWR> callback)
        {
            var container = _WriteQueContainerPool.Get();
            container.Initialize(jObj, errResponse, callback, 0);
            _WriteFastTrackWaiting.Enqueue(container);
        }

        public virtual void ReadFastTrack(TRS readMdl, Action<Exception> errResponse, Action<TRR> callback)
        {
            if (!MonoBehaviourEventHelper.IsMainThread)
            {
                ThreadDispatcher.Enqueue(() =>
                {
                    Debug.LogError("ReadFastTrack MainThread 아니라서 ThreadDispatcher 사용함");
                    _ReadFastTrack(readMdl, errResponse, callback);
                });
                return;
            }
            _ReadFastTrack(readMdl, errResponse, callback);
        }
        private void _ReadFastTrack(TRS readMdl, Action<Exception> errResponse, Action<TRR> callback)
        {
            var container = _ReadQueContainerPool.Get();
            container.Initialize(readMdl, errResponse, callback, 0);
            _ReadFastTrackWaiting.Enqueue(container);
        }

        public virtual void WriteNormalTrack(JObject jobj, Action<Exception> errResponse, Action<TWR> callback,
            int interval = 3)
        {
            if (!MonoBehaviourEventHelper.IsMainThread)
            {
                ThreadDispatcher.Enqueue(() =>
                {
                    Debug.LogError("WriteNormalTrack MainThread 아니라서 ThreadDispatcher 사용함");

                    _WriteNormalTrack(jobj, errResponse, callback, interval);
                });
                return;
            }

            _WriteNormalTrack(jobj, errResponse, callback, interval);
        }

        public void WriteNormalTrack(TWS writeReqMdl, Action<Exception> errResponse, Action<TWR> callback,
            int interval = 3)
        {
            WriteNormalTrack(GetWriteJObjByWriteMdl(writeReqMdl), errResponse, callback, interval);
        }

        private void _WriteNormalTrack(JObject jobj, Action<Exception> errResponse, Action<TWR> callback, int interval)
        {
            var container = _WriteQueContainerPool.Get();
            container.Initialize(jobj, errResponse, callback, interval);
            _WriteNormalTrackWaiting.Enqueue(container);
        }

        public virtual void ReadNormalTrack(TRS readMdl, Action<Exception> errResponse, Action<TRR> callback,
            int interval = 2)
        {
            if (!MonoBehaviourEventHelper.IsMainThread)
            {
                ThreadDispatcher.Enqueue(() =>
                {
                    Debug.LogError("ReadNormalTrack MainThread 아니라서 ThreadDispatcher 사용함");
                    _ReadNormalTrack(readMdl, errResponse, callback, interval);
                });
                return;
            }

            _ReadNormalTrack(readMdl, errResponse, callback, interval);
        }

        private void _ReadNormalTrack(TRS readMdl, Action<Exception> errResponse, Action<TRR> callback, int interval)
        {
            var container = _ReadQueContainerPool.Get();
            container.Initialize(readMdl, errResponse, callback, interval);
            _ReadNormalTrackWaiting.Enqueue(container);
        }

        protected virtual JObject GetWriteJObjByWriteMdl(TWS writeMdl) { return JObject.FromObject(writeMdl); }
        protected virtual string GetReadURLByReadMdl(TRS readMdl) { return GetUrlWithQuery(readMdl); }


        public virtual void WriteLazyTrack(JObject jObj, Action<Exception> errResponse, Action<TWR> callback)
        {
            if (!MonoBehaviourEventHelper.IsMainThread)
            {
                ThreadDispatcher.Enqueue(() => _WriteLazyTrack(jObj, errResponse, callback));
                return;
            }
            _WriteLazyTrack(jObj, errResponse, callback);
        }
        private void _WriteLazyTrack(JObject jObj, Action<Exception> errResponse, Action<TWR> callback)
        {
            _WriteLazyTrackWaiting.Enqueue((jObj, errResponse, callback));
        }

        public virtual void ReadLazyTrack(string readURL, Action<Exception> errResponse, Action<TRR> callback)
        {
            ReadCustom(readURL, errResponse, callback);
        }

        public virtual void ReadCustom<T>(string readURL, Action<Exception> errResponse, Action<T> callback)
            where T : class
        {
            if (!MonoBehaviourEventHelper.IsMainThread)
            {
                ThreadDispatcher.Enqueue(() => _ReadCustom<T>(readURL, errResponse, callback));
                return;
            }

            _ReadCustom<T>(readURL, errResponse, callback);
        }
        private void _ReadCustom<T>(string readURL, Action<Exception> errResponse, Action<T> callback) where T : class
        {
            _ReadLazyTrackWaiting.Enqueue((readURL, errResponse,
                (data) => { OnResponseProcess(data, errResponse, callback); }));
        }

        private void EnsureCancelTokenSrc(ref CancellationTokenSource cts)
        {
            if (cts == null || cts.IsCancellationRequested)
            {
                cts?.Dispose();
                cts = new CancellationTokenSource();
            }
        }

        protected void RunPost(JObject jObj, Action<Exception> onError, Action<TWR> callback, int trackPriority)
        {
            if (trackPriority==2) _WriteFastTaskCnt++;
            else if (trackPriority == 1) _WriteNormalTaskCnt++;
            else if (trackPriority == 0) _WriteLazyTaskCnt++;

            string taskRootName = "POST_";
            if (trackPriority != 2)
                taskRootName += $"{(trackPriority == 1 ? "NormalTrack" : "LazyTrack")}";

            string password = GetPassword();
            //credential
            if (!string.IsNullOrEmpty(password))
                jObj.Add(propertyName_PasswordKey, password);

            EnsureCancelTokenSrc(ref cts_write);

            RunHttpTask(
#if UNITY_EDITOR
                        taskRootName,
#endif
            () => _PostTask(jObj, onError, callback, cts_write.Token),
            activeTasks_write,
            () =>
            {
                if ((trackPriority == 2 && --_WriteFastTaskCnt == 0)
                    || (trackPriority == 1 && --_WriteNormalTaskCnt == 0)
                    || (trackPriority == 0 && --_WriteLazyTaskCnt == 0))
                {
#if UNITY_EDITOR
                    Debug.Log($"<size=17>All {taskRootName} is Finished.</size>");
#endif
                }
            },
            onError, 2).Forget();
        }

        protected void RunGet(string url, Action<Exception> onError, Action<string> onResponse, int trackPriority, Action<TRR> normalTrackCallback)
        {
            if (trackPriority == 2) _ReadFastTaskCnt++;
            else if (trackPriority == 1) _ReadNormalTaskCnt++;
            else if (trackPriority == 0) _ReadLazyTaskCnt++;

            string taskRootName = "GET_";
            if (trackPriority != 2)
                taskRootName += $"{(trackPriority == 1 ? "NormalTrack" : "LazyTrack")}";

            EnsureCancelTokenSrc(ref cts_read);

            RunHttpTask(
#if UNITY_EDITOR
                taskRootName,
#endif
                () => _GetTask(url, onError, onResponse, trackPriority >= 0, normalTrackCallback, cts_read.Token),
                activeTasks_read,
                () =>
                {
                    if ((trackPriority == 2 && --_ReadFastTaskCnt == 0)
                        || (trackPriority == 1 && --_ReadNormalTaskCnt == 0)
                        || (trackPriority == 0 && --_ReadLazyTaskCnt == 0))
                    {
#if UNITY_EDITOR
                        Debug.Log($"<size=17>All {taskRootName} is Finished.</size>");
#endif
                    }
                },
                onError, 3).Forget();

        }

        private async UniTaskVoid RunHttpTask(
#if UNITY_EDITOR
            string trackParentName,
#endif
            Func<UniTask> getTask, List<UniTask> activeTasks,
            Action onTaskComplete,
            Action<Exception> onError,
            int maxRetries)
        {
            if (MonoBehaviourEventHelper.IS_QUIT)
            {
                return;
            }

            int retries = 0;
            int retryDelay = 1000;
            int index = activeTasks.Count;
#if UNITY_EDITOR
            string taskName = $"{trackParentName} [{index}]";
#endif

            while (retries < maxRetries)
            {
#if UNITY_EDITOR
                Debug.Log($"<size=17>{taskName} : Run</size> (retries:{retries})");
#endif
                bool isAdded = false;
                UniTask task = default;
                try
                {
                    task = getTask();
                    activeTasks.Add(task);
                    isAdded = true;

                    await task;
#if UNITY_EDITOR
                    Debug.Log($"<size=17>{taskName} : Success Finish!</size> (retries:{retries})");
#endif
                    break;
                }
                catch (OperationCanceledException cacnelEx)
                {
                    onError?.Invoke(cacnelEx); // 실패 시 에러 콜백 호출
                    break;
                }
                catch (Exception ex)
                {
                    if (MonoBehaviourEventHelper.IS_QUIT)
                    {
                        break;
                    }
                    if (++retries < maxRetries)
                    {
#if UNITY_EDITOR
                        Debug.LogError($"<size=17>{taskName} : Failed</size> (retries:{retries})\n{ex.Message}");
#endif
                        await UniTask.Delay(retryDelay * (int)Math.Pow(2, retries)); // 재시도 간 대기
                    }
                    else
                    {
#if UNITY_EDITOR
                        Debug.LogError($"<size=17>{taskName} : Failed - MaxRetries</size> (retries:{retries})\n{ex.Message}");
#endif
                        onError?.Invoke(ex); // 실패 시 에러 콜백 호출
                        break;
                    }
                }
                finally
                {
                    if (isAdded)
                        activeTasks.Remove(task);
                }
            }
            onTaskComplete?.Invoke();
        }

        async UniTask _PostTask(JObject jo, Action<Exception> onError, Action<TWR> callback, CancellationToken cancellationToken)
        {
            using var webRequest = new UnityWebRequest(baseURL, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jo.ToString());

            webRequest.SetRequestHeader("Content-Type", "application/json");
            using var uploadHdlr = new UploadHandlerRaw(bodyRaw);
            using var downloadHdlr = new DownloadHandlerBuffer();
            webRequest.uploadHandler = uploadHdlr;
            webRequest.downloadHandler = downloadHdlr;
            webRequest.timeout = 77;

            var sendRequestTask = webRequest.SendWebRequest().WithCancellation(cancellationToken);
            // 비동기 요청 실행
            try
            {
                await sendRequestTask;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("POST Request was canceled.");
                callback?.Invoke(null);
                onError?.Invoke(new OperationCanceledException("The POST request was canceled."));
                return; // 요청이 취소되었으므로 함수 종료
            }
            catch (Exception ex)
            {
                Debug.LogError("[ERROR - UnityWebReqError_POST] : " + ex);
                callback?.Invoke(null);
                onError?.Invoke(ex);
                return;
            }

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var data = Newtonsoft.Json.JsonConvert.DeserializeObject<TWR>(webRequest.downloadHandler.text);
                // var data = Utf8Json.JsonSerializer.Deserialize<TWR>(webRequest.downloadHandler.data); //BUG : Utf8Json IL2CPP 지원안함
                callback?.Invoke(data);
            }
            else
            {
                Debug.LogError(webRequest.result.ToString() + " : " + (webRequest.error ?? string.Empty));
                callback?.Invoke(null);
            }
        }

        async UniTask _GetTask(string uri, Action<Exception> onError, Action<string> onResponse, bool isNotLazyTrack, Action<TRR> normalTrackCallback,
                               CancellationToken cancellationToken)
        {
#if UNITY_EDITOR
            Debug.Log("[WebRequest_GET] " + uri);
#endif
            using UnityWebRequest webRequest = UnityWebRequest.Get(uri);
            webRequest.timeout = 77;

            // 비동기 요청 실행 및 취소 지원
            var sendRequestTask = webRequest.SendWebRequest().WithCancellation(cancellationToken);

            try
            {
                await sendRequestTask;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("GET Request was canceled.");
                onError?.Invoke(new OperationCanceledException("The GET request was canceled."));
                if (isNotLazyTrack)
                    normalTrackCallback?.Invoke(null);
                else
                    onResponse?.Invoke(null);
                return;
            }
            catch (Exception de)
            {
                Debug.LogError("[ERROR - UnityWebReqError_GET] : " + de);
                onError?.Invoke(de);
                if (isNotLazyTrack)
                    normalTrackCallback?.Invoke(null);
                else
                    onResponse?.Invoke(null);
                return;
            }

            // 성공 시
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                var dataTxt = webRequest.downloadHandler.text;
                if (isNotLazyTrack)
                    OnResponseProcess(dataTxt, onError, normalTrackCallback);
                else
                    onResponse?.Invoke(dataTxt);

            }
            else
            {
                // 실패 시
                string errorMsg = webRequest.error;
                onError?.Invoke(new HttpRequestException(errorMsg));
                Debug.LogError("[ERROR - UnityWebReqError_GET] : " + errorMsg);
            }
        }

        protected T OnResponseProcess<T>(string dataTxt, Action<Exception> onError, Action<T> callback) where T : class
        {
            T deserialize = null;
            if (dataTxt != null)
            {
                try
                {
                    deserialize = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(dataTxt);
                    // deserialize = Utf8Json.JsonSerializer.Deserialize<T>(data);  //BUG: IL2CPP 지원안함
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }

            callback?.Invoke(deserialize);

            if (HasErrorByResponse(deserialize, dataTxt, out string errorMsg))
            {
                onError?.Invoke(new HttpRequestException(errorMsg));
                Debug.LogError("[ERROR - UGSWebError_GET] : " + errorMsg);
            }

            return deserialize;
        }

        protected bool HasErrorByResponse<T>(T responseObj, string dataTxt, out string errMsg)
        {
            bool isError = responseObj == null;
            errMsg = isError ? $"JsonSerializer.Deserialize Error ({dataTxt})" : null;

            return isError | HasErrorByResponse<T>(responseObj, out var _);
        }

        protected virtual bool HasErrorByResponse<T>(T responseObj, out string errMsg)
        {
            errMsg = null;
            return false;
        }


        public void CancelAllWrite()
        {
            CancelAndWaitAllTasks(cts_write, activeTasks_write).Forget();
        }

        public void CancelAllRead()
        {
            CancelAndWaitAllTasks(cts_read, activeTasks_read).Forget();
        }

        protected async UniTask CancelAndWaitAllTasks(CancellationTokenSource cancelTknSrc , List<UniTask> activeTasks)
        {
            if (cancelTknSrc != null && !cancelTknSrc.IsCancellationRequested)
            {
                cancelTknSrc.Cancel();
                cancelTknSrc.Dispose();
            }

            await UniTask.WhenAll(activeTasks).ContinueWith(activeTasks.Clear);
        }

        protected void OnApplicationQuit()
        {
            _WriteNormalTrackWaiting.Clear();
            _WriteLazyTrackWaiting.Clear();
            _WriteNormalTaskCnt = 0;
            _WriteLazyTaskCnt = 0;
            cts_write?.Cancel();
            cts_write?.Dispose();
            cts_write = null;

            _ReadNormalTrackWaiting.Clear();
            _ReadLazyTrackWaiting.Clear();
            _ReadLazyTaskCnt = 0;
            _ReadNormalTaskCnt = 0;
            cts_read?.Cancel();
            cts_read?.Dispose();
            cts_read = null;

            //GC.Collect();
            //GC.WaitForPendingFinalizers();
        }
    }
}

#endif
