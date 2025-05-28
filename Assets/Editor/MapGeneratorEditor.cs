using UnityEditor;
using UnityEngine;

namespace Editor
{
    [CustomEditor(typeof(MapGenerator))]
    public class MapGeneratorEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var mapGen = (MapGenerator)target;

            if (DrawDefaultInspector())
            {
                mapGen.GenerateMap();
            }

            if (GUILayout.Button("Generate"))
            {
                mapGen.GenerateMap();
            }
        }
    }
}
