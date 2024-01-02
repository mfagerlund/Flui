using System;
using FluiDemo.Bootstrap;
using UnityEngine;
using UnityEngine.UIElements;

namespace FluiDemo
{
    public class Fadable : MonoBehaviour
    {
        private UIDocument _document;
        private Action _onHide;
        protected VisualElement RootVisualElement { get; set; }

        public void Show(Action onHide)
        {
            _onHide = onHide;
            gameObject.SetActive(true);
        }

        protected void Connect()
        {
            _document ??= GetComponent<UIDocument>();
            RootVisualElement = _document.rootVisualElement;
        }

        public virtual void OnEnable()
        {
            Connect();
            CommonHelper.FadeIn(this, RootVisualElement);
        }

        public void Hide()
        {
            // gameObject.SetActive(false);
            CommonHelper.FadeOut(
                this,
                RootVisualElement,
                () => gameObject.SetActive(false));
            _onHide();
        }
    }
}