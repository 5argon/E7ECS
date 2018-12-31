using System;
using Unity.Entities;
using UnityEngine;

namespace E7.ECS.AttachGameObject
{
    [Serializable]
    public struct AttachPrefab : ISharedComponentData
    {
        public GameObject gameObjectEntity;
    }
}
