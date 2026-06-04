# Agents Instructions

## Project Overview

This is the documentation site for **Blazing Story** — a Blazor reimplementation (clone) of [Storybook](https://storybook.js.org/), providing a UI component catalog for Blazor applications.

- **Blazing Story repository**: https://github.com/jsakamoto/BlazingStory
- **Site framework**: [Docusaurus](https://docusaurus.io/)
- **Deployment**: GitHub Pages via GitHub Actions

## Tech Stack

- Node.js / TypeScript
- Docusaurus v3.10 (with `@docusaurus/faster`)
- React 19
- Markdown / MDX for documentation content

## Directory Structure

- `versioned_docs/version-<X>/` — versioned documentation source files (Markdown/MDX); **there is no `docs/` directory** because `includeCurrentVersion: false` is set in `docusaurus.config.ts`
- `versioned_sidebars/` — sidebar configuration for each versioned snapshot
- `versions.json` — ordered list of published versions (latest first)
- `src/css/` — global CSS overrides (`custom.css`)
- `static/` — static assets (images, favicon, etc.)

## Common Commands

- `npm install` — install dependencies
- `npm start` — start local dev server
- `npm run build` — production build (output to `build/`)
- `npm run typecheck` — TypeScript type check

## Current Published Versions

| Version key | Label in dropdown | Notes |
|---|---|---|
| `v1.0.0-preview.83` | `Current Version` | latest |
| `v1.0.0-preview.81` | `1.0.0-preview.81 ~ 82` | covers previews 81–82 |
| `v1.0.0-preview.68` | `1.0.0-preview.68 ~ 80` | covers previews 68–80 |
| `v1.0.0-preview.67` | `1.0.0-preview.67 or before` | oldest |

## Releasing a New Version

Because `includeCurrentVersion: false`, **all documentation lives in `versioned_docs/`**. To publish a new version (e.g., `v1.0.0-preview.XX`):

1. Edit the content files in the latest `versioned_docs/version-<current>/` directory until the new content is ready.

2. Run the versioning command to snapshot the current latest versioned docs into a new version:
   ```
   npm run docusaurus -- docs:version v1.0.0-preview.XX
   ```
   This creates `versioned_docs/version-v1.0.0-preview.XX/`, `versioned_sidebars/version-v1.0.0-preview.XX-sidebars.json`, and prepends the new version to `versions.json`.

3. After running the command, verify `versions.json` via the **terminal** (e.g., `Get-Content versions.json`).
   > **Note:** Do NOT use the file-reading tool to check `versions.json` right after the command — it may return stale/cached content and falsely suggest the command failed. Always use the terminal to confirm.

4. Update the `versions` object in `docusaurus.config.ts`:
   - Add the new version with label `"Current Version"`:
     ```ts
     "v1.0.0-preview.XX": {
       label: "Current Version",
       badge: false,
     },
     ```
   - Change the previous "Current Version" entry's label to a range covering the preview numbers it spans — from the version itself up to `XX - 1`. Do **not** include a leading `v` in the label:
     ```ts
     "v1.0.0-preview.<prev>": {
       label: "1.0.0-preview.<prev> ~ <XX-1>",
       badge: false,
     },
     ```
     For example, when adding `v1.0.0-preview.85` where the previous current was `v1.0.0-preview.83`:
     ```ts
     "v1.0.0-preview.83": {
       label: "1.0.0-preview.83 ~ 84",
       badge: false,
     },
     ```
     If `XX` immediately follows `<prev>` with no gap (i.e., `XX = prev + 1`), the range collapses to just the single version number:
     ```ts
     "v1.0.0-preview.<prev>": {
       label: "1.0.0-preview.<prev>",
       badge: false,
     },
     ```

5. Verify the build succeeds: `npm run build`
