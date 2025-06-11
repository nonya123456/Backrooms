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
                DestroyImmediate(mapGen.Map);
                mapGen.GenerateMap();
                mapGen.BuildNavMesh();
            }

            if (GUILayout.Button("Generate"))
            {
                DestroyImmediate(mapGen.Map);
                mapGen.GenerateMap();
                mapGen.BuildNavMesh();
            }
        }
    }
}
