# MCP Server Feature

## Summary

Blazing Story provides an **MCP server** feature that allows Blazing Story to expose information about its components, stories, and documentation pages to AI agents, enabling highly accurate code generation and documentation lookup.

To catch up on the power of the Blazing Story's MCP server feature, you can watch the following introduction video.

<video controls width="100%" style={{maxWidth: '800px'}}>
  <source src="https://github.com/user-attachments/assets/1e4ced81-8d06-4714-ad89-4b0987d958c9" type="video/mp4" />
  Your browser does not support the video tag.
</video>

Currently, the MCP server feature is available only in the Blazing Story app running on Blazor Server. It is not available in the Blazing Story app running on Blazor WebAssembly.

## Available Tools

Once an AI agent is connected to the Blazing Story MCP server, the following tools become available to it. They fall into two groups: tools for exploring **components and their stories**, and tools for accessing **custom documentation pages**.

### Component and Story Tools

These tools let an AI agent discover exactly which components your catalog offers and how to use them correctly — so it can generate code that matches your real component APIs instead of guessing.

| Tool | What it does |
| --- | --- |
| `getComponents` | Lists every component in the catalog with its name, type, and a short summary. *Gives the agent a map of what is available before it writes any code.* |
| `getComponentParameters` | Returns all parameters of a given component — their names, types, summaries, and allowed option values (such as enum choices). *Prevents the agent from inventing non-existent parameters or passing invalid values.* |
| `getComponentStories` | Returns all stories defined for a given component, each with a ready-to-use code snippet. *Lets the agent follow your established usage patterns by learning from working examples.* |

### Custom Documentation Page Tools

These tools expose your hand-written custom pages and Markdown pages — getting-started guides, setup instructions, design notes, and the like — so the agent can consult your project's own documentation instead of relying on general knowledge.

| Tool | What it does |
| --- | --- |
| `getCustomPages` | Lists all custom pages and Markdown pages by title. |
| `getCustomPageContent` | Returns the full rendered content of a page identified by its title. |
| `searchCustomPages` | **Performs a full-text search across all pages and returns the most relevant ones ranked by BM25 Okapi relevance scoring**, each accompanied by a contextual snippet around the matched terms. *The agent can find the right guidance by keyword — without knowing the exact page title — which makes even large documentation sets genuinely discoverable.* |

## Create A New Blazing Story App With The MCP Server Feature

When you create a new Blazing Story app project, you can enable the MCP server feature by checking the "Enable the MCP server feature" checkbox in the project creation dialog.

![Creating a new Blazing Story app with the MCP Server option](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/add-a-new-project-mcp-option.png)

Or, if you are working on dotnet CLI, you can do that by running the `dotnet new blazingstoryserver` command with the `-mcp` option, like below:

```shell
dotnet new blazingstoryserver -mcp
```

## Migration Your Blazing Story App To Enable The MCP Server Feature

To enable the MCP server feature in your existing Blazing Story app, you need to follow these steps:

1. Upgrade your Blazing Story app project to reference the latest version of the Blazing Story NuGet package.

2. Add the `BlazingStory.McpServer` package reference to your Blazing Story app project.

   If you are working on Visual Studio, you can do that by right-clicking the project in the solution explorer and selecting "Manage NuGet Packages...", then searching for the `BlazingStory.McpServer` package and installing it.

   If you are working on dotnet CLI, you can do that with the following command:

   ```shell
   dotnet add package BlazingStory.MCPServer
   ```

3. Add the `AddBlazingStoryMcpServer` method call to the `Program.cs` file of your Blazing Story app project to register the MCP server services in the dependency injection container.

   ```csharp title="📄 Program.cs"
   ...
   // 👇 Add the necessary using directive for the MCP server.
   using BlazingStory.McpServer;
   ...
   var builder = WebApplication.CreateBuilder(args);
   ...
    // 👇 Add services to the container.
   builder.Services.AddBlazingStoryMcpServer();
   ...
   ```  
4. Add the `MapBlazingStoryMcp` method call to the `app` object in the `Program.cs` file to register the Blazing Story MCP server middleware.

   ```csharp title="📄 Program.cs"
   ...
   app.UseHttpsRedirection();

   // 👇 Register the Blazing Story MCP server middleware.
   app.MapBlazingStoryMcp();
   app.MapStaticAssets();
   app.UseRouting();
   app.UseAntiforgery();
   ...
   ```

## Usage

The transport type of the MCP server feature of the Blazing Story is **Streamable HTTP** and **SSE**. It doesn't support STDIO transport type for now.

Once you have enabled the MCP server feature in your Blazing Story app, you can access the MCP server at the `/mcp/blazingstory` endpoint of your Blazing Story app for Streamable HTTP access, or at the `/mcp/blazingstory/sse` endpoint for SSE access.
