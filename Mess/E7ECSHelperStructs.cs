using Unity.Entities;
using Unity.Collections;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Jobs;

namespace E7.ECS
{
    public interface IInjectedCheckable
    {
        bool Injected { get; }
    }

    public interface IECSQuery
    {
        ComponentGroup Query { get; }
        int ComponentGroupLength { get; }
        bool Injected { get; }
    }

    /// <summary>
    /// Depricated, Unity will phase out injected struct.
    /// </summary>
    public struct SingleTransformComponent<T> : IInjectedCheckable
     where T : Component
    {
        [ReadOnly] public ComponentArray<T> components;
        public TransformAccessArray taa;
        public readonly int Length;

        public T First => components[0];
        public IEnumerable<T> ComponentIterator
        {
            get
            {
                for (int i = 0; i < Length; i++)
                {
                    yield return components[i];
                }
            }
        }

        /// <summary>
        /// When you have multiple injects the system will activates even with one struct injection passed. 
        /// You can use this so that `.First` is safe to use.
        /// </summary>
        public bool Injected => Length != 0;
    }

    /// <summary>
    /// Depricated, Unity will phase out injected struct.
    /// </summary>
    public struct SingleComponentDepricated<T> : IInjectedCheckable
     where T : Component
    {
        [ReadOnly] public ComponentArray<T> components;
        public readonly int Length;
        public T First => components[0];

        public IEnumerable<T> ComponentIterator
        {
            get
            {
                for (int i = 0; i < Length; i++)
                {
                    yield return components[i];
                }
            }
        }

        /// <summary>
        /// When you have multiple injects the system will activates even with one struct injection passed. 
        /// You can use this so that `.First` is safe to use.
        /// </summary>
        public bool Injected => Length != 0;
    }


    /// <summary>
    /// Deprecated, Unity will phase out injected struct.
    /// </summary>
    public struct SingleComponentData<T> : IInjectedCheckable
     where T : struct, IComponentData
    {
        [ReadOnly] public ComponentDataArray<T> datas;
        [ReadOnly] public EntityArray entities;
        public readonly int Length;
        public readonly int GroupIndex;

        public void SetChangedFilter(ComponentSystemBase cs) => cs.ComponentGroups[GroupIndex].SetFilterChanged(ComponentType.ReadOnly<T>());

        /// <summary>
        /// Do not use this in a job since Burst cannot do foreach
        /// (foreach translates to finally)
        /// </summary>
        public IEnumerable<T> DataIterator
        {
            get
            {
                for (int i = 0; i < Length; i++)
                {
                    yield return datas[i];
                }
            }
        }

        /// <summary>
        /// If you are planning to use this it is best to do it in the job so that auto dependencies works.
        /// IF you `First` before entering the job you might access the underlying data while other systems are busy writing to it.
        /// </summary>
        public T First
        {
            get => datas[0];
            set => datas[0] = value;
        }

        /// <summary>
        /// When you have multiple injects the system will activates even with one struct injection passed. 
        /// You can use this so that `.First` is safe to use.
        /// </summary>
        public bool Injected => Length != 0;
    }

    /// <summary>
    /// Depricated, Unity will phase out injected struct.
    /// </summary>
    public struct SingleComponentDataRW<T> : IInjectedCheckable
     where T : struct, IComponentData
    {
        public ComponentDataArray<T> datas;
        [ReadOnly] public EntityArray entities;
        public readonly int Length;

        public IEnumerable<T> DataIterator
        {
            get
            {
                for (int i = 0; i < Length; i++)
                {
                    yield return datas[i];
                }
            }
        }

        /// <summary>
        /// If you are planning to use this it is best to do it in the job so that auto dependencies works.
        /// IF you `First` before entering the job you might access the underlying data while other systems are busy writing to it.
        /// </summary>
        public T First
        {
            get => datas[0];
            set => datas[0] = value;
        }

        /// <summary>
        /// When you have multiple injects the system will activates even with one struct injection passed. 
        /// You can use this so that `.First` is safe to use.
        /// </summary>
        public bool Injected => Length != 0;
    }
}