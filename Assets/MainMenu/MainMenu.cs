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
        [SerializeField] private GameSettings.GameSettings _gameSettings;
        [SerializeField] private BootstrapDemo _bootstrapDemo;
        [SerializeField] private ListUi.ListUi _listUi;

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
                    .Button("List", ctx => ShowList())
                    .Label("Time", ctx => $"Time: {DateTime.Now:HH:mm:ss}")
            );
        }

        private void ShowBootstrapDemo()
        {
            gameObject.SetActive(false);
            _bootstrapDemo.Show(() => gameObject.SetActive(true));
        }

        private void ShowGameSettings()
        {
            gameObject.SetActive(false);
            _gameSettings.Show(() => gameObject.SetActive(true));
        }

        private void ShowList()
        {
            gameObject.SetActive(false);
            _listUi.Show(() => gameObject.SetActive(true));    
        }
    }
}