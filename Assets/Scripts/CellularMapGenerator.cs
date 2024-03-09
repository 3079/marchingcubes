using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class CellularMapGenerator : MapGenerator
{
    private CellularMapConfig _config;
    public override MapConfig config { get => _config; protected set => _config = value as CellularMapConfig; }
    private System.Random _random;

    private int _iterator = 1;

    // private int[,,] _initMap;
    
    public override void InitializeMap(MapConfig config)
    {
        this.config = config;
        if (_config == null) return;
        RegenerateMap();
    }
    
    public override bool ShouldRegenerate(MapConfig other)
    {
        return other.resolution != _config.resolution ||
               other.useSeed != _config.useSeed ||
               (_config.useSeed && other.seed != _config.seed);
    }

    public override bool ShouldRecalculate(MapConfig other)
    {
        if (other is CellularMapConfig cOther)
        {
            return cOther.smoothIterations != _config.smoothIterations ||
                   !cOther.size.Equals(_config.size) ||
                   cOther.threshold != _config.threshold ||
                   cOther.smoothIterations != _config.smoothIterations ||
                   cOther.maxNeighbourThreshold != _config.maxNeighbourThreshold ||
                   cOther.minNeighbourThreshold != _config.minNeighbourThreshold;
        }

        return false;
    }

    public override void RegenerateMap()
    {
        GenerateMap(_config.resolution);
        
        RecalculateMap();
    }

    public override void RecalculateMap()
    {
        FilterMap();
        
        for (int i = 0; i < _config.smoothIterations; ++i)
        {
            SmoothMap();
        }
    }

    protected override void GenerateMap(Vector3Int resolution)
    {
        _random = new System.Random(_config.useSeed ? _config.seed.GetHashCode() : (DateTime.UtcNow - DateTime.MinValue).TotalSeconds.ToString().GetHashCode());
        
        map = new int[resolution.x, resolution.y, resolution.z];
        valueMap = new bool[resolution.x, resolution.y, resolution.z];
        
        for (int x = 0; x < resolution.x; ++x)
        {
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int z = 0; z < resolution.z; ++z)
                {
                    // TODO add configurable boundary variable 
                    
                    if (x == 0 || x == resolution.x - 1 || y == 0 || y == resolution.y - 1 || z == 0 || z == resolution.z - 1)
                    {
                        map[x, y, z] = 100;
                    }
                    else
                    {
                        map[x, y, z] = _random.Next(0, 100);
                    }
                }
            }
        }
    }

    private void ResetMap(Vector3Int resolution)
    {
        for (int x = 0; x < resolution.x; ++x)
        {
            for (int y = 0; y < resolution.y; ++y)
            {
                for (int z = 0; z < resolution.z; ++z)
                {
                    valueMap[x, y, z] = map[x, y, z] < _config.threshold;
                }
            }
        }
    }

    private void SmoothMap()
    {
        bool[,,] mapCopy = valueMap;
        for (int x = 0; x < _config.resolution.x; ++x)
        {
            for (int y = 0; y < _config.resolution.y; ++y)
            {
                for (int z = 0; z < _config.resolution.z; ++z)
                {
                    int neighbours = CountNeighbors(x, y, z, mapCopy);
                    if (neighbours > _config.maxNeighbourThreshold)
                        valueMap[x, y, z] = true;
                    else if (neighbours < _config.minNeighbourThreshold)
                        valueMap[x, y, z] = false;
                }
            }
        }
    }

    private int CountNeighbors(int x, int y, int z, bool[,,] map)
    {
        int count = 0;
        for (int i = x - 1; i <= x + 1; ++i)
        {
            for (int j = y - 1; j <= y + 1; ++j)
            {
                for (int k = z - 1; k <= z + 1; ++k)
                {
                    if (i < 0 || i >= _config.resolution.x || j < 0 || j >= _config.resolution.y || k < 0 || k >= _config.resolution.z)
                    {
                        ++count;
                        continue;
                    }
                    
                    if (map[i, j, k] && (i != x || j != y || k != z)) ++count;
                }
            }
        }
        return count;
    }

    private void Update()
    {
        if (_config.drawGizmos && Input.GetMouseButtonDown(0))
        {
            _iterator = (_iterator + 1) % _config.resolution.y;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(transform.position, (Vector3)_config.resolution * _config.size);
        
        if (!_config.drawGizmos) return;

        for (int x = (_iterator + _config.resolution.y - 1) % config.resolution.y;
             x != (_iterator + 1) % _config.resolution.y;
             x = (x + 1) % _config.resolution.y)
        {
            for (int i = 0; i < _config.resolution.x; ++i)
            {
                for (int j = 0; j < _config.resolution.z; ++j)
                {
                    Gizmos.color = valueMap[i, x, j] ? Color.black : Color.white;
                    // Gizmos.DrawCube(new Vector3(-resolution / 2f + i * size - size / 2f, x * size - size / 2f, -resolution / 2f + j * size -size / 2f), new Vector3(size, size, size));
                    Gizmos.DrawSphere(
                        new Vector3(transform.position.x -_config.resolution.x * _config.size / 2f + i * _config.size + _config.size / 2f,
                            transform.position.y -_config.resolution.y * _config.size / 2f + x * _config.size + _config.size / 2f,
                            transform.position.z + _config.resolution.z * _config.size / 2f - j * _config.size - _config.size / 2f),
                        0.25f
                        );
                }
            }
        }
    }
}
