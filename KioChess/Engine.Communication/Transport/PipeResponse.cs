using MessagePack;

namespace Engine.Communication.Transport;

[MessagePackObject]
public class PipeResponse<T>
{
    [Key(0)]
    public string RequestId { get; set; } = string.Empty;

    [Key(1)]
    public bool Success { get; set; }

    [Key(2)]
    public T Result { get; set; }

    [Key(3)]
    public string ErrorMessage { get; set; }

    [Key(4)]
    public string ErrorType { get; set; }

    [Key(5)]
    public DateTime ResponseTime { get; set; } = DateTime.UtcNow;
}
