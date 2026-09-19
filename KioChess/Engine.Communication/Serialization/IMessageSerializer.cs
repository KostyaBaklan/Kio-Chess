namespace Engine.Communication.Serialization;

public interface IMessageSerializer
{
    byte[] Serialize<T>(T message);
    T Deserialize<T>(byte[] data);
    ValueTask<byte[]> SerializeAsync<T>(T message, CancellationToken cancellationToken = default);
    ValueTask<T> DeserializeAsync<T>(byte[] data, CancellationToken cancellationToken = default);
}
