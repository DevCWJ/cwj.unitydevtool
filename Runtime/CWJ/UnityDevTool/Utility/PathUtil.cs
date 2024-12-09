using CWJ.AccessibleEditor;

using System;
using System.IO;
using System.Collections.Generic;

using UnityEngine;
using static System.Environment;
namespace CWJ
{
    public static class PathUtil
    {
        private const string PathSeparators = "/\\";

        /// <summary>
        /// Path.GetDirectory 여러번부르는거 한번에 부르셈.<br/>+C#8.0 ReadOnlySpan 활용해놔서 긴 경로도 최적화 가능
        /// </summary>
        /// <param name="path">원본 경로 문자열</param>
        /// <param name="levels">상위 디렉토리로 갈 수. (1이면 Path.GetDirectoryName과 같음)</param>
        /// <param name="asMuchAsPossible">levels 까지 안되도 가능한 만큼만 반환할 것인지. (<see langword="false"/>: 원본 반환)</param>
        /// <returns>줄인 경로 문자열</returns>
        public static string GetParentDirectory(string path, int levels, bool asMuchAsPossible = true)
        {
            if (path == null) return null;
            if (levels <= 0) return path;

            ReadOnlySpan<char> span = path.AsSpan();
            int currentIndex = span.Length;
            ReadOnlySpan<char> pathSeparators = PathSeparators.AsSpan();
            for (int i = 0; i < levels; i++)
            {
                int lastSlash = span.Slice(0, currentIndex).LastIndexOfAny(pathSeparators);

                if (lastSlash == -1)
                {
                    if (asMuchAsPossible)
                    {
                        break;
                    }
                    else
                    {
                        return path;
                    }
                }

                currentIndex = lastSlash;
            }

            return path.Substring(0, currentIndex);
        }
        public static string EditorExePath
        {
            get
            {
#if UNITY_EDITOR
                return UnityEditor.EditorApplication.applicationPath.ToValidDirectoryPathByApp(false);
#else
                throw new System.Security.SecurityException("EditorApplication.applicationPath is UnityEditor's Property");
#endif
            }
        }

        public static string GetProjectFolderPath(string folder)
        {
            var path = Path.Combine(Application.dataPath, $"..\\{folder}");
            return Path.GetFullPath(path);
        }

        public static string GetRelativePathWithProject(this string path)
        {
            var p1 = new System.Uri(path);
            var p2 = new System.Uri(ProjectPath);
            var r = p2.MakeRelativeUri(p1);
            return r.ToString();
        }

        public static string ProjectPath { get; } = GetProjectFolderPath("");
        public static string PackagesPath { get; } = GetProjectFolderPath("Packages");
        public static string LibraryPath { get; } = GetProjectFolderPath("Library");
        public static string LibraryPackageCachePath { get; } = GetProjectFolderPath("Library/PackageCache");
        public static string LogsPath { get; } = GetProjectFolderPath("Logs");
        public static string TempPath { get; } = GetProjectFolderPath("Temp");
        public static string UserSettingsPath { get; } = GetProjectFolderPath("UserSettings");
        public static string BuildPath { get; } = GetProjectFolderPath("Build");

        public static string BuildPath_StandaloneOSX { get; } = GetProjectFolderPath("Build/StandaloneOSX");
        public static string BuildPath_iOS { get; } = GetProjectFolderPath("Build/iOS");

        public static string BuildPath_Android_Mono { get; } = GetProjectFolderPath("Build/Android_Mono");
        public static string BuildPath_Android_IL2CPP { get; } = GetProjectFolderPath("Build/Android_IL2CPP");

        public static string BuildPath_StandaloneWindows64_Mono { get; } = GetProjectFolderPath("Build/StandaloneWindows64_Mono");
        public static string BuildPath_StandaloneWindows64_IL2CPP { get; } = GetProjectFolderPath("Build/StandaloneWindows64_IL2CPP");

        public static string BuildPath_StandaloneLinux64 { get; } = GetProjectFolderPath("Build/StandaloneLinux64");

        public static string BuildPath_PS4 { get; } = GetProjectFolderPath("Build/PS4");
        public static string BuildPath_PS5 { get; } = GetProjectFolderPath("Build/PS5");

        public static string BuildPath_WebGL { get; } = GetProjectFolderPath("Build/WebGL");

        public static string BuildPath_DedicatedServer { get; } = GetProjectFolderPath("Build/DedicatedServer");

