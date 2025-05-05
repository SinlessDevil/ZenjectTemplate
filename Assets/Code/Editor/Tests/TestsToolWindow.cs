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
            var window = GetWindow<TestsToolWindow>("TestsToolWindow");
            window.position = new Rect(100, 100, 1600, 700);
            window.Show();
        }

        [FoldoutGroup("Grouped Edit Mode", expanded: false)] [TableList]
        public List<GroupedTestGroup> GroupedEditMode = new();

        [FoldoutGroup("Grouped Play Mode", expanded: false)] [TableList]
        public List<GroupedTestGroup> GroupedPlayMode = new();

        public void OnEnable() => RefreshTests();

        [Button("🔍 Find All Tests", ButtonSizes.Large), GUIColor(0.3f, 0.6f, 1f)]
        private void RefreshTests()
        {
            GroupedEditMode.Clear();
            GroupedPlayMode.Clear();

            var api = new TestRunnerApi();
            api.RetrieveTestList(TestMode.EditMode, root => CollectTestsRecursive(root, TestPlatform.EditMode));
            api.RetrieveTestList(TestMode.PlayMode, root => CollectTestsRecursive(root, TestPlatform.PlayMode));
        }

        [Button("➕ Add [Ignore] Attribute Tests", ButtonSizes.Large), GUIColor(1f, 0.6f, 0.2f)]
        private void AddIgnoreAttributeTests()
        {
            var testsToIgnore = GroupedEditMode.Concat(GroupedPlayMode)
                .SelectMany(g => g.Tests)
                .Where(t => !t.Enabled && t.IsChanged);
            AddIgnoreAttributes(testsToIgnore);
        }

        [Button("➖ Remove [Ignore] Attribute Tests", ButtonSizes.Large), GUIColor(1f, 0.3f, 0.3f)]
        private void RemoveIgnoreAttributeTests()
        {
            var testsToRestore = GroupedEditMode.Concat(GroupedPlayMode)
                .SelectMany(g => g.Tests)
                .Where(t => t.Enabled && t.IsChanged);
            RemoveIgnoreAttributes(testsToRestore);
        }

        [Button("📋 Open Test Runner", ButtonSizes.Large), GUIColor(0.8f, 0.8f, 1f)]
        private void OpenTestRunner()
        {
            var testRunnerType = Type.GetType("UnityEditor.TestTools.TestRunner.TestRunnerWindow,UnityEditor.TestRunner");
            if (testRunnerType != null)
            {
                GetWindow(testRunnerType, false, "Test Runner");
            }
            else
            {
                Debug.LogWarning("Test Runner Window not found. Make sure 'Test Framework' package is installed.");
            }
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
                    var methodPattern = $@"^(\s*)\[Test\]\s*(public\s+void\s+{methodName}\s*\(\))";
                    var match = Regex.Match(content, methodPattern, RegexOptions.Multiline);
                    if (!match.Success || content.Contains(IgnoreText)) 
                        continue;

                    var indent = match.Groups[1].Value;

                    Debug.Log($"[TestsTool] Added [Ignore] in Method: {test.FullName}");

                    var updated = Regex.Replace(content, methodPattern,
                        $"{indent}[Test]\n{indent}[Ignore(\"Disabled via TestsToolWindow\")]\n{indent}$2",
                        RegexOptions.Multiline);
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

                    var ignorePattern = $@"^\s*\[Ignore\(""Disabled via TestsToolWindow""\)\]\s*\n";
                    var updated = Regex.Replace(content, ignorePattern, "", RegexOptions.Multiline);
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
                var config = new TestCaseConfig(adaptor.FullName, adaptor.RunState != RunState.Ignored);
                var assemblyName = adaptor.TypeInfo?.Assembly?.GetName()?.Name ?? "Unknown";

                var groupList = platform == TestPlatform.EditMode ? GroupedEditMode : GroupedPlayMode;
                var group = groupList.FirstOrDefault(g => g.AssemblyName == assemblyName);
                if (group == null)
                {
                    group = new GroupedTestGroup(assemblyName);
                    groupList.Add(group);
                }

                group.Tests.Add(config);
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
        [TableColumnWidth(250, Resizable = true)] [ReadOnly]
        public string FullName;
        
        [GUIColor(0.2f, 0.8f, 0.2f)]
        [BoxGroup("Enabled", showLabel: false)]
        [TableColumnWidth(150, Resizable = false)] [ShowInInspector] [field: SerializeField]
        public bool Enabled
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

        [GUIColor(1f, 0f, 0)]
        [BoxGroup("IsChanged", showLabel: false)]
        [TableColumnWidth(150, Resizable = false)] [ShowInInspector, ReadOnly, ShowIf(nameof(IsChanged))]
        public bool IsChanged = false;

        private bool _enabled;

        public TestCaseConfig(string fullName, bool enabled)
        {
            FullName = fullName;
            _enabled = enabled;
        }
    }

    [Serializable]
    public class GroupedTestGroup
    {
        [TableColumnWidth(175, Resizable = false)] [ReadOnly] 
        public string AssemblyName;

        [TableList] public List<TestCaseConfig> Tests = new();

        public GroupedTestGroup(string assemblyName)
        {
            AssemblyName = assemblyName;
        }
    }
}