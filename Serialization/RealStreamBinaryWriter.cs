#define I_WANNA_CREATE_MY_OWN_WORLDS_BUT_GIVE_ME_THOSE_HOOKS

using System;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;

namespace E7.ECS
{
    public unsafe class RealStreamBinaryWriter : Unity.Entities.Serialization.BinaryWriter
    {
        private Stream stream;
        private byte[] buffer;

        public RealStreamBinaryWriter(Stream stream, int bufferSize = 65536)
        {
            this.stream = stream;
            buffer = new byte[bufferSize];
        }

        public void Dispose()
        {
            stream.Dispose();
        }

        public void WriteBytes(void* data, int bytes)
        {
            int remaining = bytes;
            int bufferSize = buffer.Length;

            fixed (byte* fixedBuffer = buffer)
            {
                while (remaining != 0)
                {
                    int bytesToWrite = Math.Min(remaining, bufferSize);
                    UnsafeUtility.MemCpy(fixedBuffer, data, bytesToWrite);
                    stream.Write(buffer, 0, bytesToWrite);
                    data = (byte*)data + bytesToWrite;
                    remaining -= bytesToWrite;
                }
            }
        }
    }
}