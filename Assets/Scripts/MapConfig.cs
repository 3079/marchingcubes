using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public abstract class MapConfig : IEquatable<MapConfig>
{
    public abstract Vector3Int resolution { get; protected set; }
    public abstract float size { get; protected set; }
    public abstract int threshold { get; protected set; }
    public abstract bool useSeed { get; protected set; }
    public abstract bool spherical { get; protected set; }
    public abstract float radius { get; protected set; }
    public abstract string seed { get; protected set; }
    public abstract bool drawGizmos { get; protected set; }
    
    private MapConfig _oldConfig;

    public abstract void CopyFrom(MapConfig other);

    protected virtual void CopyFromBase(MapConfig other)
    {
        resolution = other.resolution;
        size = other.size;
        useSeed = other.useSeed;
        seed = other.seed;
        drawGizmos = other.drawGizmos;
    }

    public abstract bool Equals(MapConfig other);

    protected virtual bool EqualsBase(MapConfig other) 
    {
        return other.resolution == resolution &&
               other.size.Equals(size) &&
               other.spherical == spherical &&
               other.radius.Equals(radius) &&
               other.useSeed == useSeed &&
               other.seed == seed &&
               other.drawGizmos == drawGizmos;
    }

    public abstract MapConfig Clone();
}
