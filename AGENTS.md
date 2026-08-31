# Blazing Story Docs (Blazor)

Documentation site for [Blazing Story](https://github.com/jsakamoto/BlazingStory) (a Storybook clone for Blazor). A .NET 10 Blazor WebAssembly single-page app that fetches versioned Markdown from `wwwroot` at runtime and renders it client-side.

## Project layout

- `Docs/` — the Blazor WASM app (`BlazingStory.Docs.csproj`).
  - `Pages/DocPage.razor` — the single route (`/` and `/{*Path}`) that resolves `{version}/{slug}` from the URL, loads the sidebar and Markdown, and renders the page.
  - `Layout/` — chrome components: `Sidebar`, `TableOfContents`, `NavBar`, `Breadcrumbs`, `DocPagination`, `VersionSelector`, `ThemeToggle`, `SiteFooter`, `MainLayout`.
  - `Services/`
    - `DocsCatalogService` — fetches and caches `Docs/versions.json` and each version's `sidebar.json`.
    - `MarkdownService` — fetches a `.md` file, strips YAML front matter, runs it through the Markdig pipeline, and caches the rendered `DocContent`.
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

## How content resolves

`DocPage.razor` parses the URL path as `{version}/{slug}`; if the first segment isn't a known version it falls back to `versions.json`'s `defaultVersion` and treats the whole path as the slug. The default version's docs are served at the bare slug (no version prefix) — see `DocRoutes.Doc`. Every relative link/image in a Markdown file is rewritten at render time by `DocumentPostProcessor.RewriteLinks`, so authors write plain relative Markdown links/paths and never hand-craft app routes.

## Adding or editing a documentation page

1. Add/edit the `.md` file under `wwwroot/Docs/v{version}/EN/`. Images go in that version's `assets/` folder; link to them with a relative path.
2. Add the page to the matching `sidebar.json` (`slug` = filename without `.md`, relative to the `EN` folder — e.g. `addons/overview`). A page not listed in `sidebar.json` cannot be reached from the sidebar and its prev/next pagination won't include it, even if the file exists.
3. Only touch `versions.json` / create a new `v{version}` folder when cutting a new documentation version, not for routine content edits — existing published versions are treated as frozen snapshots (each has its own copy of every file).
4. Admonitions use GFM alert syntax (`> [!NOTE]`, `> [!TIP]`, `> [!IMPORTANT]`, `> [!WARNING]`, `> [!CAUTION]`), not Docusaurus `:::` fences — `UseAlertBlocks()` in `MarkdownService` is what renders these.
5. Code fences use the language identifiers Markdig/highlighting expect after `DocumentPostProcessor` normalization (`csharp`, `bash`, `cshtml`, `markup`, etc., or their common aliases like `cs`/`sh`/`razor`/`html`).

## Running / building

- Run locally: `dotnet run --project "Docs/BlazingStory.Docs.csproj"` (dev server at `http://localhost:5030`, per `Properties/launchSettings.json`).
- Build: `dotnet build "Docs/BlazingStory.Docs.csproj"`.
- No test suite in this repo.

## Conventions

- Target framework is `net10.0` with nullable reference types and implicit usings enabled — keep new C# consistent with that.
- Async I/O uses `async`/`await` throughout (`ValueTask` for the cached service methods); follow the same pattern for new async code.
- This is a content-heavy repo: most changes are Markdown edits under `wwwroot/Docs/`, not C# changes. Treat past published version folders as read-only history unless explicitly asked to backport a fix.
