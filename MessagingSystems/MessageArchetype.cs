using Unity.Entities;

namespace E7.ECS
{
    /// <summary>
    /// Just a wrapper that this is not an ordinary archetype
    /// </summary>
    public struct MessageArchetype
    {
        public EntityArchetype archetype;
    }
}