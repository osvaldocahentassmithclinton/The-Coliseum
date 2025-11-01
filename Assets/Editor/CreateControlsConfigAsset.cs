using UnityEditor;
using UnityEngine;

public static class CreateControlsConfigAsset
{
    [MenuItem("Assets/Create/Controls/ControlsConfig")]
    public static void CreateAsset()
    {
        var asset = ScriptableObject.CreateInstance<ControlsConfig>();
        string path = AssetDatabase.GenerateUniqueAssetPath("Assets/ControlsConfig.asset");
        AssetDatabase.CreateAsset(asset, path);
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
    }
}
