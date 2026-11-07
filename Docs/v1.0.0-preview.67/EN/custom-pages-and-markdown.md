# 📝 Custom Pages & Markdown

Blazing Story allows you to add custom pages that can include any content you want. These pages will be shown in the sidebar's navigation tree. It is helpful for adding a "Welcome" page, "Getting Started" page, or any other custom page. The custom pages are a Razor component so you can use any Blazor features.

You can add a custom page by creating a new Razor component file with the `[CustomPage]` attribute, like below. (The pages only need the `CustomPage` attribute, no filename requirements like "page.custom.razor.")

```csharp
@attribute [CustomPage("Examples/Getting Started")]

<h2>🚀 Getting Started</h2>
<h3>Example scenario</h3>
<p>For the example scenario, you already have a Blazor WebAssembly application project, "MyBlazorWasmApp1", that includes the "Button" component.</p>
...

```

After creating a custom page like the one above, you will see the custom page like the following example.

![Example of Custom Pages](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/example-of-custom-pages.png)

You can set the custom page's position in the sidebar navigation tree by specifying the title string of the `CustomPage` attribute parameter, which includes a slash separator. The slash separator works to represent the hierarchy of the sidebar navigation tree, like the one inside the `Stories` attribute parameter.

## Using Markdown files for Custom Pages

In addition to Razor components, Blazing Story now supports using **pure** Markdown files (.md) as Custom Pages through integration with the [**"MD2RazorGenerator"**](https://github.com/jsakamoto/MD2RazorGenerator) NuGet package [![NuGet Package](https://img.shields.io/nuget/v/MD2RazorGenerator.svg)](https://www.nuget.org/packages/MD2RazorGenerator/). This allows you to write documentation in Markdown rather than Razor syntax for a more streamlined authoring experience.

To use Markdown files as Custom Pages:

1. Add the "MD2RazorGenerator" NuGet package to your Blazing Story project:

```
dotnet add package MD2RazorGenerator
```

2. Create Markdown (.md) files in your project and use YAML frontmatter with the `$attribute` key to specify the `CustomPage` attribute:

```markdown
---
$attribute: CustomPage("Examples/Getting Started")
---
## 🚀 Getting Started<
### Example scenario
For the example scenario, you already have a Blazor WebAssembly application project, "MyBlazorWasmApp1", that includes the "Button" component.
...
```

3. The MD2RazorGenerator will automatically convert your Markdown files into Razor components with the `CustomPage` attribute applied.

This approach combines the simplicity of Markdown for documentation with the power of Blazor's component model.
