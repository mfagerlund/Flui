// ReSharper disable InconsistentNaming

using System;
using Flui.Binder;
using FluiDemo.Bootstrap;
using UnityEngine;
using UnityEngine.UIElements;

namespace FluiDemo.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        private UIDocument _document;
        private VisualElement _rootVisualElement;
        [SerializeField] private GameSettings.GameSettings _gameSettings;
        [SerializeField] private BootstrapDemo _bootstrapDemo;
        [SerializeField] private ListUi.ListUi _listUi;

        private readonly FluiBinderRoot<MainMenu, VisualElement> _root = new();

        private void OnEnable()
        {
            _document ??= GetComponent<UIDocument>();
            _rootVisualElement = _document.rootVisualElement;
            CommonHelper.FadeIn(this, _rootVisualElement);
        }

        private void Update()
        {
            _root.BindGui(this, _rootVisualElement,
                x => x
                    .Button("BootstrapDemo", _ => ShowBootstrapDemo())
                    .Button("GameSettingsMenu", _ => ShowGameSettings())
                    .Button("List", _ => ShowList())
                    .Label("Time", _ => $"Time: {DateTime.Now:HH:mm:ss}")
            );
        }

        private void ShowBootstrapDemo()
        {
            Hide();
            _bootstrapDemo.Show(() => gameObject.SetActive(true));
        }
        
        private void ShowGameSettings()
        {
            Hide();
            _gameSettings.Show(() => gameObject.SetActive(true));
        }

        private void ShowList()
        {
            Hide();
            _listUi.Show(() => gameObject.SetActive(true));
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