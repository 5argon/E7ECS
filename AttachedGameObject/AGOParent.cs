using Unity.Entities;

namespace E7.ECS.AttachGameObject
{
    public struct AGOParent : IComponentData
    {
        public Entity parent;
    }
}
