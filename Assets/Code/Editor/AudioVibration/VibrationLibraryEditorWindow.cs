using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Code.Services.AudioVibrationFX.Vibration;
using Code.StaticData.AudioVibration;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Code.Editor.AudioVibration
{
    public class VibrationLibraryEditorWindow : OdinEditorWindow
    {
        private const string VibrationDataPath = "StaticData/Vibration/VibrationsData";
        private const string EnumOutPutPath = "Assets/Code/Services/AudioVibrationFX/Vibration/VibrationType.cs";

        [MenuItem("Tools/Audio Vibration Window/Vibration Library", false, 2002)]
        private static void OpenWindow()
        {
            GetWindow<VibrationLibraryEditorWindow>().Show();
        }
        
        private VibrationsData _vibrationsData;

        [BoxGroup("Existing Enum"), ReadOnly]
        [MultiLineProperty(5)]
        [SerializeField]
        private string _vibrationEnumPreview;

        [BoxGroup("Vibrations")]
        [ShowInInspector, Searchable]
        [ListDrawerSettings(
            Expanded = true,
            DraggableItems = true,
            ShowPaging = true,
            ListElementLabelName = "Name"
        )]
        private List<VibrationData> _editableVibrations;

        [BoxGroup("Generation")]
        [Button("Generate Enum", ButtonSizes.Large)]
        [GUIColor(0f, 1f, 0f)]
        private void GenerateEnum()
        {
            if (_vibrationsData == null)
            {
                Debug.LogError("[VibrationLibraryEditorWindow] VibrationsData not assigned or not loaded.");
                return;
            }

            GenerateEnumFile(EnumOutPutPath, "VibrationType", _editableVibrations);
            AssignEnumTypes(_editableVibrations);
            SaveData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            UpdateEnumPreview();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            _vibrationsData = Resources.Load<VibrationsData>(VibrationDataPath);

            if (_vibrationsData != null)
            {
                _editableVibrations = _vibrationsData.Vibrations;
                UpdateEnumPreview();
            }
            else
            {
                Debug.LogError($"VibrationsData not found at Resources/{VibrationDataPath}.asset");
                _editableVibrations = new List<VibrationData>();
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            SaveData();
        }

        private void GenerateEnumFile(string path, string enumName, List<VibrationData> vibrationList)
        {
            var names = vibrationList
                .Where(v => !string.IsNullOrWhiteSpace(v.Name))
                .Select(v => v.Name.Replace(" ", "_").Replace("-", "_").Replace(".", "_").Trim())
                .Distinct()
                .ToList();

            using (var writer = new StreamWriter(path))
            {
                writer.WriteLine("using System;");
                writer.WriteLine();
                writer.WriteLine("namespace Code.Services.AudioVibrationFX.Vibration");
                writer.WriteLine("{");
                writer.WriteLine("    [Serializable]");
                writer.WriteLine($"    public enum {enumName}");
                writer.WriteLine("    {");
                writer.WriteLine("        Unknown = -1,");

                for (int i = 0; i < names.Count; i++)
                    writer.WriteLine($"        {names[i]} = {i},");

                writer.WriteLine("    }");
                writer.WriteLine("}");
            }

            Debug.Log($"[VibrationLibraryEditorWindow] Enum {enumName} generated at {path}");
        }

        private void AssignEnumTypes(List<VibrationData> list)
        {
            foreach (var vibration in list)
            {
                var sanitized = vibration.Name.Replace(" ", "_").Replace("-", "_").Replace(".", "_").Trim();
                if (Enum.TryParse(sanitized, out VibrationType parsed))
                    vibration.VibrationType = parsed;
                else
                    vibration.VibrationType = VibrationType.Unknown;
            }
        }

        public void UpdateVibrationTypesAfterReload()
        {
            foreach (var vibration in _editableVibrations)
            {
                var sanitized = vibration.Name.Replace(" ", "_").Replace("-", "_").Replace(".", "_").Trim();

                if (Enum.TryParse(sanitized, out VibrationType parsed))
                    vibration.VibrationType = parsed;
                else
                    vibration.VibrationType = VibrationType.Unknown;
            }

            SaveData();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        private void SaveData()
        {
            if (_vibrationsData != null)
            {
                EditorUtility.SetDirty(_vibrationsData);
            }
        }

        private void UpdateEnumPreview()
        {
            _vibrationEnumPreview = string.Join(", ",
                Enum.GetNames(typeof(VibrationType)));
        }
    }
}
