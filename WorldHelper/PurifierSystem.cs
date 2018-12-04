using Unity.Entities;
using Unity.Collections;
using UnityEngine;

/// <summary>
/// Remove or set default components of ALL entities in the world at once.
/// It will not work on entities without that component.
/// Subclass and call a series of Clean/Remove on your `OnPurification`.
/// 
/// When you are going to serialize a world for example,
/// put a subclass of this system in that world and run Update
/// once to clean up unwanted components.
/// </summary>
public abstract class PurifierSystem : ComponentSystem
{
    protected override void OnCreateManager() { }

    EntityCommandBuffer ecb;
    protected override void OnUpdate()
    {
        ecb = new EntityCommandBuffer(Allocator.Temp);
        OnPurification();
        ecb.Playback(EntityManager);
    }

    protected abstract void OnPurification();

    protected void Clean<T>()  where T : struct, IComponentData
    {
        var cg = GetComponentGroup(ComponentType.Create<T>());
        using (var aca = cg.CreateArchetypeChunkArray(Allocator.TempJob))
        {
            for (int i = 0; i < aca.Length; i++)
            {
                var na = aca[i].GetNativeArray(GetArchetypeChunkEntityType());
                for (int j = 0; j < na.Length; j++)
                {
                    ecb.SetComponent<T>(default);
                }
            }
        }
    }

    protected void CleanShared<T>() where T : struct, ISharedComponentData 
    {
        var cg = GetComponentGroup(ComponentType.Create<T>());
        using (var aca = cg.CreateArchetypeChunkArray(Allocator.TempJob))
        {
            for (int i = 0; i < aca.Length; i++)
            {
                var na = aca[i].GetNativeArray(GetArchetypeChunkEntityType());
                for (int j = 0; j < na.Length; j++)
                {
                    ecb.SetSharedComponent<T>(default);
                }
            }
        }
    }

    protected void Remove<T>()
    {
        var cg = GetComponentGroup(ComponentType.Create<T>());
        using (var aca = cg.CreateArchetypeChunkArray(Allocator.TempJob))
        {
            for (int i = 0; i < aca.Length; i++)
            {
                var na = aca[i].GetNativeArray(GetArchetypeChunkEntityType());
                for (int j = 0; j < na.Length; j++)
                {
                    ecb.RemoveComponent<T>(na[j]);
                }
            }
        }
    }
}
