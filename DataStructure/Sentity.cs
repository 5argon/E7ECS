using System;
using Unity.Entities;

#if UNITY_EDITOR
#endif

/// <summary>
/// C# serializable Entity.. lol
/// </summary>
[Serializable]
public struct Sentity : IEquatable<Sentity>
{
    public int Index;
    public int Version;

    public static bool operator ==(Sentity lhs, Sentity rhs)
    {
        return lhs.Index == rhs.Index && lhs.Version == rhs.Version;
    }

    public static bool operator !=(Sentity lhs, Sentity rhs)
    {
        return lhs.Index != rhs.Index || lhs.Version != rhs.Version;
    }

    public override bool Equals(object compare)
    {
        return this == (Sentity)compare;
    }

    public override int GetHashCode()
    {
        return Index;
    }

    public static Sentity Null => new Sentity();

    public bool Equals(Sentity entity)
    {
        return entity.Index == Index && entity.Version == Version;
    }

    public override string ToString()
    {
        return $"Sentity Index: {Index} Version: {Version}";
    }

    public static explicit operator Sentity(Entity e)
    {
        return new Sentity { Index = e.Index, Version = e.Version };
    }

    public static explicit operator Entity(Sentity e)
    {
        return new Entity { Index = e.Index, Version = e.Version };
    }
}
