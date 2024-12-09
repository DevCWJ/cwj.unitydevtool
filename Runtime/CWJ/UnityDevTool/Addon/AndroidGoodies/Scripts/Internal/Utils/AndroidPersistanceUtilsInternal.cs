namespace DeadMosquito.AndroidGoodies.Internal
{
	using System;
	using System.IO;
	using UnityEngine;

	public static class AndroidPersistanceUtilsInternal
	{
		const string FileProviderClass = "androidx.core.content.FileProvider";

		const string GoodiesTmpImageFileName = "android-goodies-tmp-image.png";

		// TODO test
		public static string SaveWallpaperImageToExternalStorage(Texture2D tex2D)
		{
			return SaveImageToCacheDirectoryPath(tex2D);
		}

		// TODO test
		public static AndroidJavaObject /* Uri */ SaveWallpaperImageToExternalStorageUri(Texture2D tex2D)
		{
			return SaveImageToCacheDirectory(tex2D);
		}

		// TODO test
		public static AndroidJavaObject SaveImageToCacheDirectory(Texture2D tex2D)
		{
			return GetUriFromFilePath(SaveImageToCacheDirectoryPath(tex2D));
		}

		static string SaveImageToCacheDirectoryPath(Texture2D tex2D)
		{
			var filePath = Path.Combine(AGUtils.CacheDirectory.CallStr("getAbsolutePath"), GoodiesTmpImageFileName);
			if (Debug.isDebugBuild)
			{
				Debug.Log("Android Goodies ----- saving file to temporary file: " + filePath);
			}

			try
			{
				var file = File.Open(filePath, FileMode.OpenOrCreate);
				var binary = new BinaryWriter(file);
				binary.Write(tex2D.EncodeToPNG());
				file.Close();
				return filePath;
			}
			catch (Exception e)
			{
				Debug.LogError("Android Goodies failed to save file " + filePath);
				Debug.LogException(e);
			}

			return null;
		}

		public static AndroidJavaObject GetUriFromFilePath(string saveFilePath)
		{
			AndroidJavaObject uri;
			if (AGDeviceInfo.SDK_INT >= AGDeviceInfo.VersionCodes.N)
			{
				using (var c = new AndroidJavaClass(FileProviderClass))
				{
					var provider = AGDeviceInfo.GetApplicationPackage() + ".multipicker.fileprovider";
					uri = c.CallStaticAJO("getUriForFile", AGUtils.Activity, provider, AGUtils.NewJavaFile(saveFilePath));
				}
			}
			else
			{
				uri = AndroidUri.FromFile(saveFilePath);
			}

			return uri;
		}

		public static void RefreshGallery(string filePath)
		{
			if (AGDeviceInfo.SDK_INT >= AGDeviceInfo.VersionCodes.KITKAT)
			{
				ScanFile(filePath, null);
			}
			else
			{
				var uri = AndroidUri.FromFile(filePath);
				var intent = new AndroidIntent(AndroidIntent.ActionMediaMounted, uri);
				AGUtils.SendBroadcast(intent.AJO);
			}
		}

		public static string InsertImage(Texture2D texture2D, string title, string description)
		{
			using (var mediaClass = new AndroidJavaClass(C.AndroidProviderMediaStoreImagesMedia))
			{
				using (var cr = AGUtils.ContentResolver)
				{
					var image = AGUtils.Texture2DToAndroidBitmap(texture2D);
					var imageUrl = mediaClass.CallStaticStr("insertImage", cr, image, title, description);
					return imageUrl;
				}
			}
		}

		public static void ScanFile(string filePath, Action<string, AndroidJavaObject> onScanCompleted)
		{
			var listener = onScanCompleted == null ? null : new OnScanCompletedListener(onScanCompleted);
			using (var c = new AndroidJavaClass(C.AndroidMediaMediaScannerConnection))
			{
				c.CallStatic("scanFile", AGUtils.Activity, new[] { filePath }, null, listener);
			}
		}
	}
}