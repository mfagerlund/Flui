using System;
using UnityEngine.UIElements;

namespace Flui.Binder
{
    public class FluiBinderRoot<TContext, TVisualElement> where TVisualElement : VisualElement
    {
        private FluiBinder<TContext, TVisualElement> _root;

        public void BindGui(
            TContext context,
            TVisualElement root,
            Action<FluiBinder<TContext, TVisualElement>> buildAction)
        {
            if (root == null)
            {
                _root = null;
                return;
            }

            if (_root != null && (_root.Element != root || !Equals(_root.Context, context)))
            {
                FluiBinderStats.TotalRebuild++;
                _root = null;
            }

            _root ??= new FluiBinder<TContext, TVisualElement>("root", context, root);
            _root.PrepareVisit();
            buildAction(_root);
            _root.RemoveUnvisited();
        }

        public string HierarchyAsString()
        {
            if (_root == null)
            {
                return "NO ROOT!";
            }

            return _root.HierarchyAsString();
        }
    }
}