#define I_AM_WORRIED_ABOUT_EXECEPTION_PERFORMANCE

using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

namespace E7.ECS
{
    [UpdateBefore(typeof(DestroyMessageSystem))]
    public abstract class ReactiveCS<MESSAGEGROUP> : JobComponentSystem
    where MESSAGEGROUP : struct, IMessageGroup
    {
        ComponentGroup messageGroup;
        protected override void OnCreateManager()
        {
            messageGroup = GetComponentGroup(
                ComponentType.ReadOnly<MESSAGEGROUP>(),
                ComponentType.ReadOnly<DestroyMessageSystem.MessageEntity>()
            );
        }

        protected virtual void OnBeforeAllMessages() { }
        protected virtual JobHandle OnAfterAllMessages(JobHandle inputDeps) => inputDeps;

        protected abstract void OnReaction();
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            using (var na = messageGroup.ToEntityArray(Allocator.Temp))
            {
                for (int i = 0; i < na.Length; i++)
                {
                    iteratingEntity = na[i];
                    OnReaction();
                }
            }
            return OnAfterAllMessages(inputDeps);
        }

        private protected Entity iteratingEntity;

        /// <summary>
        /// Use this overload if you are not going to use the `out` variable. (empty message content) Saves you a `GetComponentData`.
        /// </summary>
        protected bool ReactsTo<T>() where T : struct, IMessage => EntityManager.HasComponent<T>(iteratingEntity);

        protected bool ReactsTo<T>(out T reactiveComponent) where T : struct, IMessage
        {
            //Debug.Log("Checking with " + typeof(T).Name);
            if (ReactsTo<T>())
            {
                reactiveComponent = EntityManager.GetComponentData<T>(iteratingEntity);
                return true;
            }
            reactiveComponent = default;
            return false;
        }
    }
}