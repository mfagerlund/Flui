using System;
using Flui.Binder;
using UnityEngine;
using UnityEngine.UIElements;

namespace FluiDemo.Bootstrap.Binder
{
    public class BootstrapBinderDemo : Fadable
    {
        private FluiBinderRoot<BootstrapBinderDemo, VisualElement> _root = new();
        
        private void Update()
        {
            _root.BindGui(
                this,
                RootVisualElement,
                x => x
                    .Button("TopClose", ctx => Close())
                    .Button("Close", ctx => Close())
            );
        }
    }
}