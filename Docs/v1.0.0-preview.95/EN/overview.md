---
slug: /
title: Overview
---

# ![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/icon.min.64x64.svg) Blazing Story

The clone of ["Storybook"](https://storybook.js.org/) for Blazor, a frontend workshop for building UI components and pages in isolation.

[![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/social-preview.png)](https://jsakamoto.github.io/BlazingStory/)

The "Blazing Story" is built on **almost 100% Blazor native** (except only a few JavaScript helper codes), so you don't have to care about `npm`, `package.json`, `webpack`, and any JavaScript/TypeScript code. You can create a UI catalog application **on the Blazor way!**

In addition, Blazing Story integrates with AI coding assistants in two complementary ways: an [**MCP server feature**](./mcp-server-feature) that exposes story and component metadata to AI agents at runtime, and [**agent skills**](./agent-skills) that teach AI tools how to author stories and addons in idiomatic Blazing Story style.

And once your components are cataloged, you can protect their appearance with [**visual regression testing**](./visual-regression-testing): a project template turns every story in your app into an automated screenshot test, so unintended visual changes are caught before they ship.

You can try it out from the live demonstration site at the following link: https://jsakamoto.github.io/BlazingStory/
