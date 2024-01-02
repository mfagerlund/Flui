// ReSharper disable InconsistentNaming

using System;
using Flui.Binder;
using FluiDemo.Bootstrap;
using FluiDemo.Bootstrap.Binder;
using FluiDemo.Bootstrap.Creator;
using FluiDemo.GameSettings.Binder;
using FluiDemo.GameSettings.Creator;
using FluiDemo.ListUi.Creator;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace FluiDemo.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        private UIDocument _document;
        private VisualElement _rootVisualElement;

        [SerializeField] private GameSettingsBinder gameSettingsBinder;
        [SerializeField] private GameSettingsCreator gameSettingsCreator;

        [SerializeField] private BootstrapBinderDemo bootstrapBinderDemo;
        [SerializeField] private BootstrapCreatorDemo bootstrapCreatorDemo;

        [SerializeField] private ListUi.Binder.ListUiBinder listUiBinder;
        [SerializeField] private ListUiCreator listUiCreator;

        [SerializeField] private Mixed.Mixed _mixed;

        private readonly FluiBinderRoot<MainMenu, VisualElement> _root = new();

        private void OnEnable()
        {
            _document ??= GetComponent<UIDocument>();
            _rootVisualElement = _document.rootVisualElement;
            CommonHelper.FadeIn(this, _rootVisualElement);
        }

        private void Update()
        {
            _root.BindGui(this, _rootVisualElement, x => x
                .Group("FluiBinderDemos", ctx => ctx, g => g
                    .Button("BootstrapDemo", _ => ShowBootstrapBinderDemo())
                    .Button("GameSettingsMenu", _ => ShowGameSettingsBinder())
                    .Button("List", _ => ShowListBinder())
                )
                .Group("FluiCreatorDemos", ctx => ctx, g => g
                    .Button("BootstrapDemo", _ => ShowBootstrapCreatorDemo())
                    .Button("GameSettingsMenu", _ => ShowGameSettingsCreator())
                    .Button("List", _ => ShowListCreator())
                )
                .Button("Mixed", _ => ShowMixed())
                .Label("Time", _ => $"Time: {DateTime.Now:HH:mm:ss}")
            );
        }

        private void ShowBootstrapBinderDemo()
        {
            Hide();
            bootstrapBinderDemo.Show(Show);
        }

        private void ShowBootstrapCreatorDemo()
        {
            Hide();
            bootstrapCreatorDemo.Show(Show);
        }

        private void ShowGameSettingsBinder()
        {
            Hide();
            gameSettingsBinder.Show(Show);
        }

        private void ShowGameSettingsCreator()
        {
            Hide();
            gameSettingsCreator.Show(Show);
        }

        private void ShowListBinder()
        {
            Hide();
            listUiBinder.Show(Show);
        }

        private void ShowListCreator()
        {
            Hide();
            listUiCreator.Show(Show);
        }

        private void ShowMixed()
        {
            Hide();
            _mixed.Show(Show);
        }

        private void Show()
        {
            gameObject.SetActive(true);
        }

        private void Hide()
        {
            CommonHelper.FadeOut(
                this,
                _rootVisualElement,
                () => gameObject.SetActive(false));
        }
    }
}