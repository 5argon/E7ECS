#define I_AM_WORRIED_ABOUT_EXECEPTION_PERFORMANCE

using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;

namespace E7.ECS
{
    [UpdateBefore(typeof(DestroyMessageSystem))]
    public abstract class ReactiveCS<ReactiveGroup> : JobComponentSystem
    where ReactiveGroup : struct, IMessageGroup
    {
        protected virtual void OnBeforeAllMessages() { }
        protected virtual JobHandle OnAfterAllMessages(JobHandle inputDeps) => inputDeps;

        protected abstract void OnReaction();
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            OnBeforeAllMessages();
            for (int i = 0; i < InjectedReactivesInGroup.Entities.Length; i++)
            {
                iteratingEntity = InjectedReactivesInGroup.Entities[i];
                OnReaction();
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
        /// <summary>
        /// Captures reactive entities ready to be destroy after the task.
        /// </summary>
        protected struct ReactiveInjectGroup : IMessageInjectGroup<ReactiveGroup>
        {
            [ReadOnly] public ComponentDataArray<ReactiveGroup> reactiveGroups;
            [ReadOnly] public ComponentDataArray<DestroyMessageSystem.MessageEntity> reactiveEntityTag;
            public EntityArray entities;
            public readonly int Length;

            public ComponentDataArray<ReactiveGroup> MessageGroups => reactiveGroups;
            public ComponentDataArray<DestroyMessageSystem.MessageEntity> MessageEntity => reactiveEntityTag;
            public EntityArray Entities => entities;
        }
        [Inject] private protected ReactiveInjectGroup injectedReactivesInGroup;

        private protected ReactiveInjectGroup InjectedReactivesInGroup => injectedReactivesInGroup;
    }
}