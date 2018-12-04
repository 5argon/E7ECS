#define I_AM_WORRIED_ABOUT_EXECEPTION_PERFORMANCE

using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using Unity.Burst;
using UnityEngine.Jobs;

namespace E7.ECS
{
    //[UpdateBefore(typeof(EndFrameBarrier))]
    [UpdateBefore(typeof(DestroyMessageBarrier))]
    public class DestroyMessageSystem : JobComponentSystem
    {
        public class DestroyMessageBarrier : BarrierSystem { }
        [Inject] DestroyMessageBarrier barrier;

        public struct MessageEntity : IComponentData { }

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            return new DestroyMessagesJob(){
                command = barrier.CreateCommandBuffer().ToConcurrent(),
            }.Schedule(this, inputDeps);
        }

        [BurstCompile]
        struct DestroyMessagesJob : IJobProcessComponentDataWithEntity<MessageEntity>
        {
            public EntityCommandBuffer.Concurrent command;
            public void Execute(Entity e, int i, [ReadOnly] ref MessageEntity me)
            {
                command.DestroyEntity(i, e);
            }
        }
    }
}