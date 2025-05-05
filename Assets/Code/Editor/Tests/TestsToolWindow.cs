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
            var testsByFile = new Dictionary<string, List<string>>();

            foreach (var test in tests)
            {
                var methodName = test.FullName.Split('.').Last();
                var files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    if (File.ReadAllText(file).Contains($"void {methodName}("))
                    {
                        if (!testsByFile.ContainsKey(file))
                            testsByFile[file] = new List<string>();

                        testsByFile[file].Add(methodName);
                        break;
                    }
                }
            }

            foreach (var kvp in testsByFile)
            {
                var file = kvp.Key;
                var methodNames = kvp.Value;
                var lines = File.ReadAllLines(file).ToList();
                bool modified = false;

                for (int i = 0; i < lines.Count - 1; i++)
                {
                    foreach (var methodName in methodNames)
                    {
                        if (lines[i].Trim() == "[Test]" &&
                            lines[i + 1].Contains($"void {methodName}("))
                        {
                            lines[i] = "        [Test, Ignore(\"Disabled via TestsToolWindow\")]";
                            modified = true;
                            Debug.Log($"[TestsTool] Добавлен Ignore в метод: {methodName}");
                        }
                    }
                }

                if (modified)
                {
                    File.WriteAllLines(file, lines);
                }
            }

            AssetDatabase.Refresh();
        }

        private void RemoveIgnoreAttributes(IEnumerable<TestCaseConfig> tests)
        {
            var testsByFile = new Dictionary<string, List<string>>();

            foreach (var test in tests)
            {
                var methodName = test.FullName.Split('.').Last();
                var files = Directory.GetFiles(Application.dataPath, "*.cs", SearchOption.AllDirectories);

                foreach (var file in files)
                {
                    if (File.ReadAllText(file).Contains($"void {methodName}("))
                    {
                        if (!testsByFile.ContainsKey(file))
                            testsByFile[file] = new List<string>();

                        testsByFile[file].Add(methodName);
                        break;
                    }
                }
            }

            foreach (var kvp in testsByFile)
            {
                var file = kvp.Key;
                var methodNames = kvp.Value;
                var lines = File.ReadAllLines(file).ToList();
                bool modified = false;

                for (int i = 0; i < lines.Count - 1; i++)
                {
                    foreach (var methodName in methodNames)
                    {
                        if (lines[i].Trim().StartsWith("[Test, Ignore(\"Disabled via TestsToolWindow\")") &&
                            lines[i + 1].Contains($"void {methodName}("))
                        {
                            lines[i] = "        [Test]";
                            modified = true;
                            Debug.Log($"[TestsTool] Удалён Ignore из метода: {methodName}");
                        }
                    }
                }

                if (modified)
                {
                    File.WriteAllLines(file, lines);
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