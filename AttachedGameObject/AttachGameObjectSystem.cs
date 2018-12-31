using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Rendering;
using UnityEngine;
using System.Collections.Generic;
using Unity.Jobs;
using UnityEngine.Jobs;
using Unity.Burst;
using Unity.Mathematics;

namespace E7.ECS.AttachGameObject
{

    /// <summary>
    /// Instantiate game object at the root level based on one Entity.
    /// Removing the entity will destroy that game object.
    /// That game object is invisible in the scene.
    /// 
    /// The inactive is achieved by setting `.enabled` to false/true on all components except GOE.
    /// </summary>
    public class AttachGameObjectSystem : ComponentSystem
    {
        private struct InactiveState : ISystemStateComponentData { }

        /// <summary>
        /// We would like to know when the child is going away by itself
        /// by checking on this remaining state vs. disappeared AGOParent.
        /// </summary>
        private struct RegisteredState : ISystemStateComponentData { }

        private NativeMultiHashMap<Entity, Entity> parentToChild;

        ComponentGroup registerSideChannelGroup;
        ComponentGroup registeredGroup;
        // ComponentGroup toInactiveGroup;
        // ComponentGroup toActiveGroup;

        protected override void OnCreateManager()
        {
            parentToChild = new NativeMultiHashMap<Entity, Entity>(1024, Allocator.Persistent);

            registerSideChannelGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]{
                    ComponentType.Create<AttachGameObject>(),
                },
                Any = new ComponentType[]{
                },
                None = new ComponentType[]{
                },
            });

            registeredGroup = GetComponentGroup(new EntityArchetypeQuery
            {
                All = new ComponentType[]{
                    //Check if its parent still exist?
                },
                Any = new ComponentType[]{
                    ComponentType.Create<AGOParent>(),
                    ComponentType.Create<RegisteredState>(),
                },
                None = new ComponentType[]{
                },
            });



            // //Fix non-update bug
            // GetArchetypeChunkSharedComponentType<AttachGameObject>();
            // GetArchetypeChunkComponentType<AGOParent>(isReadOnly: true);
        }

        protected override void OnDestroyManager()
        {
            parentToChild.Dispose();
        }

        public GameObjectEntity GetCorrespondingFirst(Entity e)
        {
            parentToChild.TryGetFirstValue(e, out Entity firstChild, out var iterator);
            return EntityManager.GetComponentObject<GameObjectEntity>(firstChild);
        }

        /// <summary>
        /// Just get rid of entity will normally destroy the corresponding object.
        /// However in the Attach case you will bug out the TransformSystem since it is expecting
        /// the child to go away first, unregister, then you can take the parent away.
        /// 
        /// By using this you can force destroy the child first even if the parent is still there.
        /// Make sure to update the transform system once before destroying the parent next.
        /// </summary>
        public void DestroyAllCorresponding(Entity e)
        {
            List<GameObjectEntity> list = new List<GameObjectEntity>();
            GetCorrespondingAll(e, list);
            foreach (var item in list)
            {
                GameObject.Destroy(item.gameObject);
            }
        }

        public void GetCorrespondingAll(Entity e, List<GameObjectEntity> allGoe)
        {
            Entity child;
            if (parentToChild.TryGetFirstValue(e, out child, out var iterator))
            {
                do
                {
                    var goe = EntityManager.GetComponentObject<GameObjectEntity>(child);
                    allGoe.Add(goe);
                }
                while (parentToChild.TryGetNextValue(out child, ref iterator));
            }
        }

        ArchetypeChunkEntityType et;
        //ArchetypeChunkComponentType<RegisteredState> registerType;
        ArchetypeChunkComponentType<InactiveState> inactiveType;
        ArchetypeChunkSharedComponentType<AttachGameObject> attachType;
        ComponentDataFromEntity<AGOInactive> attachInactiveCdfe;
        // private void Gather()
        // {
        //     et = GetArchetypeChunkEntityType();
        //     //registerType = GetArchetypeChunkComponentType<RegisteredState>(isReadOnly: true);
        //     inactiveType = GetArchetypeChunkComponentType<InactiveState>(isReadOnly: true);
        //     attachType = GetArchetypeChunkSharedComponentType<AttachGameObject>();
        //     attachInactiveType = GetArchetypeChunkComponentType<AGOInactive>(isReadOnly: true);
        // }

        protected override void OnUpdate()
        {
            et = GetArchetypeChunkEntityType();

            List<AttachGameObject> attaches = new List<AttachGameObject>();
            List<int> attachIndexes = new List<int>();
            EntityManager.GetAllUniqueSharedComponentData<AttachGameObject>(attaches, attachIndexes);

            //New register, destroy that side channel entity and create + attach game object.
            using (var aca = registerSideChannelGroup.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                for (int i = 0; i < aca.Length; i++)
                {
                    var ac = aca[i];
                    var ea = ac.GetNativeArray(et).ToArray(); //Prevents invalidation
                    for (int j = 0; j < ea.Length; j++)
                    {
                        Entity disposableEntity = ea[j];
                        var ago = EntityManager.GetSharedComponentData<AttachGameObject>(disposableEntity);

                        GameObject instantiatedGo = GameObject.Instantiate(ago.gameObjectEntity);
                        instantiatedGo.hideFlags = HideFlags.HideAndDontSave; //TODO: Is this correct?

                        //OnEnable activated, can get the entity now because it has GOE
                        Entity goeEntity = instantiatedGo.GetComponent<GameObjectEntity>().Entity;

                        //It will constantly check for its parent's component what should it do to itself.
                        EntityManager.AddComponentData(goeEntity, new AGOParent { parent = ago.parent });
                        EntityManager.AddComponentData(goeEntity, new RegisteredState());
                        //Also parent to child remembered
                        parentToChild.Add(ago.parent, goeEntity);

                        if ((ago.modeFlag & AttachGameObject.ModeFlag.AttachTransform) != 0)
                        {
                            Entity attachEntity = EntityManager.CreateEntity();
                            EntityManager.AddComponentData(attachEntity, new Attach { Parent = ago.parent, Child = goeEntity });
                        }

                        EntityManager.DestroyEntity(disposableEntity);
                    }
                }
            }

            using (var aca = registeredGroup.CreateArchetypeChunkArray(Allocator.TempJob))
            {
                if (aca.Length > 0)
                {
                    List<(GameObject go, Entity children, Entity itsParent)> gameObjectToDestroy = new List<(GameObject go, Entity children, Entity itsParent)>();
                    et = GetArchetypeChunkEntityType();
                    // inactiveType = GetArchetypeChunkComponentType<InactiveState>(isReadOnly: true);

                    var agoParentType = GetArchetypeChunkComponentType<AGOParent>(isReadOnly: true);
                    var registeredType = GetArchetypeChunkComponentType<RegisteredState>(isReadOnly: true);
                    // attachInactiveCdfe = GetComponentDataFromEntity<AGOInactive>(isReadOnly: true);

                    for (int i = 0; i < aca.Length; i++)
                    {
                        var ac = aca[i];

                        var childs = ac.GetNativeArray(agoParentType);
                        var ea = ac.GetNativeArray(et);

                        //We could not skip iterating over any chunk because we must
                        //ask the parent which isn't in this chunk.

                        if (ac.Has(registeredType) && !ac.Has(agoParentType))
                        {
                            //The children disappeared on its own
                            Debug.Log($"Clean up children!!");
                            for (int j = 0; j < ea.Length; j++)
                            {
                                PostUpdateCommands.RemoveComponent<RegisteredState>(ea[j]);
                            }
                        }
                        else
                        {
                            for (int j = 0; j < ea.Length; j++)
                            {
                                //Parent disappeared, destroy children.
                                bool parentExist = EntityManager.Exists(childs[j].parent);
                                if (!parentExist)
                                {
                                    if (EntityManager.HasComponent<Transform>(ea[j]))
                                    {
                                        var transform = EntityManager.GetComponentObject<Transform>(ea[j]);
                                        gameObjectToDestroy.Add((transform.gameObject, ea[j], childs[j].parent));
                                    }
                                    else if (EntityManager.HasComponent<RectTransform>(ea[j]))
                                    {
                                        var rectTransform = EntityManager.GetComponentObject<RectTransform>(ea[j]);
                                        gameObjectToDestroy.Add((rectTransform.gameObject, ea[j], childs[j].parent));
                                    }
                                    if (ac.Has(registeredType))
                                    {
                                        PostUpdateCommands.RemoveComponent<RegisteredState>(ea[j]);
                                    }
                                }
                            }
                        }
                    }
                    foreach (var g in gameObjectToDestroy)
                    {
                        GameObject.Destroy(g.go);
                        parentToChild.Remove(g.itsParent);

                    }
                }
            }
        }
    }
}
