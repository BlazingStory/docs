# ✍️ Documentation Enhancement

By default, the "Docs" pages in Blazing Story do not contain detailed descriptions.

![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/docs-page-with-no-description.png)

## Adding Descriptions to Parameters

To add more detailed descriptions of component parameters to the "Docs" pages, write a summary of the component parameters in your UI component’s .razor file using the ["XML document comment"](https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/) format, as shown below.

```csharp
@code {
    ...
    // 👇 Add these triple slash comments.
    //    See also: https://learn.microsoft.com/dotnet/csharp/language-reference/xmldoc/

    /// <summary>
    /// Set a text that is button caption.
    /// </summary>
    [Parameter, EditorRequired]
    public string? Text { get; set; }

    /// <summary>
    /// Set a color of the button. <see cref="ButtonColor.Default"/> is default.
    /// </summary>
    [Parameter]
    public ButtonColor Color { get; set; } = ButtonColor.Default;
    ...
```

And next, enable generating an XML documentation file in your UI component library's project file (.csproj).

If you are working on Visual Studio, you can do that by turning on the option `Documentation file - Generate a file containing API documentation" from the property GUI window of the project (You can find that option in the "Build" > "Output" category. You can also find it more easily by searching with the "Documentation file" keyword inside the project property GUI window).

![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/configure-xml-doc-comment.png)

Or, you can also do that by adding the `<GenerateDocumentationFile>` MSBuild property with `true` in a project file (.csproj) of your UI component library, like below.

```xml
<!-- 📄 This is a project file (.csproj) of your UI component library -->
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    ...
    <!-- 👇 Add this entry. -->
    <GenerateDocumentationFile>True</GenerateDocumentationFile>
    ...
```

After doing that, the XML documentation comments you added to parameters will appear on the "Docs" pages.

![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/docs-page-with-parameter-description.png)

## Adding Descriptions to Component

To add a description for the component itself, rather than for each of its parameters, you have to create a partial class file for the .razor file.

For example, if you have a "Button.razor" component, you should add a corresponding "Button.razor.cs" file to your project and include the component's summary in that .razor.cs file, as shown below.

```csharp
// 📄 This is a partial class file "Button.razor.cs" 
//     of the "Button.razor" Razor component file.

// 👇 Add the triple slash comments to the component class.

/// <summary>
/// This is basic button component.
/// </summary>
public partial class Button
{
}
```

After doing that, the XML documentation comments you added will appear on the "Docs" pages.

![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/docs-page-with-component-description.png)

## Adding Descriptions to Individual Stories

You can provide descriptions for individual stories in addition to component and parameter descriptions. This is useful for explaining a particular story's specific context or purpose.

To add a description to a story, use the `<Description>` render fragment parameter within the `<Story>` component in your `.stories.razor` file. The content of the `<Description>` render fragment will be rendered in the "Docs" page, below the story's name.

Here's an example of how to add a description to a story:

```html
    ...
    <Story Name="Default">
        <!--
        👇 Add a description for this story inside 
        the <Description> render fragment, as shown below:
        -->
        <Description>
            <b>Description:</b>
            <span>
            This section describes the usual usage of
            the <code>&lt;IconButton&gt;</code> component.
            </span>
        </Description>
        ...
```

This will display the provided HTML content as the description for the "Default" story on the "Docs" page, as shown below:

![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/docs-page-with-story-description.png)

You can use any HTML markup within the `<Description>` render fragment to format your story's description.
