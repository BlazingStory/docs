# JavaScript API

## Summary

A running Blazing Story app publishes a global object named **`BlazingStory`** in the browser. It lets external tooling, such as a browser automation script or an end-to-end test runner, ask the app which stories it contains and wait until a view has finished rendering.

The API is available regardless of the Blazor hosting model, so it works the same in a Blazing Story app running on Blazor WebAssembly and on Blazor Server. No package reference, no configuration, and no code of your own is needed to enable it.

This API is what the [visual regression test project template](./visual-regression-testing) is built on, but it is not limited to that use. Any tool that can evaluate JavaScript in the page, such as Playwright, Puppeteer, or Selenium, can use it.

## The `BlazingStory` Global Object

```typescript
interface BlazingStoryAPI {
  getStoryIndex(): Promise<StoryIndex>;
  readyView(): Promise<void>;
}

declare const BlazingStory: BlazingStoryAPI;
```

The global object is installed while the app's scripts are loading, but the methods above depend on the Blazor side being up. Both of them return a `Promise` that simply stays pending until the app is ready, so awaiting them is all you have to do. An automation script should still wait for the global itself to exist before touching it, because the page may be evaluated before the scripts have loaded.

```javascript
await page.waitForFunction(() => typeof BlazingStory !== "undefined");
```

### `getStoryIndex(): Promise<StoryIndex>`

Returns the **story index**, an inventory of everything the Blazing Story app contains: its stories, its "Docs" pages, and its custom pages. The shape is the same as the story index JSON of Storybook, so tooling written for Storybook can consume it as is.

```typescript
interface StoryIndex {
  /** The story index format version. */
  v: number;
  /** All entries, keyed by their ID. */
  entries: Record<string, StoryIndexEntry>;
}

interface StoryIndexEntry {
  /** The ID of the entry, e.g. "example-button--primary" */
  id: string;
  /** The path of the entry in the navigation tree, e.g. "Example/Button" */
  title: string;
  /** The display name of the entry, e.g. "Primary" */
  name: string;
  /** Whether the entry is a rendered story or a documentation page. */
  type: "story" | "docs";
}
```

An example of what it returns:

```json
{
  "v": 1,
  "entries": {
    "example-button--docs": {
      "id": "example-button--docs",
      "title": "Example/Button",
      "name": "Docs",
      "type": "docs"
    },
    "example-button--primary": {
      "id": "example-button--primary",
      "title": "Example/Button",
      "name": "Primary",
      "type": "story"
    }
  }
}
```

The `id` of an entry is what identifies a view in the app's URL. To display one entry in isolation, without the navigation sidebar and the addon panels, navigate to the following URL.

```
/iframe.html?id=<the id of the entry>&viewMode=story
```

> [!NOTE]
> "Docs" pages and custom pages are reported with the `"docs"` type. Filter the entries by `type === "story"` when you are interested in the rendered stories only.

### `readyView(): Promise<void>`

Returns a `Promise` that resolves once the currently displayed view has settled: the components have rendered, the fonts and stylesheets have loaded, and the opening transition has finished.

This is the method to await before capturing a screenshot or asserting on the rendered result. Waiting for the browser's load event is not enough, because a Blazor app continues to render after that point.

## Example

The following Playwright script prints the name of every story, then captures the preview area of each one.

```javascript
// Read the story index from the running app.
await page.goto("http://localhost:5000/");
await page.waitForFunction(() => typeof BlazingStory !== "undefined");
const index = await page.evaluate(() => BlazingStory.getStoryIndex());
const stories = Object.values(index.entries).filter((entry) => entry.type === "story");

for (const story of stories) {
  console.log(`${story.title} - ${story.name}`);

  // Open the story in isolation, and wait until the view has settled.
  await page.goto(`/iframe.html?id=${encodeURIComponent(story.id)}&viewMode=story`);
  await page.waitForFunction(() => typeof BlazingStory !== "undefined");
  await page.evaluate(() => BlazingStory.readyView());

  await page.locator(".preview-story-area").screenshot({ path: `${story.id}.png` });
}
```

## See Also

- [Visual Regression Testing](./visual-regression-testing), a project template that builds a complete screenshot testing project on top of this API.
