using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine.Jobs;

namespace E7.ECS
{
    /// <summary>
    /// Message will be destroyed all at <see cref="InitializationSystemGroup">
    /// </summary>
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public class DestroyMessageSystem : ComponentSystem
    {
        public struct MessageEntity : IComponentData { }

        EntityQuery cg;
        protected override void OnCreateManager()
        {
            cg = GetEntityQuery(ComponentType.ReadOnly<MessageEntity>());
        }

        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(cg);
        }
    }
}