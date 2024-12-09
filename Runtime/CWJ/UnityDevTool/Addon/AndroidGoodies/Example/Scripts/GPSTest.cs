namespace AndroidGoodiesExamples
{
	using System;
	using DeadMosquito.AndroidGoodies;
	using JetBrains.Annotations;
	using UnityEngine;
	using UnityEngine.UI;

	public class GPSTest : MonoBehaviour
	{
		public const double AmsterdamLatitude = 52.3745913;
		public const double AmsterdamLongitude = 4.8285751;
		public const double BrusselsLatitude = 50.854954;
		public const double BrusselsLongitude = 4.3053508;

		public Text gpsText;
		public Text gpsInfoText;

		void Start()
		{
			try
			{
				gpsText.text = AGGPS.GetLastKnownGPSLocation().ToString();
			}
			catch (Exception)
			{
				Debug.Log("Could not get last known location.");
			}

			TestDistanceBetween();
		}

		void TestDistanceBetween()
		{
			var results = new float[3];
			AGGPS.DistanceBetween(AmsterdamLatitude, AmsterdamLongitude,
				BrusselsLatitude, BrusselsLongitude, results);
			gpsInfoText.text = $"DistanceBetween results: {results[0]}, Initial bearing: {results[1]}, Final bearing: {results[2]}";
		}

		[UsedImplicitly]
		public void OnStartTrackingLocation()
		{
			AGPermissions.ExecuteIfHasPermission(AGPermissions.ACCESS_FINE_LOCATION, StartLocationTacking, 
				() => ExampleUtil.ShowPermissionErrorToast(AGPermissions.ACCESS_FINE_LOCATION));
		}

		void StartLocationTacking()
		{
			// Minimum time interval between location updates, in milliseconds.
			const long minTimeInMillis = 0;
			// Minimum distance between location updates, in meters.
			const float minDistanceInMetres = 0;
			AGGPS.RequestLocationUpdates(minTimeInMillis, minDistanceInMetres, OnLocationChanged);
		}

		[UsedImplicitly]
		public void OnStopTrackingLocation()
		{
			AGGPS.RemoveUpdates();
		}

		void OnLocationChanged(AGGPS.Location location)
		{
			Debug.Log($"Location changed: {location}");
			gpsText.text = location.ToString();
		}
	}
}

