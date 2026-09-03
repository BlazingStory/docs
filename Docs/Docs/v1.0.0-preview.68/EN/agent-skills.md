# Agent Skills

Blazing Story publishes **agent skills** that help AI coding assistants generate idiomatic Blazing Story code on your behalf — for example, scaffolding a `.stories.razor` file or wiring up a custom addon.

> [!IMPORTANT]
> The skills are maintained in a dedicated repository. **For the current list of skills and the most up-to-date instructions, refer to the source of truth:**
>
> ➡️ **https://github.com/BlazingStory/agent-skills**

<video controls width="100%" style={{maxWidth: '800px'}}>
  <source src="https://github.com/user-attachments/assets/aeea68f9-5ff0-4edf-aa57-6d6725ea88a9" type="video/mp4" />
  Your browser does not support the video tag.
</video>

## Available Skills

| Skill | What it does |
|---|---|
| `blazing-story-story` | Generates a `.stories.razor` file for a Blazor UI component |
| `blazing-story-addon` | Generates and registers a custom addon (toolbar item, panel tab, or preview decorator) |

## Installation

Skills are installed through the [GitHub CLI](https://cli.github.com/) using the `gh skill` subcommand:

```shell
gh skill install BlazingStory/agent-skills blazing-story-story
gh skill install BlazingStory/agent-skills blazing-story-addon
```

See the [agent-skills repository](https://github.com/BlazingStory/agent-skills) for the supported `gh` version and any additional prerequisites.

## Usage

Once installed, your AI assistant invokes the matching skill automatically when it recognizes a relevant request, such as:

- *"Create a story for the Button component"* → triggers `blazing-story-story`
- *"Add a dark mode toggle addon"* → triggers `blazing-story-addon`

## See Also

- [MCP Server Feature](./mcp-server-feature) — exposes story and component metadata to AI agents at runtime, complementing the static knowledge provided by the agent skills.
