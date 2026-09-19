using MessagePack;

namespace Engine.Communication.Serialization;

public class MessagePackSerializer : IMessageSerializer
{
    private readonly MessagePackSerializerOptions _options;

    public MessagePackSerializer()
    {
        _options = MessagePackSerializerOptions.Standard
            .WithCompression(MessagePackCompression.Lz4BlockArray);
    }

    public byte[] Serialize<T>(T message)
    {
        return MessagePack.MessagePackSerializer.Serialize(message, _options);
    }

    public T Deserialize<T>(byte[] data)
    {
        return MessagePack.MessagePackSerializer.Deserialize<T>(data, _options);
    }

    public ValueTask<byte[]> SerializeAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        return new ValueTask<byte[]>(Serialize(message));
    }

    public ValueTask<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default)
    {
        return new ValueTask<T>(Deserialize<T>(data));
    }
}
