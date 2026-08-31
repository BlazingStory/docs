# Blazing Story Documentation

This repository contains the documentation site for [Blazing Story](https://github.com/jsakamoto/BlazingStory), a clone of "Storybook" for Blazor, a frontend workshop for building UI components and pages in isolation.

The site is a Blazor WebAssembly app. It loads versioned Markdown files from `Docs/wwwroot/Docs` at runtime and renders them in the browser.

## Repository Overview

- **Purpose.** Host and manage the documentation for Blazing Story.
- **Framework.** Built with Blazor WebAssembly (.NET 10).

## Related Repository

- Main Blazing Story GitHub. [https://github.com/jsakamoto/BlazingStory](https://github.com/jsakamoto/BlazingStory)

## Build Instructions

**Requirements**
- .NET 10 SDK

### Development Server

```bash
dotnet run --project Docs
```

This starts the site at `http://localhost:5030`.

### Production Build

```bash
dotnet publish Docs -c Release
```

This generates the static site under `Docs/bin/Release/net10.0/publish/wwwroot`.

## License

Copyright (c) 2025-2026 J.Sakamoto

This documentation is licensed under the [CC BY-SA 4.0](LICENSE).  
You are free to share and adapt the material as long as you provide appropriate credit and distribute your contributions under the same license.
