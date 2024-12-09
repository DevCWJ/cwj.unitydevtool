namespace DeadMosquito.AndroidGoodies.Internal
{
	using System;

	public static class LocationUtils
	{
		public static void ComputeDistanceAndBearing(double lat1, double lon1,
			double lat2, double lon2, float[] results)
		{
			// Based on http://www.ngs.noaa.gov/PUBS_LIB/inverse.pdf
			// using the "Inverse Formula" (section 4)

			var MAXITERS = 20;
			// Convert lat/long to radians
			lat1 *= Math.PI / 180.0;
			lat2 *= Math.PI / 180.0;
			lon1 *= Math.PI / 180.0;
			lon2 *= Math.PI / 180.0;

			var a = 6378137.0; // WGS84 major axis
			var b = 6356752.3142; // WGS84 semi-major axis
			var f = (a - b) / a;
			var aSqMinusBSqOverBSq = (a * a - b * b) / (b * b);

			var L = lon2 - lon1;
			var A = 0.0;
			var U1 = Math.Atan((1.0 - f) * Math.Tan(lat1));
			var U2 = Math.Atan((1.0 - f) * Math.Tan(lat2));

			var cosU1 = Math.Cos(U1);
			var cosU2 = Math.Cos(U2);
			var sinU1 = Math.Sin(U1);
			var sinU2 = Math.Sin(U2);
			var cosU1cosU2 = cosU1 * cosU2;
			var sinU1sinU2 = sinU1 * sinU2;

			var sigma = 0.0;
			var deltaSigma = 0.0;
			var cosSqAlpha = 0.0;
			var cos2SM = 0.0;
			var cosSigma = 0.0;
			var sinSigma = 0.0;
			var cosLambda = 0.0;
			var sinLambda = 0.0;

			var lambda = L; // initial guess
			for (var iter = 0; iter < MAXITERS; iter++)
			{
				var lambdaOrig = lambda;
				cosLambda = Math.Cos(lambda);
				sinLambda = Math.Sin(lambda);
				var t1 = cosU2 * sinLambda;
				var t2 = cosU1 * sinU2 - sinU1 * cosU2 * cosLambda;
				var sinSqSigma = t1 * t1 + t2 * t2; // (14)
				sinSigma = Math.Sqrt(sinSqSigma);
				cosSigma = sinU1sinU2 + cosU1cosU2 * cosLambda; // (15)
				sigma = Math.Atan2(sinSigma, cosSigma); // (16)
				var sinAlpha = (sinSigma == 0) ? 0.0 : cosU1cosU2 * sinLambda / sinSigma; // (17)
				cosSqAlpha = 1.0 - sinAlpha * sinAlpha;
				cos2SM = (cosSqAlpha == 0) ? 0.0 : cosSigma - 2.0 * sinU1sinU2 / cosSqAlpha; // (18)

				var uSquared = cosSqAlpha * aSqMinusBSqOverBSq; // defn
				A = 1 + (uSquared / 16384.0) * // (3)
					(4096.0 + uSquared *
						(-768 + uSquared * (320.0 - 175.0 * uSquared)));
				var B = (uSquared / 1024.0) * // (4)
				        (256.0 + uSquared *
					        (-128.0 + uSquared * (74.0 - 47.0 * uSquared)));
				var C = (f / 16.0) *
				        cosSqAlpha *
				        (4.0 + f * (4.0 - 3.0 * cosSqAlpha)); // (10)
				var cos2SMSq = cos2SM * cos2SM;
				deltaSigma = B * sinSigma * // (6)
				             (cos2SM + (B / 4.0) *
					             (cosSigma * (-1.0 + 2.0 * cos2SMSq) -
					              (B / 6.0) * cos2SM *
					              (-3.0 + 4.0 * sinSigma * sinSigma) *
					              (-3.0 + 4.0 * cos2SMSq)));

				lambda = L +
				         (1.0 - C) * f * sinAlpha *
				         (sigma + C * sinSigma *
					         (cos2SM + C * cosSigma *
						         (-1.0 + 2.0 * cos2SM * cos2SM))); // (11)

				var delta = (lambda - lambdaOrig) / lambda;
				if (Math.Abs(delta) < 1.0e-12)
				{
					break;
				}
			}

			var distance = (float) (b * A * (sigma - deltaSigma));
			results[0] = distance;
			if (results.Length > 1)
			{
				var initialBearing = (float) Math.Atan2(cosU2 * sinLambda,
					cosU1 * sinU2 - sinU1 * cosU2 * cosLambda);
				initialBearing *= 180.0f / (float) Math.PI;
				results[1] = initialBearing;
				if (results.Length > 2)
				{
					var finalBearing = (float) Math.Atan2(cosU1 * sinLambda,
						-sinU1 * cosU2 + cosU1 * sinU2 * cosLambda);
					finalBearing *= 180.0f / (float) Math.PI;
					results[2] = finalBearing;
				}
			}
		}
	}
}