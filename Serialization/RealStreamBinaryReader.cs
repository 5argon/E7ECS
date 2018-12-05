#define I_WANNA_CREATE_MY_OWN_WORLDS_BUT_GIVE_ME_THOSE_HOOKS

using System;
using System.IO;
using Unity.Collections.LowLevel.Unsafe;

namespace E7.ECS
{
    public unsafe class RealStreamBinaryReader : Unity.Entities.Serialization.BinaryReader
    {
        private Stream stream;
        private byte[] buffer;

        public RealStreamBinaryReader(Stream stream, int bufferSize = 65536)
        {
            this.stream = stream;
            buffer = new byte[bufferSize];
        }

        public void Dispose()
        {
            stream.Dispose();
        }

        public void ReadBytes(void* data, int bytes)
        {
            int remaining = bytes;
            int bufferSize = buffer.Length;

            fixed (byte* fixedBuffer = buffer)
            {
                while (remaining != 0)
                {
                    int read = stream.Read(buffer, 0, Math.Min(remaining, bufferSize));
                    remaining -= read;
                    UnsafeUtility.MemCpy(data, fixedBuffer, read);
                    data = (byte*)data + read;
                }
            }
        }
    }
}