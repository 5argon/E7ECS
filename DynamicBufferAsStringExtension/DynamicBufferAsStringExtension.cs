using Unity.Entities;
using System.Text;

public interface IStringBuffer { byte character { get; set; } }

public static class DynamicBufferAsStringExtension
{
    /// <summary>
    /// Currently allocate garbage. TODO: Go unsafe
    /// </summary>
    public static string AssembleString<T>(this DynamicBuffer<T> isb)
    where T : struct, IStringBuffer
    {
        byte[] chars = new byte[isb.Length];
        for(int i = 0; i < isb.Length; i++)
        {
            chars[i] = isb[i].character;
        }
        return Encoding.UTF8.GetString(chars);
    }

    public static void AppendString<T>(this DynamicBuffer<T> isb, string s)
    where T : struct, IStringBuffer
    {
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        foreach(byte b in bytes)
        {
            isb.Add(new T { character = b });
        }
    }
}
