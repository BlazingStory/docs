# 🎀 Customize the title and brand logo

## Customize the title

If you want to customize the title of your Blazing Story app, set the `Title` parameter of the `BlazingStoryApp` component in your `App.razor` file.

```html
@* 📄 App.razor *@
<BlazingStoryApp Title="My Story" ...>
</BlazingStoryApp>
```

The string value specified in the `Title` parameter of the `BlazingStoryApp` will appear in the title of your Blazing Story app document and in the brand logo area placed at the top of the sidebar.

![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/change-the-title.png)

## Customize the brand logo

You can also replace the brand logo contents at the top of the sidebar entirely by marking up the `BrandLogoArea` render fragment parameter of the `BlazingStoryApp` component, like below.

```html
@* 📄 App.razor *@
<BlazingStoryApp Title="My Story" ...>

    <!-- replace the brand logo contents entirely. -->
    <BrandLogoArea>
        <div style="font-family:'Comic Sans MS';">
            @context.Title
        </div>
    </BrandLogoArea>

</BlazingStoryApp>
```

![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/customize-brand-logo.png)

You can refer to the `BlazingStoryApp` component instance via the `context` argument, so you can retrieve the title string, which is specified in the `Title` parameter of the `BlazingStoryApp` component.

> [!Note]  
> The 'BlazingStoryApp' component instance is also provided as a cascade parameter value. So you can get the reference to the instance of the `BlazingStoryApp` component anywhere in your Razor component implemented in your Blazing Story app.

If you want to only customize the logo icon or the URL of the link, not want to change the entire brand logo HTML contents, you can use the `BrandLogo` built-in component instead. The `BrandLogo` component will render the default design of the brand logo of the Blazing Story app and expose some parameters, such as `IconUrl`, `Url`, and `Title`, to customize them.

```html
@* 📄 App.razor *@
<BlazingStoryApp Title="My Story" ...>
    <BrandLogoArea>
        <BrandLogo IconUrl="https://github.githubassets.com/apple-touch-icon-76x76.png" Url="..." />
    </BrandLogoArea>
</BlazingStoryApp>
```

![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/customize-brand-logo-icon.png)

> [!Note]  
> The `BrandLogo` component uses the `Title` parameter of the `BlazingStoryApp` component by default for rendering the title text inside it. But if the `BrandLogo` component's `Title` parameter is specified, the `BrandLogo` component uses it rather than the `Title` parameter of the `BlazingStoryApp` component.
