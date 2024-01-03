using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Flui.Binder;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flui.Creator
{
    public partial class FluiCreator<TContext, TVisualElement> : IFluiCreator
        where TVisualElement : VisualElement
    {
        public FluiCreator<TChildContext, TChildVisualElement> RawCreate<TChildContext, TChildVisualElement>(
            string name,
            string classes,
            Func<TContext, TChildContext> contextFunc,
            Action<FluiCreator<TChildContext, TChildVisualElement>> buildAction = null,
            Action<FluiCreator<TChildContext, TChildVisualElement>> initiateAction = null,
            Action<FluiCreator<TChildContext, TChildVisualElement>> updateAction = null)
            where TChildVisualElement : VisualElement, new()
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new InvalidOperationException("Each VisualElement must have a name.");
            }

            var rawChild = _childCreators.GetOrCreate(name, () =>
            {
                var visualElement = new TChildVisualElement
                {
                    name = name
                };
                var flui = new FluiCreator<TChildContext, TChildVisualElement>(name, contextFunc(Context), visualElement);
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

            var child = (FluiCreator<TChildContext, TChildVisualElement>)rawChild;
            child._visited = true;
            child._updateAction?.Invoke(child);
            child._valueBinding?.Update();
            FluiHelper.SetClasses(child.Element, classes);

            if (buildAction != null)
            {
                buildAction(child);
            }

            return child;
        }

        public FluiCreator<TContext, TVisualElement> Optional(
            Func<TContext, bool> predicate,
            Action<FluiCreator<TContext, TVisualElement>> buildAction)
        {
            if (predicate(Context))
            {
                buildAction(this);
            }

            return this;
        }

        public FluiCreator<TContext, TVisualElement> VisualElement(
            string name,
            string classes,
            Action<FluiCreator<TContext, VisualElement>> buildAction = null,
            Action<FluiCreator<TContext, VisualElement>> initiateAction = null,
            Action<FluiCreator<TContext, VisualElement>> updateAction = null)
        {
            RawCreate<TContext, VisualElement>(name, classes, x => x, buildAction, initiateAction, updateAction);
            return this;
        }

        public FluiCreator<TContext, TVisualElement> Group<TChildContext>(
            string name,
            string classes,
            Func<TContext, TChildContext> contextFunc,
            Action<FluiCreator<TChildContext, VisualElement>> buildAction = null,
            Action<FluiCreator<TChildContext, VisualElement>> initiateAction = null,
            Action<FluiCreator<TChildContext, VisualElement>> updateAction = null) where TChildContext : class
        {
            RawCreate(name, classes, contextFunc, buildAction, initiateAction, updateAction);
            return this;
        }

        public FluiCreator<TContext, TVisualElement> ScrollView(
            string name,
            string classes,
            Action<FluiCreator<TContext, ScrollView>> buildAction = null,
            Action<FluiCreator<TContext, ScrollView>> initiateAction = null,
            Action<FluiCreator<TContext, ScrollView>> updateAction = null)
        {
            RawCreate(name, classes, x => x, buildAction, initiateAction, updateAction);
            return this;
        }

        public FluiCreator<TContext, TVisualElement> Label(
            Expression<Func<TContext, string>> propertyFunc,
            string classes,
            Action<FluiCreator<TContext, Label>> buildAction = null,
            Action<FluiCreator<TContext, Label>> initiateAction = null,
            Action<FluiCreator<TContext, Label>> updateAction = null)
        {
            string name = ReflectionHelper.GetPath(propertyFunc);
            var getter = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            RawCreate(
                name,
                classes,
                x => x,
                buildAction,
                b => { initiateAction?.Invoke(b); },
                b =>
                {
                    b.Element.text = getter(b.Context);
                    updateAction?.Invoke(b);
                });
            return this;
        }

        public FluiCreator<TContext, TVisualElement> Label(
            string name,
            Func<TContext, string> textFunc,
            string classes,
            Action<FluiCreator<TContext, Label>> buildAction = null,
            Action<FluiCreator<TContext, Label>> initiateAction = null,
            Action<FluiCreator<TContext, Label>> updateAction = null)
        {
            RawCreate(
                name,
                classes,
                x => x,
                buildAction,
                initiateAction,
                b =>
                {
                    b.Element.text = textFunc(Context);
                    updateAction?.Invoke(b);
                });
            return this;
        }

        public FluiCreator<TContext, TVisualElement> Slider(
            string name,
            string label,
            string classes,
            float lowValue,
            float highValue,
            Func<TContext, float> getValue,
            Action<TContext, float> setValue,
            Action<FluiCreator<TContext, Slider>> buildAction = null,
            Action<FluiCreator<TContext, Slider>> initiateAction = null,
            Action<FluiCreator<TContext, Slider>> updateAction = null)
        {
            RawCreate(
                    name,
                    classes,
                    x => x,
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

        public FluiCreator<TContext, TVisualElement> Slider(
            string name,
            string label,
            string classes,
            float lowValue,
            float highValue,
            Expression<Func<TContext, float>> propertyFunc,
            Action<FluiCreator<TContext, Slider>> buildAction = null,
            Action<FluiCreator<TContext, Slider>> initiateAction = null,
            Action<FluiCreator<TContext, Slider>> updateAction = null)
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);

            return Slider(
                name,
                label,
                classes,
                lowValue,
                highValue,
                getFunc,
                setFunc,
                buildAction,
                initiateAction,
                updateAction);
        }

        public FluiCreator<TContext, TVisualElement> Slider(
            Expression<Func<TContext, float>> propertyFunc,
            string classes,
            float lowValue,
            float highValue,
            Action<FluiCreator<TContext, Slider>> buildAction = null,
            Action<FluiCreator<TContext, Slider>> initiateAction = null,
            Action<FluiCreator<TContext, Slider>> updateAction = null)
        {
            var name = ReflectionHelper.GetPath(propertyFunc);

            return Slider(
                name,
                AddSpacesToSentence(name),
                classes,
                lowValue,
                highValue,
                propertyFunc,
                buildAction,
                initiateAction,
                updateAction);
        }

        public FluiCreator<TContext, TVisualElement> Foldout<TChildContext>(
            string name,
            string text,
            Func<TContext, TChildContext> contextFunc,
            bool initialOpen,
            string classes,
            Action<FluiCreator<TChildContext, Foldout>> bindAction = null,
            Action<FluiCreator<TChildContext, Foldout>> initiateAction = null,
            Action<FluiCreator<TChildContext, Foldout>> updateAction = null)
        {
            RawCreate<TChildContext, Foldout>(
                name,
                classes,
                contextFunc,
                bindAction,
                initiateAction: x =>
                {
                    x.Element.value = initialOpen;
                    x.Element.text = text;
                    initiateAction?.Invoke(x);
                },
                updateAction);

            return this;
        }

        public FluiCreator<TContext, TVisualElement> Toggle(
            Expression<Func<TContext, bool>> propertyFunc,
            string classes,
            Action<FluiCreator<TContext, Toggle>> buildAction = null,
            Action<FluiCreator<TContext, Toggle>> initiateAction = null,
            Action<FluiCreator<TContext, Toggle>> updateAction = null)
        {
            var name = ReflectionHelper.GetPath(propertyFunc);
            return Toggle(name, AddSpacesToSentence(name), classes, propertyFunc, buildAction, initiateAction, updateAction);
        }

        public FluiCreator<TContext, TVisualElement> Toggle(
            string name,
            string label,
            string classes,
            Expression<Func<TContext, bool>> propertyFunc,
            Action<FluiCreator<TContext, Toggle>> buildAction = null,
            Action<FluiCreator<TContext, Toggle>> initiateAction = null,
            Action<FluiCreator<TContext, Toggle>> updateAction = null)
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);

            return Toggle(name, label, classes, getFunc, setFunc, buildAction, initiateAction, updateAction);
        }

        public FluiCreator<TContext, TVisualElement> Toggle(
            string name,
            string label,
            string classes,
            Func<TContext, bool> getValue,
            Action<TContext, bool> setValue,
            Action<FluiCreator<TContext, Toggle>> buildAction = null,
            Action<FluiCreator<TContext, Toggle>> initiateAction = null,
            Action<FluiCreator<TContext, Toggle>> updateAction = null)
        {
            RawCreate(
                    name,
                    classes,
                    x => x,
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

        public FluiCreator<TContext, TVisualElement> DropdownField(
            string name,
            string label,
            string classes,
            List<string> choices,
            Func<TContext, int> getValue,
            Action<TContext, int> setValue,
            Action<FluiCreator<TContext, DropdownField>> buildAction = null,
            Action<FluiCreator<TContext, DropdownField>> initiateAction = null,
            Action<FluiCreator<TContext, DropdownField>> updateAction = null)
        {
            RawCreate(
                    name,
                    classes,
                    x => x,
                    buildAction,
                    s =>
                    {
                        s.Element.label = label;
                        s.Element.index = getValue(Context);
                        s.Element.focusable = true;
                        s.Element.choices = choices;
                        // s.Element.Focus();
                        s._valueBinding = new ValueBinding<int>(
                            () => getValue(Context), v => setValue(Context, v),
                            () => s.Element.index, v => s.Element.index = v).SetLockedFunc(() => s.IsFocused);
                        initiateAction?.Invoke(s);
                    },
                    updateAction)
                .Set(x => x.PurgeUnmanagedChildren = false);

            return this;
        }

        public FluiCreator<TContext, TVisualElement> DropdownField(
            string name,
            string label,
            string classes,
            List<string> choices,
            Expression<Func<TContext, int>> propertyFunc,
            Action<FluiCreator<TContext, DropdownField>> buildAction = null,
            Action<FluiCreator<TContext, DropdownField>> initiateAction = null,
            Action<FluiCreator<TContext, DropdownField>> updateAction = null)
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);

            return DropdownField(name, label, classes, choices, getFunc, setFunc, buildAction, initiateAction, updateAction);
        }

        public FluiCreator<TContext, TVisualElement> EnumField<TEnum>(
            Expression<Func<TContext, TEnum>> propertyFunc,
            string classes,
            Action<FluiCreator<TContext, EnumField>> buildAction = null,
            Action<FluiCreator<TContext, EnumField>> initiateAction = null,
            Action<FluiCreator<TContext, EnumField>> updateAction = null) where TEnum : Enum
        {
            var name = ReflectionHelper.GetPath(propertyFunc);
            return EnumField(name, AddSpacesToSentence(name), classes, propertyFunc, buildAction, initiateAction, updateAction);
        }

        public FluiCreator<TContext, TVisualElement> EnumField<TEnum>(
            string name,
            string label,
            string classes,
            Expression<Func<TContext, TEnum>> propertyFunc,
            Action<FluiCreator<TContext, EnumField>> buildAction = null,
            Action<FluiCreator<TContext, EnumField>> initiateAction = null,
            Action<FluiCreator<TContext, EnumField>> updateAction = null) where TEnum : Enum
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);

            return EnumField(name, label, classes, getFunc, setFunc, buildAction, initiateAction, updateAction);
        }

        public FluiCreator<TContext, TVisualElement> EnumField<TEnum>(
            string name,
            string label,
            string classes,
            Func<TContext, TEnum> getValue,
            Action<TContext, TEnum> setValue,
            Action<FluiCreator<TContext, EnumField>> buildAction = null,
            Action<FluiCreator<TContext, EnumField>> initiateAction = null,
            Action<FluiCreator<TContext, EnumField>> updateAction = null) where TEnum : Enum
        {
            RawCreate(
                    name,
                    classes,
                    x => x,
                    buildAction,
                    s =>
                    {
                        s.Element.label = label;

                        s.Element.Init(getValue(Context));
                        var valueBinding = new ValueBinding<TEnum>(
                            () => getValue(Context), v => setValue(Context, v),
                            () => (TEnum)s.Element.value, v => s.Element.value = v);
                        s._valueBinding = valueBinding;
                        initiateAction?.Invoke(s);
                    },
                    updateAction)
                .Set(x => x.PurgeUnmanagedChildren = false);

            return this;
        }

        public FluiCreator<TContext, TVisualElement> TextFieldReadOnly<TValue>(
            Expression<Func<TContext, TValue>> propertyFunc,
            Func<TContext, string> propertyStringFunc,
            string label = null,
            string labelPrefix = "")
        {
             var name = ReflectionHelper.GetPath(propertyFunc);
            VisualElement(name, "row", pr => pr
                .Label(name + "Label", _ => label ?? (labelPrefix + AddSpacesToSentence(name)), "unity-base-field")
                .Label(name + "Value", y => AddSpacesToSentence(propertyStringFunc(y)), "value")
            );
            return this;
        }

        public FluiCreator<TContext, TVisualElement> TextFieldReadOnly(
            string name,
            string label,
            string value)
        {
            VisualElement(name, "row", pr => pr
                .Label(name + "Label", _ => label, "unity-base-field")
                .Label(name + "Value", y => value, "value")
            );
            return this;
        }
        
        public FluiCreator<TContext, TVisualElement> TextField(
            string name,
            string label,
            string classes,
            Func<TContext, string> getValue,
            Action<TContext, string> setValue,
            Action<FluiCreator<TContext, TextField>> buildAction = null,
            Action<FluiCreator<TContext, TextField>> initiateAction = null,
            Action<FluiCreator<TContext, TextField>> updateAction = null)
        {
            RawCreate(
                    name,
                    classes,
                    x => x,
                    buildAction,
                    s =>
                    {
                        s.Element.label = label;
                        s.Element.value = getValue(Context);
                        s.Element.focusable = true;
                        // s.Element.Focus();
                        var valueBinding = SetUpdateOnReturn(
                            s,
                            new ValueBinding<string>(
                                () => getValue(Context), v => setValue(Context, v),
                                () => s.Element.value, v => s.Element.value = v));


                        s._valueBinding = valueBinding;
                        initiateAction?.Invoke(s);
                    },
                    updateAction)
                .Set(x => x.PurgeUnmanagedChildren = false);

            return this;
        }

        public FluiCreator<TContext, TVisualElement> TextField(
            Expression<Func<TContext, string>> propertyFunc,
            string classes,
            Action<FluiCreator<TContext, TextField>> buildAction = null,
            Action<FluiCreator<TContext, TextField>> initiateAction = null,
            Action<FluiCreator<TContext, TextField>> updateAction = null)
        {
            var name = ReflectionHelper.GetPath(propertyFunc);

            return TextField(name, AddSpacesToSentence(name), classes, propertyFunc, buildAction, initiateAction, updateAction);
        }

        public FluiCreator<TContext, TVisualElement> TextField(
            string name,
            string label,
            string classes,
            Expression<Func<TContext, string>> propertyFunc,
            Action<FluiCreator<TContext, TextField>> buildAction = null,
            Action<FluiCreator<TContext, TextField>> initiateAction = null,
            Action<FluiCreator<TContext, TextField>> updateAction = null)
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);

            return TextField(name, label, classes, getFunc, setFunc, buildAction, initiateAction, updateAction);
        }

        public FluiCreator<TContext, TVisualElement> IntegerField(
            Expression<Func<TContext, int>> propertyFunc,
            string classes,
            Action<FluiCreator<TContext, IntegerField>> buildAction = null,
            Action<FluiCreator<TContext, IntegerField>> initiateAction = null,
            Action<FluiCreator<TContext, IntegerField>> updateAction = null)
        {
            var name = ReflectionHelper.GetPath(propertyFunc);
            return IntegerField(name, AddSpacesToSentence(name), classes, propertyFunc, buildAction, initiateAction, updateAction);
        }

        public FluiCreator<TContext, TVisualElement> IntegerField(
            string name,
            string label,
            string classes,
            Expression<Func<TContext, int>> propertyFunc,
            Action<FluiCreator<TContext, IntegerField>> buildAction = null,
            Action<FluiCreator<TContext, IntegerField>> initiateAction = null,
            Action<FluiCreator<TContext, IntegerField>> updateAction = null)
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);

            return IntegerField(name, label, classes, getFunc, setFunc, buildAction, initiateAction, updateAction);
        }

        public FluiCreator<TContext, TVisualElement> IntegerField(
            string name,
            string label,
            string classes,
            Func<TContext, int> getValue,
            Action<TContext, int> setValue,
            Action<FluiCreator<TContext, IntegerField>> buildAction = null,
            Action<FluiCreator<TContext, IntegerField>> initiateAction = null,
            Action<FluiCreator<TContext, IntegerField>> updateAction = null)
        {
            RawCreate(
                    name,
                    classes,
                    x => x,
                    buildAction,
                    s =>
                    {
                        s.Element.label = label;
                        s.Element.value = getValue(Context);
                        s.Element.focusable = true;
                        // s.Element.Focus();
                        s._valueBinding = SetUpdateOnReturn(
                            s,
                            new ValueBinding<int>(
                                () => getValue(Context), v => setValue(Context, v),
                                () => s.Element.value, v => s.Element.value = v));
                        initiateAction?.Invoke(s);
                    },
                    updateAction)
                .Set(x => x.PurgeUnmanagedChildren = false);

            return this;
        }

        public FluiCreator<TContext, TVisualElement> FloatField(
            string name,
            string label,
            string classes,
            Func<TContext, float> getValue,
            Action<TContext, float> setValue,
            Action<FluiCreator<TContext, FloatField>> buildAction = null,
            Action<FluiCreator<TContext, FloatField>> initiateAction = null,
            Action<FluiCreator<TContext, FloatField>> updateAction = null)
        {
            RawCreate(
                    name,
                    classes,
                    x => x,
                    buildAction,
                    s =>
                    {
                        s.Element.label = label;
                        s.Element.value = getValue(Context);
                        s.Element.focusable = true;
                        // s.Element.Focus();
                        s._valueBinding = SetUpdateOnReturn(
                            s, new ValueBinding<float>(
                                () => getValue(Context), v => setValue(Context, v),
                                () => s.Element.value, v => s.Element.value = v));
                        initiateAction?.Invoke(s);
                    },
                    updateAction)
                .Set(x => x.PurgeUnmanagedChildren = false);

            return this;
        }

        public FluiCreator<TContext, TVisualElement> FloatField(
            Expression<Func<TContext, float>> propertyFunc,
            string classes,
            Action<FluiCreator<TContext, FloatField>> buildAction = null,
            Action<FluiCreator<TContext, FloatField>> initiateAction = null,
            Action<FluiCreator<TContext, FloatField>> updateAction = null)
        {
            var name = ReflectionHelper.GetPath(propertyFunc);

            return FloatField(name, AddSpacesToSentence(name), classes, propertyFunc, buildAction, initiateAction, updateAction);
        }

        public FluiCreator<TContext, TVisualElement> FloatField(
            string name,
            string label,
            string classes,
            Expression<Func<TContext, float>> propertyFunc,
            Action<FluiCreator<TContext, FloatField>> buildAction = null,
            Action<FluiCreator<TContext, FloatField>> initiateAction = null,
            Action<FluiCreator<TContext, FloatField>> updateAction = null)
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);

            return FloatField(name, label, classes, getFunc, setFunc, buildAction, initiateAction, updateAction);
        }

        public FluiCreator<TContext, TVisualElement> Button(
            string name,
            string text,
            string classes,
            Action<FluiCreator<TContext, Button>> onClick,
            Action<FluiCreator<TContext, Button>> buildAction = null,
            Action<FluiCreator<TContext, Button>> initiateAction = null,
            Action<FluiCreator<TContext, Button>> updateAction = null)
        {
            RawCreate(
                name,
                classes,
                x => x,
                buildAction,
                b =>
                {
                    b.Element.text = text;
                    if (onClick != null)
                    {
                        b.Element.clicked += () => onClick(b);
                    }

                    initiateAction?.Invoke(b);
                },
                b => { updateAction?.Invoke(b); });
            return this;
        }

        public FluiCreator<TContext, TVisualElement> ForEach<TChildContext>(
            Expression<Func<TContext, IEnumerable<TChildContext>>> itemsFunc,
            string classes,
            Action<FluiCreator<TChildContext, VisualElement>> bindAction = null,
            Action<FluiCreator<TChildContext, VisualElement>> initiateAction = null,
            Action<FluiCreator<TChildContext, VisualElement>> updateAction = null)
        {
            var name = ReflectionHelper.GetPath(itemsFunc);
            var getFunc = ReflectionHelper.GetPropertyValueFunc(itemsFunc);
            ForEach(name, getFunc, classes, bindAction, initiateAction, updateAction);
            return this;
        }

        public FluiCreator<TContext, TVisualElement> ForEach<TChildContext>(
            string name,
            Func<TContext, IEnumerable<TChildContext>> itemsFunc,
            string classes,
            Action<FluiCreator<TChildContext, VisualElement>> bindAction = null,
            Action<FluiCreator<TChildContext, VisualElement>> initiateAction = null,
            Action<FluiCreator<TChildContext, VisualElement>> updateAction = null)
        {
            RawCreate<TContext, VisualElement>(
                name,
                "",
                x => x,
                s =>
                {
                    // No bind action for the container (?)
                },
                s => { },
                s =>
                {
                    s.SynchronizeList(
                        classes,
                        itemsFunc,
                        bindAction,
                        initiateAction,
                        updateAction);
                });

            return this;
        }

        private void SynchronizeList<TChildContext>(
            string classes,
            Func<TContext, IEnumerable<TChildContext>> itemsFunc,
            Action<FluiCreator<TChildContext, VisualElement>> bindAction,
            Action<FluiCreator<TChildContext, VisualElement>> initiateAction,
            Action<FluiCreator<TChildContext, VisualElement>> updateAction)
        {
            var children = itemsFunc(Context);
            HashSet<VisualElement> unvisited = new HashSet<VisualElement>(Element.Children());
            foreach (var context in children)
            {
                // Could be slow - need two way dictionary
                var rawFlui = _childCreators.Values.SingleOrDefault(x => Equals(x.Context, context));
                var veName = "";
                if (rawFlui == null)
                {
                    var visualElement = new VisualElement();
                    veName = visualElement.name = Guid.NewGuid().ToString().Replace("-", "");
                    Element.Add(visualElement);
                }
                else
                {
                    veName = rawFlui.Name;
                }

                var flui = RawCreate<TChildContext, VisualElement>(
                    veName,
                    classes,
                    xy => context,
                    bindAction,
                    initiateAction,
                    updateAction);

                unvisited.Remove(flui.Element);
            }

            foreach (var visualElement in unvisited)
            {
                if (visualElement.parent != null)
                {
                    Element.Remove(visualElement);
                }

                var childCreator = _childCreators.SafeGetValue(visualElement.name);
                if (childCreator != null)
                {
                    FluiCreatorStats.FluisRemoved += childCreator.GetHierarchyChildCount();
                    _childCreators.Remove(visualElement.name);
                }
            }
        }

        public FluiCreator<TContext, TVisualElement> EnumButtons<TEnum>(
            string name,
            string classes,
            Expression<Func<TContext, TEnum>> propertyFunc,
            Action<EnumButtonBinder<TEnum>> buttonsAction,
            string activeClass = "active",
            Action<FluiCreator<TContext, VisualElement>> bindAction = null,
            Action<FluiCreator<TContext, VisualElement>> initiateAction = null,
            Action<FluiCreator<TContext, VisualElement>> updateAction = null) where TEnum : Enum
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            var bb = new EnumButtonBinder<TEnum>();
            buttonsAction(bb);
            var buttons = bb.Buttons;
            RawCreate(
                name,
                classes,
                x => x,
                f =>
                {
                    foreach (var button in buttons)
                    {
                        f.Button(
                            button.Value.ToString(),
                            button.Label,
                            button.Classes,
                            b => setFunc(b.Context, button.Value),
                            buildAction: b => b
                                .OptionalClass(activeClass, ctx => Equals(getFunc(ctx), button.Value)));
                    }

                    bindAction?.Invoke(f);
                },
                initiateAction,
                updateAction);
            return this;
        }

        public class EnumButtonBinder<TEnum> where TEnum : Enum
        {
            public List<EnumButton<TEnum>> Buttons { get; set; } = new();

            public EnumButtonBinder<TEnum> EnumButton(TEnum value, string label, string classes)
            {
                Buttons.Add(new EnumButton<TEnum>(value, label, classes));
                return this;
            }
        }

        public class EnumButton<TEnum> where TEnum : Enum
        {
            public TEnum Value { get; }
            public string Label { get; }
            public string Classes { get; }

            public EnumButton(TEnum value, string label, string classes)
            {
                Value = value;
                Label = label;
                Classes = classes;
            }
        }

        public FluiCreator<TContext, TVisualElement> EnumSwitch<TEnum>(
            string name,
            string classes,
            Func<TContext, TEnum> enumGetter,
            Action<Switcher<TEnum>> bindAction)
        {
            RawCreate<TContext, VisualElement>(
                name,
                classes,
                x => x,
                buildAction: null,
                initiateAction: null,
                updateAction: h =>
                {
                    var switcher = new Switcher<TEnum>(h, enumGetter(Context));
                    bindAction(switcher);
                });
            return this;
        }

        public class Switcher<TEnum>
        {
            private readonly FluiCreator<TContext, VisualElement> _fluiCreator;
            private readonly TEnum _currentValue;

            public Switcher(FluiCreator<TContext, VisualElement> fluiCreator, TEnum currentValue)
            {
                _fluiCreator = fluiCreator;
                _currentValue = currentValue;
            }

            public Switcher<TEnum> Case<TChildContext>(
                TEnum value,
                string classes,
                Func<TContext, TChildContext> contextFunc,
                Action<FluiCreator<TChildContext, VisualElement>> bindAction = null,
                Action<FluiCreator<TChildContext, VisualElement>> initiateAction = null,
                Action<FluiCreator<TChildContext, VisualElement>> updateAction = null) where TChildContext : class
            {
                if (Equals(_currentValue, value))
                {
                    _fluiCreator
                        .RawCreate<TChildContext, VisualElement>(
                            value.ToString(),
                            classes,
                            contextFunc,
                            bindAction,
                            initiateAction,
                            updateAction);
                }

                return this;
            }
        }

        public FluiCreator<TContext, TVisualElement> Set(Action<FluiCreator<TContext, TVisualElement>> setAction)
        {
            setAction(this);
            return this;
        }

        public static string AddSpacesToSentence(string text)
        {
            text = text.Replace("_", "");
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(char.ToUpper(text[0]));

            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]) && char.IsLower(text[i - 1]))
                {
                    newText.Append(' ');
                }

                newText.Append(text[i]);
            }

            return newText.ToString();
        }

        private IValueBinding SetUpdateOnReturn<T, TVE>(FluiCreator<TContext, TVE> fluiCreator, ValueBinding<T> valueBinding)
            where TVE : VisualElement
        {
            valueBinding.SetLockedFunc(() =>
            {
                if (!fluiCreator.IsFocused)
                {
                    return false;
                }

                if (Input.GetKeyDown(KeyCode.Return))
                {
                    // We're focused and the user pressed enter.
                    return false;
                }

                return fluiCreator.IsFocused;
            });
            return valueBinding;
        }
    }
}