using Unity.Entities;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using System;
using Unity.Jobs;

namespace E7.ECS
{

    /// <summary>
    /// Special purpose chunk iteration wrapper based on ComponentGroup geared for only one IComponentData.
    /// Requires registering with the system on OnCreateManager.
    /// You can take it to the job after some ceremonies.
    /// </summary>
    public struct SingleQuery<D> : IDisposable
    where D : struct, IComponentData
    {
        [NativeSetClassTypeToNullOnSchedule] private EntityQuery cg;
        [NativeSetClassTypeToNullOnSchedule] private ComponentSystemBase system;

        /// <summary>
        /// Useful when you want to send it to IJobChunk
        /// </summary>
        public EntityQuery Query { get => cg; set => cg = value; }

        /// <summary>
        /// Used in IJobChunk
        /// </summary>
        public ArchetypeChunkComponentType<D> GetACCT() => system.GetArchetypeChunkComponentType<D>(isReadOnly: true);

        /// <summary>
        /// Make itself known to the system's component groups.
        /// Pattern in your OnCreateManager is like : sq.Register(GetEntityQuery(sq))
        /// </summary>
        public void Register(EntityQuery cg, ComponentSystemBase cs)
        {
            Query = cg;
            system = cs;
        }

        public int ComponentGroupLength => Query.CalculateEntityCount();

        /// <summary>
        /// Only works in the main thread as it calculate component group's length
        /// </summary>
        public bool Injected => ComponentGroupLength > 0;

        public bool Any => NativeArrayOfChunk.Any;

        /// <summary>
        /// Very lazy method and only works in the main thread.
        /// </summary>
        public Entity FirstEntity
        {
            get
            {
                using (var ea = cg.ToEntityArray(Allocator.Temp))
                {
                    return ea[0];
                }
            }
        }

        /// <summary>
        /// Prepare() in the main thread needed.
        /// </summary>
        public bool FirstChunkUnchanged => !NativeArrayOfChunk.FirstChunkChanged;

        /// <summary>
        /// Equivalent to multiple chunk iteration ceremonies that are needed in the main thread. Do it before sending itself to the job.
        /// If you Prepare to use in the main thread and not sending to the job, you have to Dispose manually.
        /// </summary>
        public SingleQuery<D> Prepare(out JobHandle prepareJh)
        {
            var acct = GetACCT();
            var aca = cg.CreateArchetypeChunkArray(Allocator.TempJob, out JobHandle pj);
            NativeArrayOfChunk = new ArchetypeChunkIterator { aca = aca, acct = acct, lastSystemVersionKeep = system.LastSystemVersion };
            prepareJh = pj;
            return this;
        }

        /// <summary>
        /// Equivalent to multiple chunk iteration ceremonies that are needed in the main thread. Do it before sending itself to the job.
        /// If you Prepare to use in the main thread and not sending to the job, you have to Dispose manually.
        /// </summary>
        public SingleQuery<D> Prepare()
        {
            var acct = GetACCT();
            var aca = cg.CreateArchetypeChunkArray(Allocator.TempJob);
            NativeArrayOfChunk = new ArchetypeChunkIterator { aca = aca, acct = acct, lastSystemVersionKeep = system.LastSystemVersion };
            return this;
        }

        /// <summary>
        /// Single typed chunk iterator.
        /// </summary>
        public struct ArchetypeChunkIterator
        {
            [ReadOnly] [DeallocateOnJobCompletion] public NativeArray<ArchetypeChunk> aca;
            //Make this from out of job to be use in-job.
            [ReadOnly] public ArchetypeChunkComponentType<D> acct;

            public uint lastSystemVersionKeep;

            //Makes `First` property more liberal to use.
            //The attribute allows this field to be empty at first in a job.
            [NativeDisableContainerSafetyRestriction] private NativeArray<D> cachedFirstChunk;
            public NativeArray<D> this[int chunkIndex]
            {
                get
                {
                    bool firstChunk = chunkIndex == 0;
                    if (firstChunk && cachedFirstChunk.IsCreated)
                    {
                        return cachedFirstChunk;
                    }
#if UNITY_EDITOR
                    if (aca.IsCreated == false)
                    {
                        throw new Exception($"You didn't call .Prepare() yet before using...");
                    }
#endif
                    var na = aca[chunkIndex].GetNativeArray(acct);
                    if (firstChunk)
                    {
                        cachedFirstChunk = na;
                    }
                    return na;
                    
                }
            }

            public void Dispose() => aca.Dispose();
            public bool Any => cachedFirstChunk.Length != 0;
            public bool IsCreated => aca.IsCreated;
            public bool FirstChunkChanged => ChunkChanged(0);
            public bool ChunkChanged(int i) => aca[i].DidChange(acct, lastSystemVersionKeep);
        }

        /// <summary>
        /// Do not call this if you are sending prepares to the job.
        /// Must call this if you prepare and use it in the main thread.
        /// </summary>
        public void Dispose()
        {
            NativeArrayOfChunk.Dispose();
        }

        /// <summary>
        /// Valid after PrepareForRead/Write
        /// </summary>
        public int ChunkCount => NativeArrayOfChunk.aca.Length;

        /// <summary>
        /// Please iterate through `ChunkCount` as an indexer.
        /// </summary>
        public ArchetypeChunkIterator NativeArrayOfChunk { get; private set; }

        public D First => NativeArrayOfChunk[0][0];

        /// <summary>
        /// When you sure your single component would all be together in one chunk you can iterate through them all with this.
        /// It can throw to let you know if that is not the case.
        /// </summary>
        public NativeArray<D> FirstChunk
        {
            get
            {
#if UNITY_EDITOR
                if (ChunkCount != 1)
                {
                    throw new Exception($"You are using FirstChunk but chunk count is not 1 but {ChunkCount}. Probably you are expecting there is one chunk?");
                }
#endif
                return NativeArrayOfChunk[0];
            }
        }

        /// <summary>
        /// Allows stunt flying in OnCreateManager.
        /// </summary>
        public static implicit operator ComponentType(SingleQuery<D> s) => ComponentType.ReadOnly<D>();
    }

}