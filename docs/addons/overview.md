# Addons Overview

## What is an Addon?

Blazing Story supports an **addon mechanism** that lets you extend the UI with your own custom components. You can add items to the toolbar, add tabs to the addon panel below the canvas, or place additional Blazor components alongside the story preview, all by writing standard Blazor components.

An addon is a class that implements the `IAddon` interface. Through the `Initialize(IAddonBuilder builder)` method, the addon registers its Blazor component types with the Blazing Story runtime, which then renders them at the appropriate locations.

## Addon Component Types

There are three types of component slots that an addon can contribute to:

| Type | Location | Typical use |
|------|----------|-------------|
| **Toolbar content** | Toolbar above the story canvas | Toggle switches, drop-down menus, buttons that affect preview state |
| **Panel** | Add-on panel tabs below the canvas | Information displays, logs, property editors |
| **Preview decorator** | Rendered alongside the story in the preview frame | Injecting CSS rules, applying JavaScript-driven side effects (e.g., background color, event interception), or rendering additional DOM content |

## NuGet Packages

The addon API is distributed in separate NuGet packages:

| Package | Purpose |
|---------|---------|
| `BlazingStory.Addons` | Public API: `IAddon`, `IAddonBuilder`, `IAddonStore`, `GlobalArguments`, `ViewMode`, `PanelTitle` |
| `BlazingStory.Addons.BuiltIns` | Built-in addons shipped with Blazing Story |
| `BlazingStory.ToolKit` | Reusable UI components (buttons, menus, icons, etc.) and utilities for building addons |

:::note
The main `BlazingStory` package depends on `BlazingStory.Addons.BuiltIns`, which in turn depends on `BlazingStory.Addons` and `BlazingStory.ToolKit`. When using the standard Blazing Story project templates, all packages are already available. You only need to explicitly reference `BlazingStory.Addons` (and optionally `BlazingStory.ToolKit`) when creating a **separate class library project** for your own addon.
:::

## Built-in Addons

Blazing Story comes with the following addons pre-installed:

### Toolbar Addons

| Addon | Description |
|-------|-------------|
| **Grid** | Toggles a background grid overlay on the story canvas |
| **Background** | Changes the background color of the canvas between light, dark, and transparent |
| **Measure** | Toggles measurement guides to inspect spacing and dimensions |
| **Outlines** | Highlights the outlines of all elements on the canvas |
| **Change Size** | Lets you select a preset viewport size for the canvas |

### Panel Addons

| Addon | Description |
|-------|-------------|
| **Controls** | Displays the interactive controls panel for component parameters |
| **Actions** | Logs component events as they are fired during story interaction |

## Sharing State: GlobalArguments

A toolbar content component often needs to communicate its state to a preview decorator. For example, the **Background** addon toolbar sets a background color that the preview decorator then applies to the canvas.

This is done via `GlobalArguments`, a dictionary-like object provided by the Blazing Story runtime. Both toolbar content components and preview decorators receive a `GlobalArguments` instance as a `[CascadingParameter]`. Any value written by a toolbar component is automatically propagated to the decorator.

```csharp
// Toolbar component: write a value
Globals["myAddon.value"] = someValue;

// Preview decorator: read the value
if (Globals.TryGetValue("myAddon.value", out var value)) { ... }
```

See [Creating Custom Addons](./creating-addons) for a full walkthrough.
