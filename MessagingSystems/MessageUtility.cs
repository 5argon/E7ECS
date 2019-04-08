#define I_AM_WORRIED_ABOUT_EXECEPTION_PERFORMANCE

using Unity.Entities;

namespace E7.ECS
{
    public static class MessageUtility
    {
        /// <summary>
        /// You can manually inject only one type of message of the group with the output of this to GetEntityQuery
        /// </summary>
        public static ComponentType[] GetMessageTypes<Message, MessageGroup>()
        where Message : struct, IMessage
        where MessageGroup : struct, IMessageGroup
        {
            return new ComponentType[] {
                ComponentType.ReadOnly<Message>(),
                ComponentType.ReadOnly<MessageGroup>(),
                ComponentType.ReadOnly<DestroyMessageSystem.MessageEntity>()
            };
        }
    }
}