# Flui
Fluid Unity UI Toolkit binding. Allows you to easiy connect backend code to UI Toolkit frontend.

Install Flui by opening Package Manager in Unity, pressing the "+" symbol and selecting "Install Package From GIT url", use this url: https://github.com/mfagerlund/Flui.git?path=/Assets/Package

## FluiBinder vs FluiCreator
FluiBinder is used when you have a UIDocument that you want to bind in code (bind meaning populate texts or execution button actions)

FluiCreator is used when you want to create the VisualElements in code, and you also want to bind them.

## Included Resources
Flui includes a number of scripts - described below, and also two USS files, one including "required" classes and one with a USS that emulates Bootstrap to some extent.

For demos, clone https://github.com/mfagerlund/Flui.git and look at the included project.

## FluiCreator
### Simple Example
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

### Involved Example
In this example there is a number of buttons that control what panel is visible - and each panel contains a large number of fields.

Note that these convenience functions sometimes allow you to switch context - in effect to drill down into a more complex data structure as you're binding values.

```csharp
_root.BindGui(this, _document.rootVisualElement, x => x
	.Label("compact-settings", ctx => ctx.Settings.CompactString)
	.EnumButtons(
		"left-panel",
		ctx => ctx.ActivePanel,
		b => b
			.EnumButton(Panel.ControlSettings)
			.EnumButton(Panel.ScreenSettings)
			.EnumButton(Panel.VolumeSettings)
			.EnumButton(Panel.GraphicSettings)
			.EnumButton(Panel.KeyboardSettings))
	.EnumSwitch(
		"right-panel",
		ctx => ctx.ActivePanel, p => p
			.Case(
				"ControlSettingsPanel", Panel.ControlSettings, ctx => ctx.Settings.ControlSettings, c => c
					.Toggle("SimpleControls", t => t.SimpleControls)
					.Toggle("Vibration", t => t.Vibration)
					.Toggle("ButtonConfiguration", t => t.ButtonConfiguration)
					.Slider("CameraDistance", t => t.CameraDistance, lowValue: 1, highValue: 20)
					.Toggle("ScreenVibration", t => t.ScreenVibration)
					.Toggle("ShowSpecialAttack", t => t.ShowSpecialAttack)
					.TextField("UserName", t => t.UserName)
			)
			.Case("ScreenSettingsPanel", Panel.ScreenSettings, ctx => ctx.Settings.ScreenSettings, c => c
				.IntegerField("Width", t => t.Width)
				.IntegerField("Height", t => t.Height)
				.FloatField("PixelDensity", t => t.PixelDensity)
				.DropdownField("ColorMode", t => t.ColorModeId)
				.EnumField("CycleMode", t => t.CycleMode)
			)
			.Case("VolumeSettingsPanel", Panel.VolumeSettings, ctx=>ctx)
			.Case("GraphicSettingsPanel", Panel.GraphicSettings, ctx=>ctx)
			.Case("KeyboardSettingsPanel", Panel.KeyboardSettings, ctx=>ctx)
	)
	.Button("Ok", ctx => Hide())
	.Button("Return", ctx => Hide())
);
```

## FluiCreator

Here's an example where the entire ui is created in code instead of just being bound after the fact.

```csharp
    public class ListUiCreator
    {
        private readonly List<Office> _offices = Office.CreateOfficeList();
        private FluiCreatorRoot<ListUiCreator, VisualElement> _root = new();

        private void Update()
        {
            Bind();
        }

        private void Bind()
        {
			_document ??= GetComponent<UIDocument>();
            _root.CreateGui(this, _document.rootVisualElement, r => r
                .VisualElement("unnamed0", "row", unnamed0 => unnamed0
                    .Label("ListExamples", _ => "List Examples", "h2")
                    .Button(_ => AddOffice(), "btn-primary")
                    .Button(_ => RandomizeSalaries(), "btn-primary")
                    .Button(_ => Close(), "btn-danger")
                )
                .ScrollView("unnamed1", "", unnamed1 => unnamed1
                    .VisualElement("Root", "", root => root
                        .VisualElement("Offices", "", offices => offices
                            .VisualElement("ve", "", ve => ve
                                .ForEach(x => x._offices,
                                    "",
                                    office => office
                                        .VisualElement("unnamed2", "row", unnamed2 => unnamed2
                                            .Label("Label", _ => "Office: ", "h3")
                                            .Label(x => x.Name, "h3")
                                            .Button("DeleteOffice", "Delete Office", "btn-warning", x => DeleteOffice(x.Element, x.Context))
                                        )
                                        .VisualElement("unnamed3", "", unnamed3 => unnamed3
                                            .VisualElement("List", "table", list => list
                                                .VisualElement("Header", "tr", header => header
                                                    .Label("Name", _ => "Name", "th")
                                                    .Label("Title", _ => "Title", "th")
                                                    .Label("Salary", _ => "Salary", "th")
                                                    .VisualElement("unnamed4", "", unnamed4 => unnamed4
                                                        .Button("Add", "Add", "btn-primary, btn-sm", _ => office.Context.AddRandomEmployee())
                                                    )
                                                )
                                                .ForEach(x => x.Employees, "tr", employee => employee
                                                    .Label(x => x.Name, "td")
                                                    .Label(x => x.Title, "td")
                                                    .Label("salary", x => $"{x.Salary:0}", "td")
                                                    .Button("delete", "Delete", "btn-warning", x => DeleteEmployee(x.Element, office.Context, x.Context))
                                                )
                                                .VisualElement("Footer", "tr", footer => footer
                                                    .Label("Name", _ => "", "td")
                                                    .Label("Title", _ => "", "td")
                                                    .Label("Salary", _ => "0", "td")
                                                    .VisualElement("unnamed5", "")
                                                )
                                            )
                                        )
                                )
                            )
                        )
                    )
                )
            );
        }

        private void RandomizeSalaries()
        {
            _offices.ForEach(office =>
            {
                foreach (var employee in office.Employees)
                {
                    employee.Salary = Random.Range(1, 6);
                }

                office.Employees = office.Employees.OrderBy(x => x.Salary).ToList();
            });
        }

        private void DeleteOffice(VisualElement officeElement, Office office)
        {
            FluiHelper.ExecuteAfterClassTransition(
                officeElement,
                "transparent",
                "opacity",
                () => _offices.Remove(office));
        }

        private void AddOffice()
        {
            var office =
                new Office
                {
                    Name = Guid.NewGuid().ToString().Replace("-", "").Substring(0, 6),
                };
            _offices.Add(office);

            for (int i = 0; i < Random.Range(1, 4); i++)
            {
                office.AddRandomEmployee();
            }
        }

        private void DeleteEmployee(
            VisualElement element,
            Office office,
            Employee employee)
        {
            FluiHelper.ExecuteAfterClassTransition(
                element,
                "transparent",
                "opacity",
                () => office.Employees.Remove(employee));
        }
		
		private void Close()
		{
			...
		}
    }
```
