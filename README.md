# Blazing Story Documentation

This repository contains the documentation site for [Blazing Story](https://github.com/jsakamoto/BlazingStory), a clone of "Storybook" for Blazor, a frontend workshop for building UI components and pages in isolation.

The site is a Blazor WebAssembly app. It loads versioned Markdown files from `Docs/Docs` at runtime and renders them in the browser.

The search box finds pages by meaning, not by keyword. It also runs in the browser, so the site still works as plain static files with no server behind it.

## Repository Overview

- **Purpose.** Host and manage the documentation for Blazing Story.
- **Framework.** Built with Blazor WebAssembly (.NET 11).

## Related Repository

- Main Blazing Story GitHub. [https://github.com/jsakamoto/BlazingStory](https://github.com/jsakamoto/BlazingStory)

## Build Instructions

**Requirements**
- .NET 11 SDK (currently RC1, so install it with a preview-enabled feed/installer)
- Node.js
- An internet connection for the first build

The build also creates the search index for every documentation version, bundles the transformers.js and PrismJS libraries the search box and the code blocks run on, and compiles the TypeScript of the site. The first build installs the npm packages for all of that and downloads the embedding model (about 23 MB) from Hugging Face, which it keeps in `Docs.IndexGenerator/.model-cache/`, so it takes a while. Later builds reuse all of it and skip whatever did not change. None of what the build writes is in this repository, so there is nothing to regenerate by hand.

### Development Server

```bash
dotnet run --project Docs
```

This starts the site at `http://localhost:5030`.

### Production Build

```bash
dotnet publish Docs -c Release
```

This generates the static site under `Docs/bin/Release/net11.0/publish/wwwroot`.

## License and Third-Party Notices

Copyright (c) 2025-2026 J.Sakamoto

This documentation is licensed under the [CC BY-SA 4.0](LICENSE).  
You are free to share and adapt the material as long as you provide appropriate credit and distribute your contributions under the same license.

This site also uses third-party libraries and resources; see [THIRD-PARTY-NOTICES.txt](THIRD-PARTY-NOTICES.txt) for details.
