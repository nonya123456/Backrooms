using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(CameraController))]
    public class CameraControllerEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var cc = (CameraController)target;

            DrawDefaultInspector();

            if (GUILayout.Button("Reset"))
            {
                cc.Reset();
            }
        }
    }
}
