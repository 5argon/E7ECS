using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using Unity.Jobs;
using System.Collections.Generic;

/// <summary>
/// Remove or set to default components of ALL entities in the world at once.
/// It will work only on entities with that component.
/// Subclass and call a series of Clean/Remove on your `OnPurification`.
/// 
/// When you are going to serialize a world for example,
/// put a subclass of this system in that world and run Update
/// once to clean up unwanted components.
/// TODO : Wait for "chunk operation" API.
/// </summary>
public abstract class PurifierSystem : JobComponentSystem
{
    EntityCommandBuffer ecb;
    JobHandle input;

    List<JobHandle> jobHandles;
    List<EntityCommandBuffer> ecbs;
    protected override void OnCreateManager()
    {
        jobHandles = new List<JobHandle>();
        ecbs = new List<EntityCommandBuffer>();
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        input = inputDeps;
        ecbs.Clear();
        jobHandles.Clear();
        OnPurification();
        foreach (var jh in jobHandles)
        {
            jh.Complete();
        }
        foreach (var ecb in ecbs)
        {
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
        return inputDeps;
    }

    protected abstract void OnPurification();

    protected void Clean<T>() where T : struct, IComponentData
    {
        var cg = GetComponentGroup(ComponentType.Create<T>());
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        ecbs.Add(ecb);
        var job = new CleanJob<T>
        {
            entiType = GetArchetypeChunkEntityType(),
            ecb = ecb.ToConcurrent(),
        }
        .Schedule(cg, input);
        jobHandles.Add(job);
    }

    struct CleanJob<T> : IJobChunk where T : struct, IComponentData
    {
        [ReadOnly] public ArchetypeChunkEntityType entiType;
        public EntityCommandBuffer.Concurrent ecb;
        public void Execute(ArchetypeChunk ac, int i)
        {
            var na = ac.GetNativeArray(entiType);
            for (int j = 0; j < na.Length; j++)
            {
                ecb.SetComponent<T>(i, na[j], default);
            }
        }
    }

    protected void CleanShared<T>() where T : struct, ISharedComponentData 
    {
        var cg = GetComponentGroup(ComponentType.Create<T>());
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        ecbs.Add(ecb);
        var job = new CleanSharedJob<T>
        {
            entiType = GetArchetypeChunkEntityType(),
            ecb = ecb.ToConcurrent(),
        }
        .Schedule(cg, input);
        jobHandles.Add(job);
    }

    struct CleanSharedJob<T> : IJobChunk where T : struct, ISharedComponentData
    {
        [ReadOnly] public ArchetypeChunkEntityType entiType;
        public EntityCommandBuffer.Concurrent ecb;
        public void Execute(ArchetypeChunk ac, int i)
        {
            var na = ac.GetNativeArray(entiType);
            for (int j = 0; j < na.Length; j++)
            {
                ecb.SetSharedComponent<T>(i, na[j], default);
            }
        }
    }

    protected void Remove<T>() where T : struct
    {
        var cg = GetComponentGroup(ComponentType.Create<T>());
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        ecbs.Add(ecb);
        var job = new RemoveJob<T>
        {
            entiType = GetArchetypeChunkEntityType(),
            ecb = ecb.ToConcurrent(),
        }
        .Schedule(cg, input);
        jobHandles.Add(job);
    }

    struct RemoveJob<T> : IJobChunk where T : struct
    {
        [ReadOnly] public ArchetypeChunkEntityType entiType;
        public EntityCommandBuffer.Concurrent ecb;
        public void Execute(ArchetypeChunk ac, int i)
        {
            var na = ac.GetNativeArray(entiType);
            for (int j = 0; j < na.Length; j++)
            {
                ecb.RemoveComponent<T>(i, na[j]);
            }
        }
    }
}
