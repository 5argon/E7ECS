using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine.Jobs;

namespace E7.ECS
{
    /// <summary>
    /// Any message user should explicitly specify that they run BEFORE this system.
    /// </summary>
    public class DestroyMessageSystem : ComponentSystem
    {
        public struct MessageEntity : IComponentData { }

        ComponentGroup cg;
        protected override void OnCreateManager()
        {
            cg = GetComponentGroup(ComponentType.ReadOnly<MessageEntity>());
        }

        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(cg);
        }
    }
}