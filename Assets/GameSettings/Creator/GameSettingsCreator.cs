// ReSharper disable InconsistentNaming

using System;
using System.Collections;
using System.Collections.Generic;
using Flui.Creator;
using FluiDemo.Bootstrap;
using UnityEngine;
using UnityEngine.UIElements;

namespace FluiDemo.GameSettings.Creator
{
    public class GameSettingsCreator : MonoBehaviour
    {
        private UIDocument _document;
        [SerializeField] private bool _pause;
        [SerializeField] private bool _rebuild;

        private FluiCreatorRoot<GameSettingsCreator, VisualElement> _root = new();
        public GameSettings GameSettings { get; set; } = new GameSettings();
        public Panel ActivePanel { get; set; } = Panel.ScreenSettings;

        private Action _onHide;
        private VisualElement _rootVisualElement;

        public void Show(Action onHide)
        {
            _onHide = onHide;
            gameObject.SetActive(true);
        }

        public void OnEnable()
        {
            _document ??= GetComponent<UIDocument>();
            _rootVisualElement = _document.rootVisualElement;
            CommonHelper.FadeIn(this, _rootVisualElement);
        }

        private void Hide()
        {
            // gameObject.SetActive(false);
            CommonHelper.FadeOut(
                this,
                _rootVisualElement,
                () => gameObject.SetActive(false));
            _onHide();
        }

        private void OnValidate()
        {
            if (Application.isPlaying || !gameObject.activeSelf) return;
            StartCoroutine(BindCoRoutine());
        }

        private void Update()
        {
            Bind();
        }

        public enum Panel
        {
            ControlSettings,
            ScreenSettings,
            VolumeSettings,
            GraphicSettings,
            KeyboardSettings
        }

        IEnumerator BindCoRoutine()
        {
            if (_pause)
            {
                yield break;
            }

            yield return null;

            Bind();
        }

