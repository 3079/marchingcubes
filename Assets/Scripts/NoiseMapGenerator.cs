using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class NoiseMapGenerator : MapGenerator
{
    private NoiseMapConfig _config;
    public override MapConfig config { get => _config; protected set => _config = value as NoiseMapConfig; }
    private int _iterator = 1;
    public override void InitializeMap(MapConfig config)
    {
        this.config = config;
        if (_config == null) return;
        RegenerateMap();
    }
    
    public override bool ShouldRegenerate(MapConfig other)
    {
        if (other is NoiseMapConfig cOther)
        {
            return other.resolution != _config.resolution ||
                   other.spherical != _config.spherical ||
                   other.useSeed != _config.useSeed ||
                   (_config.useSeed && other.seed != _config.seed) ||
                   CompareNoises(cOther);
        }

        return false;
    }
    
    public override void RegenerateMap()
    {
        GenerateMap(_config.resolution);
        
        RecalculateMap();
    }

    public override bool ShouldRecalculate(MapConfig other)
    {
        if (other is NoiseMapConfig cOther)
        {
            return !cOther.size.Equals(_config.size) ||
                   !cOther.radius.Equals(_config.radius) ||
                   cOther.threshold != _config.threshold;
        }

        return false;
    }

    private bool CompareNoises(NoiseMapConfig other)
    {
        if (other.noises.Count != _config.noises.Count)
        {
            Debug.Log("Noises were modified");
            return true;
        }

        bool noiseChanged = false;
        for (int i = 0; i < other.noises.Count; ++i)
        {
            // noiseChanged |= other.noises[i].pos != _config.noises[i].pos;
            noiseChanged |= other.noises[i] != _config.noises[i];
        }

        if(noiseChanged) Debug.Log("Noises were modified");
        return noiseChanged;
    }
    
    public override void RecalculateMap()
    {
        FilterMap();
    }

    protected override void GenerateMap(Vector3Int resolution)
    {
        map = new int[resolution.x + 1, resolution.y + 1, resolution.z + 1];
        valueMap = new bool[resolution.x + 1, resolution.y + 1, resolution.z + 1];

        Vector3 pos = Vector3.zero;
        
        for (int x = 0; x <= resolution.x; ++x)
        {
            for (int y = 0; y <= resolution.y; ++y)
            {
                for (int z = 0; z <= resolution.z; ++z)
                {
                    pos.x = -resolution.x * _config.size / 2f + x * _config.size;
                    pos.y = -resolution.y * _config.size / 2f + y * _config.size;
                    pos.z = resolution.z * _config.size / 2f - z * _config.size;
                    float noiseValue = 1f;
                    foreach (var noise in _config.noises)
                    {
                        noiseValue *= noise.Evaluate(pos);
                    }
                    map[x, y, z] = (int) ((config.spherical ? 
                        Mathf.Clamp01(1f - Dist(pos, config.radius) + noiseValue) :
                        noiseValue) * 100);
                }
            }
        }
    }

    private float Dist(Vector3 pos, float radius)
    {
        var d = Vector3.Distance(transform.position, pos) / _config.size;
        // return Mathf.Clamp01(d / radius);
        return d / radius;
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
        Gizmos.DrawWireCube(transform.position, (Vector3) _config.resolution * _config.size);
        
        if (!_config.drawGizmos) return;

        for (int x = (_iterator + _config.resolution.y - 1) % config.resolution.y;
             x != (_iterator + 1) % _config.resolution.y;
             x = (x + 1) % _config.resolution.y)
        {
            for (int i = 0; i <= _config.resolution.x; ++i)
            {
                for (int j = 0; j <= _config.resolution.z; ++j)
                {
                    Gizmos.color = valueMap[i, x, j] ? Color.black : Color.white;
                    // Gizmos.DrawCube(new Vector3(-resolution / 2f + i * size - size / 2f, x * size - size / 2f, -resolution / 2f + j * size -size / 2f), new Vector3(size, size, size));
                    Gizmos.DrawSphere(
                        new Vector3(transform.position.x -_config.resolution.x * _config.size / 2f + i * _config.size,
                            transform.position.y -_config.resolution.y * _config.size / 2f + x * _config.size,
                            transform.position.z + _config.resolution.z * _config.size / 2f - j * _config.size),
                        0.25f
                    );
                }
            }
        }
    }
}
