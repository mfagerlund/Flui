// ReSharper disable InconsistentNaming

using System;
using Flui.Binder;
using FluiDemo.Bootstrap;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace FluiDemo.MainMenu
{
    public class MainMenu : MonoBehaviour
    {
        private UIDocument _document;
        [SerializeField] private GameSettings.GameSettings _gameSettings;
        [SerializeField] private BootstrapDemo _bootstrapDemo;

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

        private void ShowGameSettings()
        {
            gameObject.SetActive(false);
            // _gameSettings.gameObject.SetActive(true);
            _gameSettings.Show(() => gameObject.SetActive(true));
        }

        private void ShowBootstrapDemo()
        {
            gameObject.SetActive(false);
            _bootstrapDemo.Show(() => gameObject.SetActive(true));
        }
    }
}