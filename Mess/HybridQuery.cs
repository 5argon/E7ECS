using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using System;

namespace E7.ECS
{
    /// <summary>
    /// Query a MonoBehaviour component in ECS produced by GameObjectEntity.
    /// </summary>
    public struct HybridQuery<T> : IECSQuery 
    where T : Component
    {
        public ComponentGroup Query { get; private set; }
        public void Register(ComponentGroup cg)
        {
            this.Query = cg;
        }

        public int ComponentGroupLength => Query.CalculateLength();
        public bool Injected => ComponentGroupLength > 0;
        public T First => Query.GetComponentArray<T>()[0];

        public IEnumerable<T> Components
        {
            get
            {
                var ca = Query.GetComponentArray<T>();
                for (int i = 0; i < ca.Length; i++)
                {
                    yield return ca[i];
                }
            }
        }

        public static implicit operator ComponentType(HybridQuery<T> s) => ComponentType.ReadOnly<T>();
    }

    /// <summary>
    /// Query an entity with 1 shared and 1 component data.
    /// The intention of shared is to hold scene's data via SharedComponentDataWrapper for a hybrid approach.
    /// I heard that ComponentDataArray we used here is to be deprecated?
    /// </summary>
    public struct HybridQuery<SHARED, DATA> : IECSQuery 
    where SHARED : struct, ISharedComponentData 
    where DATA : struct, IComponentData
    {
        public ComponentGroup Query { get; private set; }
        public void Register(ComponentGroup cg)
        {
            this.Query = cg;
        }

        /// <summary>
        /// Set a Changed filter on component data in the group also
        /// </summary>
        public void RegisterChanged(ComponentGroup cg)
        {
            this.Query = cg;
            SetFilterChangedOndata();
        }

        public int ComponentGroupLength => Query.CalculateLength();
        public bool Injected => ComponentGroupLength > 0;

        /// <summary>
        /// When `foreach` on this, CDA and SCDA is created once.
        /// </summary>
        public IEnumerable<(SHARED component, DATA data)> ComponentDataPair
        {
            get
            {
                var ca = Query.GetSharedComponentDataArray<SHARED>();
                var cda = Query.GetComponentDataArray<DATA>();
                for (int i = 0; i < ca.Length; i++)
                {
                    yield return (ca[i], cda[i]);
                }
            }
        }

        /// <summary>
        /// Make the group returns just the data that changed.
        /// </summary>
        public void SetFilterChangedOndata() => Query.SetFilterChanged(ComponentType.Create<DATA>());

        public static implicit operator ComponentType[] (HybridQuery<SHARED, DATA> s) => new ComponentType[] { ComponentType.ReadOnly<SHARED>(), ComponentType.ReadOnly<DATA>() };
        public ComponentType[] Writable => new ComponentType[] { ComponentType.ReadOnly<SHARED>(), ComponentType.Create<DATA>() };
    }


}