        public static string BuildPath_activeBuildTarget { get; } = GetProjectFolderPath("Build/activeBuildTarget");

        public static string ProjectSettingsPath { get; } = GetProjectFolderPath("ProjectSettings");
        public static string ConsoleLogPath { get; } = Path.GetDirectoryName(Application.consoleLogPath);

#if UNITY_EDITOR
        [UnityEditor.MenuItem("CWJ/Path/Log Selection Folders")]
        public static void LogSelectionFolders()
        {
            var fs = GetSelectionFolders();
            var dirs = "";
            foreach (var item in fs)
            {
                dirs += $"{item}  |  ";
            }

            Debug.Log($"{dirs}");
        }

        [UnityEditor.MenuItem("CWJ/Path/Log Environment SpecialFolder")]
#endif
        static void LogEnvironmentSpecialFolder()
        {
            foreach (SpecialFolder item in System.Enum.GetValues(typeof(SpecialFolder)))
            {
                Debug.Log($"{item.ToString().Html(HexColor.Azure)}:    {GetFolderPath(item)}");
            }
        }
#if UNITY_EDITOR
        [UnityEditor.MenuItem("CWJ/Path/Create Build Folder")]
#endif
        static void CreateBuildFolder()
        {
            List<string> list = new List<string>()
                                {
                                    PathUtil.BuildPath_StandaloneOSX,
                                    PathUtil.BuildPath_iOS,
                                    PathUtil.BuildPath_Android_Mono,
                                    PathUtil.BuildPath_Android_IL2CPP,
                                    PathUtil.BuildPath_StandaloneWindows64_Mono,
                                    PathUtil.BuildPath_StandaloneWindows64_IL2CPP,
                                    PathUtil.BuildPath_StandaloneLinux64,
                                    PathUtil.BuildPath_PS4,
                                    PathUtil.BuildPath_PS5,
                                    PathUtil.BuildPath_WebGL,
                                    PathUtil.BuildPath_DedicatedServer,
                                    PathUtil.BuildPath_activeBuildTarget,
                                };

            foreach (var dir in list)
            {
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
            }

            Debug.Log($"Create packaging directory");
            System.Diagnostics.Process.Start(PathUtil.BuildPath);
        }
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Path/Open ConsoleLog Folder")]
#endif
        static void OpenLogFolder()
        {
            var dir = PathUtil.ConsoleLogPath;
            Debug.Log($"open {dir}");
            System.Diagnostics.Process.Start(dir);
        }
#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Path/Open AssetStoreCache Folder")]
#endif
        public static void OpenAssetStoreCacheFolder()
        {
            var dir = Path.Combine(GetFolderPath(SpecialFolder.ApplicationData),
                                   "Unity/Asset Store-5.x");
            dir = Path.GetFullPath(dir);
            Debug.Log($"open {dir}");
            System.Diagnostics.Process.Start(dir);
        }

