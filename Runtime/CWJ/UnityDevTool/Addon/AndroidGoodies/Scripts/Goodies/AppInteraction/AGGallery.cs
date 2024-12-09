using System.Collections.Generic;

namespace DeadMosquito.AndroidGoodies
{
	using System;
	using Internal;
	using JetBrains.Annotations;
	using UnityEngine;

	/// <summary>
	/// Methods to interact with device gallery.
	/// </summary>
	[PublicAPI]
	public static class AGGallery
	{
		static Action<ImagePickResult> _onSuccessAction;
		static Action<List<ImagePickResult>> _onSuccessMultiplesAction;
		static Action<string> _onCancelAction;

		/// <summary>
		/// Picks the image from gallery.
		/// </summary>
		/// <param name="onSuccess">On success callback. Image is received as callback parameter</param>
		/// <param name="onError">On error callback.</param>
		/// <param name="maxSize">Max image size. If provided image will be downscaled.</param>
		/// <param name="shouldGenerateThumbnails">Whether thumbnail images will be generated. Used to show small previews of image.</param>
		[PublicAPI]
		public static void PickImageFromGallery([NotNull] Action<ImagePickResult> onSuccess, Action<string> onError,
			ImageResultSize maxSize = ImageResultSize.Original, bool shouldGenerateThumbnails = true)
		{
			if (AGUtils.IsNotAndroid())
			{
				return;
			}

			Check.Argument.IsNotNull(onSuccess, "onSuccess");

			_onSuccessAction = onSuccess;
			_onCancelAction = onError;

			AGActivityUtils.PickPhotoFromGallery(maxSize, shouldGenerateThumbnails, false);
		}

		/// <summary>
		/// Picks multiple images from gallery.
		/// </summary>
		/// <param name="onSuccess">On success callback. List of images is received as callback parameter</param>
		/// <param name="onError">On error callback.</param>
		/// <param name="maxSize">Max image size. If provided image will be downscaled.</param>
		/// <param name="shouldGenerateThumbnails">Whether thumbnail images will be generated. Used to show small previews of image.</param>
		[PublicAPI]
		public static void PickMultipleImagesFromGallery([NotNull] Action<List<ImagePickResult>> onSuccess, Action<string> onError,
			ImageResultSize maxSize = ImageResultSize.Original, bool shouldGenerateThumbnails = true)
		{
			if (AGUtils.IsNotAndroid())
			{
				return;
			}

			Check.Argument.IsNotNull(onSuccess, "onSuccess");

			_onSuccessMultiplesAction = onSuccess;
			_onCancelAction = onError;

			AGActivityUtils.PickPhotoFromGallery(maxSize, shouldGenerateThumbnails, true);
		}

		/// <summary>
		/// Insert the image into the device gallery
		/// </summary>
		/// <param name="texture2D">Texture2D to save.</param>
		/// <param name="title">Title.</param>
		/// <param name="description">Description of the image</param>
		/// <returns>Uri to the saved file</returns>
		public static void InsertImageToGallery([NotNull] Texture2D texture2D, [NotNull] string title, string description = null)
		{
			if (AGUtils.IsNotAndroid())
			{
				return;
			}

			AGFileUtils.InsertImageToGallery(texture2D, title, description);
		}

		/// <summary>
		/// Call this method after you have saved the image for it to appear in gallery applications
		/// </summary>
		/// <param name="filePath">File path to scan</param>
		[PublicAPI]
		public static void RefreshFile([NotNull] string filePath)
		{
			Check.Argument.IsStrNotNullOrEmpty(filePath, "path");

			if (AGUtils.IsNotAndroid())
			{
				return;
			}

			AndroidPersistanceUtilsInternal.RefreshGallery(filePath);
		}


		// Invoked by UnityPlayer.SendMessage
		internal static void OnSuccessTrigger(string imageCallbackJson)
		{
			if (_onSuccessAction == null)
			{
				return;
			}

			var image = ImagePickResult.FromJson(imageCallbackJson);
			_onSuccessAction(image);
		}

		internal static void OnPickMultipleSuccessTrigger(string imageCallbackJson)
		{
			if (_onSuccessMultiplesAction == null)
			{
				return;
			}

			var images = ImagePickResult.FromJsonArray(imageCallbackJson);
			_onSuccessMultiplesAction(images);
		}

		internal static void OnErrorTrigger(string errorMessage)
		{
			if (_onCancelAction == null)
			{
				return;
			}

			_onCancelAction(errorMessage);
			_onCancelAction = null;
		}
	}
}