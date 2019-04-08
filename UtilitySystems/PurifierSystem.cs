using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using System.Collections.Generic;

/// <summary>
/// Remove or set to default components of ALL entities in the world at once.
/// It will work only on entities with that component.
/// Subclass and call a series of Clean/Remove on your `OnUpdate`.
/// 
/// When you are going to serialize a world for example,
/// put a subclass of this system in that world and run Update
/// once to clean up unwanted components.
/// 
/// The clean or remove is based on whole-chunk operations. No iteration made.
/// </summary>
public abstract class PurifierSystem : ComponentSystem
{
    protected void Clean<T>() where T : struct, IComponentData
    {
        var cg = Entities.WithAll<T>().ToEntityQuery();
        EntityManager.RemoveComponent(cg, ComponentType.ReadOnly<T>());
        EntityManager.AddComponent(cg, ComponentType.ReadOnly<T>());
    }

    /// <summary>
    /// ISharedComponentData with default value is special.
    /// It does not generates `GameObject` dependency, yet the type is still in the Archetype.
    /// "Cleaning" them will do just that. Preserving archetype but does not care about its value.
    /// </summary>
    protected void CleanShared<T>() where T : struct, ISharedComponentData 
    {
        var cg = Entities.WithAll<T>().ToEntityQuery();
        EntityManager.RemoveComponent(cg, ComponentType.ReadOnly<T>());
        EntityManager.AddSharedComponentData(cg, default(T));
    }

    protected void Remove<T>() where T : struct
    {
        var cg = Entities.WithAll<T>().ToEntityQuery();
        EntityManager.RemoveComponent(cg, ComponentType.ReadOnly<T>());
    }
}
