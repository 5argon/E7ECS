using Unity.Entities;

namespace E7.ECS
{
    public static class MessageECBExtension
    {
        public static void Message<MessageComponent>(this EntityCommandBuffer.Concurrent ecb, int jobIndex, MessageArchetype msa, MessageComponent rx)
        where MessageComponent : struct, IMessage
        {
            Message(ecb, jobIndex, msa);
            ecb.SetComponent(jobIndex, rx);
        }

        public static void Message(this EntityCommandBuffer.Concurrent ecb, int jobIndex, MessageArchetype msa)
        {
            ecb.CreateEntity(jobIndex, msa.archetype);
        }

        public static void Message<MessageComponent>(this EntityCommandBuffer ecb, MessageArchetype msa, MessageComponent rx)
        where MessageComponent : struct, IMessage
        {
            Message(ecb, msa);
            ecb.SetComponent(rx);
        }

        public static void Message(this EntityCommandBuffer ecb, MessageArchetype msa)
        {
            ecb.CreateEntity(msa.archetype);
        }
    }
}