namespace DeadMosquito.AndroidGoodies
{
	using System;
	using Internal;
	using JetBrains.Annotations;
	using UnityEngine;

	[PublicAPI]
	public static class AGFileUtils
	{
		/// <summary>
		/// Returns absolute path to application-specific directory on the primary shared/external storage device where the application can place cache files it owns.
		/// </summary>
		[PublicAPI]
		public static string ExternalCacheDirectory => AGUtils.IsNotAndroid() ? null : AGUtils.ExternalCacheDirectory.GetAbsolutePath();

		/// <summary>
		/// Returns the absolute path to the application specific cache directory on the filesystem.
		/// </summary>
		[PublicAPI]
		public static string CacheDirectory => AGUtils.IsNotAndroid() ? null : AGUtils.CacheDirectory.GetAbsolutePath();

		/// <summary>
		/// Returns the absolute path to the application specific cache directory on the filesystem designed for storing cached code.
		/// </summary>
		[PublicAPI]
		public static string CodeCacheDirectory => AGUtils.IsNotAndroid() ? null : AGUtils.CodeCacheDirectory.GetAbsolutePath();

		/// <summary>
		/// Returns the absolute path to the directory on the filesystem where all private files belonging to this app are stored.
		/// </summary>
		[PublicAPI]
		public static string DataDir => AGUtils.IsNotAndroid() ? null : AGUtils.DataDir.GetAbsolutePath();

		/// <summary>
		/// Return the primary shared/external storage directory where this application's OBB files (if there are any) can be found.
		/// </summary>
		[PublicAPI]
		public static string ObbDir => AGUtils.IsNotAndroid() ? null : AGUtils.ObbDir.GetAbsolutePath();

		/// <summary>
		/// Insert the image into the device gallery
		/// </summary>
		/// <param name="texture2D">Texture2D to save.</param>
		/// <param name="title">Title.</param>
		/// <param name="description">Description of the image</param>
		/// <returns>Uri to the saved file</returns>
		public static string InsertImageToGallery([NotNull] Texture2D texture2D, string title, string description = null)
		{
			if (AGUtils.IsNotAndroid())
			{
				return null;
			}

			if (texture2D == null)
			{
				throw new ArgumentNullException(nameof(texture2D), "Image to save cannot be null");
			}

			return AndroidPersistanceUtilsInternal.InsertImage(texture2D, title, description);
		}

		/// <summary>
		/// Loads image by URI to Texture2D
		/// </summary>
		/// <returns>Loaded image as Texture2D.</returns>
		/// <param name="imageUri">Android Image URI.</param>
		[PublicAPI]
		public static Texture2D ImageUriToTexture2D(string imageUri)
		{
			if (AGUtils.IsNotAndroid())
			{
				return null;
			}

			Check.Argument.IsStrNotNullOrEmpty(imageUri, "imageUri");

			return AGUtils.TextureFromUriInternal(imageUri);
		}
	}
}