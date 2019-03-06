using Unity.Entities;
using UnityEngine;

namespace E7.ECS
{
    public interface IMessage : ITag { }
    public interface IMessageGroup : IComponentData { }
    public interface ITag : IComponentData { }
}