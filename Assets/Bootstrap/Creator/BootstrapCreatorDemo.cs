using Flui.Creator;
using UnityEngine;
using UnityEngine.UIElements;

namespace FluiDemo.Bootstrap.Creator
{
    public class BootstrapCreatorDemo : Fadable
    {
        [SerializeField] private StyleSheet[] _styleSheets;
        [SerializeField] private bool _pause;

        private FluiCreatorRoot<BootstrapCreatorDemo, VisualElement> _root = new();

        private void OnValidate()
        {
            if (Application.isPlaying) return;
            Connect();
            Update();
        }

        private void Update()
        {
            if (RootVisualElement == null)
            {
                Connect();
            }

            if (RootVisualElement == null)
            {
                return;
            }
            
            foreach (var styleSheet in _styleSheets)
            {
                RootVisualElement.styleSheets.Add(styleSheet);
            }

            if (!_pause)
            {
                _root.CreateGui(
                        this,
                        RootVisualElement, x => x
                            // CODE
                            .ScrollView("unnamed0", "", unnamed0 => unnamed0
                                .VisualElement("unnamed1", "row", unnamed1 => unnamed1
                                    .Button("TopClose", "Close", "btn-warning", _ => Hide())
                                ).VisualElement("unnamed2", "row", unnamed2 => unnamed2
                                    .Button("Button", "Primary", "btn-primary", null)
                                    .Button("Button2", "Secondary", "btn-secondary", null)
                                    .Button("Button3", "Success", "btn-success", null)
                                    .Button("Button4", "Info", "btn-info", null)
                                    .Button("Button5", "Warning", "btn-warning", null)
                                    .Button("Button6", "Danger", "btn-danger", null)
                                ).VisualElement("unnamed3", "row", unnamed3 => unnamed3
                                    .Button("Button", "Primary", "btn-primary, disabled", null)
                                    .Button("Button2", "Secondary", "btn-secondary, disabled", null)
                                    .Button("Button3", "Success", "btn-success, disabled", null)
                                    .Button("Button4", "Info", "btn-info, disabled", null)
                                    .Button("Button5", "Warning", "btn-warning, disabled", null)
                                    .Button("Button6", "Danger", "btn-danger, disabled", null)
                                ).VisualElement("unnamed4", "row", unnamed4 => unnamed4
                                    .Label("Typography", _ => "Typography", "h2")
                                ).VisualElement("unnamed5", "row", unnamed5 => unnamed5
                                    .VisualElement("unnamed6", "", unnamed6 => unnamed6
                                        .Label("Heading1", _ => "Heading 1", "h1")
                                        .Label("Heading2", _ => "Heading 2", "h2")
                                        .Label("Heading3", _ => "Heading 3", "h3")
                                        .Label("Heading4", _ => "Heading 4", "h4")
                                        .Label("Heading5", _ => "Heading 5", "h5")
                                        .Label("Heading6", _ => "Heading 6", "h6")
                                    ).VisualElement("unnamed7", "", unnamed7 => unnamed7
                                        .Label("EmphasisClasses", _ => "Emphasis Classes", "h2")
                                        .Label("Primary:Fuscedapibus,tellusaccursuscommodo,tortormaurisnibh.", _ => "Primary: Fusce dapibus, tellus ac cursus commodo, tortor mauris nibh.", "text-primary")
                                        .Label("Secondary:Nullamiddoloridnibhultriciesvehiculautidelit.", _ => "Secondary: Nullam id dolor id nibh ultricies vehicula ut id elit.", "text-secondary")
                                        .Label("Success:Pellentesqueornaresemlaciniaquamvenenatisvestibulum.", _ => "Success: Pellentesque ornare sem lacinia quam venenatis vestibulum.", "text-success")
                                        .Label("Info:Etiamportasemmalesuadamagnamolliseuismod.", _ => "Info: Etiam porta sem malesuada magna mollis euismod.", "text-info")
                                        .Label("Warning:Donecullamcorpernullanonmetusauctorfringilla.", _ => "Warning: Donec ullamcorper nulla non metus auctor fringilla.", "text-warning")
                                        .Label("Danger:Duismollis,estnoncommodoluctus,nisieratporttitorligula.", _ => "Danger: Duis mollis, est non commodo luctus, nisi erat porttitor ligula.", "text-danger")
                                    )).VisualElement("unnamed8", "row", unnamed8 => unnamed8
                                    .Label("Tables", _ => "Tables", "h2")
                                ).VisualElement("unnamed9", "table", unnamed9 => unnamed9
                                    .VisualElement("unnamed10", "row", unnamed10 => unnamed10
                                        .VisualElement("unnamed11", "th", unnamed11 => unnamed11
                                            .Label("Type", _ => "Type", "")
                                        ).VisualElement("unnamed12", "th", unnamed12 => unnamed12
                                            .Label("Columnheading", _ => "Column heading", "")
                                        ).VisualElement("unnamed13", "th", unnamed13 => unnamed13
                                            .Label("Columnheading", _ => "Column heading", "")
                                        ).VisualElement("unnamed14", "th", unnamed14 => unnamed14
                                            .Label("Columnheading", _ => "Column heading", "")
                                        )).VisualElement("unnamed15", "row", unnamed15 => unnamed15
                                        .VisualElement("unnamed16", "th", unnamed16 => unnamed16
                                            .Label("Default", _ => "Default", "")
                                        ).VisualElement("unnamed17", "td", unnamed17 => unnamed17
                                            .Label("Columncontent", _ => "Column content", "")
                                        ).VisualElement("unnamed18", "td", unnamed18 => unnamed18
                                            .Label("Columncontent", _ => "Column content", "")
                                        ).VisualElement("unnamed19", "td", unnamed19 => unnamed19
                                            .Label("Columncontent", _ => "Column content", "")
                                        )).VisualElement("unnamed20", "row", unnamed20 => unnamed20
                                        .VisualElement("unnamed21", "th, primary", unnamed21 => unnamed21
                                            .Label("Primary", _ => "Primary", "")
                                        ).VisualElement("unnamed22", "td, primary", unnamed22 => unnamed22
                                            .Label("Columncontent", _ => "Column content", "")
                                        ).VisualElement("unnamed23", "td, primary", unnamed23 => unnamed23
                                            .Label("Columncontent", _ => "Column content", "")
                                        ).VisualElement("unnamed24", "td, primary", unnamed24 => unnamed24
                                            .Label("Columncontent", _ => "Column content", "")
                                        )).VisualElement("unnamed25", "row", unnamed25 => unnamed25
                                        .VisualElement("unnamed26", "th, secondary", unnamed26 => unnamed26
                                            .Label("Secondary", _ => "Secondary", "")
                                        ).VisualElement("unnamed27", "td, secondary", unnamed27 => unnamed27
                                            .Label("Columncontent", _ => "Column content", "")
                                        ).VisualElement("unnamed28", "td, secondary", unnamed28 => unnamed28
                                            .Label("Columncontent", _ => "Column content", "")
                                        ).VisualElement("unnamed29", "td, secondary", unnamed29 => unnamed29
                                            .Label("Columncontent", _ => "Column content", "")
                                        )).VisualElement("unnamed30", "row", unnamed30 => unnamed30
                                        .VisualElement("unnamed31", "th, success", unnamed31 => unnamed31
                                            .Label("Success", _ => "Success", "")
                                        ).VisualElement("unnamed32", "td, success", unnamed32 => unnamed32
                                            .Label("Columncontent", _ => "Column content", "")
                                        ).VisualElement("unnamed33", "td, success", unnamed33 => unnamed33
                                            .Label("Columncontent", _ => "Column content", "")
                                        ).VisualElement("unnamed34", "td, success", unnamed34 => unnamed34
                                            .Label("Columncontent", _ => "Column content", "")
                                        )).VisualElement("unnamed35", "row", unnamed35 => unnamed35
                                        .VisualElement("unnamed36", "th, warning", unnamed36 => unnamed36
                                            .Label("Success", _ => "Success", "")
                                        ).VisualElement("unnamed37", "td, warning", unnamed37 => unnamed37
                                            .Label("Columncontent", _ => "Column content", "")
                                        ).VisualElement("unnamed38", "td, warning", unnamed38 => unnamed38
                                            .Label("Columncontent", _ => "Column content", "")
                                        ).VisualElement("unnamed39", "td, warning", unnamed39 => unnamed39
                                            .Label("Columncontent", _ => "Column content", "")
                                        )).VisualElement("unnamed40", "row", unnamed40 => unnamed40
                                        .VisualElement("unnamed41", "th, danger", unnamed41 => unnamed41
                                            .Label("Danger", _ => "Danger", "")
                                        ).VisualElement("unnamed42", "td, danger", unnamed42 => unnamed42
                                            .Label("Columncontent", _ => "Column content", "")
                                        ).VisualElement("unnamed43", "td, danger", unnamed43 => unnamed43
                                            .Label("Columncontent", _ => "Column content", "")
                                        ).VisualElement("unnamed44", "td, danger", unnamed44 => unnamed44
                                            .Label("Columncontent", _ => "Column content", "")
                                        ))).VisualElement("unnamed45", "row", unnamed45 => unnamed45
                                    .Button("Close", "Close", "btn-warning", _ => Hide())
                                ))
                        // CODE
                    );
            }
        }
    }
}