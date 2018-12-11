#define I_WANNA_CREATE_MY_OWN_WORLDS_BUT_GIVE_ME_THOSE_HOOKS

using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using System.Collections.Generic;
using UnityEngine.Jobs;
using Unity.Transforms;
using Unity.Rendering;
using System.Reflection;
using System;
using System.Linq;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace E7.ECS
{
    public static class WorldHelper
    {
#if UNITY_EDITOR
        [MenuItem("CONTEXT/MeshInstanceRendererComponent/Copy from MeshRenderer and MeshFilter", false, 150)]
        static void CopyTransform()
        {
            foreach (var go in Selection.gameObjects)
            {
                var mir = go.GetComponent<MeshInstanceRendererComponent>();
                var mr = go.GetComponent<MeshRenderer>();
                var mf = go.GetComponent<MeshFilter>();
                var v = mir.Value;
                if (mr != null)
                {
                    v.material = mr.sharedMaterial;
                }
                if (mf != null)
                {
                    v.mesh = mf.sharedMesh;
                }
                mir.Value = v;
            }
        }
#endif
        private const int noPayloadSize = 24;
        private const int withPayloadSize = 32;
        private const int multipleOf = 1024;
        public static int CalculateMinimumChunkSize(int createInstantiateDestroy, int remove, params (int sizeOf, int count)[] addSet)
        {
            var exactSize =
            (createInstantiateDestroy * noPayloadSize) +
            (remove * withPayloadSize) +
            addSet.Sum(x => (withPayloadSize + x.sizeOf) * x.count);

            return (int)(math.ceil(exactSize / (float)multipleOf) * multipleOf);
        }

        public static T LateInject<T>(this World world, T w) where T : ScriptBehaviourManager
        {
            if (w == null)
            {
                return world.GetOrCreateManager<T>();
            }
            else
            {
                return w;
            }
        }

        /// <summary>
        /// Instantiate the entire GOE tree and do a complete Attach with the top most level replaced by Entity of choice.
        /// Returned NHM maps entity on your template GOE to the instantiated entities. The other one is for iteration, you may
        /// want to add some more components.
        /// Please dispose both native containers.
        /// </summary>
        public static  void TraverseAttachInstantiate(GameObjectEntity topLevelGoe, Entity parent, EntityManager em, 
        out NativeHashMap<Entity, Entity> remapper, out NativeArray<Entity> instantiated)
        {
            // EntityRemapUtility.AddEntityRemapping(
            List<(Entity, Entity)> remapList = new List<(Entity, Entity)>();
            EntityArchetype attach = em.CreateArchetype(ComponentType.Create<Attach>());
            Stack<Entity> entityStack = new Stack<Entity>();

            //Top level GOE not instantiated, you can use any external parent.
            entityStack.Push(parent);
            DoAllImmediateChildren(topLevelGoe);

            remapper = new NativeHashMap<Entity, Entity>(remapList.Count, Allocator.Persistent);
            instantiated = new NativeArray<Entity>(remapList.Count, Allocator.Persistent);

            for (int i = 0; i < remapList.Count; i++)
            {
                remapper.TryAdd(remapList[i].Item1, remapList[i].Item2);
                instantiated[i] = remapList[i].Item2;
            }
            //Debug.Log($"Made {remapper.Length} entities!");

            return;

            void DoAllImmediateChildren(GameObjectEntity goe)
            {
                //For all child,
                var childCount = goe.transform.childCount;
                for (int i = 0; i < childCount; i++)
                {
                    var child = goe.transform.GetChild(i);

                    //Skips the entire tree of inactive game object.
                    if(!child.gameObject.activeInHierarchy) continue;

                    var childGoe = child.GetComponent<GameObjectEntity>();
                    if (childGoe == null)
                    {
                        throw new ArgumentException($"Object {child.name} does not have GameObjectEntity. You must attach GOE on everything in the tree.");
                    }

                    //Copy child transforms first
                    var pos = childGoe.GetComponent<PositionComponent>();
                    var rot = childGoe.GetComponent<RotationComponent>();
                    var scale = childGoe.GetComponent<ScaleComponent>();
                    if (pos != null)
                        pos.Value = new Position { Value = child.localPosition };
                    if (rot != null)
                        rot.Value = new Rotation { Value = child.localRotation };
                    if (scale != null)
                        scale.Value = new Scale { Value = child.localScale };

                    //Instantiate each child along with all wrappers.
                    Entity instantiatedChild = em.Instantiate(childGoe.Entity);
                    remapList.Add((childGoe.Entity, instantiatedChild));

                    //Create attach side-channel entity for self and instantiated parent
                    Entity att = em.CreateEntity(attach);
                    em.SetComponentData(att, new Attach() { Parent = entityStack.Peek(), Child = instantiatedChild });
                    //Debug.Log($"{instantiatedChild} from {child.name} -> Attached to {entityStack.Peek()} ({child.parent.name})");

                    //Prepare itself to be a parent of any childs
                    entityStack.Push(instantiatedChild);
                    DoAllImmediateChildren(childGoe);
                    //When done, the next sibling should be able to peek the parent of this one too.
                    entityStack.Pop();
                }
            }
        }

        public static void IncreaseVersion()
        {
            World.Active.GetOrCreateManager<VersionBumperSystem>().BumpVersion();
        }

        public static void CopyAllEntities(World fromWorld, World toWorld)
        {
            var ecs = fromWorld.CreateManager<EntityCloningSystem>();
            ecs.CloneTo(toWorld);
            fromWorld.DestroyManager(ecs);
        }

        public static World CreateWorld(string name, params Type[] systemTypes)
        {
            World w = new World(name);
            return AddSystemsToWorld(w, systemTypes);
        }

        public static World AddSystemsToWorld(World w, params Type[] systemTypes)
        {
            foreach (var t in systemTypes)
            {
                if (!t.IsSubclassOf(typeof(ComponentSystemBase)))
                {
                    throw new ArgumentException($"Hawawa! This type {t.Name} is not a system!");
                }
                w.GetOrCreateManager(t);
            }
            return w;
        }

        /// <summary>
        /// PlayerLoop will be DEFAULT player loop + your world. Any modification to the current loop are lost.
        /// </summary>
        public static void UseWorldAndSetAsActive(World w)
        {
            if (w != null)
            {
                World.Active = w;
                ScriptBehaviourUpdateOrder.UpdatePlayerLoop(w);
            }
            else
            {
                throw new Exception("You cannot use a null world..");
            }
        }

        /// <summary>
        /// PlayerLoop will be back to default one.
        /// </summary>
        public static void DisposeAndStopUsingWorld(World w)
        {
            if (w != null)
            {
                World.Active = null;
                w.Dispose();
                ScriptBehaviourUpdateOrder.UpdatePlayerLoop();
            }
        }

        static void DomainUnloadShutdown()
        {
            World.DisposeAllWorlds();
            ScriptBehaviourUpdateOrder.UpdatePlayerLoop();
        }

#if I_WANNA_CREATE_MY_OWN_WORLDS_BUT_GIVE_ME_THOSE_HOOKS
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        public static void RegisterHybridHooks()
        {
            var typeInTheCorrectAssembly = typeof(GameObjectArray);
            var hybridHooks = new System.Type[]{
                typeInTheCorrectAssembly.Assembly.GetType("Unity.Entities.GameObjectArrayInjectionHook"),
                typeInTheCorrectAssembly.Assembly.GetType("Unity.Entities.TransformAccessArrayInjectionHook"),
                typeInTheCorrectAssembly.Assembly.GetType("Unity.Entities.ComponentArrayInjectionHook"),
            };
            foreach (var hook in hybridHooks)
            {
                InjectionHookSupport.RegisterHook(System.Activator.CreateInstance(hook) as InjectionHook);
            }

            PlayerLoopManager.RegisterDomainUnload(DomainUnloadShutdown, 10000);
        }
    }
}