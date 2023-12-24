using System;
using Flui.Binder;
using UnityEngine;
using UnityEngine.UIElements;

namespace FluiDemo.Bootstrap
{
    public class BootstrapDemo : MonoBehaviour
    {
        private UIDocument _document;
        private VisualElement _rootVisualElement;
        private FluiBinderRoot<BootstrapDemo, VisualElement> _root;
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
            _root ??= new FluiBinderRoot<BootstrapDemo, VisualElement>();
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
            // gameObject.SetActive(false);
            CommonHelper.FadeOut(
                this, 
                _rootVisualElement, 
                () => gameObject.SetActive(false));
            _onHide();
        }

        private void LogState(string method)
        {
            if (_document == null)
            {
                Debug.Log($"{method}: document == null");
            }
            else if (_document.rootVisualElement == null)
            {
                Debug.Log($"{method}: rootVisualElement == null");
            }
            else
            {
                Debug.Log($"{method}: All Set!");
            }
        }
    }
}