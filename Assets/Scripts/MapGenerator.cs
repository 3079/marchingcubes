using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// public abstract class MapGenerator<TMapConfig> : MonoBehaviour where TMapConfig : MapConfig
public abstract class MapGenerator : MonoBehaviour
{
    public abstract MapConfig config { get; protected set; }
    public int[,,] map { get; protected set; }
    public bool[,,] valueMap { get; protected set; }
    
    public abstract void InitializeMap(MapConfig config);

    public abstract bool ShouldRegenerate(MapConfig config);
    protected abstract void GenerateMap(Vector3Int resolution);
    public abstract void RegenerateMap();
    public abstract bool ShouldRecalculate(MapConfig config);
    public abstract void RecalculateMap();
    
    protected virtual void FilterMap()
    {
        valueMap = new bool[map.GetLength(0), map.GetLength(1), map.GetLength(2)];
        
        for (int i = 0; i < valueMap.GetLength(0); ++i)
        {
            for (int j = 0; j < valueMap.GetLength(1); ++j)
            {
                for (int k = 0; k < valueMap.GetLength(2); ++k)
                {
                    valueMap[i, j, k] = map[i, j, k] > config.threshold;
                }
            }
        }
    }
}
