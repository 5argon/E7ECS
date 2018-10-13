#define I_WANNA_CREATE_MY_OWN_WORLDS_BUT_GIVE_ME_THOSE_HOOKS

using Unity.Entities;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Experimental.PlayerLoop;
using System.Collections.Generic;
using UnityEngine.Jobs;
using System.Reflection;
using System;

namespace E7.ECS
{
    /// <summary>
    /// Its purpose of existence is just to increase the global version number on manual update.
    /// </summary>
    [DisableAutoCreation]
    internal class VersionBumperSystem : ComponentSystem
    {
        protected override void OnCreateManager() => this.Enabled = false;
        public void BumpVersion()
        {
            this.Enabled = true;
            Update();
            this.Enabled = false;
        }
        protected override void OnUpdate() {}
    }

    public static class WorldHelper
    {
        public static void IncreaseVersion()
        {
            World.Active.GetOrCreateManager<VersionBumperSystem>().BumpVersion();
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
                if(!t.IsSubclassOf(typeof(ComponentSystemBase)))
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