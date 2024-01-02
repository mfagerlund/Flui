using System;
using UnityEngine.UIElements;

namespace Flui.Creator
{
    public class FluiCreatorRoot<TContext, TVisualElement>
        where TVisualElement : VisualElement
    {
        private FluiCreator<TContext, TVisualElement> _root;

        public void CreateGui(
            TContext context,
            TVisualElement root,
            Action<FluiCreator<TContext, TVisualElement>> createAction)
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
            createAction?.Invoke(_root);
            _root.RemoveUnvisited();
        }
    }
}