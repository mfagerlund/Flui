# Flui
Fluid Unity UI Toolkit binding. Allows you to easiy connect backend code to UI Toolkit frontend.

## Example
Flui binds through the name of a component - or actually a Q query, but that typically means a name. Given a simple ui that looks like this:

```xml
<ui:UXML xmlns:ui="UnityEngine.UIElements" xmlns:uie="UnityEditor.UIElements" xsi="http://www.w3.org/2001/XMLSchema-instance" engine="UnityEngine.UIElements" editor="UnityEditor.UIElements" noNamespaceSchemaLocation="../../UIElementsSchema/UIElements.xsd" editor-extension-mode="False">
    <Style src="project://database/Assets/Bootstrap/BootstrapUss.uss?fileID=7433441132597879392&amp;guid=534b208ba7f75194ebac2458c626ada3&amp;type=3#BootstrapUss" />
    <Style src="project://database/Assets/MainMenu/MainMenuUss.uss?fileID=7433441132597879392&amp;guid=7918688154ada1843a1f112b7a379fa9&amp;type=3#MainMenuUss" />
    <ui:VisualElement style="flex-grow: 1; align-items: center; justify-content: center;">
        <ui:Label tabindex="-1" text="Select Demo" display-tooltip-when-elided="true" class="h3 menu-item" />
        <ui:Button text="Game Settings Menu Demo" display-tooltip-when-elided="true" name="GameSettingsMenu" class="btn-primary menu-item" />
        <ui:Button text="Bootstrap Demo" display-tooltip-when-elided="true" name="BootstrapDemo" class="btn-primary menu-item" />
        <ui:Label tabindex="-1" text="Time: 15:22:11" parse-escape-sequences="true" display-tooltip-when-elided="true" name="Time" class="menu-item" style="-unity-text-align: upper-right;" />
    </ui:VisualElement>
</ui:UXML>
```

You can bind the buttons to actions - and the label to code that generates text.

```csharp
private readonly FluiBinderRoot<MainMenu, VisualElement> _root = new();

private void Update()
{
	if (_document == null)
	{
		_document = GetComponent<UIDocument>();
	}

	if (_document == null)
	{
		throw new InvalidOperationException("_document not assigned");
	}

	_root.BindGui(this, _document.rootVisualElement,
		x => x
			.Button("BootstrapDemo", ctx => ShowBootstrapDemo())
			.Button("GameSettingsMenu", ctx => ShowGameSettings())
			.Label("Time", ctx => $"Time: {DateTime.Now:hh:mm:ss}")
	);
}
```
