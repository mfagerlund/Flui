// ReSharper disable InconsistentNaming

using System;
using System.Collections;
using Flui.Binder;
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

        public void Show(Action onHide)
        {
            _onHide = onHide;
            gameObject.SetActive(true);
        }

        private void Hide()
        {
            gameObject.SetActive(false);
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
            if (_document == null)
            {
                _document = GetComponent<UIDocument>();
            }

            if (_document == null)
            {
                throw new InvalidOperationException("_document not assigned");
            }

            if (_rebuild)
            {
                _root = new FluiBinderRoot<GameSettings, VisualElement>();
                _rebuild = false;
            }

            _root.BindGui(this, _document.rootVisualElement, x => x
                .Label("compact-settings", ctx => ctx.Settings.CompactString)
                .EnumButtons<Panel>(
                    "left-panel",
                    ctx => ctx.ActivePanel,
                    b => b
                        .EnumButton(Panel.ControlSettings)
                        .EnumButton(Panel.ScreenSettings)
                        .EnumButton(Panel.VolumeSettings)
                        .EnumButton(Panel.GraphicSettings)
                        .EnumButton(Panel.KeyboardSettings))
                .Group("ControlSettingsPanel", ctx => ctx, cs => cs
                    .SetHidden(ctx => ctx.ActivePanel != Panel.ControlSettings)
                    .Group("Inner", ctx => ctx.Settings.ControlSettings, inner => inner
                        .Toggle("SimpleControls", t => t.SimpleControls)
                        .Toggle("Vibration", t => t.Vibration)
                        .Toggle("ButtonConfiguration", t => t.ButtonConfiguration)
                        .Slider("CameraDistance", t => t.CameraDistance, lowValue: 1, highValue: 20)
                        .Toggle("ScreenVibration", t => t.ScreenVibration)
                        .Toggle("ShowSpecialAttack", t => t.ShowSpecialAttack)
                        .TextField("UserName", t => t.UserName))
                )
                .Group("ScreenSettingsPanel", ctx => ctx, cs => cs
                    .SetHidden(ctx => ctx.ActivePanel != Panel.ScreenSettings)
                    .Group("Inner", ctx => ctx.Settings.ScreenSettings, inner => inner
                        .IntegerField("Width", t => t.Width)
                        .IntegerField("Height", t => t.Height)
                        .FloatField("PixelDensity", t => t.PixelDensity)
                        .Dropdown("ColorMode", t => t.ColorModeId)
                        .EnumDropdown("CycleMode", t => t.CycleMode)
                    )
                )
                .Group("VolumeSettingsPanel", ctx => ctx, cs => cs
                    .SetHidden(ctx => ctx.ActivePanel != Panel.VolumeSettings))
                .Group("GraphicSettingsPanel", ctx => ctx, cs => cs
                    .SetHidden(ctx => ctx.ActivePanel != Panel.GraphicSettings))
                .Group("KeyboardSettingsPanel", ctx => ctx, cs => cs
                    .SetHidden(ctx => ctx.ActivePanel != Panel.KeyboardSettings))
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