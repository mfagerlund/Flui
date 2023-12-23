using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

namespace Flui.Builder
{
    public class FluiBuilder<TContext, TVisualElement> : IFluiBuilder where TVisualElement : VisualElement
    {
        // TODO: Connect to existing uxml templates
        // TODO: Render lists
        // TODO: Text/Number Input
        // TODO: Better class handling
        // TODO: Checkbox 
        // TODO: Dropdown
        // TODO: Two way binding
        public string Name { get; }
        public bool Visited => _visited;
        public bool PurgeUnmanagedChildren { get; set; } = true;
        VisualElement IFluiBuilder.VisualElement => Element;
        public TContext Context { get; set; }
        public TVisualElement Element { get; private set; }
        private readonly Dictionary<string, IFluiBuilder> _childBuilders = new();
        private readonly HashSet<VisualElement> _childVisualElements = new();
        private Action<FluiBuilder<TContext, TVisualElement>> _updateAction;
        private bool _visited;
        private IValueBinding _valueBinding;

        public FluiBuilder(string name, TContext context, TVisualElement element)
        {
            FluiBuilderStats.FluisCreated++;
            Name = name;
            Context = context;
            Element = element;
        }

        ~FluiBuilder()
        {
            FluiBuilderStats.FluisDestroyed++;
        }

        public bool IsFocused => Element.focusController.focusedElement == Element;

        public void PrepareVisit()
        {
            _visited = false;
            _childBuilders.Values.ForEach(x => x.PrepareVisit());
        }

        public void RemoveUnvisited()
        {
            foreach (var flui in _childBuilders.Values.ToList())
            {
                if (!flui.Visited)
                {
                    // Remove fluis that weren't visited
                    FluiBuilderStats.FluisRemoved++;
                    _childBuilders.Remove(flui.Name);
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
                        FluiBuilderStats.UnparentedVisualElementsRemoved++;
                        Element.Remove(visualElement);
                    }
                }
            }
        }

        public FluiBuilder<TContext, TVisualElement> Optional(
            Func<TContext, bool> predicate,
            Action<FluiBuilder<TContext, TVisualElement>> buildAction)
        {
            if (predicate(Context))
            {
                buildAction(this);
            }

            return this;
        }

