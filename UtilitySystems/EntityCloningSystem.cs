using Unity.Entities;
using Unity.Collections;

namespace E7.ECS
{
    /// <summary>
    /// Used by WorldHelper.CopyAllEntities
    /// </summary>
    public class EntityCloningSystem : ComponentSystem
    {
        internal struct Cloned : ISystemStateComponentData { }

        ComponentGroup clonedGroup;
        protected override void OnCreateManager()
        {
            clonedGroup = GetComponentGroup(ComponentType.Create<Cloned>());
        }

        /// <summary>
        /// Clone from the world this system resides in to any destination world.
        /// TODO : Add `EntityArchetypeQuery` support
        /// </summary>
        public void CloneTo(World destinationWorld)
        {
            EntityManager destinationEntityManager = destinationWorld.GetExistingManager<EntityManager>();
            using (var ea = EntityManager.GetAllEntities(Allocator.Temp))
            {
                for (int i = 0; i < ea.Length; i++)
                {
                    Entity cloned = EntityManager.Instantiate(ea[i]);
                    EntityManager.AddComponentData(cloned, new Cloned());
                }
            }
            using (var remap = new NativeArray<EntityRemapUtility.EntityRemapInfo>(clonedGroup.CalculateLength(), Allocator.TempJob))
            {
                destinationEntityManager.MoveEntitiesFrom(EntityManager, clonedGroup, remap);
            }
            var destEcs = destinationWorld.CreateManager<EntityCloningSystem>();
            destEcs.CleanCloned();
            destinationWorld.DestroyManager(destEcs);
        }

        internal void CleanCloned()
        {
            var enType = GetArchetypeChunkEntityType();
            var aca = clonedGroup.CreateArchetypeChunkArray(Allocator.TempJob);
            for (int i = 0; i < aca.Length; i++)
            {
                var ea = aca[i].GetNativeArray(enType);
                for (int j = 0; j < ea.Length; j++)
                {
                    EntityManager.RemoveComponent<Cloned>(ea[j]);
                }
            }
        }

        protected override void OnUpdate(){}
    }
}