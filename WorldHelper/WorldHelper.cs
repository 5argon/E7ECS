using Unity.Entities;
using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine.LowLevel;

namespace E7.ECS
{
    public static class WorldHelper
    {
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

        public static T LateInject<T>(this World world, T w) where T : ComponentSystem
        {
            if (w == null)
            {
                return world.GetOrCreateSystem<T>();
            }
            else
            {
                return w;
            }
        }

        public static void IncreaseVersion()
        {
            World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<VersionBumperSystem>().BumpVersion();
        }

        public static void CopyAllEntities(World fromWorld, World toWorld)
        {
            var ecs = fromWorld.CreateSystem<EntityCloningSystem>();
            ecs.CloneTo(toWorld);
            fromWorld.DestroySystem(ecs);
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
                w.GetOrCreateSystem(t);
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
                World.DefaultGameObjectInjectionWorld = w;
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
                World.DefaultGameObjectInjectionWorld = null;
                w.Dispose();
                ScriptBehaviourUpdateOrder.SetPlayerLoop(PlayerLoop.GetDefaultPlayerLoop());
            }
        }
    }
}