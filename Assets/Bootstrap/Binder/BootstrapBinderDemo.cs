using System;
using Flui.Binder;
using UnityEngine;
using UnityEngine.UIElements;

namespace FluiDemo.Bootstrap.Binder
{
    public class BootstrapBinderDemo : MonoBehaviour
    {
        private UIDocument _document;
        private VisualElement _rootVisualElement;
        private FluiBinderRoot<BootstrapBinderDemo, VisualElement> _root = new();
        private Action _onHide;

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

        private void Update()
        {
            _root.BindGui(
                this,
                _rootVisualElement,
                x => x
                    .Button("TopClose", ctx => Hide())
                    .Button("Close", ctx => Hide())
            );
        }

        private void Hide()
        {
            CommonHelper.FadeOut(
                this, 
                _rootVisualElement, 
                () => gameObject.SetActive(false));
            _onHide();
        }
    }
}