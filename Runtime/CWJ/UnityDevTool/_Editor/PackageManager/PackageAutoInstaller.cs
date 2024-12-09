#if UNITY_EDITOR
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace CWJ.Installer
{
	public static class PackageAutoInstaller
	{
		private const string CWJ_Installer_Name = "com.cwj.installer";

		private static readonly string[] InstallGitUrls = new string[]
		                                                  {
			                                                  UniRxUrl,
			                                                  UniTaskUrl,
			                                                  UnityDevToolUrl
		                                                  };

		private const string UniRxUrl = "com.neuecc.unirx@https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx/Scripts";
		private const string UniTaskUrl = "com.cysharp.unitask@https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask";
		private const string UnityDevToolUrl = "com.cwj.unitydevtool@https://github.com/DevCWJ/cwj.unitydevtool.git";

		[InitializeOnLoadMethod]
		static void Init()
		{
			PackageManagerExtensions.RegisterExtension(new UpmExtension());
		}

		class UpmExtension : IPackageManagerExtension
		{
			public VisualElement CreateExtensionUI()
			{
				VisualElement ExtentionRoot = new VisualElement();
				VisualElement label = new VisualElement();
				ExtentionRoot.Add(label);
				detail = new Label();
				detail.text = "cwj";
				label.Add(detail);


				VisualElement buttons = new VisualElement();
				ExtentionRoot.Add(buttons);

				buttons.style.flexDirection = FlexDirection.Row;
				buttons.style.flexWrap = Wrap.Wrap;

				const int width = 160;

				opengit = new Button();
				opengit.text = "Open Git Link";
				opengit.clicked += Opengit_clicked;
				opengit.style.width = width;
				buttons.Add(opengit);
				return ExtentionRoot;
			}

			private void Opengit_clicked()
			{
				if (current?.source == PackageSource.Git)
				{
					var url = current.GetType().GetField("m_ProjectDependenciesEntry",
					                                     BindingFlags.NonPublic | BindingFlags.Instance)
					                 .GetValue(current) as string;
					Debug.Log($"OPEN LINK：{url}");
					Application.OpenURL(url);
				}
			}

			public PackageInfo current = null;
			private Button openFolder;
			private Button opengit;
			private Label detail;

			public void OnPackageSelectionChange(PackageInfo packageInfo)
			{
				current = packageInfo;
				bool isGit = current?.source == PackageSource.Git;

				detail.text = $"[Git : {isGit}]";

				if (current != null)
				{
					Debug.LogError("Installed: " + packageInfo.displayName + "\n" + current.packageId);
					if (current.name == CWJ_Installer_Name || InstallGitUrls.Contains(current.packageId))
					{
						EditorApplication.update += InstallPackages;
					}
				}

				opengit.SetEnabled(isGit);
			}

			public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
			{
			}

			public void OnPackageRemoved(PackageInfo packageInfo)
			{
			}
		}


		private static AddRequest currentRequest;
		private static int currentIndex;

		private static void InstallPackages()
		{
			// Unity 에디터가 실행 중일 때만 동작
			if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
				return;

			// 설치 요청이 없으면 새 요청 시작
			if (currentRequest == null && currentIndex < InstallGitUrls.Length)
			{
				Debug.LogError($"Installing package: {InstallGitUrls[currentIndex]}");
				currentRequest = Client.Add(InstallGitUrls[currentIndex]);
			}

			// 요청 상태 확인
			if (currentRequest != null && currentRequest.IsCompleted)
			{
				if (currentRequest.Status == StatusCode.Success)
				{
					Debug.LogError($"Successfully installed: {InstallGitUrls[currentIndex]}");
				}
				else if (currentRequest.Status >= StatusCode.Failure)
				{
					Debug.LogError($"Failed to install: {InstallGitUrls[currentIndex]} - {currentRequest.Error.message}");
				}

				// 다음 패키지로 이동
				currentRequest = null;
				currentIndex++;
			}

			// 모든 패키지 설치 완료 시 업데이트 종료
			if (currentIndex >= InstallGitUrls.Length)
			{
				Debug.LogError("All packages installed.");
				EditorApplication.update -= InstallPackages;
			}
		}
	}
}

#endif
