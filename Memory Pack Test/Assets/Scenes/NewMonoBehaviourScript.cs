using MemoryPack;
using System;
using System.Buffers;
using UnityEngine;
using UnityEngine.Profiling;

[MemoryPackable]
public partial struct Data
{
    public int MyProperty1;
    public int MyProperty2;
}

public class NewMonoBehaviourScript : MonoBehaviour
{
    Data data = new Data { MyProperty1 = 99, MyProperty2 = 9999 };
    ArrayBufferWriter<byte> writer = new ArrayBufferWriter<byte>();

    void Start()
    {
        MemoryPackSerializer.Serialize(writer, data);
        Debug.LogWarning(writer.WrittenMemory);
        var data2 = MemoryPackSerializer.Deserialize<Data>(writer.WrittenSpan);
        writer.Clear();
        Debug.LogWarning(data2);
    }

    private void Update()
    {
        参考CacheRent();
    }

    void 参考()
    {
        Profiler.BeginSample(nameof(参考));
        for (int i = 0; i < 10000; i++)
        {
            var d = MemoryPackSerializer.Serialize(data);
            var data2 = MemoryPackSerializer.Deserialize<Data>(d);
        }
        Profiler.EndSample();
    }

    void 参考Cache()
    {
        Profiler.BeginSample(nameof(参考Cache));
        for (int i = 0; i < 10000; i++)
        {
            MemoryPackSerializer.Serialize(writer, data);
            var data2 = MemoryPackSerializer.Deserialize<Data>(writer.WrittenSpan);// .ToArray()才与下面等价，用于网络通信
            writer.Clear();
        }
        Profiler.EndSample();
    }

    void 参考CacheRent()
    {
        Profiler.BeginSample(nameof(参考Cache));
        for (int i = 0; i < 10000; i++)
        {
            MemoryPackSerializer.Serialize(writer, data);
            byte[] rentedArray = ArrayPool<byte>.Shared.Rent(writer.WrittenSpan.Length);
            writer.WrittenSpan.CopyTo(rentedArray);
            var data2 = MemoryPackSerializer.Deserialize<Data>(rentedArray);
            ArrayPool<byte>.Shared.Return(rentedArray);
            writer.Clear();
        }
        Profiler.EndSample();
    }
}
