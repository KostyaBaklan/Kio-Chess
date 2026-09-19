using MessagePack;

namespace Engine.Communication.Transport;

[MessagePackObject]
public class PipeRequest<T>
{
    [Key(0)]
    public string RequestId { get; set; } = Guid.NewGuid().ToString();

    [Key(1)]
    public string MethodName { get; set; } = string.Empty;

    [Key(2)]
    public T Parameters { get; set; }

    [Key(3)]
    public DateTime RequestTime { get; set; } = DateTime.UtcNow;
}
