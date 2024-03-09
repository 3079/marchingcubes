using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[Serializable]
public class CellularMapConfig : MapConfig
{
    [Header("General Map Settings")]
    [Range(0, 100)] [SerializeField] private int _threshold = 50;
    [SerializeField] private Vector3Int _resolution = new Vector3Int(16, 16, 16);
    [SerializeField] private float _size = 1f;
    
    [SerializeField] private bool _useSeed = true;
    [SerializeField] private string _seed = "bebra";
    
    [SerializeField] private bool _spherical = false;
    [SerializeField] private float _radius = 1f;
    
    [Header("Cellular Settings")]
    [SerializeField] private int _smoothIterations = 1;
    [SerializeField] private int _maxNeighbourThreshold = 21;
    [SerializeField] private int _minNeighbourThreshold = 19;
    
    [Header("Debug")]
    [SerializeField] private bool _drawGizmos = false;
    
    public override Vector3Int resolution { get => _resolution; protected set => _resolution = value; }

    public override float size { get => _size; protected set => _size = value; }
    public override int threshold { get => _threshold; protected set => _threshold = value; }
    public override bool useSeed { get => _useSeed; protected set => _useSeed = value; }
    public override string seed { get => _seed; protected set => _seed = value; }
    
    public override bool drawGizmos { get => _drawGizmos; protected set => _drawGizmos = value; }
    public override bool spherical { get => _spherical; protected set => _spherical = value; }
    public override float radius { get => _radius; protected set => _radius = value; }
    public int smoothIterations => _smoothIterations;
    public int maxNeighbourThreshold => _maxNeighbourThreshold;
    public int minNeighbourThreshold => _minNeighbourThreshold;

    private MapConfig _oldConfig;

    public override void CopyFrom(MapConfig other)
    {
        CopyFromBase(other);
        if(other is CellularMapConfig config)
        {
            _threshold = config.threshold;
            _smoothIterations = config.smoothIterations;
            _maxNeighbourThreshold = config.maxNeighbourThreshold;
            _minNeighbourThreshold = config.minNeighbourThreshold;
        }
    }

    public override bool Equals(MapConfig other)
    {
        if(other is CellularMapConfig config)
        {
            return Equals(config) &&
                   config.threshold == _threshold &&
                   config.smoothIterations == _smoothIterations &&
                   config.maxNeighbourThreshold == _maxNeighbourThreshold &&
                   config.minNeighbourThreshold == _minNeighbourThreshold;
        }

        return false;
    }

    public override MapConfig Clone()
    {
        MapConfig config = new CellularMapConfig();
        config.CopyFrom(this);
        return config;
    }
}
