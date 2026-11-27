# 🔥 Hot Reloading [Preview] [Unstable]

Unfortunately, the hot reloading feature won't work well on the "Blazing Story". Now, contributors are working on enabling the hot reloading feature for Blazing Story, but it is not completed yet.

However, if you want to try it, you can do that by setting the `EnableHotReloading` parameter of the `BlazingStoryApp` component to `true` explicitly in your `App.razor` file.

```html
<BlazingStoryApp EnableHotReloading="true">
    <!-- ... -->
</BlazingStoryApp>
```

After doing that, you will see the changes you made in the "stories" files, and component library projects will be reflected in the preview area of "Blazing Story" without restarting the app.

But please remember that it is really unstable. In our experience, it doesn't work on a "Doc" page. Visiting a "Doc" page often stops the entire "Blazing Story" app. Once it happens, there is no way to recover it except to re-launch the app as far as we know (when you use the `dotnet watch` command, hit Ctrl + R).

Therefore, the hot reloading feature is still a preview feature. We are working on it, but we cannot guarantee that it will work well in the future.
