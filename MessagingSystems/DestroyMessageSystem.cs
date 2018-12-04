#define I_AM_WORRIED_ABOUT_EXECEPTION_PERFORMANCE

using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine.Jobs;

namespace E7.ECS
{
    public class DestroyMessageSystem : ComponentSystem
    {
        public struct MessageEntity : IComponentData { }

        ComponentGroup cg;
        protected override void OnCreateManager()
        {
            cg = GetComponentGroup(ComponentType.Create<MessageEntity>());
        }

        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(cg);
        }
    }
}