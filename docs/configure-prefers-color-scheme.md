# ⚙️ Configure prefers color scheme

By default, the "Blazing Story" app respects the system's color scheme, such as "dark" or "light". If you want to keep the "Blazing Story" app's color scheme to be either "dark" or "light" regardless of the system's color scheme, you can do that by setting the `AvailableColorScheme` parameter of the `BlazingStoryApp` component in your `App.razor` file.

If you set that parameter with `Dark` or `Light`, except `Both`, the "Blazing Story" app will be displayed with the specified color scheme regardless of the system's color scheme.

```razor
@* 📄 App.razor *@
<BlazingStoryApp 
    Assemblies="[typeof(App).Assembly]"  
    AvailableColorSchemes="AvailableColorSchemes.Light">
    @* This app will be displayed "light" mode only 👆*@
</BlazingStoryApp>
```
