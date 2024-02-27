using Flui.Binder;
using UnityEngine;
using UnityEngine.UIElements;

namespace FluiDemo.DarkMode.Binder
{
    public class DarkModeBinder : Fadable
    {
        private readonly FluiBinderRoot<DarkModeBinder, VisualElement> _root = new();
        [field: SerializeField] public string Message { get; set; } = "(coming soon)";

        public void Primary() => Message = "Primary";  
        public void Success() => Message = "Success";
        public void Secondary() => Message = "Secondary";
        public void Warning() => Message = "Warning";
        public void Danger() => Message = "Danger";
        [field: SerializeField] public bool ButtonsEnabled { get; set; } = true;

        void Update()
        {
            // RootVisualElement.BringToFront();
            _root.BindGui(this, RootVisualElement, r => r
                .Button(b => b.Primary(), enabledFunc: _ => ButtonsEnabled)
                .Button(b => b.Success(), enabledFunc: _ => ButtonsEnabled)
                .Button(b => b.Secondary(), enabledFunc: _ => ButtonsEnabled)
                .Button(b => b.Warning(), enabledFunc: _ => ButtonsEnabled)
                .Button(b => b.Danger(), enabledFunc: _ => ButtonsEnabled)
                .Button(b => b.Close(), enabledFunc: _ => ButtonsEnabled)
                .Toggle(x => x.ButtonsEnabled)
                .Label("Message", x => x.Message)
            );
        }
    }
}