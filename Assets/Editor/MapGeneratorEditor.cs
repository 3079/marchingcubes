using UnityEditor;
using UnityEngine;

[CustomEditor (typeof(TerrainGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        TerrainGenerator gen = (TerrainGenerator) target;
        MapGenerator mapGen = gen.mapGen;

        MapConfig config = gen.config;

        if (DrawDefaultInspector())
        {
            mapGen.RegenerateMap(); 
            gen.UpdateConfig();
            // Debug.Log("map regeneration triggered from editor");
        }
    }
}
