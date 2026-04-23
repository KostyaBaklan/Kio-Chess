using MessagePack;

namespace Engine.Communication.Transport;

[MessagePackObject]
public class PipeMessage<T>
{
    [Key(0)]
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    [Key(1)]
    public string MessageType { get; set; } = typeof(T).FullName ?? typeof(T).Name;

    [Key(2)]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    [Key(3)]
    public T Payload { get; set; }
}
