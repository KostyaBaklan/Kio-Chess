using System.Runtime.Serialization;

namespace GamesServices;

[DataContract]
public class SequenceModel
{
    [DataMember]
    public byte[] Sequence { get; set; }

    [DataMember]
    public short Move { get; set; }

    [DataMember]
    public int White { get; set; }

    [DataMember]
    public int Draw { get; set; }

    [DataMember]
    public int Black { get; set; }
}