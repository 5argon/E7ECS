using Unity.Entities;

namespace E7.ECS
{
    public static class MessageECBExtension
    {
        public static void Message<MessageComponent>(this EntityCommandBuffer.Concurrent ecb, int jobIndex, MessageArchetype msa, MessageComponent rx)
        where MessageComponent : struct, IMessage
        {
            Entity createdEntity = Message(ecb, jobIndex, msa);
            ecb.SetComponent(jobIndex, createdEntity, rx);
        }

        public static void Message<MessageComponent>(this EntityCommandBuffer ecb, MessageArchetype msa, MessageComponent rx)
        where MessageComponent : struct, IMessage
        {
            Entity createdEntity = Message(ecb, msa);
            ecb.SetComponent(createdEntity, rx);
        }

        public static Entity Message(this EntityCommandBuffer.Concurrent ecb, int jobIndex, MessageArchetype msa)
        {
            return ecb.CreateEntity(jobIndex, msa.archetype);
        }

        public static Entity Message(this EntityCommandBuffer ecb, MessageArchetype msa)
        {
            return ecb.CreateEntity(msa.archetype);
        }
    }
}