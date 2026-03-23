# Creating Custom Addons

This page walks you through creating your own Blazing Story addon (a custom toolbar button, panel tab, or preview decorator) step by step.

## Prerequisites

To create a custom addon, you need to reference the `BlazingStory.Addons` NuGet package. If you are adding the addon directly inside your existing Blazing Story app project, the package is already available transitively via `BlazingStory`. If you are creating a **separate Razor Class Library (RCL)** for reuse across projects, add the package explicitly:

```shell
dotnet add package BlazingStory.Addons
```

## Step 1: Create the Addon Class

An addon is a plain C# class that implements the `IAddon` interface from the `BlazingStory.Addons` namespace. Its `Initialize` method receives an `IAddonBuilder` and registers any combination of toolbar content, panel, and preview decorator components:

```csharp
using BlazingStory.Addons;

public class MyCustomAddon : IAddon
{
    public void Initialize(IAddonBuilder builder)
    {
        // Register a toolbar content component (shown only in Story view)
        builder.AddToolbarContent<MyToolbarContent>(
            order: 300,
            match: viewMode => viewMode == ViewMode.Story);

        // Register a panel tab component (shown only in Story view)
        builder.AddPanel<MyPanel>(
            order: 300,
            match: viewMode => viewMode == ViewMode.Story);

        // Register a preview decorator (placed alongside the story in the preview frame)
        builder.AddPreviewDecorator<MyPreviewDecorator>();
    }
}
```

### Order parameter

The `order` parameter controls the position of the component relative to other registered toolbar items or panels. Lower values appear first (further to the left in the toolbar, or as earlier tabs in the panel). Built-in addons use orders in the range 100-500, so values like `300` or higher are safe for custom addons.

### ViewMode

The `match` predicate receives a `ViewMode` value and should return `true` when the component should be visible. `ViewMode` has three members:

| Value | Description |
|-------|-------------|
| `ViewMode.Story` | The individual story canvas view |
| `ViewMode.Docs` | The documentation page view |
| `ViewMode.CustomPage` | A custom page defined by the user |

Use `ViewMode.Story` for most toolbar and panel addons. To show a component in all views, return `true` unconditionally:

```csharp
match: _ => true
```

## Step 2: Create the Razor Components

### Toolbar Content Component

