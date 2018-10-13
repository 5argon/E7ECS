using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using System.Collections.Generic;

namespace E7.ECS
{
    /// <summary>
    /// Just a wrapper that this is not an ordinary archetype
    /// </summary>
    public struct MessageArchetype
    {
        public EntityArchetype archetype;
    }

    public static class EntityManagerExtension
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

        /// <summary>
        /// Removes a tag component if it is there.
        /// </summary>
        public static void RemoveTag<T>(this EntityManager em, Entity entity) where T : struct, IComponentData, ITag
        {
            if (em.HasComponent<T>(entity))
            {
                em.RemoveComponent<T>(entity);
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

        public static void Message<MessageComponent, MessageGroup>(this EntityManager ecb, MessageComponent rx, MessageGroup rg)
        where MessageComponent : struct, IMessage
        where MessageGroup : struct, IMessageGroup
        {
            //Debug.Log($"Issuing {typeof(ReactiveComponent).Name} (ECB)");
            var e = ecb.CreateEntity();
            ecb.AddComponentData<MessageComponent>(e, rx);
            ecb.AddSharedComponentData<MessageGroup>(e, rg);
            //TODO : Create an archetype that has this because we always need this...
            ecb.AddSharedComponentData<DestroyMessageSystem.MessageEntity>(e, default);
        }
    }

    public static class EntityCommandBufferExtension
    {
        /// <summary>
        /// Please use archetype from EntityManager.CreateReactiveArchetype! 
        /// Creating will be faster but we still have to SetComponent once.
        /// </summary>
        public static void Message<MessageComponent>(this EntityCommandBuffer.Concurrent ecb, int jobIndex, MessageArchetype msa, MessageComponent rx)
        where MessageComponent : struct, IMessage
        {
            Message(ecb, jobIndex, msa);
            ecb.SetComponent(jobIndex, rx);
        }

        /// <summary>
        /// Please use archetype from EntityManager.CreateReactiveArchetype! 
        /// Creating will be faster but we still have to SetComponent once.
        /// </summary>
        public static void Message(this EntityCommandBuffer.Concurrent ecb, int jobIndex, MessageArchetype msa)
        {
            ecb.CreateEntity(jobIndex, msa.archetype);
        }

        /// <summary>
        /// Please use archetype from EntityManager.CreateReactiveArchetype! 
        /// Creating will be faster but we still have to SetComponent once.
        /// </summary>
        public static void Message<MessageComponent>(this EntityCommandBuffer ecb, MessageArchetype msa, MessageComponent rx)
        where MessageComponent : struct, IMessage
        {
            Message(ecb, msa);
            ecb.SetComponent(rx);
        }

        /// <summary>
        /// Please use archetype from EntityManager.CreateReactiveArchetype! 
        /// </summary>
        public static void Message(this EntityCommandBuffer ecb, MessageArchetype msa)
        {
            ecb.CreateEntity(msa.archetype);
        }

        /// <summary>
        /// No upsert check!
        /// Be careful not to add duplicate tags!
        /// </summary>
        public static void AddTag<T>(this EntityCommandBuffer ecb, Entity addToEntity)
        where T : struct, IComponentData, ITag
        => AddTag<T>(ecb, addToEntity, default(T));

        public static void AddTag<T>(this EntityCommandBuffer ecb, Entity addToEntity, T data)
        where T : struct, IComponentData, ITag
        {
            ecb.AddComponent<T>(addToEntity, data);
        }

        public static void AddTag<T>(this EntityCommandBuffer.Concurrent ecb, int jobIndex, Entity addToEntity)
        where T : struct, IComponentData, ITag
        => AddTag<T>(ecb, jobIndex, addToEntity, default(T));

        public static void AddTag<T>(this EntityCommandBuffer.Concurrent ecb, int jobIndex, Entity addToEntity, T data)
        where T : struct, IComponentData, ITag
        {
            ecb.AddComponent<T>(jobIndex, addToEntity, data);
        }

        /// <summary>
        /// Determine whether it is an Add or Set command based on if it currently has a component at the time of calling this or not.
        /// </summary>
        public static void AddTag<T>(this EntityCommandBuffer ecb, Entity addToEntity, EntityManager em)
        where T : struct, IComponentData, ITag
        => AddTag<T>(ecb, addToEntity, default, em);

        /// <summary>
        /// Determine whether it is an Add or Set command based on if it currently has a component at the time of calling this or not.
        /// </summary>
        public static void AddTag<T>(this EntityCommandBuffer ecb, Entity addToEntity, T data, EntityManager em)
        where T : struct, IComponentData, ITag
        {
            //Debug.Log($"Adding tag " + typeof(T).Name);
            if (em.HasComponent<T>(addToEntity) == false)
            {
                //Debug.Log($"Choose to add {addToEntity.Index}");
                ecb.AddComponent<T>(addToEntity, data);
            }
            else
            {
                //Debug.Log($"Choose to set! {addToEntity.Index}");
                ecb.SetComponent<T>(addToEntity, data);
            }
        }

        /// <summary>
        /// An overload suitable to use with system with EntityManager.
        /// Contains HasComponent check.
        /// </summary>
        public static void RemoveTag<ReactiveComponent>(this EntityCommandBuffer ecb, Entity e, EntityManager em)
        where ReactiveComponent : struct, IComponentData, ITag
        {
            if (em.HasComponent<ReactiveComponent>(e))
            {
                RemoveTag<ReactiveComponent>(ecb, e);
            }
        }


        /// <summary>
        /// End a tag response routine by removing a component from an entity. You must specify a reactive component type manually.
        /// </summary>
        public static void RemoveTag<ReactiveComponent>(this EntityCommandBuffer ecb, Entity e)
        where ReactiveComponent : struct, IComponentData, ITag
        {
            ecb.RemoveComponent<ReactiveComponent>(e);
        }
    }

    public static class ComponentDataArrayExtension
    {
        /// <summary>
        /// Like `EntityArray.ToArray` but for CDA.
        /// </summary>
        public static List<T> CopyToList<T>(this ComponentDataArray<T> cda) where T : struct, IComponentData
        {
            using (var na = new NativeArray<T>(cda.Length, Allocator.Temp))
            {
                cda.CopyTo(na);
                List<T> list = new List<T>(na);
                return list;
            }
        }

        /// <summary>
        /// Like `EntityArray.ToArray` but for CDA.
        /// </summary>
        public static List<T> CopyToList<T>(this ComponentDataArray<T> cda, IComparer<T> sorting) where T : struct, IComponentData
        {
            var list = CopyToList<T>(cda);
            list.Sort(sorting);
            return list;
        }
    }
}