using Unity.Entities;
using Unity.Collections;

namespace E7.ECS
{
    public static class MessageEMExtension
    {
        /// <summary>
        /// If you just loop and destroy each one in EntityArray without a command buffer, it will cause problems mid-loop!
        /// </summary>
        public static void DestroyAllInEntityArray(this EntityManager em, EntityArray ea)
        {
            using (var na = new NativeArray<Entity>(ea.Length, Allocator.Temp))
            {
                ea.CopyTo(na, 0);
                for (int i = 0; i < na.Length; i++)
                {
                    em.DestroyEntity(na[i]);
                }
            }
        }

        /// <summary>
        /// Now this supports zero-sized component.
        /// </summary>
        public static void UpsertComponentData<T>(this EntityManager em, Entity entity) where T : struct, IComponentData
        {
            if (em.HasComponent<T>(entity) == false)
            {
                em.AddComponent(entity,typeof(T));
            }
        }

        public static void UpsertComponentData<T>(this EntityManager em, Entity entity, T tagContent) where T : struct, IComponentData
        {
            if (em.HasComponent<T>(entity) == false)
            {
                em.AddComponentData<T>(entity, tagContent);
            }
            else
            {
                em.SetComponentData<T>(entity, tagContent);
            }
        }

        public static bool TryGetComponent<T>(this EntityManager em, Entity entity, out T componentData) where T : struct, IComponentData
        {
            if(em.HasComponent<T>(entity))
            {
                componentData = em.GetComponentData<T>(entity);
                return true;
            }
            else
            {
                componentData = default;
                return false;
            }
        }

        public static MessageArchetype CreateMessageArchetype<MessageComponent, MessageGroup>(this EntityManager em)
        where MessageComponent : struct, IMessage
        where MessageGroup : struct, IMessageGroup
        {
            return new MessageArchetype
            {
                archetype = em.CreateArchetype(
                ComponentType.ReadOnly<MessageComponent>(),
                ComponentType.ReadOnly<MessageGroup>(),
                ComponentType.ReadOnly<DestroyMessageSystem.MessageEntity>()
                )
            };
        }

        public static void Message<MessageComponent, MessageGroup>(this EntityManager ecb)
        where MessageComponent : struct, IMessage
        where MessageGroup : struct, IMessageGroup
        => Message<MessageComponent, MessageGroup>(ecb, default, default);

        public static void Message<MessageComponent, MessageGroup>(this EntityManager ecb, MessageComponent rx)
        where MessageComponent : struct, IMessage
        where MessageGroup : struct, IMessageGroup
        => Message<MessageComponent, MessageGroup>(ecb, rx, default);

        private static void Message<MessageComponent, MessageGroup>(this EntityManager ecb, MessageComponent rx, MessageGroup rg)
        where MessageComponent : struct, IMessage
        where MessageGroup : struct, IMessageGroup
        {
            //Debug.Log($"Issuing {typeof(ReactiveComponent).Name} (ECB)");
            var e = ecb.CreateEntity();
            ecb.AddComponentData<MessageComponent>(e, rx);
            ecb.AddComponentData<MessageGroup>(e, rg);
            ecb.AddComponentData<DestroyMessageSystem.MessageEntity>(e, default);
        }
    }
}