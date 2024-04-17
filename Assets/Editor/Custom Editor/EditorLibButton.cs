using UnityEngine;
using UnityEditor;


[CustomEditor(typeof(EditorLibrary))]
public class EditorLibButton : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorLibrary info = (EditorLibrary)target;
        if (GUILayout.Button("Load_All_Data"))
        {
            info.LoadAllData();
            EditorUtility.SetDirty(info);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}
