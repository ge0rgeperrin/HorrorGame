using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Mfuscator {

	[Serializable]
	internal sealed class SettingsObject {
		// defaults
		public bool enable = true;
		public int callbackOrder = 5002;
		public bool logInfo = true;
		public Shared.Settings inter = new() {
			removeStringLiterals = true,
			preserveUnityCrashHandler = false,
			renameExports = true,
			removeMonoExports = true,
			modifyInternalStructures = false,
			detectProxyLibraries = false
		};
	}
	internal static class Settings {

		private static SettingsObject _object;
		public static SettingsObject Object {
			get {
				if (_object == null)
					Load();
				return _object;
			}
		}

		private const string _FILENAME = "MFSSettings.json";
		private static string Filepath => Path.Combine(Application.dataPath, "..", _FILENAME);

		public static void Load() {
			if (File.Exists(Filepath))
				try {
					_object = JsonUtility.FromJson<SettingsObject>(File.ReadAllText(Filepath));
					return;
				} catch (Exception e) {
					Utils.LogError($"Failed to load \"{Filepath}\"\n{e}");
				}
			_object = new();
			Save();
		}
		public static void Save() {
			// tabs (divine will)
			File.WriteAllText(Filepath, JsonUtility.ToJson(Object, true).Replace("    ", "\t") + '\n');
		}
	}
	internal sealed class SettingsWindow : EditorWindow {

		// TODO: why "default"?
		[SerializeField]
		private Font _iconFont = default;

		// used to clear the cache for the builds after disabling MFS
		public const string CLEAR_CACHE_PP_SUB_KEY = "CLEAR_CACHE";

		[MenuItem("Window/MFS Settings", priority = 502)]
		private static void Open() {
			// TODO: redundant?
			Settings.Load();
			// https://youtu.be/GsOD6X-gUlw?t=91
			// TODO: remove
			_ = GetWindow<SettingsWindow>(false, "Mfuscator" + ((DateTime.UtcNow < new DateTime(2025, 1, 1)) ? " (Remastered)" : string.Empty));
		}
		private void OnFocus() {
			// TODO: redundant?
			Settings.Load();
		}
		private void CreateGUI() {
			var root = rootVisualElement;

			minSize = new(416f, 384f);
			// TODO: safe?
			maxSize = minSize;

			static void SetMargin(VisualElement element) {
				element.style.marginTop = element.style.marginRight = element.style.marginLeft = 4f;
				element.style.marginBottom = 0f;
			}
			static void Add<ValueT, FieldT>(VisualElement root, ValueT v, string label, Action<ValueT> callback, string tooltip = null) where FieldT
				: BaseField<ValueT>, new() {
				var element = new FieldT {
					label = label,
					tooltip = tooltip
				};
				SetMargin(element);
				element.Q<Label>().style.minWidth = 192f;
				element.SetValueWithoutNotify(v);
				_ = element.RegisterValueChangedCallback<ValueT>(e => {
					callback.Invoke(e.newValue);
					Settings.Save();
				});
				root.Add(element);
			}
			// TODO: "IntegerField" doesn't exist in earlier versions, so we do this
			static void AddInteger(VisualElement root, int v, string label, Action<int> callback, string tooltip = null) {
				Add<string, TextField>(root, v.ToString(), label, v => {
					if (!int.TryParse(v, out int vInt)) {
						_ = EditorUtility.DisplayDialog("Error", "Expected an integer", "Proceed");
						return;
					}
					callback.Invoke(vInt);
				}, tooltip);
			}
			void AddButton(VisualElement root, string text, string icon, Action callback, string tooltip = null) {
				var button = new Button() {
					text = text,
					tooltip = tooltip
				};
				SetMargin(button);
				button.style.paddingTop = button.style.paddingBottom = button.style.paddingRight = button.style.paddingLeft = 4f;
				button.style.unityTextAlign = TextAnchor.MiddleLeft;

				// content
				{
					var label = new Label {
						text = icon
					};
					label.style.unityFont = _iconFont;
					label.style.unityFontDefinition = new();
					label.style.unityTextAlign = TextAnchor.MiddleRight;

					button.Add(label);
				}

				root.Add(button);

				button.clicked += callback;
			}
			static void AddSpacer(VisualElement root, float height) {
				var spacer = new VisualElement();
				if (height <= 0f)
					spacer.style.flexGrow = 1f;
				else
					spacer.style.height = height;

				root.Add(spacer);
			}

			// -> main
			Add<bool, Toggle>(root, Settings.Object.enable, "<b>Enable</b>", v => {
				Settings.Object.enable = v;
				if (!Settings.Object.enable)
					PlayerPrefs.SetString(Utils.GetPlayerPrefsKey(CLEAR_CACHE_PP_SUB_KEY), "https://youtu.be/5lrqtbrI2xI");
			});
			AddInteger(root, Settings.Object.callbackOrder, "Callback Order", v => {
				Settings.Object.callbackOrder = v;
			}, "Build pipeline callback order for compatibility with third-party code");
			Add<bool, Toggle>(root, Settings.Object.logInfo, "Log Info", v => {
				Settings.Object.logInfo = v;
			}, "Do not print information logs into the console");
			Add<bool, Toggle>(root, Settings.Object.inter.removeStringLiterals, "Remove String Literals", v => {
				Settings.Object.inter.removeStringLiterals = v;
			}, "Remove anchor string literals to complicate orientation in disassembled code");
			Add<bool, Toggle>(root, Settings.Object.inter.preserveUnityCrashHandler, "Preserve Unity Crash Handler", v => {
				Settings.Object.inter.preserveUnityCrashHandler = v;
			}, "Do not exclude the standard crash handler");
			Add<bool, Toggle>(root, Settings.Object.inter.renameExports, "Rename Exports", v => {
				Settings.Object.inter.renameExports = v;
			}, "Randomly rename IL2CPP export functions to prevent automated runtime dumping");
			Add<bool, Toggle>(root, Settings.Object.inter.removeMonoExports, "Remove Mono Exports", v => {
				Settings.Object.inter.removeMonoExports = v;
			}, "Remove redundant Mono-related IL2CPP export functions");
			Add<bool, Toggle>(root, Settings.Object.inter.modifyInternalStructures, "Modify Internal Structures", v => {
				Settings.Object.inter.modifyInternalStructures = v;
			}, "Randomly modify IL2CPP internals to complicate runtime dumping");
			Add<bool, Toggle>(root, Settings.Object.inter.detectProxyLibraries, "Detect Proxy Libraries", v => {
				Settings.Object.inter.detectProxyLibraries = v;
			}, "Look for unwanted libraries");
			AddSpacer(root, 8f);
			AddButton(root, "<b>Restore IL2CPP</b>", "\\uf2ea", () => {
				Pipeline.Restore();
			}, "Attempt to restore original Unity files if they were not automatically restored after a build. May aid in case of a build failure when MFS is disabled/unavailable");
			void AddURLButton(string text, string icon, string uRL) {
				AddButton(root, text, icon, () => {
					Application.OpenURL(uRL);
				}, $"Open \"{uRL}\"");
			}
			AddURLButton("Official Website", "\\uf0db", "https://security.mew.icu");
			AddURLButton("Community", "\\uf086", "https://discord.gg/tYFstqd7jE");
			AddURLButton("Email", "\\uf0e0", $"mailto:a@mew.icu");
			AddURLButton("Donate", "<color=red>\\uf004", "https://ko-fi.com/mewiof");

			// -> spacer
			AddSpacer(root, 0f);

			// -> bottom
			var bottom = new VisualElement();
			bottom.style.flexDirection = FlexDirection.Row;

			// bottom content
			{
				void AddLabel(string text, string uRL) {
					var label = new Label() {
						text = text
					};
					SetMargin(label);
					label.style.marginBottom = 4f;

					bottom.Add(label);

					if (uRL != null) {
						label.RegisterCallback<MouseEnterEvent>(e => {
							label.text = $"<u>{label.text}</u>";
						});
						label.RegisterCallback<MouseLeaveEvent>(e => {
							label.text = label.text[3..^4];
						});
						label.RegisterCallback<ClickEvent>(e => {
							Application.OpenURL(uRL);
						});
					}
				}

				AddLabel($"Version: <b>b{Shared.BUILD_NUMBER}</b>", null);
				AddSpacer(bottom, 0f);
				// 3% chance
				if (UnityEngine.Random.value < 0.03f) {
					string[] ego = {
						"Sharks are cool. You're too",
						"Thank you for being here"
					};
					AddLabel(ego[UnityEngine.Random.Range(0, ego.Length)], "https://youtu.be/YMUhbIAA9-8");
				}
			}

			root.Add(bottom);
		}
	}
}
