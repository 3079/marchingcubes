using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct Noise
{
    enum NoiseType
    {
        Cellular,
        Perlin,
        Simplex
    }

    [SerializeField] private NoiseType type;
    [SerializeField] private Vector3 position;
    [SerializeField] private float scale;
    [SerializeField] private float modifier;
    public Vector3 pos => position;
    
    public float Evaluate(Vector3 offset)
    {
        var pos = (position + offset) / (scale <= 0 ? 0.0001f : scale);
        switch (type)
        {
            case NoiseType.Cellular:
                return noise.cellular(new float3(pos.x, pos.y, pos.z)).x;
            case NoiseType.Perlin:
                return (1 + noise.cnoise(new float3(pos.x, pos.y, pos.z))) / 2f;
            case NoiseType.Simplex:
                return (1 + noise.snoise(new float3(pos.x, pos.y, pos.z))) / 2f;
            default:
                return -1;
        }
    }

    public static bool operator ==(Noise a, Noise b)
    {
        return a.type == b.type &&
               a.pos == b.pos &&
               a.scale.Equals(b.scale) &&
               a.modifier.Equals(b.modifier);
    }
    
    public static bool operator !=(Noise a, Noise b)
    {
        return !(a == b);
    }
}

[Serializable]
public class NoiseMapConfig : MapConfig
{
    [Header("General Map Settings")]
    [SerializeField] private Vector3Int _resolution = new Vector3Int(16, 16, 16);
    [SerializeField] private float _size = 1f;
    [Range(0, 100)] [SerializeField] private int _threshold = 50;
    
    [SerializeField] private bool _useSeed = true;
    [SerializeField] private string _seed = "bebra";
    
    [SerializeField] private bool _spherical = false;
    [SerializeField] private float _radius = 1f;
    
    [Header("Cellular Settings")]

    [SerializeField] private List<Noise> _noises = new List<Noise>();
    
    [Header("Debug")]
    [SerializeField] private bool _drawGizmos = false;
    
    public override Vector3Int resolution { get => _resolution; protected set => _resolution = value; }

    public override float size { get => _size; protected set => _size = value; }
    public override int threshold { get => _threshold; protected set => _threshold = value; }
    public override bool useSeed { get => _useSeed; protected set => _useSeed = value; }
    public override string seed { get => _seed; protected set => _seed = value; }
    public override bool spherical { get => _spherical; protected set => _spherical = value; }
    public override float radius { get => _radius; protected set => _radius = value; }
    public List<Noise> noises { get => _noises; protected set => _noises = value; }
    
    public override bool drawGizmos { get => _drawGizmos; protected set => _drawGizmos = value; }
    
    public override void CopyFrom(MapConfig other)
    {
        CopyFromBase(other);
        if(other is NoiseMapConfig config)
        {
            // TODO
            _threshold = config.threshold;
            noises.Clear();
            noises.AddRange(config.noises);
            return;
        }
    }

    public override bool Equals(MapConfig other)
    {
        if(other is NoiseMapConfig config)
        {
            // TODO
            return Equals(config) &&
                   config.threshold == _threshold;
        }

        return false;
    }
    
    public override MapConfig Clone()
    {
        MapConfig config = new NoiseMapConfig();
        config.CopyFrom(this);
        return config;
    }
}
