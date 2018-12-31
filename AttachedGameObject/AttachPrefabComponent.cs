using Unity.Entities;
using Unity.Transforms;

namespace E7.ECS.AttachGameObject
{
    public class AttachPrefabComponent : SharedComponentDataWrapper<AttachPrefab>
    {
        public bool alsoAttachTransform;
        protected override void OnEnable()
        {
            var goe = GetComponent<GameObjectEntity>();
            if (goe != null && goe.EntityManager != null)
            {
                Entity sideChannel = goe.EntityManager.CreateEntity();
                goe.EntityManager.AddSharedComponentData(sideChannel,
                new AttachGameObject
                {
                    parent = goe.Entity,
                    gameObjectEntity = this.Value.gameObjectEntity,
                    modeFlag = alsoAttachTransform ? AttachGameObject.ModeFlag.AttachTransform : AttachGameObject.ModeFlag.Normal,
                });
            }
            base.OnEnable();
        }
    }
}
