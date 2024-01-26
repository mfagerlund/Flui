using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Flui.Creator
{
    public partial class FluiCreator<TContext, TVisualElement> : IFluiCreator
        where TVisualElement : VisualElement
    {
        private readonly Dictionary<string, IFluiCreator> _childCreators = new();
        private readonly HashSet<VisualElement> _childVisualElements = new();
        private Action<FluiCreator<TContext, TVisualElement>> _updateAction;
        private bool _visited;

        public FluiCreator(string name, TContext context, TVisualElement element)
        {
            FluiCreatorStats.FluisCreated++;
            Name = name;
            Context = context;
            Element = element;
        }

        ~FluiCreator()
        {
            FluiCreatorStats.FluisDestroyed++;
        }

        public string Name { get; }
        public bool Visited => _visited;
        VisualElement IFluiCreator.VisualElement => Element;
        // This can be used for noticing when a change has occured the last check.
        public IValueBinding ValueBinding { get; private set; }

        object IFluiCreator.Context => Context;

        public TContext Context { get; set; }
        public TVisualElement Element { get; }
        public bool PurgeUnmanagedChildren { get; set; } = true;
        public bool IsFocused => Element.focusController.focusedElement == Element;

        public bool IsFocusedSelfOrChildren
        {
            get
            {
                var fe = Element.focusController.focusedElement as VisualElement;
                while (fe != null)
                {
                    if (fe == Element)
                    {
                        return true;
                    }

                    fe = fe.parent;
                }

                return false;
            }
        }

        public void PrepareVisit()
        {
            _visited = false;
            _childCreators.Values.ForEach(x => x.PrepareVisit());
        }

        public void RemoveUnvisited()
        {
            foreach (var flui in _childCreators.Values.ToList())
            {
                if (!flui.Visited)
                {
                    // Remove fluis that weren't visited
                    FluiCreatorStats.FluisRemoved++;
                    _childCreators.Remove(flui.Name);
                    _childVisualElements.Remove(flui.VisualElement);
                    flui.VisualElement.parent.Remove(flui.VisualElement);
                }
                else
                {
                    // Remove fluis in children that weren't visited 
                    flui.RemoveUnvisited();
                }
            }

            // Remove visual elements that don't belong - these may have been created through some other process.
            if (PurgeUnmanagedChildren)
            {
                foreach (var visualElement in Element.Children().ToList())
                {
                    if (!_childVisualElements.Contains(visualElement))
                    {
                        FluiCreatorStats.UnparentedVisualElementsRemoved++;
                        Element.Remove(visualElement);
                    }
                }
            }
        }

        public int GetHierarchyChildCount() => 1 + _childCreators.Values.Sum(x => x.GetHierarchyChildCount());

        public FluiCreator<TContext, TVisualElement> AddClasses(string classes)
        {
            FluiHelper.AddClasses(Element, classes);
            return this;
        }

        public FluiCreator<TContext, TVisualElement> OptionalClass(string className, Func<TContext, bool> includeFunc)
        {
            if (includeFunc(Context))
            {
                Element.AddToClassList(className);
            }
            else
            {
                Element.RemoveFromClassList(className);
            }

            return this;
        }
    }
}