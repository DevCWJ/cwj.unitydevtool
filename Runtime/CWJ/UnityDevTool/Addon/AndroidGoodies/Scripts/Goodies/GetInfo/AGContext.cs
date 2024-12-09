using JetBrains.Annotations;

namespace DeadMosquito.AndroidGoodies
{
	using Internal;

	/// <summary>
	/// Provides various properties of the Android context
	/// </summary>
	[PublicAPI]
	public class AGContext
	{
		/// <summary>
		/// Returns the absolute path to the directory on the filesystem where files are created
		/// </summary>
		public static string FilesDir
		{
			get
			{
				if (AGUtils.IsNotAndroid())
				{
					return null;
				}

				return AGUtils.Activity.CallAJO("getFilesDir").GetAbsolutePath();
			}
		}
	}
}