using UnityEngine.UI;

namespace AndroidGoodiesExamples
{
	using UnityEngine;

	public class AndroidGoodiesDemoController : MonoBehaviour
	{
		public GameObject uiPanel;
		public GameObject appInteractionPanel;
		public GameObject sharePanel;
		public GameObject hardwarePanel;
		public GameObject otherStuffPanel;
		public GameObject textInfoPanel;
		public GameObject newNotificationsPanel;
		public GameObject shortcutsPanel;
		
		public GameObject audioPanel;
		public Text pauseButtonText;

		public void OnUI()
		{
			uiPanel.SetActive(true);
		}

		public void OnOpenApps()
		{
			appInteractionPanel.SetActive(true);
		}

		public void OnShare()
		{
			sharePanel.SetActive(true);
		}

		public void OnHardware()
		{
			hardwarePanel.SetActive(true);
		}

		public void OnTextInfo()
		{
			textInfoPanel.SetActive(true);
		}

		public void OnOtherStuff()
		{
			otherStuffPanel.SetActive(true);
			audioPanel.SetActive(false);
		}

		public void OnNewNotifications()
		{
			newNotificationsPanel.SetActive(true);
		}

		public void OnShortcuts()
		{
			shortcutsPanel.SetActive(true);
		}
	}
}