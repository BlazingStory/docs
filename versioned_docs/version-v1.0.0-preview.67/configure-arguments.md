# ⚙️ Configure arguments 

## Configure the control type

You can configure the type of control for the component parameters via the `<ArgType>` component inside the `<Stories>` component. The `For` lambda expression parameter of the `<ArgType>` component indicates which parameters to be applied the control type configuration. And the `Control` parameter of the `<ArgType>` component is used for specifying which type of control uses in the "Control" panel to input parameter value.

For example, if you apply the `ControlType.Color` control type to the `BackgroundColor` parameter like this, 

```html
<Stories TComponent="Button">
    <ArgType For="_ => _.BackgroundColor" Control="ControlType.Color" />
    ...
```

you will be able to specify the component's background color with the color picker, like in the following picture.

![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/control-type-colorpicker.png)

For another example, you can make the "Control" panel use dropdown list control to choose one of the values of the `enum` type instead of the radio button group control, with the following code.

```html
<Stories TComponent="Button">
    ....
    <ArgType For="_ => _.Size" Control="ControlType.Select" />
    ...
```

Then, you will get the result in the following picture.

![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/control-type-dropdownlist.png)

## Configure the initial value of the component parameter

You can specify the initial value of the component parameter by using the `<Arg>` component inside the `<Arguments>` render fragment parameter of the `<Story>` component like this:

```html
    ...
    <Story Name="Secondary">
        <Arguments>
            <Arg For="_ => _.Primary" Value="false" />
        </Arguments>
        ...
```

![](https://raw.githubusercontent.com/jsakamoto/BlazingStory/main/assets/readme-images/configure-arguments-arg.png)

## Configure component parameters which type is `RenderFragment`

Blazing Story supports the `RenderFragment` type parameter in its control panel. For example, if you have a story for a component such as having a `RenderFragment ChildContent` parameter, you can dynamically set text to that `ChildContent` parameter from the control panel UI at runtime.

(NOTICE: Currently, Blazing Story allows you to set only text to the `RenderFragment` type parameter in its control panel UI. You cannot set fragments consisting of other components or HTML tags to the `RenderFragment` type parameter. This is a limitation of Blazing Story.)

However, if you mark up the `ChildContent` parameter inside of the component's markup, you will not be able to set text to that parameter from the control panel UI. Because the `ChildContent` parameter is already set with the component's markup.

```html
    ...
    <Story Name="Default">

        <Template>
            <MyButton @attributes="context.Args">
                <!-- ❌ DON'T DO THIS! -->
                Click me
            </MyButton>
        </Template>
        ...
```

Instead, you should set the `ChildContent` parameter through the `<Arguments>` render fragment parameter of the `<Story>` component, like below.

```html
    <!-- 👍 DO THIS! -->
    ...
    <Story Name="Default">

        <Arguments>
            <Arg For="_ => _.ChildContent" Value="_childContent" />
        </Arguments>

        <Template>
            <MyButton @attributes="context.Args">
            </MyButton>
        </Template>
        ...
@code
{
    RenderFragment _childContent = @<text>Click me</text>;
}
```
