using System;
using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Flui.Binder
{
    public class FluiBinderRoot<TContext, TVisualElement> : IFluiBinderRoot where TVisualElement : VisualElement
    {
        private readonly Dictionary<string, TemplateContainer> _templates = new();
        private FluiBinder<TContext, TVisualElement> _root;

        public void BindGui(
            TContext context,
            TVisualElement root,
            Action<FluiBinder<TContext, TVisualElement>> buildAction)
        {
            if (root == null)
            {
                _root = null;
                _templates.Clear();
                return;
            }

            if (_root != null && (_root.Element != root || !Equals(_root.Context, context)))
            {
                FluiBinderStats.TotalRebuild++;
                _root = null;
            }

            _root ??= new FluiBinder<TContext, TVisualElement>(this, "root", context, root);
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

        public TemplateContainer GetOrCreateTemplate(VisualElement element, string query)
        {
            var template = _templates.GetOrCreate(query, () =>
            {
                var template = element.Q(query).Q<TemplateContainer>();
                if (template == null)
                {
                    throw new InvalidOperationException($"Unable to find template {query} in {element.name}");
                }

                template.parent.Remove(template);

                return template;
            });
            return template;
        }
    }
}