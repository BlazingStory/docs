using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace BlazingStory.Docs.IndexGenerator;

/// <summary>
/// Generates sentence embeddings using a MiniLM ONNX model.
/// </summary>
/// <remarks>
/// The values this class computes have to match what the browser side computes with
/// transformers.js, otherwise the search results become meaningless without any error.
/// That is why the quantized model file, the mean pooling, the L2 normalization, the
/// token limit, and the lower casing are all pinned down here.
/// </remarks>
public sealed class MiniLmEmbedder : IDisposable
{
    /// <summary>The maximum number of tokens the model accepts, which matches the max_seq_length of sentence-transformers.</summary>
    public const int MaxTokens = 256;

    /// <summary>The number of dimensions of a single vector, which comes from the all-MiniLM-L6-v2 model.</summary>
    public const int Dimensions = 384;

    private readonly InferenceSession _session;

    private readonly BertTokenizer _tokenizer;

    /// <summary>
    /// Initializes a new instance of the <see cref="MiniLmEmbedder"/> class.
    /// </summary>
    /// <param name="onnxModelPath">Path to the ONNX model file.</param>
    /// <param name="vocabPath">Path to the vocabulary file for the tokenizer.</param>
    public MiniLmEmbedder(string onnxModelPath, string vocabPath)
    {
        this._session = new InferenceSession(onnxModelPath);
        this._tokenizer = BertTokenizer.Create(vocabPath, new BertOptions
        {
            LowerCaseBeforeTokenization = true,   // uncased model
            ApplyBasicTokenization = true,
        });
    }

    /// <summary>
    /// Counts how many tokens the given text takes, including the [CLS] and [SEP] tokens.
    /// The chunk splitter uses this to keep every chunk inside the model's token limit.
    /// </summary>
    public int CountTokens(string text) => this._tokenizer.EncodeToIds(text).Count;

    /// <summary>
    /// Returns the token ids of the given text, to compare them against what the browser side
    /// produces for the same text.
    /// </summary>
    public IReadOnlyList<int> Tokenize(string text) => this._tokenizer.EncodeToIds(text);

    /// <summary>
    /// Computes a normalized embedding vector for the given text.
    /// </summary>
    /// <param name="text">The input text to embed.</param>
    public float[] Embed(string text)
    {
        // Encode the text into ids with [CLS] and [SEP]. If it is too long, truncate it and replace the last id with [SEP].
        var ids = this._tokenizer.EncodeToIds(text).ToArray();
        if (ids.Length > MaxTokens)
        {
            ids = ids[..MaxTokens];
            ids[^1] = this._tokenizer.SeparatorTokenId;
        }

        var length = ids.Length;
        var inputIds = new DenseTensor<long>([1, length]);
        var attentionMask = new DenseTensor<long>([1, length]);
        var tokenTypeIds = new DenseTensor<long>([1, length]);
        for (var i = 0; i < length; i++)
        {
            inputIds[0, i] = ids[i];
            attentionMask[0, i] = 1;   // no padding is needed because we process one item at a time, so this is always 1
            tokenTypeIds[0, i] = 0;
        }

        using var results = this._session.Run([
            NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),
            NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds),
        ]);

        // last_hidden_state: [1, length, 384]
        var hidden = results.First().AsTensor<float>();

        // mean pooling (a simple average, since the mask is always 1)
        var vector = new float[Dimensions];
        for (var t = 0; t < length; t++)
            for (var d = 0; d < Dimensions; d++)
                vector[d] += hidden[0, t, d];
        for (var d = 0; d < Dimensions; d++)
            vector[d] /= length;

        // L2 normalization (this makes the dot product equal to the cosine similarity at query time)
        var norm = MathF.Sqrt(vector.Sum(v => v * v));
        if (norm > 0)
        {
            for (var d = 0; d < Dimensions; d++) vector[d] /= norm;
        }

        return vector;
    }

    /// <summary>
    /// Releases the resources used by the underlying inference session.
    /// </summary>
    public void Dispose() => this._session.Dispose();
}
