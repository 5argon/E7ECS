#define I_AM_WORRIED_ABOUT_EXECEPTION_PERFORMANCE

using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using System.Collections.Generic;

namespace E7.ECS
{
    public abstract class ReactiveJCS<ReactiveGroup> : JobComponentSystem
    where ReactiveGroup : struct, IMessageGroup
    {
        protected abstract ComponentType[] ReactsTo { get; }

        private Dictionary<int, ComponentGroup> allInjects;

        protected override void OnCreateManager()
        {
            var types = ReactsTo;
            allInjects = new Dictionary<int, ComponentGroup>();
            for (int i = 0; i < types.Length; i++)
            {
                allInjects.Add(types[i].TypeIndex, GetComponentGroup(types[i], ComponentType.ReadOnly<ReactiveGroup>()));
            }
        }

        protected ComponentDataArray<T> GetReactions<T>() where T : struct, IMessage
        {
            return allInjects[TypeManager.GetTypeIndex<T>()].GetComponentDataArray<T>();
        }

        protected abstract JobHandle OnReaction(JobHandle inputDeps);

        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            var jobHandle = OnReaction(inputDeps);
            return jobHandle;
        }
    }

    [UpdateBefore(typeof(Initialization))]
    public class DestroyMessageSystem : ComponentSystem
    {
        public struct MessageEntity : ISharedComponentData { }

        struct AllMessages
        {
            [ReadOnly] public SharedComponentDataArray<MessageEntity> reactiveEntities;
            [ReadOnly] public EntityArray entities;
            public readonly int GroupIndex;
        }
        [Inject] AllMessages allReactives;


        protected override void OnUpdate()
        {
            EntityManager.DestroyEntity(ComponentGroups[allReactives.GroupIndex]);
        }
    }

    public abstract class ReactiveCSBase<ReactiveGroup> : JobComponentSystem
    where ReactiveGroup : struct, IMessageGroup
    {
        private protected abstract IMessageInjectGroup<ReactiveGroup> InjectedReactivesInGroup { get; }

        protected virtual void OnBeforeAllMessages() { }
        protected virtual JobHandle OnAfterAllMessages(JobHandle inputDeps) => inputDeps;

        protected abstract void OnReaction();
        protected override JobHandle OnUpdate(JobHandle inputDeps)
        {
            //There is a possibility that we have a mono entity but not any reactive entities in `ReactiveMonoCS`.
            //Debug.Log("REACTIVE LENGTH " + InjectedReactivesInGroup.Entities.Length);
            // try
            // {
            OnBeforeAllMessages();
            for (int i = 0; i < InjectedReactivesInGroup.Entities.Length; i++)
            {
                iteratingEntity = InjectedReactivesInGroup.Entities[i];
                OnReaction();
            }
            return OnAfterAllMessages(inputDeps);
            // }
            // catch (System.InvalidOperationException)
            // {
            // Debug.LogError("Did you use EntityManager and invalidate the injected array? Group : " + typeof(ReactiveGroup).Name);
            // throw;
            // }
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

    public static class MessageUtility
    {
        /// <summary>
        /// You can manually inject only one type of message of the group with the output of this to GetComponentGroup
        /// </summary>
        public static ComponentType[] GetMessageTypes<Message, MessageGroup>()
        where Message : struct, IMessage
        where MessageGroup : struct, IMessageGroup
        {
            return new ComponentType[] {
                ComponentType.ReadOnly<Message>(),
                ComponentType.ReadOnly<MessageGroup>(),
                ComponentType.ReadOnly<DestroyMessageSystem.MessageEntity>()
            };
        }
    }

    /// <summary>
    /// Get all of entity made from `MonoECS.Issue` and `EntityCommandBuffer.Issue` with reactive components.
    /// 
    /// Process each reactive entities captured in this frame one by one with 
    /// `OnReaction`, all of them will be destroyed automatically. (Runs only once)
    /// </summary>
    public abstract class ReactiveCS<ReactiveGroup> : ReactiveCSBase<ReactiveGroup>
    where ReactiveGroup : struct, IMessageGroup
    {
        /// <summary>
        /// Captures reactive entities ready to be destroy after the task.
        /// </summary>
        protected struct ReactiveInjectGroup : IMessageInjectGroup<ReactiveGroup>
        {
            [ReadOnly] public SharedComponentDataArray<ReactiveGroup> reactiveGroups;
            [ReadOnly] public SharedComponentDataArray<DestroyMessageSystem.MessageEntity> reactiveEntityTag;
            public EntityArray entities;
            public readonly int Length;

            public SharedComponentDataArray<ReactiveGroup> MessageGroups => reactiveGroups;
            public SharedComponentDataArray<DestroyMessageSystem.MessageEntity> MessageEntity => reactiveEntityTag;
            public EntityArray Entities => entities;
        }
        [Inject] private protected ReactiveInjectGroup injectedReactivesInGroup;

        private protected override IMessageInjectGroup<ReactiveGroup> InjectedReactivesInGroup => injectedReactivesInGroup;
    }

    /// <summary>
    /// Get all of one type of your `MonoBehaviour` that you have `GameObjectEntity` attached. 
    /// Then also get all of entity made from `MonoECS.Issue` and `EntityCommandBuffer.Issue` with reactive components.
    /// Your `MonoBehaviour` can then take action on them.
    /// 
    /// Process each reactive entities captured in this frame one by one with
    /// `OnReaction`, all of them will be destroyed automatically. (Runs only once)
    /// </summary>
    public abstract class ReactiveMonoCS<ReactiveGroup, MonoComponent> : ReactiveCS<ReactiveGroup>
    where ReactiveGroup : struct, IMessageGroup
    where MonoComponent : Component
    {
        /// <summary>
        /// Captures your `MonoBehaviour`s
        /// </summary>
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

    public abstract class TagResponseJCSBase<TagComponent> : JobComponentSystem
    where TagComponent : struct, IComponentData, ITag
    {
        protected abstract ITagResponseInjectGroup<TagComponent> InjectedGroup { get; }

        [Inject] private protected EndFrameBarrier efb;

        /// <summary>
        /// Tags are destroyed at EndFrameBarrier, not immediately.
        /// </summary>
        protected void EndAllInjectedTagResponse()
        {
            var ecb = efb.CreateCommandBuffer();
            for (int i = 0; i < InjectedGroup.Entities.Length; i++)
            {
                ecb.RemoveComponent<TagComponent>(InjectedGroup.Entities[i]);
            }
        }
    }

    /// <summary>
    /// When you want to make a reactive system with additional data on that entity.
    /// Take the content out before sending them to the job so that `data` can be written to.
    /// Use `InjectedGroup` to get the data.
    /// </summary>
    public abstract class TagResponseDataJCS<TagComponent, DataComponent> : TagResponseJCSBase<TagComponent>
    where TagComponent : struct, IComponentData, ITag
    where DataComponent : struct, IComponentData
    {
        protected struct InjectGroup : ITagResponseDataInjectGroup<TagComponent, DataComponent>
        {
            [ReadOnly] public ComponentDataArray<TagComponent> reactiveComponents;
            [ReadOnly] public EntityArray entities;
            public ComponentDataArray<DataComponent> datas { get; }
            public readonly int Length;

            public ComponentDataArray<TagComponent> TagComponents => reactiveComponents;
            public EntityArray Entities => entities;
        }

        [Inject] private protected InjectGroup injectedGroup;
        protected override ITagResponseInjectGroup<TagComponent> InjectedGroup => injectedGroup;
    }
}