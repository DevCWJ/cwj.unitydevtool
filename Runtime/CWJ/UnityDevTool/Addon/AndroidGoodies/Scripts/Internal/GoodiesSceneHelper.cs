using JetBrains.Annotations;

namespace DeadMosquito.AndroidGoodies.Internal
{
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using UnityEngine;

	class GoodiesSceneHelper : MonoBehaviour
	{
		static GoodiesSceneHelper _instance;
		static readonly object InitLock = new object();
		readonly object _queueLock = new object();
		readonly List<Action> _queuedActions = new List<Action>();
		readonly List<Action> _executingActions = new List<Action>();

		public static GoodiesSceneHelper Instance
		{
			get
			{
				if (_instance == null)
				{
					Init();
				}

				return _instance;
			}
		}

		public static bool IsInImmersiveMode { set; private get; }

		Texture2D LastTakenScreenshot { get; set; }

		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
		static void Init()
		{
			if (AGUtils.IsNotAndroid())
			{
				return;
			}
			
			lock (InitLock)
			{
				if (!ReferenceEquals(_instance, null))
				{
					return;
				}

				var instances = FindObjectsOfType<GoodiesSceneHelper>();

				if (instances.Length > 1)
				{
					Debug.LogError(typeof(GoodiesSceneHelper) + " Something went really wrong " +
					               " - there should never be more than 1 " + typeof(GoodiesSceneHelper) +
					               " Reopening the scene might fix it.");
				}
				else if (instances.Length == 0)
				{
					var singleton = new GameObject();
					_instance = singleton.AddComponent<GoodiesSceneHelper>();
					singleton.name = "GoodiesSceneHelper";
					singleton.hideFlags = HideFlags.HideAndDontSave;

					DontDestroyOnLoad(singleton);

					Debug.Log("[Singleton] An _instance of " + typeof(GoodiesSceneHelper) +
					          " is needed in the scene, so '" + singleton.name +
					          "' was created with DontDestroyOnLoad.");
				}
				else
				{
					Debug.Log("[Singleton] Using _instance already created: " + _instance.gameObject.name);
				}
			}
		}

		GoodiesSceneHelper()
		{
		}

		internal static void Queue(Action action)
		{
			if (action == null)
			{
				Debug.LogWarning("Trying to queue null action");
				return;
			}

			lock (_instance._queueLock)
			{
				_instance._queuedActions.Add(action);
			}
		}

		void OnApplicationFocus(bool focusStatus)
		{
			if (focusStatus && IsInImmersiveMode)
			{
				AGUIMisc.EnableImmersiveMode();
			}
		}

		void Update()
		{
			MoveQueuedActionsToExecuting();

			while (_executingActions.Count > 0)
			{
				var action = _executingActions[0];
				_executingActions.RemoveAt(0);
				action();
			}
		}

		void MoveQueuedActionsToExecuting()
		{
			lock (_queueLock)
			{
				while (_queuedActions.Count > 0)
				{
					var action = _queuedActions[0];
					_executingActions.Add(action);
					_queuedActions.RemoveAt(0);
				}
			}
		}

		#region share_screenshot

		public void SaveScreenshotToGallery(Action<string> onScreenSaved)
		{
			StartCoroutine(TakeScreenshot(Screen.width, Screen.height, onScreenSaved));
		}

		IEnumerator TakeScreenshot(int width, int height, Action<string> onScreenSaved)
		{
			yield return new WaitForEndOfFrame();

			var texture = new Texture2D(width, height, TextureFormat.RGB24, true);
			texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
			texture.Apply();
			LastTakenScreenshot = texture;
			var imageTitle = "Screenshot-" + DateTime.Now.ToString("yy-MM-dd-hh-mm-ss");
			var uri = AndroidPersistanceUtilsInternal.InsertImage(LastTakenScreenshot, imageTitle, "My awesome screenshot");
			onScreenSaved(uri);
		}

		#endregion

		// These are invoked from Java by UnityPlayer.UnitySendMessage

		#region picker_callbacks

		[UsedImplicitly]
		public void OnPickGalleryImageSuccess(string message)
		{
			AGGallery.OnSuccessTrigger(message);
		}
		
		[UsedImplicitly]
		public void OnPickMultipleGalleryImages(string message)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("OnPickMultipleGalleryImages: " + message);
			}
			
			AGGallery.OnPickMultipleSuccessTrigger(message);
		}

		[UsedImplicitly]
		public void OnPickGalleryImageError(string message)
		{
			AGGallery.OnErrorTrigger(message);
		}

		[UsedImplicitly]
		public void OnPickPhotoImageSuccess(string message)
		{
			AGCamera.OnSuccessTrigger(message);
		}

		[UsedImplicitly]
		public void OnPickPhotoImageError(string message)
		{
			AGCamera.OnErrorTrigger(message);
		}

		[UsedImplicitly]
		public void OnPickContactSuccess(string message)
		{
			AGContacts.OnSuccessTrigger(message);
		}

		[UsedImplicitly]
		public void OnPickContactError(string message)
		{
			AGContacts.OnErrorTrigger(message);
		}

		[UsedImplicitly]
		public void OnRequestPermissionsResult(string message)
		{
			AGPermissions.OnRequestPermissionsResult(message);
		}

		[UsedImplicitly]
		public void OnPickAudioSuccess(string message)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("Audio picker success: " + message);
			}

			AGFilePicker.OnAudioSuccessTrigger(message);
		}

		[UsedImplicitly]
		public void OnPickAudioError(string message)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("Audio picker error: " + message);
			}

			AGFilePicker.OnAudioErrorTrigger(message);
		}

		[UsedImplicitly]
		public void OnPickVideoSuccess(string message)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("Video picker success: " + message);
			}

			AGFilePicker.OnVideoSuccessTrigger(message);
		}

		[UsedImplicitly]
		public void OnPickVideoError(string message)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("Video picker error: " + message);
			}

			AGFilePicker.OnVideoErrorTrigger(message);
		}

		[UsedImplicitly]
		public void OnRecordVideoSuccess(string message)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("Video picker success: " + message);
			}

			AGCamera.OnVideoSuccessTrigger(message);
		}

		[UsedImplicitly]
		public void OnRecordVideoError(string message)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("Video picker error: " + message);
			}

			AGCamera.OnVideoErrorTrigger(message);
		}

		[UsedImplicitly]
		public void OnPickFileError(string message)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("File picker error: " + message);
			}

			AGFilePicker.OnFileErrorTrigger(message);
		}

		[UsedImplicitly]
		public void OnPickFileSuccess(string message)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("File picker success: " + message);
			}

			AGFilePicker.OnFileSuccessTrigger(message);
		}

		#endregion

		[UsedImplicitly]
		public void OnPrintSuccess()
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("Print success.");
			}

			AGPrintHelper.OnPrintSuccess();
		}
		
		[UsedImplicitly]
		public void OnNotificationReceived(string notificationId)
		{
			if (Debug.isDebugBuild)
			{
				Debug.Log("Notification received with id: " + notificationId);
			}

			if (AGNotificationManager._onNotificationReceived != null)
			{
				AGNotificationManager._onNotificationReceived(int.Parse(notificationId));
			}
		}
	}
}