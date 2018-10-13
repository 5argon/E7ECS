using Unity.Entities;
using UnityEngine;
using System.Collections.Generic;

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

        public int Length => Query.CalculateLength();
        public bool Injected => Length > 0;
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
    /// Query a hybrid component attached with one data.
    /// I heard that ComponentDataArray we used here is to be deprecated?
    /// </summary>
    public struct HybridQuery<T, D> : IECSQuery 
    where T : Component
    where D : struct, IComponentData
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

        public int Length => Query.CalculateLength();
        public bool Injected => Length > 0;
        public T FirstComponent => Query.GetComponentArray<T>()[0];
        public D FirstData => Query.GetComponentDataArray<D>()[0];

        public IEnumerable<(T component, D data)> ComponentDataPair
        {
            get
            {
                var ca = Query.GetComponentArray<T>();
                var cda = Query.GetComponentDataArray<D>();
                for (int i = 0; i < ca.Length; i++)
                {
                    yield return (ca[i], cda[i]);
                }
            }
        }

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

        public IEnumerable<D> Data
        {
            get
            {
                var ca = Query.GetComponentDataArray<D>();
                for (int i = 0; i < ca.Length; i++)
                {
                    yield return ca[i];
                }
            }
        }

        /// <summary>
        /// Make the group returns just the data that changed.
        /// </summary>
        public void SetFilterChangedOndata() => Query.SetFilterChanged(ComponentType.Create<D>());

        public static implicit operator ComponentType[] (HybridQuery<T, D> s) => new ComponentType[] { ComponentType.ReadOnly<T>(), ComponentType.ReadOnly<D>() };
        public ComponentType[] Writable => new ComponentType[] { ComponentType.ReadOnly<T>(), ComponentType.Create<D>() };
    }

    // /// <summary>
    // /// Query entities that contains one IComponentData
    // /// </summary>
    // public struct ECSQuery<D> : IECSQuery 
    // where D : struct, IComponentData
    // {
    //     public ComponentGroup Query { get; private set; }
    //     private ComponentSystemBase system;
    //     public void Register(ComponentGroup cg, ComponentSystemBase cs)
    //     {
    //         this.Query = cg;
    //         this.system = cs;
    //     }

    //     /// <summary>
    //     /// Set a Changed filter on component data in the group also
    //     /// </summary>
    //     public void RegisterChanged(ComponentGroup cg)
    //     {
    //         this.Query = cg;
    //         SetFilterChangedOndata();
    //     }

    //     public int Length => Query.CalculateLength();
    //     public bool Injected => Length > 0;

    //     public ArchetypeChunkComponentType<D> ACCT => system.GetArchetypeChunkComponentType<D>(isReadOnly: true);
    //     public ArchetypeChunkComponentType<D> WritableACCT => system.GetArchetypeChunkComponentType<D>(isReadOnly: false);

    //     public IEnumerable<D> Data
    //     {
    //         get
    //         {
    //             var acct = ACCT;
    //             using(var aca = Query.CreateArchetypeChunkArray(Allocator.Temp))
    //             {
    //                 for (int i = 0; i < aca.Length; i++)
    //                 {
    //                     var ac = aca[i];
    //                     var na = ac.GetNativeArray(acct); //We do NOT dispose this native array I think.
    //                     ac.
    //                     for (int j = 0; j < na.Length ; j++)
    //                     {
    //                         yield return na[j];
    //                     }
    //                 }
    //             }
    //         }
    //     }

    //     /// <summary>
    //     /// Make the group returns just the data that changed.
    //     /// </summary>
    //     public void SetFilterChangedOndata() 
    //     {
    //         Query.SetFilterChanged(new ComponentType[] { ComponentType.Create<D>() });
    //     }

    //     public static implicit operator ComponentType (ECSQuery<D> s) => ComponentType.ReadOnly<D>();
    //     public ComponentType Writable => ComponentType.ReadOnly<D>();
    // }

    // /// <summary>
    // /// Query entities with 2 IComponentData.
    // /// If you use a tag component/zero-sized component it should go into `D2`
    // /// </summary>
    // public struct ECSQuery<D1, D2> : IECSQuery 
    // where D1 : struct, IComponentData
    // where D2 : struct, IComponentData
    // {
    //     public ComponentGroup Query { get; private set; }
    //     public void Register(ComponentGroup cg)
    //     {
    //         this.Query = cg;
    //     }

    //     /// <summary>
    //     /// Set a Changed filter on component data in the group also
    //     /// </summary>
    //     public void RegisterChanged(ComponentGroup cg)
    //     {
    //         this.Query = cg;
    //         SetFilterChangedOndata();
    //     }

    //     public int Length => Query.CalculateLength();
    //     public bool Injected => Length > 0;

    //     public IEnumerable<(D1 component, D2 data)> DataPair
    //     {
    //         get
    //         {
    //             var cda1 = Query.GetComponentDataArray<D1>();
    //             var cda2 = Query.GetComponentDataArray<D2>();
    //             for (int i = 0; i < cda1.Length; i++)
    //             {
    //                 yield return (cda1[i], cda2[i]);
    //             }
    //         }
    //     }

    //     public IEnumerable<D1> Data
    //     {
    //         get
    //         {
    //             var cda = Query.GetComponentDataArray<D1>();
    //             for (int i = 0; i < cda.Length; i++)
    //             {
    //                 yield return cda[i];
    //             }
    //         }
    //     }

    //     public IEnumerable<D2> SecondaryData
    //     {
    //         get
    //         {
    //             var cda = Query.GetComponentDataArray<D2>();
    //             for (int i = 0; i < cda.Length; i++)
    //             {
    //                 yield return cda[i];
    //             }
    //         }
    //     }

    //     /// <summary>
    //     /// If you use the 2nd generic as tag component, this can remove them all at once.
    //     /// </summary>
    //     public void RemoveAllTags(EntityCommandBuffer ecb)
    //     {
    //         foreach(var tag in SecondaryData)
    //         {
    //         }
    //     }

    //     /// <summary>
    //     /// Make the group returns just the data that changed.
    //     /// </summary>
    //     public void SetFilterChangedOndata() 
    //     {
    //         Query.SetFilterChanged(new ComponentType[] { ComponentType.Create<D1>(), ComponentType.Create<D2>() });
    //     }

    //     public static implicit operator ComponentType[] (ECSQuery<D1, D2> s) => new ComponentType[] { ComponentType.ReadOnly<D1>(), ComponentType.ReadOnly<D2>() };
    //     public ComponentType[] Writable => new ComponentType[] { ComponentType.ReadOnly<D1>(), ComponentType.Create<D2>() };
    // }

}