        public static List<string> GetSelectionFolders()
        {
            List<string> list = new List<string>();
#if UNITY_EDITOR
            UnityEngine.Object[] items = UnityEditor.Selection.GetFiltered<UnityEngine.Object>(UnityEditor.SelectionMode.Assets);
            foreach (var item in items)
            {
                var path = UnityEditor.AssetDatabase.GetAssetPath(item);
                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (System.IO.Directory.Exists(path))
                {
                    list.Add(path);
                }
            }
#endif
            return list;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="title"></param>
        /// <param name="defaultDirectory"></param>
        /// <param name="filters">"파일설명", "확장자,확장자" 규칙임. <br/>예시: "Image files", "png,jpg,jpeg", "All files", "*"</param>
        /// <returns>path</returns>
        public static void OpenFileDialog(string title, Action<string> setPathCallback, string defaultDirectory = null, params string[] filters)
        {
            string pathValue = string.Empty;
#if UNITY_EDITOR
            if (Application.isEditor)
            {
                pathValue = UnityEditor.EditorUtility.OpenFilePanelWithFilters(title, defaultDirectory ?? UnityEngine.Application.dataPath, filters ?? new string[2] { "모든파일", "*" });
                setPathCallback.Invoke(pathValue ?? string.Empty);
                GUIUtility.ExitGUI();
                return;
            }
#endif
            //var paths = OpenFileDialogWindows(title, defaultDirectory, multiSelect: false, filters);
            //return paths.Length > 0 ? null : paths[0];
        }

        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="title"></param>
        ///// <param name="defaultDirectory"></param>
        ///// <param name="filters">"파일설명", "확장자,확장자" 규칙임. <br/>예시: "Image files", "png,jpg,jpeg", "All files", "*"</param>
        ///// <returns></returns>
        //public static string[] OpenFileDialogWindows(string title, string defaultDirectory = null, bool multiSelect = true, params string[] filters)
        //{
        //    using (var dialog = new Ookii.Dialogs.VistaOpenFileDialog())
        //    {
        //        if (filters != null && filters.Length > 1)
        //        {
        //            for (int i = 1; i < filters.Length; i+=2)
        //                filters[i] = "*." + filters[i].Replace(",", ";*.");
        //        }
        //        else
        //            filters = new string[2] { "All Files", "*.*" };

        //        dialog.Title = title;
        //        dialog.InitialDirectory = defaultDirectory ?? UnityEngine.Application.dataPath;
        //        dialog.Filter = string.Join("|", filters);
        //        dialog.FilterIndex = 1;
        //        dialog.Multiselect = multiSelect;
        //        var result = dialog.ShowDialog();
        //        var filenames = result == System.Windows.Forms.DialogResult.OK ?
        //            dialog.FileNames : new string[0];
        //        return filenames;
        //    }
        //}

        public static string WithDoubleQuotes(this string value)
        {
            return $"\"{value}\"";
        }

        public static string Combine(params string[] paths)
        {
            char separatorChar = Path.DirectorySeparatorChar;

            if (string.IsNullOrEmpty(paths[0].Trim().TrimEnd(separatorChar)))
            {
                Debug.LogError("ERROR! " + 0 + " is null");
                return null;
            }

            string finalPath = paths[0].Trim();

            int length = paths.Length;
            for (int i = 1; i < length; i++)
            {
                string addPath = paths[i].Trim().TrimStart(separatorChar);
                if (string.IsNullOrEmpty(addPath))
                {
                    Debug.LogError("ERROR! " + i + " is null");
                    return null;
                }

                finalPath = finalPath.TrimEnd(separatorChar) + separatorChar + addPath;
            }
            return finalPath;
        }

        public static string Combine(string pathA, string pathB)
        {
            pathA = pathA.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).Trim().TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            pathB = pathB.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar).Trim().TrimStart(Path.DirectorySeparatorChar);
            return pathA + pathB;
        }

        public const char PATH_DELIMITER_WINDOWS = '\\';

        public const char PATH_DELIMITER_UNIX = '/';

        public static readonly char[] PathDelimiters = { '\\', '/' };

        public static char GetValidSeparatorChar() => Application.platform == RuntimePlatform.WindowsEditor ? PATH_DELIMITER_WINDOWS : PATH_DELIMITER_UNIX;

        public static char GetInvalidSeparatorChar() => Application.platform != RuntimePlatform.WindowsEditor ? PATH_DELIMITER_WINDOWS : PATH_DELIMITER_UNIX;


        /// <summary>
        /// <para>EditorApplication.applicationPath.ValidatePath(false) 는 유니티에디터 실행파일 위치</para>
        /// string projectPath = Application.dataPath.Substring(0, Application.dataPath.LastIndexOf('/') + 1).ValidatePath();
        /// return projectPath.Substring(0, projectPath.Length - 1); 는 프로젝트 경로
        /// <para>폴더일 경우 addEndDelimiter : true</para>
        /// </summary>
        /// <param name="path"></param>
        /// <param name="needEndDelimiter"></param>
        /// <returns></returns>
        public static string ToValidDirectoryPathByApp(this string path, bool needEndDelimiter)
        {
            char validDelimiter = GetValidSeparatorChar();
            char invalidDelimiter = GetInvalidSeparatorChar();

            string result = path.Trim().Replace(invalidDelimiter, validDelimiter);

            if (needEndDelimiter && !result[result.Length - 1].Equals(validDelimiter))
            {
                result += validDelimiter;
            }

            return string.Join(string.Empty, result.Split(Path.GetInvalidPathChars()));
        }



