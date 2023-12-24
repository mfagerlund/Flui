// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.UIElements;

namespace Flui.Binder
{
    public partial class FluiBinder<TContext, TVisualElement> : IFluiBinder
        where TVisualElement : VisualElement
    {
        private readonly Dictionary<VisualElement, IFluiBinder> _childBinders = new();
        private Action<FluiBinder<TContext, TVisualElement>> _updateAction;
        private Action<FluiBinder<TContext, TVisualElement>> _bindAction;
        private bool _visited;
        private IValueBinding _valueBinding;
        private Func<TContext, bool> _hiddenFunc;
        private Func<TContext, bool> _invisibleFunc;
        // Data to use for complex binding actions - such as in ForEach
        // private object _data;

        public FluiBinder(string query, TContext context, TVisualElement element)
        {
            FluiBinderStats.FluiBinderCreated++;
            Query = query;
            Context = context;
            Element = element;
        }

        ~FluiBinder()
        {
            FluiBinderStats.FluidBinderDestroyed++;
        }

        public bool Visited => _visited;
        public TVisualElement Element { get; }
        VisualElement IFluiBinder.VisualElement => Element;
        object IFluiBinder.Context => Context;
        IEnumerable<IFluiBinder> IFluiBinder.GetChildren() => _childBinders.Values;
        public string Query { get; }
        public TContext Context { get; }
        public bool Visible => !Hidden && !Invisible;
        public bool Hidden { get; set; } = false;
        public bool Invisible { get; set; } = false;
        public bool IsFocused => Element.focusController?.focusedElement == Element;

        private FluiBinder<TChildContext, TChildVisualElement> RawBind<TChildContext, TChildVisualElement>(
            string query,
            Func<TContext, TChildContext> contextFunc,
            Action<FluiBinder<TChildContext, TChildVisualElement>> bindAction = null,
            Action<FluiBinder<TChildContext, TChildVisualElement>> initiateAction = null,
            Action<FluiBinder<TChildContext, TChildVisualElement>> updateAction = null) where TChildVisualElement : VisualElement, new()
        {
            var visualElement = Element.Q<TChildVisualElement>(query);
            if (visualElement == null)
            {
                throw new InvalidOperationException(
                    $"The query '{query}' doesn't return a {typeof(TChildVisualElement)} on '{Element.name}'");
            }

            var rawChild = _childBinders.GetOrCreate(visualElement, () =>
            {
                var binder = new FluiBinder<TChildContext, TChildVisualElement>(query, contextFunc(Context), visualElement)
                {
                    _updateAction = updateAction,
                    _bindAction = bindAction
                };
                initiateAction?.Invoke(binder);
                return binder;
            });

            if (rawChild.Visited)
            {
                throw new InvalidOperationException(
                    $"The query '{query}) on '{Element.name}' has already been visited - only use one binding per visual element");
            }

            var child = (FluiBinder<TChildContext, TChildVisualElement>)rawChild;
            child.Update();
            return child;
        }

        private void Update()
        {
            _visited = true;
            UpdateVisibility();
            if (!Invisible && !Hidden)
            {
                _updateAction?.Invoke(this);
                _bindAction?.Invoke(this);
                _valueBinding?.Update();
            }
        }

        private void UpdateVisibility()
        {
            if (_hiddenFunc != null)
            {
                Hidden = _hiddenFunc(Context);
            }

            if (_invisibleFunc != null)
            {
                Invisible = _invisibleFunc(Context);
            }

            if (Hidden)
            {
                Element.AddToClassList("hidden");
            }
            else
            {
                Element.RemoveFromClassList("hidden");
            }

            if (Invisible)
            {
                Element.AddToClassList("invisible");
            }
            else
            {
                Element.RemoveFromClassList("invisible");
            }
        }

        public FluiBinder<TContext, TVisualElement> OptionalClass(string className, Func<TContext, bool> includeFunc)
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

        public FluiBinder<TContext, TVisualElement> SetHiddenFunc(Func<TContext, bool> hiddenFunc)
        {
            _hiddenFunc = hiddenFunc;
            return this;
        }

        public FluiBinder<TContext, TVisualElement> SetInvisibleFunc(Func<TContext, bool> invisibleFunc)
        {
            _invisibleFunc = invisibleFunc;
            return this;
        }

        public void PrepareVisit()
        {
            _visited = false;
            _childBinders.Values.ForEach(x => x.PrepareVisit());
        }

        public void RemoveUnvisited()
        {
            foreach (var child in _childBinders.Values.ToList())
            {
                if (!child.Visited)
                {
                    FluiBinderStats.FluiBinderRemoved++;
                    _childBinders.Remove(child.VisualElement);
                }
                else
                {
                    child.RemoveUnvisited();
                }
            }
        }

        public string HierarchyAsString()
        {
            var sb = new StringBuilder();
            Recurse(this, 0);

            void Recurse(IFluiBinder fluiBinder, int indent)
            {
                // hi={fluiBinder.Hidden}, iv={fluiBinder.Invisible}
                var state = fluiBinder.Hidden ? "Hidden" : (fluiBinder.Invisible ? "Invisible" : "Visible");
                sb.AppendLine($"{new string(' ', indent * 2)}q={fluiBinder.Query}, st={state}");
                if (fluiBinder.Visible)
                {
                    foreach (var binder in fluiBinder.GetChildren())
                    {
                        Recurse(binder, indent + 1);
                    }
                }
            }

            return sb.ToString();
        }
    }
}