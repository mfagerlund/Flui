using System;
using UnityEngine.UIElements;

namespace Flui.Binder
{
    public class FluiBinderRoot<TContext, TVisualElement> where TVisualElement : VisualElement
    {
        public FluiBinder<TContext, TVisualElement> Root { get; private set; }

        public void BindGui(
            TContext context,
            TVisualElement root,
            Action<FluiBinder<TContext, TVisualElement>> buildAction)
        {
            if (root==null)
            {
                if (Root != null)
                {
                    FluiBinderStats.TotalRebuild++;
                    Root = null;
                }
                return;
            }
            
            if (Root != null && (Root.Element != root || !Equals(Root.Context, context)))
            {
                FluiBinderStats.TotalRebuild++;
                Root = null;
            }

            Root ??= new FluiBinder<TContext, TVisualElement>("root", context, root);
            Root.PrepareVisit();
            buildAction(Root);
            Root.RemoveUnvisited();
        }

        public string HierarchyAsString()
        {
            if (Root == null)
            {
                return "NO ROOT!";
            }
            return Root.HierarchyAsString();
        }
    }
}