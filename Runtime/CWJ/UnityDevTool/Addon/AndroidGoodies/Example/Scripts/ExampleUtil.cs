namespace AndroidGoodiesExamples
{
	using DeadMosquito.AndroidGoodies;

	public static class ExampleUtil
	{
		public static void ShowPermissionErrorToast(string permission)
		{
			var message = $"{permission} runtime permission missing in AndroidManifest.xml or user did not grant the permission.";
			AGUIMisc.ShowToast(message);
		}
	}
}