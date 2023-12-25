using System;
using UnityEngine.UIElements;

namespace Flui.Creator
{
    public class FluiCreatorRoot<TContext, TVisualElement>
        where TVisualElement : VisualElement
        where TContext : class
    {
        private FluiCreator<TContext, TVisualElement> _root;

        public void CreateGui(
            TContext context,
            TVisualElement root,
            Action<FluiCreator<TContext, TVisualElement>> buildAction)
        {
            if (root == null)
            {
                _root = null;
                return;
            }

            if (_root != null && (_root.Element != root || !Equals(_root.Context, context)))
            {
                FluiCreatorStats.TotalRebuild++;
                _root = null;
            }

            _root ??= new FluiCreator<TContext, TVisualElement>("root", context, root);
            _root.PrepareVisit();
            // BuildAction should really be set, otherwise what's the point of all this?
            buildAction?.Invoke(_root);
            _root.RemoveUnvisited();
        }
    }
}