        public static void AddStartUpFolder(this string path) //미완성
        {
            string dirPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Startup), Path.GetFileName(path));
            if (File.Exists(dirPath))
            {
                return;
            }
            File.Copy(path, dirPath);
        }

        public static string GetValidPath(string startPath, string endPath, string separator = " ", params string[] middlePaths)
        {
            string validPath = "";

            ArrayUtil.Add(ref middlePaths, null);

            for (int i = 0; i < middlePaths.Length; i++)
            {
                string tailPath = string.IsNullOrEmpty(middlePaths[i]) ? endPath : string.Join(separator, middlePaths[i], endPath);
                validPath = string.Join(separator, startPath, tailPath);
                if (validPath.IsFileExists(false))
                {
                    return validPath.ToValidDirectoryPathByApp(false);
                }
            }
            return "";
        }

        public static bool IsDirectory(string path)
        {
            return File.GetAttributes(path).HasFlag(FileAttributes.Directory);
        }

        public static void SetHideFile(string path, bool enabledHide)
        {
            SetFileHidden(path, File.GetAttributes(path), enabledHide);
        }

        public static void SetFileHidden(string path, FileAttributes fileAtt, bool enabledHide)
        {
            fileAtt = (enabledHide ? fileAtt | FileAttributes.Hidden : fileAtt & ~FileAttributes.Hidden);
            File.SetAttributes(path, fileAtt);
        }

        public static bool IsFolderExists(this string path, bool createFolder, bool isPrintLog = true)
        {
            if (string.IsNullOrEmpty(path)) return false;

            string directoryPath = Path.GetDirectoryName(path);

            if (Directory.Exists(directoryPath))
            {
                return true;
            }
            else
            {
                if (createFolder)
                {
                    try
                    {
                        DirectoryInfo folderInfo = new DirectoryInfo(directoryPath);
                        folderInfo.Create();
                        if (isPrintLog) Debug.LogWarning(directoryPath + "\n해당 경로/폴더가 존재하지않아서 생성함");
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                }
                else
                {
                    if (isPrintLog) Debug.LogError(directoryPath + "\n해당 경로/폴더가 존재하지 않음");
                    return false;
                }
            }
        }

        public static bool IsFileExists(this string path, bool createFolder, bool isPrintLog = true)
        {
            return IsFolderExists(path, createFolder, isPrintLog) && File.Exists(path);
        }

        public static string GetFilePathPreventOverlap(string dirPath, string originFileName, int maxCnt = 100)
        {
            if (originFileName.Length == 0) { return ""; }

            string fileName = originFileName;

            int indexOfDot = fileName.LastIndexOf(".");
            string name = fileName.Substring(0, indexOfDot);
            string ext = fileName.Substring(indexOfDot);

            int fileCnt = 1;
            string filePath = "";

            while (fileCnt < maxCnt)
            {
                filePath = Path.Combine(dirPath, fileName);
                if (!File.Exists(filePath))
                {
                    break;
                }

                fileName = name + " (" + ++fileCnt + ")" + ext;
            }

            if (fileCnt == maxCnt)
            {
                UnityEngine.Debug.LogError("중복된 파일이 너무많아서 작업중단\n" + dirPath + originFileName);
            }

            return filePath;
        }

        public static string GetProgramFilesPath(bool is86 = false)
        {
            return is86 ? Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) : Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        }

        public static void OpenFolder(this string path)
        {

#if UNITY_EDITOR
            if (path.StartsWith("/Android/data/") || path.StartsWith("/mnt/sdcard/Android/data"))
            {
                return;
            }
#endif
            path = Path.GetDirectoryName(path);
            var di = new DirectoryInfo(path);
            if (!di.Exists)
            {
                di.Create();
            }
            System.Diagnostics.Process.Start(path);
        }


        public static void ClearCache()
        {
            _MyRelativePath = null;
            _MyAbsolutePath_CWJ = null;
            _MyAbsolutePath_UnityDevTool = null;
            _MyVersion = null;
        }


        ///// <summary>
        /////
        ///// </summary>
        ///// <param name="title"></param>
        ///// <param name="defaultDirectory"></param>
        ///// <param name="filters">"파일설명", "확장자,확장자" 규칙임. <br/>예시: "Image files", "png,jpg,jpeg", "All files", "*"</param>
        ///// <returns></returns>
        //public static string[] OpenFileDialogWindows(string title, string defaultDirectory = null, bool multiSelect = true, params string[] filters)
        //{
        //    using (var dialog = new Ookii.Dialogs.VistaOpenFileDialog())
        //    {
        //        if (filters != null && filters.Length > 1)
        //        {
        //            for (int i = 1; i < filters.Length; i+=2)
        //                filters[i] = "*." + filters[i].Replace(",", ";*.");
        //        }
        //        else
        //            filters = new string[2] { "All Files", "*.*" };

        //        dialog.Title = title;
        //        dialog.InitialDirectory = defaultDirectory ?? UnityEngine.Application.dataPath;
        //        dialog.Filter = string.Join("|", filters);
        //        dialog.FilterIndex = 1;
        //        dialog.Multiselect = multiSelect;
        //        var result = dialog.ShowDialog();
        //        var filenames = result == System.Windows.Forms.DialogResult.OK ?
        //            dialog.FileNames : new string[0];
        //        return filenames;
        //    }
        //}


        public const string MyName = nameof(CWJ);
        public const string MyProjectName = "UnityDevTool";
        public const string MyCopyright = MyProjectName + " (c) 2019. " + MyName;

        //RelativePath : Assets + / AbsolutePath : Application.dataPath +
        private static string _MyRelativePath = null;
        public static string MyRelativePath
        {
            get
            {
                if (string.IsNullOrEmpty(_MyRelativePath))
                {
                    if (!IsCheckValid())
                    {
                        return string.Empty;
                    }

                    string path = $"/{MyName}/{MyProjectName}/";
                    if (!new DirectoryInfo(Application.dataPath + path).Exists)
                    {
#if UNITY_EDITOR
                        _MyRelativePath = PathUtil.GetParentDirectory(ScriptableObjectStore.CacheFilePath<ScriptableObjectStore>(), 2).Replace('\\', '/') + "/";
#endif
                        return _MyRelativePath;
                    }
                    _MyRelativePath = "Assets" + path;
                }

                return _MyRelativePath;
            }
        }


        private static string GetAbsolutePath(bool isRoot)
        {
            if (!IsCheckValid())
            {
                return string.Empty;
            }

            string path = ($"{Application.dataPath}/{PathUtil.MyRelativePath.RemoveStart("Assets/")}").ToValidDirectoryPathByApp(true);

            return PathUtil.IsFolderExists(path, false, isPrintLog: false) ? path : null;
        }

        private static string _MyAbsolutePath_UnityDevTool = null;
        public static string MyAbsolutePath_UnityDevTool
        {
            get
            {
                if (string.IsNullOrEmpty(_MyAbsolutePath_UnityDevTool))
                {
                    _MyAbsolutePath_UnityDevTool = GetAbsolutePath(false);
                }
                return _MyAbsolutePath_UnityDevTool;
            }
        }

        private static string _MyAbsolutePath_CWJ = null;
        public static string MyAbsolutePath_CWJ
        {
            get
            {
                if (string.IsNullOrEmpty(_MyAbsolutePath_CWJ))
                {
                    _MyAbsolutePath_CWJ = GetAbsolutePath(true);
                }
                return _MyAbsolutePath_CWJ;
            }
        }

        private static string _MyVersion = null;
        public static string MyVersion
        {
            get
            {
                if (string.IsNullOrEmpty(_MyVersion))
                {
                    _MyVersion = GetMyVer();
                }
                return _MyVersion;
            }
        }


        private static string GetMyVer()
        {
            string logPath = MyRelativePath + "CHANGELOG.md";
            if (!new FileInfo(logPath).Exists)
            {
                return "Not allowed from CWJ";
            }

            string[] lines = TextFileUtil.GetTxtLines(logPath);

            int index = lines.FindIndex((s) => s.StartsWith("##") && s.EndsWith("Version"));

            return index >= 0 ? lines[index + 1].Replace("##", "").TrimStart() : "";
        }

        public static string GetMyInfo(this Type type, string infoText = null)
        {
            string cwj = $"{MyProjectName} (c) 2019. {MyName}";
            if (string.IsNullOrEmpty(infoText))
            {
                return cwj + (type == null ? "" : "\n" + StringUtil.GetNicifyVariableName(type.Name)) + "\nVersion: " + MyVersion;
            }
            else
            {
                return cwj + "\n" + infoText;
            }
        }

        public static string MyIconPath => MyRelativePath + "Icon/CWJ.png";

        public const int ICON_SIZE =
#if UNITY_2019_3_OR_NEWER
            40;
#else
            45;

#endif

        public static bool IsCheckValid()
        {
            bool result = MyName
                .Equals
                (
                "C"
                +
                "W"
                +
                "J"
                );

            return result;
        }
    }

}
