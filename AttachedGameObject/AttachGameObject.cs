using System;
using Unity.Entities;
using UnityEngine;

namespace E7.ECS.AttachGameObject
{
    /// <summary>
    /// Create a NEW entity with these information.
    /// 
    /// AttachGameObjectSystem will then instantiate a game object which constantly
    /// checks on its parent along with destroying this NEW entity indicating that it had performed
    /// the task.
    /// </summary>
    public struct AttachGameObject : ISharedComponentData
    {
        [Flags]
        public enum ModeFlag
        {
            Normal = 0,
            AttachTransform = 1,
        }
    
        public Entity parent;
        public GameObject gameObjectEntity;

        /// <summary>
        /// If you choose `AttachTransform` you cannot destroy the parent before its corresponding game object
        /// because transform system will complain (preview 21). You have to somehow get rid of the game object manually first
        /// then destroy the entity owning it.
        /// 
        /// For normal mode destroying the entity can destroy the object automatically.
        /// </summary>
        public ModeFlag modeFlag;
    }
}