        private void Bind()
        {
            if (_rebuild)
            {
                _root = new FluiCreatorRoot<GameSettingsCreator, VisualElement>();
                _rebuild = false;
            }

            _root.CreateGui(this, _rootVisualElement, x => x
                    // CODE
                    .VisualElement("container", "container", container => container
                        .VisualElement("header", "header", header => header
                            .VisualElement("unnamed0", "options-header", unnamed0 => unnamed0
                                .Label("options", _ => "Options", "main-title")
                                .Label("compact-settings", _ => "CompactSettings", "")
                            ))
                        .VisualElement("panels", "panels", panels => panels
                            .EnumButtons("left-panel", "left-panel, w50", x => x.ActivePanel, leftpanel => leftpanel
                                .EnumButton(Panel.ControlSettings, "Control Settings", "btn-gold")
                                .EnumButton(Panel.ScreenSettings, "Screen Settings", "btn-gold")
                                .EnumButton(Panel.VolumeSettings, "Volume Settings", "btn-gold")
                                .EnumButton(Panel.GraphicSettings, "Graphic Settings", "btn-gold")
                                .EnumButton(Panel.KeyboardSettings, "Keyboard Settings", "disabled, btn-gold")
                            )
                            .EnumSwitch("right-panel", "w50, right-panel", ctx => ctx.ActivePanel, rightpanel => rightpanel
                                .Case(Panel.ControlSettings, "", p => p.GameSettings.ControlSettings, controlSettingsPanel => controlSettingsPanel
                                    .VisualElement("Inner", "", i => i
                                        .VisualElement("SimpleControls", "right-panel-control-holder", simpleControls => simpleControls
                                            .Toggle("SimpleControls", "Simple Controls", "", ctx => ctx.SimpleControls))
                                        .VisualElement("vibration", "right-panel-control-holder", vibration => vibration
                                            .Toggle("Vibration", "Vibration", "", ctx => ctx.Vibration))
                                        .VisualElement("buttonConfiguration", "right-panel-control-holder", buttonConfiguration => buttonConfiguration
                                            .Toggle("ButtonConfiguration", "ButtonConfiguration", "", ctx => ctx.ButtonConfiguration))
                                        .VisualElement("spacer", "panel-right-spacer")
                                        .VisualElement("cameraDistance", "right-panel-control-holder", cameraDistance => cameraDistance
                                            .Slider("CameraDistance", "Slider", "", 0, 1, ctx => ctx.CameraDistance))
                                        .VisualElement("screenVibration", "right-panel-control-holder", screenVibration => screenVibration
                                            .Toggle("ScreenVibration", "Screen Vibration", "", ctx => ctx.ScreenVibration))
                                        .VisualElement("slowSpecialAttack", "right-panel-control-holder", slowSpecialAttack => slowSpecialAttack
                                            .Toggle("ShowSpecialAttack", "Show Special Attack", "", ctx => ctx.ShowSpecialAttack))
                                        .VisualElement("spacer2", "panel-right-spacer")
                                        .VisualElement("UserName", "right-panel-control-holder", userName => userName
                                            .TextField("UserName", "User Name", "", ctx => ctx.UserName))))
                                .Case(Panel.ScreenSettings, "", p => p.GameSettings.ScreenSettings, screenSettingsPanel => screenSettingsPanel
                                    .VisualElement("Width", "right-panel-control-holder", width => width
                                        .IntegerField("Width", "Width", "", ctx => ctx.Width))
                                    .VisualElement("Height", "right-panel-control-holder", height => height
                                        .IntegerField("Height", "Height", "", ctx => ctx.Height))
                                    .VisualElement("PixelDensity", "right-panel-control-holder", pixelDensity => pixelDensity
                                        .FloatField("PixelDensity", "Float Field", "", ctx => ctx.PixelDensity))
                                    .VisualElement("spacer", "panel-right-spacer")
                                    .VisualElement("ColorMode", "right-panel-control-holder", colorMode => colorMode
                                        .DropdownField("ColorMode", "Color Mode", "", new List<string> { "a", "b" }, ctx => ctx.ColorModeId))
                                    .VisualElement("CycleMode", "right-panel-control-holder", cycleMode => cycleMode
                                        .EnumField("CycleMode", "Cycle Mode", "", ctx => ctx.CycleMode)))
                                .Case(Panel.VolumeSettings, "", p => p, volumeSettingsPanel => volumeSettingsPanel
                                    .VisualElement("unnamed1", "right-panel-control-holder", unnamed1 => unnamed1
                                        .Label("VolumeSettings-NOTIMPLEMENTED", _ => "Volume Settings - NOT IMPLEMENTED", "not-implemented")))
                                .Case(Panel.GraphicSettings, "", p => p, graphicSettingsPanel => graphicSettingsPanel
                                    .VisualElement("unnamed2", "right-panel-control-holder", unnamed2 => unnamed2
                                        .Label("GraphicsSettings-NOTIMPLEMENTED", _ => "Graphics Settings - NOT IMPLEMENTED", "not-implemented")))
                                .Case(Panel.KeyboardSettings, "", p => p, keyboardSettingsPanel => keyboardSettingsPanel
                                    .VisualElement("unnamed3", "right-panel-control-holder", unnamed3 => unnamed3
                                        .Label("KeyboardSettings-NOTIMPLEMENTED", _ => "Keyboard Settings - NOT IMPLEMENTED", "not-implemented")))
                            ))
                        .VisualElement("footer", "row, footer", footer => footer
                            .Button("Ok", "OK", "btn-primary", _ => Hide())
                            .Button("Return", "Return", "btn-primary", _ => Hide())
                            .Button("conquer-world", "Conquer World", "btn-primary, disabled", null)
                        )
                    )

                // CODE
                // .Label("compact-settings", ctx => ctx.GameSettings.CompactString)
                // .EnumButtons(
                //     "left-panel",
                //     ctx => ctx.ActivePanel,
                //     b => b
                //         .EnumButton(Panel.ControlSettings)
                //         .EnumButton(Panel.ScreenSettings)
                //         .EnumButton(Panel.VolumeSettings)
                //         .EnumButton(Panel.GraphicSettings)
                //         .EnumButton(Panel.KeyboardSettings))
                //
                // .EnumSwitch(
                //     "right-panel",
                //     ctx => ctx.ActivePanel, p => p
                //         .Case(
                //             "ControlSettingsPanel", Panel.ControlSettings, ctx => ctx.GameSettings.ControlSettings, c => c
                //                 .Toggle("SimpleControls", t => t.SimpleControls)
                //                 .Toggle("Vibration", t => t.Vibration)
                //                 .Toggle("ButtonConfiguration", t => t.ButtonConfiguration)
                //                 .Slider("CameraDistance", t => t.CameraDistance, lowValue: 1, highValue: 20)
                //                 .Toggle("ScreenVibration", t => t.ScreenVibration)
                //                 .Toggle("ShowSpecialAttack", t => t.ShowSpecialAttack)
                //                 .TextField("UserName", t => t.UserName)
                //         )
                //         .Case("ScreenSettingsPanel", Panel.ScreenSettings, ctx => ctx.GameSettings.ScreenSettings, c => c
                //             .IntegerField("Width", t => t.Width)
                //             .IntegerField("Height", t => t.Height)
                //             .FloatField("PixelDensity", t => t.PixelDensity)
                //             .DropdownField("ColorMode", t => t.ColorModeId)
                //             .EnumField("CycleMode", t => t.CycleMode)
                //         )
                //         .Case("VolumeSettingsPanel", Panel.VolumeSettings, ctx => ctx)
                //         .Case("GraphicSettingsPanel", Panel.GraphicSettings, ctx => ctx)
                //         .Case("KeyboardSettingsPanel", Panel.KeyboardSettings, ctx => ctx)
                // )
                // .Button("Ok", ctx => Hide())
                // .Button("Return", ctx => Hide())
            );
        }
    }
}