namespace AndroidGoodiesExamples
{
	using JetBrains.Annotations;
	using UnityEngine;
	using UnityEngine.UI;
	using DeadMosquito.AndroidGoodies;
	public class WifiTest : MonoBehaviour
	{
		const int FirstNetworkId = 0;
		
#pragma warning disable 0649
		[SerializeField]
		Text _resultText;
#pragma warning restore 0649

		[UsedImplicitly]
		public void ShowWifiNetworks()
		{
			AGNetwork.ShowAvailableWifiNetworks();
		}

		[UsedImplicitly]
		public void GetConfiguredNetworks()
		{
			var list = AGNetwork.ConfiguredNetworks;
			_resultText.text = "";
			foreach (var config in list)
			{
				_resultText.text += config;
			}
		}

		[UsedImplicitly]
		public void DisconnectFromWifi()
		{
			AGNetwork.Disconnect();
		}

		[UsedImplicitly]
		public void ConnectToWifiNetwork()
		{
			AGNetwork.EnableNetwork(FirstNetworkId, true);
		}

		[UsedImplicitly]
		public void EnableWifi()
		{
			AGNetwork.SetWifiEnabled(true);
		}

		[UsedImplicitly]
		public void DisableWifi()
		{
			AGNetwork.SetWifiEnabled(false);
		}
	}
}
