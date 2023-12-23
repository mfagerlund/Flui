using System;
using UnityEngine.UIElements;

namespace Flui.Builder
{
    public class FluiBuilderRoot<TContext, TVisualElement> where TVisualElement : VisualElement
    {
        private FluiBuilder<TContext, TVisualElement> _root;

        public void BuildGui(
            TContext context,
            TVisualElement root,
            Action<FluiBuilder<TContext, TVisualElement>> buildAction)
        {
            if (_root == null)
            {
                _root = new FluiBuilder<TContext, TVisualElement>("root", context, root);
            }
            else
            {
                _root.Context = context;
            }

            _root.PrepareVisit();
            buildAction(_root);
            _root.RemoveUnvisited();
        }
    }
}