A toolbar content component is a `.razor` file that renders a button, icon, or drop-down in the toolbar area. The `GlobalArguments` parameter is a shared key-value store for passing state from the toolbar to the preview decorator (see [Preview Decorator Component](#preview-decorator-component) below).

```html
@* MyToolbarContent.razor *@

<button @onclick="OnClick">
    @(_enabled ? "✔ Enabled" : "Disabled")
</button>

@code {
    /// <summary>Shared state container for communicating with the preview decorator.</summary>
    [Parameter]
    public required GlobalArguments Globals { get; init; }

    private bool _enabled = false;

    private void OnClick()
    {
        _enabled = !_enabled;
        Globals["myAddon.enabled"] = _enabled;
    }
}
```

### Panel Component

A panel component is a `.razor` file that appears as a new tab in the addon panel below the canvas. Use the `<PanelTitle>` component to supply the tab label. The `<PanelTitle>` content is rendered via a `SectionContent` mechanism; it does not appear inline in your component markup.

```html
@* MyPanel.razor *@

@* Set the tab title *@
<PanelTitle>My Panel</PanelTitle>

@* Panel body *@
<div style="padding: 16px;">
    <p>Custom panel content goes here.</p>
    <p>Value from toolbar: @_value</p>
</div>

@code {
    private string? _value;

    // Optionally, inject services or cascade values here.
}
```

### Preview Decorator Component

A preview decorator is a Blazor component that is **rendered as a sibling** of the story in the same preview frame. It does not wrap or receive the story content; instead, it renders its own HTML (for example, a `<style>` tag, an overlay `<div>`, or nothing visible at all) and uses the cascading parameters below to react to the current story and toolbar state.

| Cascading parameter | Type | Description |
|---------------------|------|-------------|
| `[CascadingParameter(Name = "Globals")]` | `IReadOnlyDictionary<string, string>?` | Global arguments written by toolbar content components |
| `[CascadingParameter(Name = "Args")]` | `IReadOnlyDictionary<string, string>?` | The current story's control arguments |
| `[CascadingParameter(Name = "Story")]` | `IStory?` | The currently displayed story |

A common pattern is to inject a `<style>` tag whose content depends on the `Globals` dictionary, similar to how the built-in Grid addon conditionally loads a stylesheet. The following example renders a CSS outline on the preview body when the toolbar toggle is on:

```html
@* MyPreviewDecorator.razor *@

@* Inject a CSS rule into the preview frame when the toggle is enabled. *@
@if (_enabled)
{
    <style>
        body { outline: 3px solid cornflowerblue !important; }
    </style>
}

@code {
    /// <summary>Reads global arguments written by toolbar components.</summary>
    [CascadingParameter(Name = "Globals")]
    public IReadOnlyDictionary<string, string>? Globals { get; set; }

    private bool _enabled =>
        Globals?.TryGetValue("myAddon.enabled", out var v) == true && v == "True";
}
```

:::note
Values in `Globals` are always serialized as strings. A `bool` written as `Globals["key"] = true` arrives in the decorator as the string `"True"`.
:::

For side effects that require JavaScript (such as changing background color or intercepting DOM events), inject `IJSRuntime` and invoke your JS from `OnAfterRenderAsync`, exactly how the built-in Background and Actions addons work.

## Step 3: Register the Addon

Addons are registered in the `<BlazingStoryApp>` component via the `OnInitialize` callback parameter. This callback receives an `IBlazingStoryBuilder` which exposes the `Addons` property of type `IAddonStore`:

```html
@* e.g. Components/Pages/Index.razor in a Blazor Server app *@

<BlazingStoryApp
    Assemblies="@(new[] { typeof(App).Assembly })"
    OnInitialize="@(builder => builder.Addons.Register<MyCustomAddon>())" />
```

To register multiple addons, use a separate method:

```html
<BlazingStoryApp
    Assemblies="@(new[] { typeof(App).Assembly })"
    OnInitialize="@ConfigureBlazingStory" />

@code {
    private void ConfigureBlazingStory(IBlazingStoryBuilder builder)
    {
        builder.Addons.Register<MyCustomAddon>();
        builder.Addons.Register<AnotherAddon>();
    }
}
```

:::note
The `IBlazingStoryBuilder` interface is in the `BlazingStory.Configurations` namespace. Add a `@using BlazingStory.Configurations` directive if the type is not resolved automatically.
:::

## Complete Example

Here is a minimal end-to-end example of an addon that changes the canvas border color when enabled:

**`MyBorderAddon.cs`**
```csharp
using BlazingStory.Addons;

public class MyBorderAddon : IAddon
{
    public void Initialize(IAddonBuilder builder)
    {
        builder.AddToolbarContent<MyBorderToolbar>(order: 300,
            match: viewMode => viewMode == ViewMode.Story);
        builder.AddPreviewDecorator<MyBorderDecorator>();
    }
}
```

**`MyBorderToolbar.razor`**
```html
<button title="Toggle border" @onclick="OnClick">
    @(_on ? "Border On" : "Border Off")
</button>

@code {
    [Parameter] public required GlobalArguments Globals { get; init; }
    private bool _on;

    private void OnClick()
    {
        _on = !_on;
        Globals["border.enabled"] = _on;
    }
}
```

**`MyBorderDecorator.razor`**
```html
@* Renders a CSS outline on the preview body when the toggle is on. *@
@if (_on)
{
    <style>
        body { outline: 3px solid royalblue !important; }
    </style>
}

@code {
    [CascadingParameter(Name = "Globals")] public IReadOnlyDictionary<string, string>? Globals { get; set; }
    private bool _on => Globals?.TryGetValue("border.enabled", out var v) == true && v == "True";
}
```

**Registration in `Index.razor`**
```html
<BlazingStoryApp
    Assemblies="@(new[] { typeof(App).Assembly })"
    OnInitialize="@(b => b.Addons.Register<MyBorderAddon>())" />
```
