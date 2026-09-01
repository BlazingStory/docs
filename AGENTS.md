# Blazing Story Docs (Blazor)

Documentation site for [Blazing Story](https://github.com/jsakamoto/BlazingStory) (a Storybook clone for Blazor). A .NET 10 Blazor WebAssembly single-page app that fetches versioned Markdown from `wwwroot` at runtime and renders it client-side.

## Project layout

- `Docs/` — the Blazor WASM app (`BlazingStory.Docs.csproj`).
  - `Pages/DocPage.razor` — the single route (`/` and `/{*Path}`) that resolves `{version}/{slug}` from the URL, loads the sidebar and Markdown, and renders the page.
  - `Layout/` — chrome components: `Sidebar`, `TableOfContents`, `NavBar`, `SearchBox`, `Breadcrumbs`, `DocPagination`, `VersionSelector`, `ThemeToggle`, `SiteFooter`, `MainLayout`.
  - `Services/`
    - `DocsCatalogService` — fetches and caches `Docs/versions.json` and each version's `sidebar.json`.
    - `MarkdownService` — fetches a `.md` file, strips YAML front matter, runs it through the Markdig pipeline, and caches the rendered `DocContent`.
    - `SearchIndexService`, `EmbeddingService` — the vector search (see below).
    - `DocRoutes` — the single source of truth for every URL/path shape (version catalog, sidebar file, markdown file, content/asset file, doc link). Route logic changes belong here.
    - `DocsUiState`, `ThemeService` — sidebar open/close and light/dark theme state.
  - `Models/` — POCOs deserialized from `versions.json` / `sidebar.json`, plus `DocContent`/`TocEntry`.
  - `MarkdownRendering/`
    - `DocumentPostProcessor` — rewrites relative Markdown links/images into app routes, normalizes code fence languages (e.g. `cs` → `csharp`, `razor` → `cshtml`), extracts the H1 title, and builds the H2/H3 table of contents.
    - `CodeBlockTitleExtension` — Markdig extension for Docusaurus-style code fence titles.
  - `wwwroot/Docs/` — the actual content, **not** the razor app:
    - `versions.json` — `{ defaultVersion, versions: [{ version, displayText }] }`.
    - `v{version}/sidebar.json` — `{ defaultSlug, sections: [{ displayText, items: [{ slug, displayText }] }] }` per version.
    - `v{version}/EN/*.md` (+ `addons/*.md`, `assets/*`) — the Markdown content and images for that version. Only `EN` exists today (see `DocRoutes.Language`).
    - `v{version}/search-index.json` + `search-index.vec` — the vector search index of that version. Generated at build time and **not committed** (see below).
- `Docs.IndexGenerator/` — the console app that builds those index files (`BlazingStory.Docs.IndexGenerator.csproj`).
  - `MarkdownChunker` — splits a Markdown file into the chunks that get indexed.
  - `MiniLmEmbedder` — turns a chunk into a 384 dimension vector with the all-MiniLM-L6-v2 ONNX model.
  - `.model-cache/` — the downloaded model files, ignored by Git.

## How content resolves

`DocPage.razor` parses the URL path as `{version}/{slug}`; if the first segment isn't a known version it falls back to `versions.json`'s `defaultVersion` and treats the whole path as the slug. The default version's docs are served at the bare slug (no version prefix) — see `DocRoutes.Doc`. Every relative link/image in a Markdown file is rewritten at render time by `DocumentPostProcessor.RewriteLinks`, so authors write plain relative Markdown links/paths and never hand-craft app routes.

## Adding or editing a documentation page

1. Add/edit the `.md` file under `wwwroot/Docs/v{version}/EN/`. Images go in that version's `assets/` folder; link to them with a relative path.
2. Add the page to the matching `sidebar.json` (`slug` = filename without `.md`, relative to the `EN` folder — e.g. `addons/overview`). A page not listed in `sidebar.json` cannot be reached from the sidebar and its prev/next pagination won't include it, even if the file exists.
3. Only touch `versions.json` / create a new `v{version}` folder when cutting a new documentation version, not for routine content edits — existing published versions are treated as frozen snapshots (each has its own copy of every file).
4. Admonitions use GFM alert syntax (`> [!NOTE]`, `> [!TIP]`, `> [!IMPORTANT]`, `> [!WARNING]`, `> [!CAUTION]`), not Docusaurus `:::` fences — `UseAlertBlocks()` in `MarkdownService` is what renders these.
5. Code fences use the language identifiers Markdig/highlighting expect after `DocumentPostProcessor` normalization (`csharp`, `bash`, `cshtml`, `markup`, etc., or their common aliases like `cs`/`sh`/`razor`/`html`).
6. Nothing has to be done about the search index. The next build regenerates the index of any version whose `.md` files or `sidebar.json` changed.

## Vector search

The search box in the navbar finds documentation by meaning rather than by keyword, with no server involved. It works in two halves that have to agree with each other.

- **At build time**, `Docs.IndexGenerator` reads `versions.json`, then each version's `sidebar.json` for the list of pages, and splits every page into chunks at its H2 headings. Each chunk becomes a 384 dimension vector, written to `search-index.vec` (raw little endian float32, no header, one chunk after another), while `search-index.json` holds the matching metadata in the same order. An MSBuild target in `BlazingStory.Docs.csproj` runs this before `Restore` and `Build`, and the generator skips any version that is already up to date.
- **At runtime**, `SearchBox` loads the model only once the visitor focuses the search box, never at startup, because the model is a 23 MB download. `EmbeddingService` turns the typed text into a vector through transformers.js, and `SearchIndexService` scores it against the current version's chunks with a dot product, which is the cosine similarity because every vector is L2 normalized.

Four things have to stay in step, and getting any of them wrong produces no error at all, only meaningless search results:

1. **The model is pinned on both sides.** `wwwroot/js/search-embeddings.js` loads transformers.js `@2.17.2`, whose default is the quantized ONNX file, which is the file the generator downloads. Raising one version without the other silently breaks every result.
2. **Symbols are stripped on both sides.** `Microsoft.ML.Tokenizers` drops every Unicode symbol character (`<`, `>`, `=`, `→`, …) while the tokenizer of transformers.js keeps each as its own token, so both sides remove them first: `MarkdownChunker.NormalizeForEmbedding` and the `\p{S}` replacement in `search-embeddings.js`. The same normalization also collapses line feeds, which the two tokenizers likewise disagree about.
3. **Heading anchors have to match what the app renders.** The search results deep link to `#{anchor}`, so `MarkdownChunker` has to keep using `UseAutoIdentifiers(AutoIdentifierOptions.GitHub)` and has to keep stripping front matter exactly like `MarkdownService.StripFrontMatter` does. Leaving the front matter in turns it into a thematic break plus a setext heading, which shifts every heading id of the document.
4. **Fenced code blocks are left out of the vectors** on purpose, since averaging a mass of code tokens into a vector pulls every code heavy chunk toward the same spot of the embedding space. Inline code stays, because it is part of a sentence.

To check that the two sides still agree, embed the same sentence on each and compare: `dotnet run --project Docs.IndexGenerator/BlazingStory.Docs.IndexGenerator.csproj -- --embed "some sentence"` prints the token ids and the vector, and the same text can be run through `js/search-embeddings.js` in the browser console. The token ids have to be identical. The vectors agree to about 0.995 or better as a dot product, not exactly, because ONNX Runtime and onnxruntime-web run the quantized model with different kernels. A result down in the low 0.9 range instead means a real mismatch, such as the quantized and fp32 model files having been mixed up. `--dump` prints how every page was split into chunks.

## Running / building

- Run locally: `dotnet run --project "Docs/BlazingStory.Docs.csproj"` (dev server at `http://localhost:5030`, per `Properties/launchSettings.json`).
- Build: `dotnet build "Docs/BlazingStory.Docs.csproj"`. The first build after a fresh clone also downloads about 23 MB of model files into `Docs.IndexGenerator/.model-cache/` and builds the search index; later builds skip both.
- No test suite in this repo.

## Conventions

- Target framework is `net10.0` with nullable reference types and implicit usings enabled — keep new C# consistent with that.
- Async I/O uses `async`/`await` throughout (`ValueTask` for the cached service methods); follow the same pattern for new async code.
- This is a content-heavy repo: most changes are Markdown edits under `wwwroot/Docs/`, not C# changes. Treat past published version folders as read-only history unless explicitly asked to backport a fix.
