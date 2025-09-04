# 🎛️ MCP Server Feature

## Summary

Blzing Story provides a **MCP server** feature that allows Blazing Story to expose information about its components and stories to AI agents, enabling highly accurate code generation.

To catch up he power of the Blazing Story's MCP server feature, you can watch the following introduction video.

https://github.com/user-attachments/assets/1e4ced81-8d06-4714-ad89-4b0987d958c9

Currently, the MCP server feature is available only in the Blazing Story app running on Blazor Server. It is not available in the Blazing Story app running on Blazor WebAssembly.

## Create a new Blazing Story app with the MCP server feature

When you create a new Blazing Story app project, you can enable the MCP server feature by checking the "Enable the MCP server feature" checkbox in the project creation dialog.

![Creating a new Blazing Story app with the MCP Server option](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/add-a-new-project-mcp-option.png)

Or, if you are working on dotnet CLI, you can do that by running the `dotnet new blazingstoryserver` command with the `-mcp` option, like below:

```shell
dotnet new blazingstoryserver -mcp
```

## Migration your Blazing Story app to enable the MCP server feature

To enable the MCP server feature in your existing Blazing Story app, you need to follow these steps:

1. Upgrade your Blazing Story app project to reference the latest version of the Blazing Story NuGet package.

2. Add the `BlazingStory.McpServer` package reference to your Blazing Story app project.

   If you are working on Visual Studio, you can do that by right-clicking the project in the solution explorer and selecting "Manage NuGet Packages...", then searching for the `BlazingStory.McpServer` package and installing it.

   If you are working on dotnet CLI, you can do that with the following command:

   ```shell
   dotnet add package BlazingStory.MCPServer
   ```

3. Add the `AddBlazingStoryMcpServer` method call to the `Program.cs` file of your Blazing Story app project to register the MCP server services in the dependency injection container.

   ```csharp
   // 📄 Program.cs
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

   ```csharp
   // 📄 Program.cs
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

The tranport type of the MCP server feature of the Blazing Story is **Streamable HTTP** and **SSE**. It doesn't support STDIO transport type for now.

Once you have enabled the MCP server feature in your Blazing Story app, you can access the MCP server at the `/mcp/blazingstory` endpoint of your Blazing Story app for Streamable HTTP access, or at the `/mcp/blazingstory/sse` endpoint for SSE access.
