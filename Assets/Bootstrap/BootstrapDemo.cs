using System;
using Flui.Binder;
using UnityEngine;
using UnityEngine.UIElements;

namespace FluiDemo.Bootstrap
{
    public class BootstrapDemo : MonoBehaviour
    {
        private UIDocument _document;
        private FluiBinderRoot<BootstrapDemo, VisualElement> _root;
        private Action _onHide;
        
        public void Show(Action onHide)
        {
            _onHide = onHide;
            gameObject.SetActive(true);
        }

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

            _root ??= new FluiBinderRoot<BootstrapDemo, VisualElement>();
            _root.BindGui(
                this,
                _document.rootVisualElement,
                x => x
                    .Button("Close", ctx => Hide())
            );
        }

        private void Hide()
        {
            gameObject.SetActive(false);
            _onHide();
        }
    }
}