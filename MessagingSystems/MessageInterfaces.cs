using Unity.Entities;
using UnityEngine;

namespace E7.ECS
{
    public interface IMessageInjectGroup<MessageGroup>
    where MessageGroup : struct, IMessageGroup
    {
        ComponentDataArray<MessageGroup> MessageGroups { get; }
        ComponentDataArray<DestroyMessageSystem.MessageEntity> MessageEntity { get; }
        EntityArray Entities { get; }
    }

    public interface ITagResponseDataInjectGroup<TagComponent, DataComponent> : ITagResponseInjectGroup<TagComponent>
    where DataComponent : struct, IComponentData
    where TagComponent : struct, IComponentData, ITag
    {
        ComponentDataArray<DataComponent> datas { get; }
    }

    public interface ITagResponseInjectGroup<TagComponent> 
    where TagComponent : struct, IComponentData, ITag
    {
        ComponentDataArray<TagComponent> TagComponents { get; }
        EntityArray Entities { get; }
    }

    public interface IMessage : ITag { }

    public interface IMessageGroup : IComponentData { }

    public interface ITag : IComponentData { }
}