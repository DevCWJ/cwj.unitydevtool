#if UNITY_EDITOR
using System.Diagnostics;
using System.IO;
using System.Reflection;
using CWJ.AccessibleEditor;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace CWJ
{
	class PackageManagerExtension
	{
		[InitializeOnLoadMethod]
		static void Init()
		{
			PackageManagerExtensions.RegisterExtension(new Ex());
		}

		class Ex : IPackageManagerExtension
		{
			public VisualElement CreateExtensionUI()
			{
				VisualElement ExtentionRoot = new VisualElement();
				VisualElement label = new VisualElement();
				ExtentionRoot.Add(label);
				detail = new Label();
				detail.text = "CWJ";
				label.Add(detail);


				VisualElement buttons = new VisualElement();
				ExtentionRoot.Add(buttons);

				buttons.style.flexDirection = FlexDirection.Row;
				buttons.style.flexWrap = Wrap.Wrap;

				const int width = 160;

				openFolder = new Button();
				openFolder.text = "Open Cache Folder";
				openFolder.clicked += Button_onClick;
				openFolder.style.width = width;
				buttons.Add(openFolder);

				opengit = new Button();
				opengit.text = "Open Git Link";
				opengit.clicked += Opengit_clicked;
				opengit.style.width = width;
				buttons.Add(opengit);

				VisualElement buttonsLine2 = new VisualElement();
				ExtentionRoot.Add(buttonsLine2);

				buttonsLine2.style.flexDirection = FlexDirection.Row;
				buttonsLine2.style.flexWrap = Wrap.Wrap;

				move2Local = new Button();
				move2Local.text = "Embed";
				move2Local.clicked += Move2PackagesFolder_clicked;
				move2Local.style.width = width;
				buttonsLine2.Add(move2Local);

				move2Cache = new Button();
				move2Cache.text = "UnEmbed";
				move2Cache.clicked += Move2LibraryFolder_clicked;
				move2Cache.style.width = width;
				buttonsLine2.Add(move2Cache);

				return ExtentionRoot;
			}

			private void Move2LibraryFolder_clicked()
			{
				if (current == null)
				{
				}
				else
				{
					DirectoryInfo info = new DirectoryInfo(current.resolvedPath);
					var foldername = info.Name;
					var desPath = Path.Combine(PathUtil.LibraryPackageCachePath, foldername);
					Debug.Log(desPath);

					if (Directory.Exists(desPath))
					{
						Directory.Delete(desPath);
					}

					Directory.Move(current.resolvedPath, desPath);
					Process.Start(desPath);
					AccessibleEditorUtil.SyncSolution();
				}
			}

			private void Move2PackagesFolder_clicked()
			{
				if (current == null)
				{
				}
				else
				{
					DirectoryInfo info = new DirectoryInfo(current.resolvedPath);
					var foldername = info.Name;
					var desPath = Path.Combine(PathUtil.PackagesPath, foldername);
					Debug.Log(desPath);

					if (Directory.Exists(desPath))
					{
						Directory.Delete(desPath);
					}

					try
					{
						Directory.Move(current.resolvedPath, desPath);
					}
					catch (IOException e)
					{
						Debug.Log($"Close IDE!!!        ".HtmlColor(HexColor.BarnRed) + e.ToString());
					}

					Process.Start(desPath);
					AccessibleEditorUtil.SyncSolution();
				}
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

			private void Button_onClick()
			{
				Debug.Log(current);
				if (current == null)
				{
					PathUtil.OpenAssetStoreCacheFolder();
				}
				else
				{
					Process.Start(current.resolvedPath);
				}
			}

			public PackageInfo current = null;
			private Button openFolder;
			private Button opengit;
			private Button move2Local;
			private Button move2Cache;
			private Label detail;

			public void OnPackageSelectionChange(PackageInfo packageInfo)
			{
				//packageInfo It is always null and should be a bug.
				current = packageInfo;
				//button.SetEnabled(false);
				bool isGit = current?.source == PackageSource.Git;
				bool? isInLocal = current?.resolvedPath.StartsWith(PathUtil.PackagesPath);
				bool canMove2Local = !isInLocal ?? false;
				bool? isInLibrary = current?.resolvedPath.StartsWith(PathUtil.LibraryPackageCachePath);
				bool canMove2Cache = !isInLibrary ?? false;

				detail.text = $"[Git : {isGit}]    [InLocalPackage : {isInLocal ?? false}]    [InLiraryCache : {isInLibrary ?? false}]";

				if (current != null)
				{
					// Debug.Log(current.displayName + "    " + StringUtil.ToStringReflection(current));
				}

				opengit.SetEnabled(isGit);
				move2Local.SetEnabled(canMove2Local);
				move2Cache.SetEnabled(canMove2Cache);
			}

			public void OnPackageAddedOrUpdated(PackageInfo packageInfo)
			{
				Debug.Log("Package AddedOrUpdated: " + packageInfo.displayName);
			}

			public void OnPackageRemoved(PackageInfo packageInfo)
			{
				Debug.Log("Package Removed: " + packageInfo.displayName);
			}
		}
	}
}

#endif
