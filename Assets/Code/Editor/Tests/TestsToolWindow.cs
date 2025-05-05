using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;
using UnityEngine.TestTools;

namespace Code.Editor.Tests
{
    public class TestsToolWindow : OdinEditorWindow
    {
        private const string IgnoreText = "[Ignore(\"Disabled via TestsToolWindow\")]";

        [MenuItem("Tools/Tests Tool Window", false, 2000)]
        private static void OpenWindow()
        {
            GetWindow<TestsToolWindow>().Show();
        }

        [FoldoutGroup("EditMode Tests", expanded: false)] [Searchable] [ListDrawerSettings(Expanded = false)]
        public List<TestCaseConfig> EditModeTests = new();

        [FoldoutGroup("PlayMode Tests", expanded: false)] [Searchable] [ListDrawerSettings(Expanded = false)]
        public List<TestCaseConfig> PlayModeTests = new();

        public void OnEnable() => RefreshTests();

        [Button("🔍 Find All Tests", ButtonSizes.Large), GUIColor(0.3f, 0.6f, 1f)]
        private void RefreshTests()
        {
            EditModeTests.Clear();
            PlayModeTests.Clear();

            var api = new TestRunnerApi();
            api.RetrieveTestList(TestMode.EditMode,
                rootTest => { CollectTestsRecursive(rootTest, TestPlatform.EditMode); });
            api.RetrieveTestList(TestMode.PlayMode,
                rootTest => { CollectTestsRecursive(rootTest, TestPlatform.PlayMode); });
        }

        [Button("➕ Add Ignore Attribute Tests", ButtonSizes.Large), GUIColor(1f, 0.6f, 0.2f)]
        private void AddIgnoreAttributeTests()
        {
            var testsToIgnore = EditModeTests.Concat(PlayModeTests)
                .Where(t => !t.Enabled && t.IsChanged);
            AddIgnoreAttributes(testsToIgnore);
        }

        [Button("➖ Remove Ignore Attribute Tests", ButtonSizes.Large), GUIColor(1f, 0.3f, 0.3f)]
        private void RemoveIgnoreAttributeTests()
        {
            var testsToRestore = EditModeTests.Concat(PlayModeTests)
                .Where(t => t.Enabled && t.IsChanged);
            RemoveIgnoreAttributes(testsToRestore);
        }

        private void AddIgnoreAttributes(IEnumerable<TestCaseConfig> tests)
        {
            foreach (var test in tests)
            {
                var methodName = test.FullName.Split('.').Last();
                var files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var content = File.ReadAllText(file);

                    var methodPattern = $@"\[Test\]\s*(public\s+void\s+{methodName}\s*\(\))";
                    var match = Regex.Match(content, methodPattern);
                    if (!match.Success || content.Contains(IgnoreText)) continue;

                    Debug.Log($"[TestsTool] Добавляю [Ignore] в метод: {test.FullName}");

                    var updated = Regex.Replace(content, methodPattern,
                        "[Test]\n[Ignore(\"Disabled via TestsToolWindow\")]\n$1");
                    File.WriteAllText(file, updated);
                    break;
                }
            }

            AssetDatabase.Refresh();
        }

        private void RemoveIgnoreAttributes(IEnumerable<TestCaseConfig> tests)
        {
            foreach (var test in tests)
            {
                var methodName = test.FullName.Split('.').Last();
                var files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    var content = File.ReadAllText(file);

                    if (!content.Contains(methodName) || !content.Contains(IgnoreText))
                        continue;

                    Debug.Log($"[TestsTool] Удаляю [Ignore] из метода: {test.FullName}");

                    var updated = content.Replace(IgnoreText + "\n", "")
                        .Replace(IgnoreText + "\r\n", "");
                    File.WriteAllText(file, updated);
                    break;
                }
            }

            AssetDatabase.Refresh();
        }

        private void CollectTestsRecursive(ITestAdaptor adaptor, TestPlatform platform)
        {
            if (!adaptor.HasChildren && !string.IsNullOrEmpty(adaptor.FullName))
            {
                var config = new TestCaseConfig(adaptor.FullName, platform.ToString(), adaptor.RunState != RunState.Ignored);
                if (platform == TestPlatform.EditMode)
                    EditModeTests.Add(config);
                else
                    PlayModeTests.Add(config);
            }
            else
            {
                foreach (var child in adaptor.Children)
                    CollectTestsRecursive(child, platform);
            }
        }
    }

    [Serializable]
    public class TestCaseConfig
    {
        [ReadOnly] public string FullName;
        [ReadOnly] public string Platform;
        [ShowInInspector] [field: SerializeField] public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled != value)
                {
                    _enabled = value;
                    IsChanged = true;
                }
            }
        }
        [ShowInInspector, ReadOnly, ShowIf(nameof(IsChanged))] public bool IsChanged = false;
        
        private bool _enabled;
        
        public TestCaseConfig(string fullName, string platform, bool enabled)
        {
            FullName = fullName;
            Platform = platform;
            _enabled = enabled;
        }
    }
}
