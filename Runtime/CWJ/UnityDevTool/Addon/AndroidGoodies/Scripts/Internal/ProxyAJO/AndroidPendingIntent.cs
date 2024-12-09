namespace DeadMosquito.AndroidGoodies.Internal
{
	using UnityEngine;

	/// <summary>
	/// Android pending intent.
	/// </summary>
	static class AndroidPendingIntent
	{
		public const int FLAG_CANCEL_CURRENT = 268435456;
		public const int FLAG_IMMUTABLE = 67108864;
		public const int FLAG_MUTABLE = 33554432;
		public const int FLAG_NO_CREATE = 536870912;
		public const int FLAG_ONE_SHOT = 1073741824;
		public const int FLAG_UPDATE_CURRENT = 134217728;

		public static AndroidJavaObject GetActivity(AndroidJavaObject intent, int id, int flag = FLAG_UPDATE_CURRENT)
		{
			using (var pic = new AndroidJavaClass(C.AndroidAppPendingIntent))
			{
				return GetActivity(intent, id, pic, AGDeviceInfo.SDK_INT >= AGDeviceInfo.VersionCodes.S ? FLAG_IMMUTABLE : FLAG_UPDATE_CURRENT);
			}
		}

		static AndroidJavaObject GetActivity(AndroidJavaObject intent, int id, AndroidJavaClass pic, int flags)
		{
			return pic.CallStaticAJO("getActivity", AGUtils.Activity, id, intent, flags);
		}

		public static AndroidJavaObject GetBroadcast(AndroidJavaObject intent, int id)
		{
			using (var pic = new AndroidJavaClass(C.AndroidAppPendingIntent))
			{
				return pic.CallStaticAJO("getBroadcast", AGUtils.Activity, id, intent, AGDeviceInfo.SDK_INT >= AGDeviceInfo.VersionCodes.S ? FLAG_IMMUTABLE : FLAG_UPDATE_CURRENT);
			}
		}
	}
}