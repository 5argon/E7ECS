using Unity.Entities;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;

namespace E7.ECS
{
    /// <summary>
    /// Tons of to be deprecated methods!
    /// </summary>
    public struct SingleComponentQuery<D>
    where D : Component
    {
        [NativeSetClassTypeToNullOnSchedule] private ComponentGroup cg;
        [NativeSetClassTypeToNullOnSchedule] private ComponentSystemBase system;
        public ComponentGroup Query { get => cg; set => cg = value; }

        public static implicit operator ComponentType(SingleComponentQuery<D> s) => ComponentType.ReadOnly<D>();
        public void Register(ComponentGroup cg, ComponentSystemBase cs)
        {
            Query = cg;
            system = cs;
        }

        public ComponentArray<D> GetComponentArray() => cg.GetComponentArray<D>();

        public IEnumerable<D> GetComponentArrayIterator()
        {
            var ca = cg.GetComponentArray<D>();
            for (int i = 0; i < ca.Length; i++)
            {
                yield return ca[i];
            }
        }

        /// <summary>
        /// Dispose the array too!!
        /// </summary>
        public NativeArray<Entity> GetEntityArray(Allocator allocator)
        {
            return cg.ToEntityArray(allocator);
        }
        
        public bool Injected => Query.CalculateLength() > 0;
    }

}