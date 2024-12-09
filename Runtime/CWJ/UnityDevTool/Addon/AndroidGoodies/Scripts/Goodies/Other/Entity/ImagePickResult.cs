namespace DeadMosquito.AndroidGoodies
{
	using System;
	using System.Collections.Generic;
	using Internal;
	using JetBrains.Annotations;
	using MiniJSON;
	using UnityEngine;
	using System.Linq;

	/// <summary>
	///     Image that was picked
	/// </summary>
	[PublicAPI]
	public class ImagePickResult
	{
		/// <summary>
		/// Path to the processed file. This will always be a local path on the device.
		/// </summary>
		[PublicAPI]
		public string OriginalPath { get; private set; }

		/// <summary>
		/// Display name of the file
		/// </summary>
		[PublicAPI]
		public string DisplayName { get; private set; }

		/// <summary>
		/// Get the path to the thumbnail(big) of the image
		/// </summary>
		[PublicAPI]
		public string ThumbnailPath { get; private set; }

		/// <summary>
		/// Get the path to the thumbnail(small) of the image
		/// </summary>
		[PublicAPI]
		public string SmallThumbnailPath { get; private set; }

		/// <summary>
		/// Get the image width
		/// </summary>
		[PublicAPI]
		public int Width { get; private set; }

		/// <summary>
		/// Get the image height
		/// </summary>
		[PublicAPI]
		public int Height { get; private set; }

		/// <summary>
		/// Get the size of the processed file in bytes
		/// </summary>
		[PublicAPI]
		public int Size { get; private set; }

		/// <summary>
		/// File creation date
		/// </summary>
		[PublicAPI]
		public DateTime CreatedAt { get; private set; }

		/// <summary>
		/// Read the picked file and load image into <see cref="Texture2D"/>
		/// </summary>
		/// <returns>Picked image as <see cref="Texture2D"/></returns>
		[PublicAPI]
		public Texture2D LoadTexture2D()
		{
			return AGUtils.Texture2DFromFile(OriginalPath);
		}

		/// <summary>
		/// Read the picked file and load thumbnail image into <see cref="Texture2D"/>
		/// NOTE: Will be null if "generateThumbnails" param is set to false when picking image
		/// </summary>
		/// <returns>Picked image thumbnail as <see cref="Texture2D"/></returns>
		[PublicAPI]
		public Texture2D LoadThumbnailTexture2D()
		{
			return CommonUtils.TextureFromFile(ThumbnailPath);
		}

		/// <summary>
		/// Read the picked file and load small thumbnail image into <see cref="Texture2D"/>
		/// NOTE: Will be null if "generateThumbnails" param is set to false when picking image
		/// </summary>
		/// <returns>Picked small image thumbnail as <see cref="Texture2D"/></returns>
		[PublicAPI]
		public Texture2D LoadSmallThumbnailTexture2D()
		{
			return CommonUtils.TextureFromFile(SmallThumbnailPath);
		}

		public static List<ImagePickResult> FromJsonArray(string json)
		{
			var arr = Json.Deserialize(json) as List<object>;
			return arr.Select(o => FromDictionary(o as Dictionary<string, object>)).ToList();
		}

		public static ImagePickResult FromJson(string json)
		{
			var dic = Json.Deserialize(json) as Dictionary<string, object>;
			return FromDictionary(dic);
		}

		static ImagePickResult FromDictionary(Dictionary<string, object> dic)
		{
			return new ImagePickResult
			{
				OriginalPath = dic.GetStr("originalPath"),
				ThumbnailPath = dic.GetStr("thumbnailPath"),
				SmallThumbnailPath = dic.GetStr("thumbnailSmallPath"),
				DisplayName = dic.GetStr("displayName"),
				Width = (int)(long)dic["width"],
				Height = (int)(long)dic["height"],
				Size = (int)(long)dic["size"],
				CreatedAt = CommonUtils.DateTimeFromMillisSinceEpoch((long)dic["createdAt"])
			};
		}

		public override string ToString()
		{
			return
				$"[ImagePickResult: OriginalPath={OriginalPath}, DisplayName={DisplayName}, ThumbnailPath={ThumbnailPath}, SmallThumbnailPath={SmallThumbnailPath}, Width={Width}, Height={Height}, Size={Size}]";
		}
	}
}