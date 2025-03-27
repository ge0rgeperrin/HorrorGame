using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEditor;
using UnityEditor.Android;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Mfuscator {

	internal sealed class Pipeline : IPreprocessBuildWithReport, IPostGenerateGradleAndroidProject, IPostprocessBuildWithReport {

		private static bool _doNotContinue;

		private static bool IsGoodReport(BuildReport report) {
			return
#if UNITY_6000_0_OR_NEWER
				report.summary.buildType == BuildType.Player &&
#endif
				report.summary.result != BuildResult.Failed &&
				report.summary.result != BuildResult.Cancelled;
		}
		private static bool IsIL2CPP(BuildReport report) {
			return PlayerSettings.GetScriptingBackend(NamedBuildTarget.FromBuildTargetGroup(report.summary.platformGroup)) == ScriptingImplementation.IL2CPP;
		}
		private static bool IsSupportedTargetPlatform(BuildReport report) {
			return
				report.summary.platform == BuildTarget.StandaloneWindows64 ||
				report.summary.platform == BuildTarget.StandaloneLinux64 ||
				//report.summary.platform == BuildTarget.StandaloneOSX ||
				report.summary.platform == BuildTarget.Android ||
				report.summary.platform == BuildTarget.iOS;
		}
		private static bool IsSupportedCompilerConfiguration(BuildReport report) {
			Il2CppCompilerConfiguration compilerConfiguration = PlayerSettings.GetIl2CppCompilerConfiguration(NamedBuildTarget.FromBuildTargetGroup(report.summary.platformGroup));
			if (compilerConfiguration == Il2CppCompilerConfiguration.Master)
				Utils.LogWarning("The \"Master\" IL2CPP compiler configuration is being used, which may cause incompatibility issues in some scenarios. It is recommended to use \"Release\" configuration");
			return compilerConfiguration == Il2CppCompilerConfiguration.Release || compilerConfiguration == Il2CppCompilerConfiguration.Master;
		}
		private static string EditorPath {
			get {
				if (Application.platform == RuntimePlatform.OSXEditor)
					return EditorApplication.applicationPath.Remove(EditorApplication.applicationPath.LastIndexOf($"/{Path.GetFileName(EditorApplication.applicationPath)}"));
				return EditorApplication.applicationPath.Remove(EditorApplication.applicationPath.LastIndexOf($"/Editor/{Path.GetFileName(EditorApplication.applicationPath)}"));
			}
		}
		private static string GetOutputPath(BuildReport report) {
			// TODO: only executables?
			return Path.GetDirectoryName(report.summary.outputPath);
		}
		private static string GetMetadataFilepath(BuildReport report) {
			if (report.summary.platform == BuildTarget.StandaloneWindows64 ||
				report.summary.platform == BuildTarget.StandaloneLinux64/* ||
				report.summary.platform == BuildTarget.StandaloneOSX*/) {
				DirectoryInfo outputDirectory = new(GetOutputPath(report));
				foreach (var directory in outputDirectory.GetDirectories())
					if (directory.FullName.EndsWith("_Data"))
						return Path.Combine(directory.FullName, "il2cpp_data", "Metadata", "global-metadata.dat");
				throw new Exception();
			}
			if (report.summary.platform == BuildTarget.iOS)
				return Path.Combine(GetOutputPath(report), Application.productName, "Data", "Managed", "Metadata", "global-metadata.dat");
			throw new NotImplementedException();
		}

		// [Unity]
		public int callbackOrder => Settings.Object.callbackOrder;

		[AOT.MonoPInvokeCallback(typeof(Shared.LogCallback))]
		private static void OnLog(IntPtr messageP, byte type) {
			string message = $"<color=#999><b>[Unmanaged]</b></color> {Marshal.PtrToStringUni(messageP)}";
			switch (type) {
				case (byte)Shared.LogType.Info: Utils.LogInfo(message); break;
				case (byte)Shared.LogType.Warning: Utils.LogWarning(message); break;
				case (byte)Shared.LogType.Error: Utils.LogError(message); break;
				default: Utils.LogError("Unknown log type"); break;
			}
		}
		// clear
		public static void Restore() {
			Shared.SetLogCallback(OnLog);
			Settings.Object.inter.editorPath = EditorPath;
			if (!Shared.Clear(Settings.Object.inter))
				Utils.LogWarning("No files were found that could be restored");
		}
		private static void ClearCache(string outputFilepath) {
			string outputPath = Path.GetDirectoryName(outputFilepath);
			if (Directory.Exists(outputPath)) {
				Directory.Delete(outputPath, true);
				_ = Directory.CreateDirectory(outputPath);
			}
			// "Bee"
			string cachePath = Path.Combine(Application.dataPath, "..", "Library", "Bee");
			if (Directory.Exists(cachePath))
				Directory.Delete(cachePath, true);
		}

		// [Unity]
		public void OnPreprocessBuild(BuildReport report) {
			// reset
			_doNotContinue = false;

			// clear cache request?
			string clearCachePPKey = Utils.GetPlayerPrefsKey(SettingsWindow.CLEAR_CACHE_PP_SUB_KEY);
			if (PlayerPrefs.HasKey(clearCachePPKey)) {
				PlayerPrefs.DeleteKey(clearCachePPKey);
				if (!Settings.Object.enable) {
					Utils.LogInfo("Cache cleanup has been requested. This build will take longer than usual to complete");
					// TODO: only executables?
					ClearCache(report.summary.outputPath);
				}
			}

			// ignore?
			static void Ignore(string reason) {
				_doNotContinue = true;
				Utils.LogInfo($"This build will be ignored ({reason})");
			}
			if (
				!Settings.Object.enable ||
				PlayerPrefs.HasKey(Utils.GetPlayerPrefsKey("IGNORE"))
				) {
				Ignore("disabled");
				return;
			}
			if (
#pragma warning disable CS0162
#if UNITY_SERVER
				true
#else
				false
#endif
				) {
				Ignore("server");
				return;
			}
#pragma warning restore CS0162
			if (
				report.summary.options.HasFlag(BuildOptions.Development)
				) {
				Ignore("development");
				return;
			}
			if (
				!IsGoodReport(report)
				) {
				Ignore("bad report");
				return;
			}
			if (
				!IsIL2CPP(report) ||
				!IsSupportedTargetPlatform(report)
				) {
				Ignore("unsupported target platform");
				return;
			}
			if (
				!IsSupportedCompilerConfiguration(report)
				) {
				Ignore("unsupported IL2CPP compiler configuration");
				return;
			}

			// TODO: only executables?
			ClearCache(report.summary.outputPath);

			Settings.Object.inter.editorVersion = Application.unityVersion;
			Settings.Object.inter.editorPath = EditorPath;
			Settings.Object.inter.targetPlatform = report.summary.platform switch {
				BuildTarget.StandaloneWindows64 => Shared.TargetPlatform.Windows,
				BuildTarget.StandaloneLinux64 => Shared.TargetPlatform.Linux,
				//BuildTarget.StandaloneOSX => Shared.TargetPlatform.macOS,
				BuildTarget.Android => Shared.TargetPlatform.Android,
				BuildTarget.iOS => Shared.TargetPlatform.iOS,
				_ => throw new NotImplementedException(),
			};

			if (!Utils.TryObtainAccess(Settings.Object.inter.editorPath)) {
				_doNotContinue = true;
				return;
			}

			Shared.SetLogCallback(OnLog);
			Shared.Clear(Settings.Object.inter);
			Shared.Pre(Settings.Object.inter);
		}
		// [Unity]
		public void OnPostGenerateGradleAndroidProject(string path) {
			// ignore?
			if (_doNotContinue)
				return;
			_doNotContinue = true;

			// "Settings.Object.inter.outputPath" is not yet used for Android
			Settings.Object.inter.metaFilepath = Path.Combine(path, "src", "main", "assets", "bin", "Data", "Managed", "Metadata", "global-metadata.dat");

			Shared.SetLogCallback(OnLog);
			Shared.Post(Settings.Object.inter);
		}
		// [Unity]
		public void OnPostprocessBuild(BuildReport report) {
			// ignore?
			if (_doNotContinue || !IsGoodReport(report))
				return;

			// called when building for iOS

			Settings.Object.inter.outputPath = GetOutputPath(report);
			Settings.Object.inter.metaFilepath = GetMetadataFilepath(report);

			Shared.SetLogCallback(OnLog);
			Shared.Post(Settings.Object.inter);
		}
	}
}
