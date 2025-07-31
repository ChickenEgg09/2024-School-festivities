using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MapGenerator))]
public class MapEditor : Editor
{
    public override void OnInspectorGUI()
    {
        MapGenerator map = (MapGenerator)target;

        if (DrawDefaultInspector())
        {
            map.GenerateMap();
        }

        if(GUILayout.Button("Generate Map"))
        {
            map.GenerateMap();
        }
    }
}
