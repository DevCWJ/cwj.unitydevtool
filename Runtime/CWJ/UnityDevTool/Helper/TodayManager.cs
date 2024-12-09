using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CWJ
{
	public class TodayManager : CWJ.Singleton.SingletonBehaviour<TodayManager>
	{
		public static string CurDateOnlyFormat => DateWithoutTimeFormat_En;

		public const string DateWithoutTimeFormat_En = "MM/dd/yyyy";
		public const string DateTimeMinFullFormat_En = DateWithoutTimeFormat_En + " HH:mm";
		public const string DateTimeSecFullFormat_En = DateWithoutTimeFormat_En + " HH:mm:ss";

		public const string DateWithoutTimeFormat_Kr = "yyyy/MM/dd";
		public const string DateTimeMinFullFormat_Kr = DateWithoutTimeFormat_Kr + " HH:mm";
		public const string DateTimeSecFullFormat_Kr = DateWithoutTimeFormat_Kr + " HH:mm:ss";

#region format method (for AOT )

		static string ConvertToStr_Sec_En(DateTime dateTime) => dateTime.ToString(DateTimeSecFullFormat_En);
		static string ConvertToStr_Min_En(DateTime dateTime) => dateTime.ToString(DateTimeMinFullFormat_En);
		static string ConvertToStrWithoutTime_En(DateTime date) => date.ToString(DateWithoutTimeFormat_En);
		static string ConvertToStr_Sec_Kr(DateTime dateTime) => dateTime.ToString(DateTimeSecFullFormat_Kr);
		static string ConvertToStr_Min_Kr(DateTime dateTime) => dateTime.ToString(DateTimeMinFullFormat_Kr);
		static string ConvertToStrWithoutTime_Kr(DateTime date) => date.ToString(DateWithoutTimeFormat_Kr);

		static DateTime getNextMin()
		{
			DateTime nextMinute = NowDt.AddMinutes(1);
			return new DateTime(nextMinute.Year, nextMinute.Month, nextMinute.Day, nextMinute.Hour, nextMinute.Minute, 0);
		}

		static DateTime getNextSec()
		{
			DateTime nextMinute = NowDt.AddSeconds(1);
			return new DateTime(nextMinute.Year, nextMinute.Month, nextMinute.Day, nextMinute.Hour, nextMinute.Minute, nextMinute.Second);
		}

#endregion

		public bool updateForEnOrKr;
		public bool updateTimeUnit_SecOrMinOnly = true;
#if UNITY_EDITOR
		[SerializeField, Readonly] string editor_TodayDateStr;
		[SerializeField, Readonly] string editor_NowDtStr;
		bool isEditorChangeDateTime;

		[InvokeButton]
		void Editor_SetNowDt(string nowStr)
		{
			if (!Application.isPlaying)
			{
				return;
			}

			if (DateTime.TryParse(nowStr, out var newNow))
			{
				StopUpdater();
				isEditorChangeDateTime = true;
				NowDt = newNow;
				TodayDate = newNow.Date;
				TodayDateStr = _ConvertDateOnlyStr(TodayDate);
				_OnUpdateTodayEvent?.Invoke(TodayDate, TodayDateStr);
			}
		}

		[InvokeButton]
		void Editor_SetDate(int year, int month, int day)
		{
			if (!Application.isPlaying)
			{
				return;
			}

			StopUpdater();
			isEditorChangeDateTime = true;
			var now = DateTime.Now;
			var newNow = new DateTime(year, month, day, now.Hour, now.Minute, now.Second);
			NowDt = newNow;
			TodayDate = newNow.Date;
			TodayDateStr = _ConvertDateOnlyStr(TodayDate);
			_OnUpdateTodayEvent?.Invoke(TodayDate, TodayDateStr);
		}

		[InvokeButton, ShowConditional(nameof(isEditorChangeDateTime))]
		void Editor_ResetDate()
		{
			if (!Application.isPlaying)
			{
				return;
			}

			StartUpdater();
			isEditorChangeDateTime = false;
		}
#endif

		public static DateTime NowDt { get; private set; }
		public static DateTime TodayDate { get; private set; }
		public static string TodayDateStr { get; private set; }
		private static DateTime convertedNowDt;
		private static string _NowDtStr;
		private static string _NowTimeStr;
		private static int _NowTimeStamp;

		/// <summary>
		///
		/// </summary>
		/// <param name="nowDateTimeStr">날짜+시간 </param>
		/// <param name="nowTimeOnlyStr">시간만</param>
		/// <param name="nowTimestamp">timestamp</param>
		public static void UpdateNowDtToString(out string nowTimeOnlyStr, out string nowDateTimeStr, out int nowTimestamp)
		{
			if (NowDt != convertedNowDt)
			{
				convertedNowDt = NowDt;
				_NowTimeStr = convertedNowDt.ToString("HH:mm:ss");
#if UNITY_EDITOR
				if (!Application.isPlaying)
				{
					_NowDtStr = ConvertToStr_Sec_Kr(convertedNowDt);
				}
				else
#endif
					_NowDtStr = TodayDateStr + " " + _NowTimeStr;

				_NowTimeStamp = CWJ.DateTimeUtil.GetTimestamp(NowDt);
			}

			nowTimeOnlyStr = _NowTimeStr;
			nowDateTimeStr = _NowDtStr;
			nowTimestamp = _NowTimeStamp;
		}


		public delegate void OnUpdateToday(DateTime todayDate, string todayDateStr);

		private static OnUpdateToday _OnUpdateTodayEvent;

		/// <summary>
		/// 구독받을때 구독할 함수를 실행함. 주의
		/// </summary>
		public static event OnUpdateToday OnUpdateTodayEvent
		{
			add
			{
				if (value != null)
				{
					if (!isInit)
					{
						Init();
						UpdateNowDateTime();
					}

					value.Invoke(TodayDate, TodayDateStr);
					_OnUpdateTodayEvent += value;
				}
			}
			remove
			{
				_OnUpdateTodayEvent -= value;
			}
		}

		private static CancellationTokenSource cancellationTokenSrc = null;


		public static Func<DateTime, string> _ConvertToDateTimeStr;
		public static Func<DateTime, string> _ConvertDateOnlyStr;
		static Func<DateTime> _GetNextCheckTime;

		protected override void _Start()
		{
			StartUpdater();
		}

		static bool isInit = false;

		static void Init()
		{
			if (isInit)
			{
				return;
			}

			isInit = true;

			var ins = __UnsafeFastIns;
			if (ins.updateForEnOrKr)
			{
				_ConvertToDateTimeStr = ins.updateTimeUnit_SecOrMinOnly ? ConvertToStr_Sec_En : ConvertToStr_Min_En;
				_ConvertDateOnlyStr = ConvertToStrWithoutTime_En;
			}
			else
			{
				_ConvertToDateTimeStr = ins.updateTimeUnit_SecOrMinOnly ? ConvertToStr_Sec_Kr : ConvertToStr_Min_Kr;
				_ConvertDateOnlyStr = ConvertToStrWithoutTime_Kr;
			}

			_GetNextCheckTime = ins.updateTimeUnit_SecOrMinOnly ? getNextSec : getNextMin;
		}

		async void StartUpdater()
		{
			StopUpdater(); // 기존에 실행 중인 작업이 있다면 중지
			Init();
			UpdateNowDateTime();
			cancellationTokenSrc = new CancellationTokenSource();
			await UpdateTimeAsync(cancellationTokenSrc.Token);
		}

		static void StopUpdater()
		{
			// 기존 작업을 취소
			if (cancellationTokenSrc != null)
			{
				cancellationTokenSrc.Cancel();
				cancellationTokenSrc.Dispose();
				cancellationTokenSrc = null;
			}
		}

		static async UniTask UpdateTimeAsync(CancellationToken cancellationToken)
		{
			while (!cancellationToken.IsCancellationRequested)
			{
				UpdateNowDateTime();

				DateTime nextMinute = _GetNextCheckTime();

				TimeSpan waitTime = nextMinute - NowDt;

				await UniTask.Delay(waitTime, cancellationToken: cancellationToken);
			}
		}

		static void UpdateNowDateTime()
		{
			DateTime prevTodayDate = TodayDate;

			NowDt = DateTime.Now;
			TodayDate = NowDt.Date;

			if (prevTodayDate != TodayDate)
			{
				TodayDateStr = _ConvertDateOnlyStr(TodayDate);
				_OnUpdateTodayEvent?.Invoke(TodayDate, TodayDateStr);
#if UNITY_EDITOR
				Debug.LogError("Update TodayDate " + (__UnsafeFastIns.editor_TodayDateStr = TodayDateStr));
#endif
			}
#if UNITY_EDITOR
			UpdateNowDtToString(out _, out __UnsafeFastIns.editor_NowDtStr, out _);
			//            Debug.LogError("Update NowDt " + ( NowDtStr));
#endif
		}

		void OnApplicationPause(bool pauseStatus)
		{
#if UNITY_EDITOR
			return;
#endif
			if (pauseStatus)
			{
				StopUpdater();
			}
			else
			{
				StartUpdater();
			}
		}

		protected override void _OnDestroy()
		{
			StopUpdater();
		}
	}
}
