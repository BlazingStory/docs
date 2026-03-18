# Sorting navigation tree items

By default, items at the same level in the sidebar navigation tree are sorted alphabetically by title, with containers/components listed before custom pages. (Even if a custom page’s title would come first alphabetically, it appears after all containers/components.)

To override the default order, set the `NavigationTreeOrder` parameter of the `BlazingStoryApp` in your `App.razor`. This parameter accepts a declarative list built with `NavigationTreeOrderBuilder.N[...]`.

Rules:
- Docs and Story items under a component keep their original order and are always listed first.
- If an item is immediately followed by an `N[...]` group, that group specifies the order of that item’s children (applied recursively).
- Unspecified siblings are appended in the default order (alphabetical, components before custom pages).

Basic example (top-level and nested ordering):

```razor
@* 📄 App.razor *@
@using static BlazingStory.Types.NavigationTreeOrderBuilder

<BlazingStoryApp Assemblies="[typeof(App).Assembly]" 
    NavigationTreeOrder="@N[
      "Welcome",                    // top-level items 
      "Components", 
         N["Layouts",               // children under "Components" 
             N["Header", "Footer"], // children under "Layouts" 
           "Button"], 
      "Templates"]" />
```

Notes:

- Titles must match the captions shown at that level (e.g., top-level “Components”, then child “Layouts”, etc.).
- Even if you list custom pages inside a component’s children, they still appear after Docs and Story items for that component.
- Any items not mentioned in `NavigationTreeOrder` remain sorted by the default.
 