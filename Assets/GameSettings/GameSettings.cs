// ReSharper disable InconsistentNaming

using System;
using System.Collections;
using Flui.Binder;
using FluiDemo.Bootstrap;
using UnityEngine;
using UnityEngine.UIElements;

namespace FluiDemo.GameSettings
{
    public class GameSettings : MonoBehaviour
    {
        private UIDocument _document;
        [SerializeField] public string _hierarchy;
        [SerializeField] private bool _pause;
        [SerializeField] private bool _rebuild;

        private FluiBinderRoot<GameSettings, VisualElement> _root = new();
        public Settings Settings { get; set; } = new Settings();
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
                _root = new FluiBinderRoot<GameSettings, VisualElement>();
                _rebuild = false;
            }

            _root.BindGui(this, _rootVisualElement, x => x
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

            _hierarchy = _root.HierarchyAsString();
        }
    }

    public class Settings
    {
        public ControlSettingsC ControlSettings { get; set; } = new();
        public ScreenSettingsC ScreenSettings { get; set; } = new();

        public string CompactString
        {
            get
            {
                var text = "";
                if (ControlSettings.SimpleControls) text += "sc";
                if (ControlSettings.Vibration) text += "v";
                if (ControlSettings.ButtonConfiguration) text += "bc";
                text += $"{ControlSettings.CameraDistance:0.0}";
                if (ControlSettings.ScreenVibration) text += "sv";
                if (ControlSettings.ShowSpecialAttack) text += "ssa";
                text += ControlSettings.UserName;
                text += "|" + ScreenSettings.Width + "x" + ScreenSettings.Height + "@" + ScreenSettings.PixelDensity;
                text += "|" + ScreenSettings.ColorModeId;
                return text;
            }
        }

        public class ControlSettingsC
        {
            public bool SimpleControls { get; set; } = true;
            public bool Vibration { get; set; } = true;
            public bool ButtonConfiguration { get; set; }
            public float CameraDistance { get; set; } = 10;
            public bool ScreenVibration { get; set; }
            public bool ShowSpecialAttack { get; set; }
            public string UserName { get; set; } = "Arnold";
        }

        public class ScreenSettingsC
        {
            public int Width { get; set; } = 320;
            public int Height { get; set; } = 240;
            public float PixelDensity { get; set; } = 0.3f;
            public int ColorModeId { get; set; }
            public CycleModeEnum CycleMode { get; set; }
        }

        public enum CycleModeEnum
        {
            Forward,
            Backward,
            ForwardWithNoHands,
            BackwardsInHeels
        }
    }
}