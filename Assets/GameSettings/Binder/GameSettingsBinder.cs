// ReSharper disable InconsistentNaming

using System;
using System.Collections;
using Flui.Binder;
using FluiDemo.Bootstrap;
using UnityEngine;
using UnityEngine.UIElements;

namespace FluiDemo.GameSettings.Binder
{
    public class GameSettingsBinder : MonoBehaviour
    {
        private UIDocument _document;
        [SerializeField] public string _hierarchy;
        [SerializeField] private bool _pause;
        [SerializeField] private bool _rebuild;

        private FluiBinderRoot<GameSettingsBinder, VisualElement> _root = new();
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
                _root = new FluiBinderRoot<GameSettingsBinder, VisualElement>();
                _rebuild = false;
            }

            _root.BindGui(this, _rootVisualElement, x => x
                .Label("compact-settings", ctx => ctx.GameSettings.CompactString)
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
                            "ControlSettingsPanel", Panel.ControlSettings, ctx => ctx.GameSettings.ControlSettings, c => c
                                .Toggle("SimpleControls", t => t.SimpleControls)
                                .Toggle("Vibration", t => t.Vibration)
                                .Toggle("ButtonConfiguration", t => t.ButtonConfiguration)
                                .Slider("CameraDistance", t => t.CameraDistance, lowValue: 1, highValue: 20)
                                .Toggle("ScreenVibration", t => t.ScreenVibration)
                                .Toggle("ShowSpecialAttack", t => t.ShowSpecialAttack)
                                .TextField("UserName", t => t.UserName)
                        )
                        .Case("ScreenSettingsPanel", Panel.ScreenSettings, ctx => ctx.GameSettings.ScreenSettings, c => c
                            .IntegerField("Width", t => t.Width)
                            .IntegerField("Height", t => t.Height)
                            .FloatField("PixelDensity", t => t.PixelDensity)
                            .DropdownField("ColorMode", t => t.ColorModeId)
                            .EnumField("CycleMode", t => t.CycleMode)
                        )
                        .Case("VolumeSettingsPanel", Panel.VolumeSettings, ctx => ctx)
                        .Case("GraphicSettingsPanel", Panel.GraphicSettings, ctx => ctx)
                        .Case("KeyboardSettingsPanel", Panel.KeyboardSettings, ctx => ctx)
                )
                .Button("Ok", ctx => Hide())
                .Button("Return", ctx => Hide())
            );

            _hierarchy = _root.HierarchyAsString();
        }
    }
}