// Computes the embedding vector of a search text in the browser, with the same model the build
// time index generator uses. The version of this library and the model file it downloads are a
// pair: transformers.js v2 loads the quantized ONNX file, which is what the generator embeds with,
// so upgrading only one of the two silently breaks every search result.
import { env, type FeatureExtractionPipeline, pipeline } from '../lib/transformers/transformers.min.js';

// Skips the library's default attempt to fetch the model from a same-origin "/models/" path first,
// which always 404s here since we don't host a local copy, before it falls back to the hub.
env.allowLocalModels = false;

let extractor: FeatureExtractionPipeline | null = null;

/**
 * Loads the feature extraction pipeline used to compute the embedding vectors.
 * The first call downloads about 23 MB, which the browser keeps in its cache storage,
 * so later visits do not pay for it again. Calling this again does nothing.
 *
 * @param {string} model - The name of the model to load for feature extraction.
 * @returns {Promise<void>} A promise that resolves once the model is ready.
 */
export async function initModel(model: string): Promise<void> {
    if (!extractor) {
        extractor = await pipeline('feature-extraction', model);
    }
}

/**
 * Computes a normalized embedding vector for the given text.
 *
 * The text is stripped of every symbol character first, because the two tokenizers disagree about
 * them: "Microsoft.ML.Tokenizers", which the build time index generator uses, drops each of them,
 * while the one here keeps each as its own token. Characters such as "<", ">", "=" and "→" are
 * common in this content, so leaving that difference in place would put the search queries in a
 * slightly different embedding space than the index. The counterpart of this lives in
 * "MarkdownChunker.NormalizeForEmbedding" of the index generator, and the two have to keep doing
 * the same thing.
 *
 * @param {string} text - The input text to embed.
 * @returns {Promise<number[]>} A promise that resolves to the embedding vector as an array of numbers.
 */
export async function embed(text: string): Promise<number[]> {
    const normalized = text.replace(/\p{S}/gu, ' ').replace(/\s+/g, ' ').trim();
    const output = await extractor?.(normalized, { pooling: 'mean', normalize: true });
    return Array.from(output?.data ?? []);
}
