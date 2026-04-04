using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CodeViewScaler))]
public class CodeViewScalerEditor : Editor
{
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        if (GUILayout.Button("Update Height")) {
            Undo.RecordObject(target, "Update Height");
            (target as CodeViewScaler).UpdateHeight();
            EditorUtility.SetDirty(target);
        }
    }
}
