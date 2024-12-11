// #if UNITY_EDITOR && !CWJ_SCENEENUM_ENABLED && !CWJ_SCENEENUM_DISABLED
// using System.IO;
// using System.Linq;
// using System.Reflection;
// using UnityEditor;
// using UnityEditor.PackageManager;
// using UnityEditor.PackageManager.Requests;
// using UnityEditor.PackageManager.UI;
// using UnityEngine;
// using UnityEngine.UIElements;
// using Debug = UnityEngine.Debug;
// using PackageInfo = UnityEditor.PackageManager.PackageInfo;
// using System.Diagnostics;
// using System;
//
// namespace CWJ.Installer
// {
// 	public static class PackageAutoInstaller
// 	{
// 		private const string CWJ_Installer_Name = "com.cwj.installer";
// 		private const string GitRepoUrl = "https://github.com/DevCWJ/cwj.installer.git";
//
// 		private static readonly string[] InstallGitUrls = new string[]
// 		                                                  {
// 			                                                  UniRxUrl,
// 			                                                  UniTaskUrl,
// 			                                                  UnityDevToolUrl
// 		                                                  };
//
// 		private const string UniRxUrl = "https://github.com/neuecc/UniRx.git?path=Assets/Plugins/UniRx/Scripts"; //com.neuecc.unirx
// 		private const string UniTaskUrl = "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask"; //com.cysharp.unitask
// 		private const string UnityDevToolUrl = "https://github.com/DevCWJ/cwj.unitydevtool.git"; //com.cwj.unitydevtool
// 		private const string descStr = "[CWJ 라이브러리 Installer]";
//
// 		[InitializeOnLoadMethod]
// 		static void Init()
// 		{
// 			PackageManagerExtensions.RegisterExtension(new UpmExtension());
// 		}
//
// 		class UpmExtension : IPackageManagerExtension
// 		{
// 			public VisualElement CreateExtensionUI()
// 			{
// 				VisualElement ExtentionRoot = new VisualElement();
// 				VisualElement labelLine = new VisualElement();
// 				ExtentionRoot.Add(labelLine);
// 				descLbl = new Label();
// 				descLbl.text = "cwj";
// 				labelLine.Add(descLbl);
//
//
// 				VisualElement buttonLine1 = new VisualElement();
// 				ExtentionRoot.Add(buttonLine1);
//
// 				buttonLine1.style.flexDirection = FlexDirection.Row;
// 				buttonLine1.style.flexWrap = Wrap.Wrap;
//
// 				const int width = 160;
//
// 				changeApiCompatibilityBtn = new Button();
// 				changeApiCompatibilityBtn.text = "Change API Compatibility";
// 				changeApiCompatibilityBtn.clicked += ChangeApiCompatibility;
// 				changeApiCompatibilityBtn.style.width = width;
// 				buttonLine1.Add(changeApiCompatibilityBtn);
//
// 				installBtn = new Button();
// 				installBtn.text = "Install Packages";
// 				installBtn.clicked += OnClickInstallBtn;
// 				installBtn.style.width = width;
// 				buttonLine1.Add(installBtn);
//
// 				VisualElement buttonsLine2 = new VisualElement();
// 				ExtentionRoot.Add(buttonsLine2);
// 				buttonsLine2.style.flexDirection = FlexDirection.Row;
// 				buttonsLine2.style.flexWrap = Wrap.Wrap;
//
// 				opengit = new Button();
// 				opengit.text = "Open Git Link";
// 				opengit.clicked += Opengit_clicked;
// 				opengit.style.width = width;
// 				buttonsLine2.Add(opengit);
// 				return ExtentionRoot;
// 			}
//
// 			private void Opengit_clicked()
// 			{
// 				if (current?.source == PackageSource.Git)
// 				{
// 					var url = current.GetType().GetField("m_ProjectDependenciesEntry",
// 					                                     BindingFlags.NonPublic | BindingFlags.Instance)
// 					                 .GetValue(current) as string;
// 					Debug.Log($"OPEN LINK：{url}");
// 					Application.OpenURL(url);
// 				}
// 			}
//
// 			public PackageInfo current = null;
// 			private Button openFolder;
// 			private Button opengit, changeApiCompatibilityBtn, installBtn;
// 			private Label descLbl;
//
// 			public void OnPackageSelectionChange(PackageInfo packageInfo)
// 			{
// 				bool canEnabled = !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling && !IsInstalling;
// 				current = packageInfo;
// 				opengit.visible = current?.source == PackageSource.Git;
//
// 				bool isTargetPackage = current != null && current.name == CWJ_Installer_Name;
// 				descLbl.visible = isTargetPackage;
// 				installBtn.visible = isTargetPackage;
// 				changeApiCompatibilityBtn.visible = isTargetPackage;
//
// 				if (!isTargetPackage)
// 				{
// 					return;
// 				}
//
// 				bool needChangeApi = PlayerSettings.GetApiCompatibilityLevel(EditorUserBuildSettings.selectedBuildTargetGroup) !=
// 				                     ApiCompatibilityLevel.NET_Unity_4_8;
//
// 				changeApiCompatibilityBtn.SetEnabled(canEnabled && needChangeApi);
// 				installBtn.SetEnabled(canEnabled && !needChangeApi);
//
// 				if (needChangeApi)
// 					descLbl.text = $"[{changeApiCompatibilityBtn.text}] 버튼을 눌러주세요.";
// 				else
// 					CheckForUpdates();
// 			}
//
// 			void OnClickInstallBtn()
// 			{
// 				if (IsInstalling) return;
//
// 				EditorApplication.update -= InstallPackagesLooper;
// 				curInstallIndex = 0;
// 				curAddRequest = null;
// 				EditorApplication.update += InstallPackagesLooper;
//
// 				InstallPackagesLooper();
// 			}
//
// 			private void CheckForUpdates()
// 			{
// 				bool needUpdate = current.CheckNeedUpdateByLastUpdateDate(out string latestVersion);
// 				installBtn.SetEnabled(string.IsNullOrEmpty(latestVersion) || !needUpdate);
// 				descLbl.text = $"{descStr}\n" + (needUpdate ? "Update가 필요합니다." : "현재 최신 버전입니다.");
// 			}
//
// 			public void OnPackageAddedOrUpdated(PackageInfo packageInfo) { }
//
// 			public void OnPackageRemoved(PackageInfo packageInfo) { }
// 		}
//
//
// 		private static AddRequest curAddRequest;
// 		private static int curInstallIndex = -1;
//
// 		private static bool IsInstalling => curInstallIndex >= 0 || curAddRequest != null;
//
// 		private static void InstallPackagesLooper()
// 		{
// 			if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling)
// 			{
// 				Debug.LogError("Cannot install packages while in play mode or compiling.");
// 				return;
// 			}
//
// 			if (curAddRequest == null && curInstallIndex < InstallGitUrls.Length)
// 			{
// 				var url = InstallGitUrls[curInstallIndex];
// 				if (!IsPackageInstalled(url))
// 				{
// 					curAddRequest = Client.Add(url);
// 				}
// 				else
// 				{
// 					Debug.Log($"Package already installed: {url}");
// 					curAddRequest = null;
// 					++curInstallIndex;
// 				}
// 			}
//
// 			if (curAddRequest != null && curAddRequest.IsCompleted)
// 			{
// 				if (curAddRequest.Status == StatusCode.Success)
// 				{
// 					Debug.Log($"Successfully installed: {InstallGitUrls[curInstallIndex]}");
// 				}
// 				else if (curAddRequest.Status >= StatusCode.Failure)
// 				{
// 					Debug.LogError($"Failed to install: {InstallGitUrls[curInstallIndex]} - {curAddRequest.Error.message}");
// 				}
//
// 				// 다음 패키지로 이동
// 				curAddRequest = null;
// 				++curInstallIndex;
// 			}
//
// 			if (curInstallIndex >= InstallGitUrls.Length)
// 			{
// 				Debug.Log("All packages installed.");
// 				EditorApplication.update -= InstallPackagesLooper;
// 				curAddRequest = null;
// 				curInstallIndex = -1;
// 			}
// 		}
//
// 		private static bool CheckNeedUpdateByLastUpdateDate(this PackageInfo packageInfo, out string latestVersion)
// 		{
// 			latestVersion = GetLatestGitTag();
//
// 			if (string.IsNullOrEmpty(latestVersion))
// 			{
// 				return true;
// 			}
//
// 			latestVersion = latestVersion.VersionNormalized()[1..];
// 			string currentVersion = packageInfo.version.VersionNormalized();
// 			return !string.IsNullOrEmpty(latestVersion) && currentVersion != latestVersion;
// 		}
//
// 		private static string VersionNormalized(this string input)
// 		{
// 			return input
// 			       .Trim()
// 			       .Replace("\n", "") // 줄바꿈 제거
// 			       .Replace("\r", "") // 캐리지 리턴 제거
// 			       .Replace("\t", ""); // 탭 제거
// 		}
//
// 		private static string GetLatestGitTag()
// 		{
// 			try
// 			{
// 				// Git 명령 실행
// 				ProcessStartInfo startInfo = new ProcessStartInfo
// 				                             {
// 					                             FileName = "git",
// 					                             Arguments = $"ls-remote --tags {GitRepoUrl}",
// 					                             RedirectStandardOutput = true,
// 					                             UseShellExecute = false,
// 					                             CreateNoWindow = true
// 				                             };
//
// 				using Process process = Process.Start(startInfo);
// 				string output = process.StandardOutput.ReadToEnd();
// 				process.WaitForExit();
//
// 				// Git 태그 목록에서 최신 태그를 추출
// 				string[] lines = output.Split('\n');
// 				string latestTag = lines
// 				                   .Where(line => line.Contains("refs/tags/"))
// 				                   .Select(line => line.Split('/').Last())
// 				                   .OrderByDescending(tag => tag)
// 				                   .FirstOrDefault();
//
// 				return latestTag;
// 			}
// 			catch (System.Exception ex)
// 			{
// 				Debug.LogError("Git 태그를 가져오는 중 오류 발생: " + ex.Message);
// 				return null;
// 			}
// 		}
//
// 		private static void ChangeApiCompatibility()
// 		{
// 			BuildTargetGroup targetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
// 			PlayerSettings.SetApiCompatibilityLevel(targetGroup, ApiCompatibilityLevel.NET_Unity_4_8);
//
// 			Debug.Log($"API Compatibility Level changed to: {ApiCompatibilityLevel.NET_Unity_4_8.ToString()} (.NET Framework)");
// 		}
//
// 		private static bool IsPackageInstalled(string packageUrl)
// 		{
// 			var listRequest = Client.List();
// 			while (!listRequest.IsCompleted) { }
//
// 			if (listRequest.Status == StatusCode.Success)
// 			{
// 				foreach (var package in listRequest.Result)
// 				{
// 					if (package.packageId.Contains(packageUrl))
// 						return true;
// 				}
// 			}
//
// 			return false;
// 		}
// 	}
// }
//
// #endif
//