        public FluiBuilder<TContext, TChildVisualElement> RawBuild<TChildVisualElement>(
            string name,
            string classes,
            Action<FluiBuilder<TContext, TChildVisualElement>> buildAction = null,
            Action<FluiBuilder<TContext, TChildVisualElement>> initiateAction = null,
            Action<FluiBuilder<TContext, TChildVisualElement>> updateAction = null) where TChildVisualElement : VisualElement, new()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Each VisualElement must have a name.");
            }

            var rawChild = _childBuilders.GetOrCreate(name, () =>
            {
                var visualElement = new TChildVisualElement
                {
                    name = name
                };
                var flui = new FluiBuilder<TContext, TChildVisualElement>(name, Context, visualElement);
                _childVisualElements.Add(visualElement);
                Element.Add(visualElement);
                flui._updateAction = updateAction;
                initiateAction?.Invoke(flui);
                return flui;
            });

            if (rawChild.Visited)
            {
                throw new InvalidOperationException(
                    $"Names must be unique within children of each VisualElement - \"{name}\" appears more than once in \"{Element.name}\".");
            }

            var child = (FluiBuilder<TContext, TChildVisualElement>)rawChild;
            child._visited = true;
            child._updateAction?.Invoke(child);
            child._valueBinding?.Update();
            SetClasses(classes, child.Element);

            if (buildAction != null)
            {
                buildAction(child);
            }

            return child;
        }

        public FluiBuilder<TContext, TVisualElement> VisualElement(
            string name,
            string classes,
            Action<FluiBuilder<TContext, VisualElement>> buildAction = null,
            Action<FluiBuilder<TContext, VisualElement>> initiateAction = null,
            Action<FluiBuilder<TContext, VisualElement>> updateAction = null)
        {
            RawBuild<VisualElement>(name, classes, buildAction, initiateAction, updateAction);
            return this;
        }

        public FluiBuilder<TContext, TVisualElement> Label(
            string name,
            string text,
            string classes,
            Action<FluiBuilder<TContext, Label>> buildAction = null,
            Action<FluiBuilder<TContext, Label>> initiateAction = null,
            Action<FluiBuilder<TContext, Label>> updateAction = null)
        {
            RawBuild<Label>(
                name,
                classes,
                buildAction,
                b =>
                {
                    b.Element.text = text;
                    initiateAction?.Invoke(b);
                },
                updateAction);
            return this;
        }

        public FluiBuilder<TContext, TVisualElement> Set(Action<FluiBuilder<TContext, TVisualElement>> setAction)
        {
            setAction(this);
            return this;
        }

        public FluiBuilder<TContext, TVisualElement> Label(
            string name,
            Func<TContext, string> textFunc,
            string classes,
            Action<FluiBuilder<TContext, Label>> buildAction = null,
            Action<FluiBuilder<TContext, Label>> initiateAction = null,
            Action<FluiBuilder<TContext, Label>> updateAction = null)
        {
            RawBuild<Label>(
                name,
                classes,
                buildAction,
                initiateAction,
                b =>
                {
                    b.Element.text = textFunc(Context);
                    updateAction?.Invoke(b);
                });
            return this;
        }

        public FluiBuilder<TContext, TVisualElement> Slider(
            string name,
            string label,
            string classes,
            float lowValue,
            float highValue,
            Func<TContext, float> getValue,
            Action<TContext, float> setValue,
            Action<FluiBuilder<TContext, Slider>> buildAction = null,
            Action<FluiBuilder<TContext, Slider>> initiateAction = null,
            Action<FluiBuilder<TContext, Slider>> updateAction = null)
        {
            RawBuild<Slider>(
                    name,
                    classes,
                    buildAction,
                    s =>
                    {
                        s.Element.label = label;
                        s.Element.value = getValue(Context);
                        s.Element.lowValue = lowValue;
                        s.Element.highValue = highValue;
                        s._valueBinding = new ValueBinding<float>(
                            () => getValue(Context), v => setValue(Context, v),
                            () => s.Element.value, v => s.Element.value = v);
                        initiateAction?.Invoke(s);
                    },
                    updateAction)
                .Set(x => x.PurgeUnmanagedChildren = false);

            return this;
        }

        public FluiBuilder<TContext, TVisualElement> Toggle(
            string name,
            string label,
            string classes,
            Func<TContext, bool> getValue,
            Action<TContext, bool> setValue,
            Action<FluiBuilder<TContext, Toggle>> buildAction = null,
            Action<FluiBuilder<TContext, Toggle>> initiateAction = null,
            Action<FluiBuilder<TContext, Toggle>> updateAction = null)
        {
            RawBuild<Toggle>(
                    name,
                    classes,
                    buildAction,
                    s =>
                    {
                        s.Element.label = label;
                        s.Element.value = getValue(Context);
                        s._valueBinding = new ValueBinding<bool>(
                            () => getValue(Context), v => setValue(Context, v),
                            () => s.Element.value, v => s.Element.value = v);
                        initiateAction?.Invoke(s);
                    },
                    updateAction)
                .Set(x => x.PurgeUnmanagedChildren = false);

            return this;
        }

        public FluiBuilder<TContext, TVisualElement> TextField(
            string name,
            string label,
            string classes,
            Func<TContext, string> getValue,
            Action<TContext, string> setValue,
            Action<FluiBuilder<TContext, TextField>> buildAction = null,
            Action<FluiBuilder<TContext, TextField>> initiateAction = null,
            Action<FluiBuilder<TContext, TextField>> updateAction = null)
        {
            RawBuild<TextField>(
                    name,
                    classes,
                    buildAction,
                    s =>
                    {
                        s.Element.label = label;
                        s.Element.value = getValue(Context);
                        s.Element.focusable = true;
                        // s.Element.Focus();
                        s._valueBinding = new ValueBinding<string>(
                            () => getValue(Context), v => setValue(Context, v),
                            () => s.Element.value, v => s.Element.value = v).SetLockedFunc(() => s.IsFocused);
                        initiateAction?.Invoke(s);
                    },
                    updateAction)
                .Set(x => x.PurgeUnmanagedChildren = false);

            return this;
        }

        public FluiBuilder<TContext, TVisualElement> Button(
            string name,
            string text,
            string classes,
            Action<TContext> onClick,
            Action<FluiBuilder<TContext, Button>> buildAction = null,
            Action<FluiBuilder<TContext, Button>> initiateAction = null,
            Action<FluiBuilder<TContext, Button>> updateAction = null)
        {
            RawBuild<Button>(
                name,
                classes,
                buildAction,
                b =>
                {
                    b.Element.text = text;
                    if (onClick != null)
                    {
                        b.Element.clicked += () => onClick(Context);
                    }

                    initiateAction?.Invoke(b);
                },
                b => { updateAction?.Invoke(b); });
            return this;
        }

        public FluiBuilder<TContext, TVisualElement> AddClasses(string classes)
        {
            SetClasses(classes, Element);
            return this;
        }

        private void SetClasses(string classes, VisualElement ve)
        {
            if (classes != null)
            {
                foreach (var @class in classes.Split(','))
                {
                    var tclass = @class.Trim();
                    if (!string.IsNullOrWhiteSpace(tclass))
                    {
                        ve.AddToClassList(tclass);
                    }
                }
            }
        }
    }
}
