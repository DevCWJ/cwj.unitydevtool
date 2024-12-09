namespace DeadMosquito.AndroidGoodies.Internal
{
	using System;
	using UnityEngine;

	public static class AGSystemService
	{
		const string VIBRATOR_SERVICE = "vibrator";
		static AndroidJavaObject _vibratorService;

		const string LOCATION_SERVICE = "location";
		static AndroidJavaObject _locationService;

		const string CONNECTIVITY_SERVICE = "connectivity";
		static AndroidJavaObject _connectivityService;

		const string WIFI_SERVICE = "wifi";
		static AndroidJavaObject _wifiService;

		const string TELEPHONY_SERVICE = "phone";
		static AndroidJavaObject _telephonyService;

		const string NOTIFICATION_SERVICE = "notification";
		static AndroidJavaObject _notificationService;
		static AndroidJavaObject _notificationServiceCompat;
		
		const string SHORTCUT_SERVICE = "shortcut";
		static AndroidJavaObject _shortcutService;

		const string CAMERA_SERVICE = "camera";
		static AndroidJavaObject _cameraService;

		const string CLIPBOARD_SERVICE = "clipboard";
		static AndroidJavaObject _clipboardService;
		
		const string ALARM_SERVICE = "alarm";
		static AndroidJavaObject _alarmService;
		
		const string BATTERY_SERVICE = "batterymanager";
		static AndroidJavaObject _batteryService;

		public static AndroidJavaObject VibratorService => _vibratorService ?? (_vibratorService = GetSystemService(VIBRATOR_SERVICE, C.AndroidOsVibrator));

		public static AndroidJavaObject LocationService => _locationService ?? (_locationService = GetSystemService(LOCATION_SERVICE, C.AndroidLocationLocationManager));

		public static AndroidJavaObject ConnectivityService => _connectivityService ?? (_connectivityService = GetSystemService(CONNECTIVITY_SERVICE, C.AndroidNetConnectivityManager));

		public static AndroidJavaObject WifiService => _wifiService ?? (_wifiService = GetSystemService(WIFI_SERVICE, C.AndroidNetWifiManager));

		public static AndroidJavaObject TelephonyService => _telephonyService ?? (_telephonyService = GetSystemService(TELEPHONY_SERVICE, C.AndroidTelephonyTelephonyManager));

		public static AndroidJavaObject NotificationService => _notificationService ?? (_notificationService = GetSystemService(NOTIFICATION_SERVICE, C.AndroidAppNotificationManager));

		public static AndroidJavaObject ShortcutService => _shortcutService ?? (_shortcutService = GetSystemService(SHORTCUT_SERVICE, C.AndroidContentPmShortcutManager));

		public static AndroidJavaObject NotificationServiceCompat => _notificationServiceCompat ?? (_notificationServiceCompat = C.AndroidAppNotificationManagerCompat.AJCCallStaticOnceAJO("from", AGUtils.Activity));

		public static AndroidJavaObject CameraService => _cameraService ?? (_cameraService = GetSystemService(CAMERA_SERVICE, C.AndroidHardwareCamera2CameraManager));

		public static AndroidJavaObject ClipboardService => _clipboardService ?? (_clipboardService = GetSystemService(CLIPBOARD_SERVICE, C.AndroidContentClipboardManager));

		public static AndroidJavaObject AlarmService => _alarmService ?? (_alarmService = GetSystemService(ALARM_SERVICE, C.AndroidAppAlarmManager));

		public static AndroidJavaObject BatteryService => _batteryService ?? (_batteryService = GetSystemService(BATTERY_SERVICE, C.AndroidOsBatteryManager));

		static AndroidJavaObject GetSystemService(string name, string serviceClass)
		{
			try
			{
				var serviceObj = AGUtils.Activity.CallAJO("getSystemService", name);
				return serviceObj.Cast(serviceClass);
			}
			catch (Exception e)
			{
				if (Debug.isDebugBuild)
				{
					Debug.LogWarning("Failed to get " + name + " service. Error: " + e.Message);
				}

				return null;
			}
		}
	}
}