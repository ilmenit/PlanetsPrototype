using UnityEditor.SceneManagement;

#if UNITY_EDITOR

using UnityEngine;
using UnityEditor;

[InitializeOnLoad]
public class AutosaveOnRun : ScriptableObject
{
    private static void StateChanged(PlayModeStateChange state)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isPlaying)
        {
            Debug.Log("Auto-Saving scene before entering Play mode: " + EditorSceneManager.GetActiveScene().name);

            EditorSceneManager.SaveScene(EditorSceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }
    }

    static AutosaveOnRun()
    {
        EditorApplication.playModeStateChanged += StateChanged;
    }
}

#endif