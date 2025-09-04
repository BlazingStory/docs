# 🚀 Getting Started

## System Requirements

.NET SDK ver.8 or later

## Example scenario

For the example scenario, you already have a Blazor WebAssembly application project, "MyBlazorWasmApp1", that includes the "Button" component.

:::note
Blazing Story supports Blazor Server application projects as well as Blazor WebAssembly application projects.
:::

```
📂 (working directory)
    + 📄 MyBlazorWasmApp1.sln
    + 📂 MyBlazorWasmApp1
        + 📄 MyBlazorWasmApp1.csproj
        + ...
        + 📂 Components
            + 📄 Button.razor
            + ...
```

## Preparation

Close all Visual Studio IDE instances (if you use Visual Studio IDE), and install the "Blazing Story" project template with the following command. (This installation instruction is enough to execute once in your development environment.)

```shell
dotnet new install BlazingStory.ProjectTemplates
```

## Creating a Blazing Story app and stories

### Step 1 - Create a new Blazing Story app project

Open the solution file (.sln) with Visual Studio, and add a new **"Blazing Story (WebAssembly App)"** project from the project templates. (In this example scenario, we named it "MyBlazorWasmApp1.Stories")

![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/add-a-new-project.png)

:::note
If you are working on a Blazor Server application project, you should add a new **"Blazing Story (Server App)"** project instead of the "Blazing Story (WebAssembly App)" project.
:::

:::note
To use the MCP server feature, you need to add a new **"Blazing Story (Server App)"** project and check the "Enable the MCP server feature" checkbox in the project creation dialog.  
Note that the MCP server feature is not available in the Blazing Story app when running on Blazor WebAssembly.
:::

If you are working on dotnet CLI, you can do that with the following commands in a terminal.

:::note
Please remind again that this example scenario assumes that there is already a solution file (.sln) in the current directory with an existing Blazor WebAssembly app project.
:::

```shell
# Create a new Blazing Story app
dotnet new blazingstorywasm -n MyBlazorWasmApp1.Stories
# Add the Blazing Story app project to the solution
dotnet sln add ./MyBlazorWasmApp1.Stories/
```

:::note
If you are working on a Blazor Server application project, you should run the `dotnet new blazingstoryserver` command.
:::

:::note
To use the MCP server feature, you need to run the `dotnet new blazingstoryserver -mcp` command.
Note that the MCP server feature is not available in the Blazing Story app when running on Blazor WebAssembly.
:::

The file layout will be the following tree.

```
📂 (working directory)
    + 📄 MyBlazorWasmApp1.sln
    + 📂 MyBlazorWasmApp1
        + ...
    + 📂 MyBlazorWasmApp1.Stories
        + 📄 MyBlazorWasmApp1.Stories.csproj✨ 👈 Add this
```

### Step 2 - Add a project reference of the Blazor Wasm app to the Blazing Story project

Next, add a project reference in the Blazing Story App project "MyBlazorWasmApp1.Stories" that refers to the Blazor WebAssembly app project "MyBlazorWasmApp1".

![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/add-a-project-reference.png)

If you are working on dotnet CLI, you can do that with the following commands in a terminal.

```shell
dotnet add ./MyBlazorWasmApp1.Stories reference ./MyBlazorWasmApp1
```

```
📂 (working directory)
    + 📄 MyBlazorWasmApp1.sln
    + 📂 MyBlazorWasmApp1 <--- refers --+
        + ...                           |
    + 📂 MyBlazorWasmApp1.Stories ------+
        + ...
```

### Step 3 - Add a "stories" file

Add a new "stories" file to the Blazing Story App project "MyBlazorWasmApp1.Stories".

A "stories" file is a normal Razor Component file (.razor), but it is annotated with the `[Stories]` attribute and includes a markup of the `<Stories>` component. There is no restriction on file layout of "stories" files, but usually, we place it in the "Stories" subfolder.

:::warning
Currently, The file name of the "stories" files must end with ".stories.razor". This is a requirement of the naming convention for available the "Show code" feature in the "Docs" pages.
:::

In this example scenario, we are going to write a "stories" for the `Button` component lived in the "MyBlazorWasmApp1" project, so we would add a new story file named "Button.stories.razor" in the "Stories" subfolder where is under the "MyBlazorWasmApp1.Stories" project.

```
📂 (working directory)
    + 📄 MyBlazorWasmApp1.sln
    + 📂 MyBlazorWasmApp1
        + ...
    + 📂 MyBlazorWasmApp1.Stories
        + 📄 MyBlazorWasmApp1.Stories.csproj
        + 📂 Stories
            + 📄 Button.stories.razor✨ 👈 Add this
```

### Step 4 - Implement the "stories"

Implement a stories.

The "Button.stories.razor" would be like the below.

```html
@using MyBlazorWasmApp1.Components
@attribute [Stories("Components/Button")]

<Stories TComponent="Button">

    <Story Name="Primary">
        <Template>
            <Button Label="Button" Primary="true" @attributes="context.Args" />
        </Template>
    </Story>

</Stories>
```

### Step 5 - Run it!

If you are working on Visual Studio, right-click the "MyBlazorWasmApp1.Stories" project in the solution explorer to show the context menu, click the "Set as Startup Project" menu item, and hit the `Ctrl` + `F5` key.

If you are working on dotnet CLI, you can do that with the following commands in a terminal.

```shell
dotnet run --project ./MyBlazorWasmApp1.Stories
```

Then you will see the clone of the "Storybook" built on Blazor! 🎉

![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/first-run-of-blazingstory.gif)
