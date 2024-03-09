using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    [SerializeReference, ConfigSerializer] private MapConfig _mapConfig;
    [SerializeField] private MeshBuilderConfig _meshConfig;
    [SerializeField] private bool _renderMesh = true;
    private MapConfig _oldMapConfig;
    private MapGenerator _mapGenerator;
    private MeshBuilder _meshBuilder;
    public MapGenerator mapGen => _mapGenerator;
    public MapConfig config => _mapConfig;

    public void UpdateConfig()
    {
        if (_mapGenerator.ShouldRegenerate(_oldMapConfig))
        {
            _mapGenerator.RegenerateMap();
            if(_renderMesh) _meshBuilder.RebuildMesh(_mapGenerator.map, _mapGenerator.valueMap, _mapConfig.threshold, _mapConfig.size);
            Debug.Log("regenerating map");
        } else if (_mapGenerator.ShouldRecalculate(_oldMapConfig))
        {
            _mapGenerator.RecalculateMap();
            if(_renderMesh) _meshBuilder.RebuildMesh(_mapGenerator.map, _mapGenerator.valueMap, _mapConfig.threshold, _mapConfig.size);
            Debug.Log("recalculating map");
        }
        
        _oldMapConfig.CopyFrom(_mapConfig);
    }

    private void Awake()
    {
        _oldMapConfig = _mapConfig.Clone();
        _mapGenerator = GetComponent<MapGenerator>();
        _meshBuilder = GetComponent<MeshBuilder>();
    }

    private void Start()
    {
        _oldMapConfig.CopyFrom(_mapConfig);
        _mapGenerator.InitializeMap(_mapConfig);
        if(_renderMesh) _meshBuilder.BuildMesh(_mapGenerator.map, _mapGenerator.valueMap, _mapConfig.threshold, _mapConfig.size);
    }
}
