#define I_AM_WORRIED_ABOUT_EXECEPTION_PERFORMANCE

using UnityEngine;
using Unity.Entities;
using Unity.Collections;
using System.Collections.Generic;

namespace E7.ECS
{
    [UpdateBefore(typeof(DestroyMessageSystem))]
    public abstract class ReactiveMonoCS<ReactiveGroup, MonoComponent> : ReactiveCS<ReactiveGroup>
    where ReactiveGroup : struct, IMessageGroup
    where MonoComponent : Component
    {
        protected struct MonoGroup
        {
            [ReadOnly] public ComponentArray<MonoComponent> monoComponents;
            public readonly int Length;
        }
        [Inject] private protected MonoGroup monoGroup;

        /// <summary>
        /// Get the first `MonoBehaviour` captured. Useful when you know there's only one in the scene to take all the reactive actions.
        /// </summary>
        protected MonoComponent FirstMono
#if !I_AM_WORRIED_ABOUT_EXECEPTION_PERFORMANCE
        => monoGroup.Length > 0 ? monoGroup.monoComponents[0] : throw new System.Exception($"You don't have any {typeof(MonoComponent).Name} which has GameObjectEntity attached...");
#else
        => monoGroup.monoComponents[0];
#endif

        /// <summary>
        /// Iterate on all `MonoBehaviour` captured.
        /// </summary>
        protected IEnumerable<MonoComponent> MonoComponents
        {
            get
            {
                for (int i = 0; i < monoGroup.Length; i++)
                {
                    yield return monoGroup.monoComponents[i];
                }
            }
        }

        protected ComponentArray<MonoComponent> MonoComponentArray => monoGroup.monoComponents;
    }
}