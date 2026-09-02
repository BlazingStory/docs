import { pipeline } from '../lib/transformers/transformers.min.js';
let extractor = null;
export async function initModel(model) {
    if (!extractor) {
        extractor = await pipeline('feature-extraction', model);
    }
}
export async function embed(text) {
    const normalized = text.replace(/\p{S}/gu, ' ').replace(/\s+/g, ' ').trim();
    const output = await extractor?.(normalized, { pooling: 'mean', normalize: true });
    return Array.from(output?.data ?? []);
}
