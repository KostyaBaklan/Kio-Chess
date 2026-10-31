using Engine.Models.Config;

namespace NnueTrainer;

/// <summary>
/// Quantized network file (little endian):
/// "KNUE", int version, int inputs, int hidden, int qa, int qb, int scale,
/// short[inputs * hidden] FT weights (input-major), short[hidden] FT bias,
/// short[2 * hidden] output weights (stm half first), int output bias.
/// Inference: cp = (sum(clamp(acc, 0, QA)^2 * w) / QA + b) * Scale / (QA * QB).
/// </summary>
internal static class NetworkExporter
{
    public const int Version = 1;

    public static void Export(NnueModel model, NnueNetworkConfiguration network, string path)
    {
        int hidden = network.Hidden;
        var ftWeights = model.FtWeights;
        var ftBias = model.FtBias;
        var outputWeights = model.OutputWeights;

        using var writer = new BinaryWriter(File.Create(path));
        writer.Write("KNUE"u8);
        writer.Write(Version);
        writer.Write(network.Inputs);
        writer.Write(hidden);
        writer.Write(network.QA);
        writer.Write(network.QB);
        writer.Write(network.Scale);

        // EmbeddingBag weight layout is [inputs, hidden] = input-major.
        foreach (var w in ftWeights) writer.Write(Quantize(w, network.QA));
        foreach (var b in ftBias) writer.Write(Quantize(b, network.QA));
        foreach (var w in outputWeights) writer.Write(Quantize(w, network.QB));
        writer.Write((int)Math.Round(model.OutputBias * network.QA * network.QB));
    }

    private static short Quantize(float value, int factor) =>
        (short)Math.Clamp(Math.Round(value * factor), short.MinValue, short.MaxValue);
}
