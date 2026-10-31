using TorchSharp;
using TorchSharp.Modules;
using static TorchSharp.torch;

namespace NnueTrainer;

/// <summary>768 -> H (shared perspective FT) x2 -> SCReLU -> 1. Output is in units of score / Scale.</summary>
internal sealed class NnueModel : nn.Module
{
    private readonly EmbeddingBag _ft;
    private readonly Parameter _ftBias;
    private readonly Linear _output;

    public NnueModel(int hidden) : base(nameof(NnueModel))
    {
        _ft = nn.EmbeddingBag(RecordBatchReader.Inputs, hidden, mode: EmbeddingBagMode.Sum);
        _ftBias = nn.Parameter(zeros(hidden));
        _output = nn.Linear(2 * hidden, 1);

        using (no_grad())
        {
            _ft.weight.normal_(0, 0.1);
            _output.weight.normal_(0, 1.0 / Math.Sqrt(2 * hidden));
            _output.bias.zero_();
        }

        RegisterComponents();
    }

    public Tensor Forward(Tensor stmIndices, Tensor stmOffsets, Tensor nstmIndices, Tensor nstmOffsets)
    {
        var stm = _ft.call(stmIndices, stmOffsets, null) + _ftBias;
        var nstm = _ft.call(nstmIndices, nstmOffsets, null) + _ftBias;
        var hidden = cat([stm, nstm], 1).clamp(0, 1).pow(2);
        return _output.call(hidden).squeeze(1);
    }

    public void Clip(double limit)
    {
        using (no_grad())
        {
            _ft.weight.clamp_(-limit, limit);
            _ftBias.clamp_(-limit, limit);
            _output.weight.clamp_(-limit, limit);
        }
    }

    public float[] FtWeights => _ft.weight.data<float>().ToArray();
    public float[] FtBias => _ftBias.data<float>().ToArray();
    public float[] OutputWeights => _output.weight.data<float>().ToArray();
    public float OutputBias => _output.bias.data<float>()[0];
}
