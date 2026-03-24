# Agents Instructions

## Project Overview

This is the documentation site for **Blazing Story** — a Blazor reimplementation (clone) of [Storybook](https://storybook.js.org/), providing a UI component catalog for Blazor applications.

- **Blazing Story repository**: https://github.com/jsakamoto/BlazingStory
- **Site framework**: [Docusaurus](https://docusaurus.io/)
- **Deployment**: GitHub Pages via GitHub Actions

## Tech Stack

- Node.js / TypeScript
- Docusaurus v3
- Markdown / MDX for documentation content

## Directory Structure

- `docs/` — documentation source files (Markdown/MDX)
- `src/` — custom Docusaurus components and pages
- `static/` — static assets
- `versioned_docs/` / `versioned_sidebars/` — versioned documentation snapshots

## Common Commands

- `npm install` — install dependencies
- `npm start` — start local dev server
- `npm run build` — production build (output to `build/`)

## Releasing a New Version

To update the default documentation version (e.g., to `v1.0.0-preview.XX`):

1. Run the versioning command to snapshot the current `docs/` content:
   ```
   npm run docusaurus -- docs:version v1.0.0-preview.XX
   ```
   This creates `versioned_docs/version-v1.0.0-preview.XX/`, `versioned_sidebars/version-v1.0.0-preview.XX-sidebars.json`, and prepends the new version to `versions.json`.

2. After running the command, verify `versions.json` via the **terminal** (e.g., `Get-Content versions.json`).
   > **Note:** Do NOT use the file-reading tool to check `versions.json` right after the command — it may return stale/cached content and falsely suggest the command failed. Always use the terminal to confirm.

3. Add the new version entry to the `versions` object in `docusaurus.config.ts`:
   ```ts
   "v1.0.0-preview.XX": {
     badge: false,
   },
   ```

4. Verify the build succeeds: `npm run build`
