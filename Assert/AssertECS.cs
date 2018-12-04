using Unity.Entities;

namespace E7.ECS
{
    public static class AssertECS
    {
        public static EntityArray EntitiesOfArchetype(this World world, params ComponentType[] types)
        {
            var ai = world.GetOrCreateManager<AssertInjector>();
            return ai.EntitiesOfArchetype(types);
        }

        public static bool HasEntityArchetype(this World world, params ComponentType[] types)
        {
            var ai = world.GetOrCreateManager<AssertInjector>();
            return ai.Any(types);
        }

        [DisableAutoCreation]
        public class AssertInjector : ComponentSystem
        {
            protected override void OnCreateManager() => this.Enabled = false;
            protected override void OnUpdate() { }

            public bool Any(params ComponentType[] types) => EntitiesOfArchetype(types).Length != 0;

            public EntityArray EntitiesOfArchetype(params ComponentType[] types)
            {
                var cg = GetComponentGroup(types);
                return cg.GetEntityArray();
            }

        }
    }
}