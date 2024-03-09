using System;
using UnityEngine;

[Serializable]
public class MeshBuilderConfig : IEquatable<MeshBuilderConfig>
{
    [SerializeField] private bool drawGizmos = false;
    [SerializeField] private bool interpolateVertices = false;

    public bool Equals(MeshBuilderConfig other)
    {
        return false;
    }
}
