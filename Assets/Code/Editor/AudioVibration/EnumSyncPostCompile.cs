using System.Linq;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;

namespace Code.Editor.AudioVibration
{
    [InitializeOnLoad]
    public static class EnumSyncPostCompile
    {
        [DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            Debug.Log("🔄 Scripts recompiled. Trying to sync enums...");

            var soundWindow = Resources.FindObjectsOfTypeAll<SoundLibraryEditorWindow>().FirstOrDefault();
            if (soundWindow != null)
            {
                soundWindow.UpdateSoundTypesAfterReload();
                Debug.Log("✅ SoundTypes synced!");
            }
            else
            {
                Debug.LogWarning("⚠️ SoundLibraryEditorWindow not open. Skipping SoundTypes sync.");
            }

            var vibrationWindow = Resources.FindObjectsOfTypeAll<VibrationLibraryEditorWindow>().FirstOrDefault();
            if (vibrationWindow != null)
            {
                vibrationWindow.UpdateVibrationTypesAfterReload();
                Debug.Log("✅ VibrationTypes synced!");
            }
            else
            {
                Debug.LogWarning("⚠️ VibrationLibraryEditorWindow not open. Skipping VibrationTypes sync.");
            }
        }
    }
}