using Unity.Entities;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections;
using System;

namespace E7.ECS
{
    /// <summary>
    /// Get a single or singleton managed component!
    /// </summary>
    public struct SingleComponentQuery<D>
    where D : Component
    {
        [NativeSetClassTypeToNullOnSchedule] private EntityQuery cg;
        [NativeSetClassTypeToNullOnSchedule] private ComponentSystemBase system;
        public EntityQuery Query { get => cg; set => cg = value; }

        public static implicit operator ComponentType(SingleComponentQuery<D> s) => ComponentType.ReadOnly<D>();
        public void Register(EntityQuery cg, ComponentSystemBase cs)
        {
            Query = cg;
            system = cs;
            //For managed component, it adds a hard update requirement.
            cs.RequireForUpdate(cg);
        }

        public D First 
        {
            get{
                var ca = cg.ToComponentArray<D>();
                if( ca.Length != 1)
                {
                    throw new Exception($"You don't use {nameof(SingleComponentQuery<D>)} when you have {ca.Length} {typeof(D).Name}!");
                }
                return ca[0];
            }
        }

        public IEnumerable<D> GetComponentArrayIterator()
        {
            var ca = cg.ToComponentArray<D>();
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
        
        public bool Injected => Query.CalculateEntityCount() > 0;
    }

}