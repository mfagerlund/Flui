using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Flui.Binder;
using UnityEngine;
using UnityEngine.UIElements;
using static Flui.FluiHelper;

namespace Flui.Creator
{
    public partial class FluiCreator<TContext, TVisualElement>
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
            child.Context = contextFunc(Context);
            child._visited = true;
            child._updateAction?.Invoke(child);
            child.ValueBinding?.Update();
            FluiHelper.AddClasses(child.Element, classes);

            if (buildAction != null)
            {
                buildAction(child);
            }

            return child;
        }

        public FluiCreator<TContext, TVisualElement> Exec(
            Action<FluiCreator<TContext, TVisualElement>> buildAction)
        {
            buildAction(this);
            return this;
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

        public FluiCreator<TContext, TVisualElement> IfOfType<TWantedContextType>(
            string classes,
            Action<FluiCreator<TWantedContextType, VisualElement>> buildAction = null,
            Action<FluiCreator<TWantedContextType, VisualElement>> initiateAction = null,
            Action<FluiCreator<TWantedContextType, VisualElement>> updateAction = null) where TWantedContextType : TContext
        {
            if (Context is TWantedContextType)
            {
                RawCreate(
                    typeof(TWantedContextType).Name,
                    classes,
                    x => (TWantedContextType)x,
                    buildAction,
                    initiateAction,
                    updateAction);
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

        public FluiCreator<TContext, TVisualElement> Group<TChildContext>(
            Expression<Func<TContext, TChildContext>> contextFunc,
            string classes,
            Action<FluiCreator<TChildContext, VisualElement>> buildAction = null,
            Action<FluiCreator<TChildContext, VisualElement>> initiateAction = null,
            Action<FluiCreator<TChildContext, VisualElement>> updateAction = null) where TChildContext : class
        {
            // string name = ReflectionHelper.GetPath(contextFunc);
            // var getter = ReflectionHelper.GetPropertyValueFunc(contextFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(contextFunc);
            RawCreate(expc.Path, classes, expc.Getter, buildAction, initiateAction, updateAction);
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
            return Label(propertyFunc, x => x, classes, buildAction, initiateAction, updateAction);
        }

        public FluiCreator<TContext, TVisualElement> Label<TValue>(
            Expression<Func<TContext, TValue>> propertyFunc,
            Func<TValue, string> toStringFunc,
            string classes,
            Action<FluiCreator<TContext, Label>> buildAction = null,
            Action<FluiCreator<TContext, Label>> initiateAction = null,
            Action<FluiCreator<TContext, Label>> updateAction = null)
        {
            // string name = ReflectionHelper.GetPath(propertyFunc);
            // var getter = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);

            RawCreate(
                expc.Path,
                classes,
                x => x,
                buildAction,
                b => { initiateAction?.Invoke(b); },
                b =>
                {
                    b.Element.text = toStringFunc(expc.Getter(b.Context));
                    updateAction?.Invoke(b);
                });
            return this;
        }

        public FluiCreator<TContext, TVisualElement> Label(
            string nameAndText,
            string classes,
            Action<FluiCreator<TContext, Label>> buildAction = null,
            Action<FluiCreator<TContext, Label>> initiateAction = null,
            Action<FluiCreator<TContext, Label>> updateAction = null)
        {
            return Label(
                SanitizeUIToolkitName(nameAndText),
                AddSpacesToSentence(nameAndText),
                classes,
                buildAction,
                initiateAction,
                updateAction);
        }

        public FluiCreator<TContext, TVisualElement> Label(
            string name,
            string text,
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
                b =>
                {
                    initiateAction?.Invoke(b);
                    b.Element.text = text;
                },
                updateAction);
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
            Action<FluiCreator<TContext, Slider>> updateAction = null,
            Func<float, string> postfixFunc = null,
            Action onModelChanged = null)
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
                        s.ValueBinding = new ValueBinding<float>(
                            () => getValue(Context), v => setValue(Context, v),
                            () => s.Element.value, v => s.Element.value = v,
                            onModelChanged);
                        initiateAction?.Invoke(s);
                    },
                    s =>
                    {
                        updateAction?.Invoke(s);
                        if (postfixFunc != null)
                        {
                            s.Element.label = label + postfixFunc(getValue(Context));
                        }
                    })
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
            Action<FluiCreator<TContext, Slider>> updateAction = null,
            Func<float, string> postfixFunc = null,
            Action onModelChanged = null)
        {
            // var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            // var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);

            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);

            return Slider(
                name,
                label,
                classes,
                lowValue,
                highValue,
                expc.Getter,
                expc.Setter,
                buildAction,
                initiateAction,
                updateAction,
                postfixFunc,
                onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> Slider(
            Expression<Func<TContext, float>> propertyFunc,
            string classes,
            float lowValue,
            float highValue,
            Action<FluiCreator<TContext, Slider>> buildAction = null,
            Action<FluiCreator<TContext, Slider>> initiateAction = null,
            Action<FluiCreator<TContext, Slider>> updateAction = null,
            Func<float, string> postfixFunc = null,
            Action onModelChanged = null)
        {
            // var name = ReflectionHelper.GetPath(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);

            return Slider(
                expc.Path,
                AddSpacesToSentence(expc.FinalPathSegment),
                classes,
                lowValue,
                highValue,
                propertyFunc,
                buildAction,
                initiateAction,
                updateAction,
                postfixFunc,
                onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> SliderInt(
            string name,
            string label,
            string classes,
            int lowValue,
            int highValue,
            Func<TContext, int> getValue,
            Action<TContext, int> setValue,
            Action<FluiCreator<TContext, SliderInt>> buildAction = null,
            Action<FluiCreator<TContext, SliderInt>> initiateAction = null,
            Action<FluiCreator<TContext, SliderInt>> updateAction = null,
            Func<int, string> postfixFunc = null,
            Action onModelChanged = null)
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
                        s.ValueBinding = new ValueBinding<int>(
                            () => getValue(Context), v => setValue(Context, v),
                            () => s.Element.value, v => s.Element.value = v,
                            onModelChanged);
                        initiateAction?.Invoke(s);
                    },
                    s =>
                    {
                        updateAction?.Invoke(s);
                        if (postfixFunc != null)
                        {
                            s.Element.label = label + postfixFunc(getValue(s.Context));
                        }
                    })
                .Set(x => x.PurgeUnmanagedChildren = false);

            return this;
        }

        public FluiCreator<TContext, TVisualElement> SliderInt(
            string name,
            string label,
            string classes,
            int lowValue,
            int highValue,
            Expression<Func<TContext, int>> propertyFunc,
            Action<FluiCreator<TContext, SliderInt>> buildAction = null,
            Action<FluiCreator<TContext, SliderInt>> initiateAction = null,
            Action<FluiCreator<TContext, SliderInt>> updateAction = null,
            Func<int, string> postfixFunc = null,
            Action onModelChanged = null)
        {
            // var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            // var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);

            return SliderInt(
                name,
                label,
                classes,
                lowValue,
                highValue,
                expc.Getter,
                expc.Setter,
                buildAction,
                initiateAction,
                updateAction,
                postfixFunc,
                onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> SliderInt(
            Expression<Func<TContext, int>> propertyFunc,
            string classes,
            int lowValue,
            int highValue,
            Action<FluiCreator<TContext, SliderInt>> buildAction = null,
            Action<FluiCreator<TContext, SliderInt>> initiateAction = null,
            Action<FluiCreator<TContext, SliderInt>> updateAction = null,
            Func<int, string> postfixFunc = null,
            Action onModelChanged = null)
        {
            // var name = ReflectionHelper.GetPath(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);

            return SliderInt(
                expc.Path,
                AddSpacesToSentence(expc.FinalPathSegment),
                classes,
                lowValue,
                highValue,
                propertyFunc,
                buildAction,
                initiateAction,
                updateAction,
                postfixFunc,
                onModelChanged);
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

        public FluiCreator<TContext, TVisualElement> Image(
            string name,
            string classes,
            Func<TContext, Sprite> spriteFunc,
            Action<FluiCreator<TContext, Image>> buildAction = null,
            Action<FluiCreator<TContext, Image>> initiateAction = null,
            Action<FluiCreator<TContext, Image>> updateAction = null)
        {
            RawCreate(
                    name,
                    classes,
                    x => x,
                    buildAction,
                    s =>
                    {
                        s.Element.sprite = spriteFunc(Context);
                        initiateAction?.Invoke(s);
                    },
                    updateAction)
                .Set(x => x.PurgeUnmanagedChildren = false);

            return this;
        }

        public FluiCreator<TContext, TVisualElement> Toggle(
            Expression<Func<TContext, bool>> propertyFunc,
            string classes,
            Action<FluiCreator<TContext, Toggle>> buildAction = null,
            Action<FluiCreator<TContext, Toggle>> initiateAction = null,
            Action<FluiCreator<TContext, Toggle>> updateAction = null,
            Action onModelChanged = null)
        {
            //var name = ReflectionHelper.GetPath(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);
            return Toggle(
                expc.Path,
                AddSpacesToSentence(expc.FinalPathSegment),
                classes,
                propertyFunc,
                buildAction,
                initiateAction,
                updateAction,
                onModelChanged: onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> Toggle(
            string name,
            string label,
            string classes,
            Expression<Func<TContext, bool>> propertyFunc,
            Action<FluiCreator<TContext, Toggle>> buildAction = null,
            Action<FluiCreator<TContext, Toggle>> initiateAction = null,
            Action<FluiCreator<TContext, Toggle>> updateAction = null,
            Action onModelChanged = null)
        {
            // var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            // var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);

            return Toggle(name, label, classes, expc.Getter, expc.Setter, buildAction, initiateAction, updateAction, onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> Toggle(
            string name,
            string label,
            string classes,
            Func<TContext, bool> getValue,
            Action<TContext, bool> setValue,
            Action<FluiCreator<TContext, Toggle>> buildAction = null,
            Action<FluiCreator<TContext, Toggle>> initiateAction = null,
            Action<FluiCreator<TContext, Toggle>> updateAction = null,
            Action onModelChanged = null)
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
                        s.ValueBinding = new ValueBinding<bool>(
                            () => getValue(Context), v => setValue(Context, v),
                            () => s.Element.value, v => s.Element.value = v,
                            onModelChanged);
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
            Action<FluiCreator<TContext, DropdownField>> updateAction = null,
            Action onModelChanged = null)
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
                        s.ValueBinding = new ValueBinding<int>(
                            () => getValue(Context), v => setValue(Context, v),
                            () => s.Element.index, v => s.Element.index = v,
                            onModelChanged); //.SetLockedFunc(() => s.IsFocused);
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
            Action<FluiCreator<TContext, DropdownField>> updateAction = null,
            Action onModelChanged = null)
        {
            // var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            // var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);

            return DropdownField(
                name,
                label,
                classes,
                choices,
                expc.Getter,
                expc.Setter,
                buildAction,
                initiateAction,
                updateAction,
                onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> DropdownField<TElement>(
            Expression<Func<TContext, TElement>> propertyFunc,
            string classes,
            List<TElement> choices,
            Func<TElement, string> labelFunc,
            Action<FluiCreator<TContext, DropdownField>> buildAction = null,
            Action<FluiCreator<TContext, DropdownField>> initiateAction = null,
            Action<FluiCreator<TContext, DropdownField>> updateAction = null,
            Action onModelChanged = null)
        {
            // It's expensive to call ReflectionHelper.GetPath(propertyFunc) over and over again each frame,
            // we could try to wrap this in something that keeps track of the data.
            // var name = ReflectionHelper.GetPath(propertyFunc);
            // var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            // var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);

            return DropdownField(
                expc.Path,
                AddSpacesToSentence(expc.FinalPathSegment),
                classes,
                choices.Select(labelFunc).ToList(),
                ctx => choices.IndexOf(expc.Getter(ctx)),
                (ctx, i) => expc.Setter(ctx, choices[i]),
                buildAction,
                initiateAction,
                updateAction,
                onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> EnumField<TEnum>(
            Expression<Func<TContext, TEnum>> propertyFunc,
            string classes,
            Action<FluiCreator<TContext, EnumField>> buildAction = null,
            Action<FluiCreator<TContext, EnumField>> initiateAction = null,
            Action<FluiCreator<TContext, EnumField>> updateAction = null,
            Action onModelChanged = null) where TEnum : Enum
        {
            // var name = ReflectionHelper.GetPath(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);
            return EnumField(
                expc.Path,
                AddSpacesToSentence(expc.FinalPathSegment),
                classes,
                propertyFunc,
                buildAction,
                initiateAction,
                updateAction,
                onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> EnumField<TEnum>(
            string name,
            string label,
            string classes,
            Expression<Func<TContext, TEnum>> propertyFunc,
            Action<FluiCreator<TContext, EnumField>> buildAction = null,
            Action<FluiCreator<TContext, EnumField>> initiateAction = null,
            Action<FluiCreator<TContext, EnumField>> updateAction = null,
            Action onModelChanged = null) where TEnum : Enum
        {
            // var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            // var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);

            return EnumField(
                name,
                label,
                classes,
                expc.Getter,
                expc.Setter,
                buildAction,
                initiateAction,
                updateAction,
                onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> EnumField<TEnum>(
            string name,
            string label,
            string classes,
            Func<TContext, TEnum> getValue,
            Action<TContext, TEnum> setValue,
            Action<FluiCreator<TContext, EnumField>> buildAction = null,
            Action<FluiCreator<TContext, EnumField>> initiateAction = null,
            Action<FluiCreator<TContext, EnumField>> updateAction = null,
            Action onModelChanged = null) where TEnum : Enum
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
                            () => (TEnum)s.Element.value, v => s.Element.value = v,
                            onModelChanged);
                        s.ValueBinding = valueBinding;
                        initiateAction?.Invoke(s);
                    },
                    updateAction)
                .Set(x => x.PurgeUnmanagedChildren = false);

            return this;
        }

        public FluiCreator<TContext, TVisualElement> TextFieldReadOnly(
            Expression<Func<TContext, string>> propertyFunc,
            string classes,
            string label = null,
            string labelPrefix = "")
        {
            return TextFieldReadOnly(propertyFunc, x => x, classes, label, labelPrefix);
        }

        public FluiCreator<TContext, TVisualElement> TextFieldReadOnly<TType>(
            Expression<Func<TContext, TType>> propertyFunc,
            Func<TType, string> toStringFunc,
            string classes,
            string label = null,
            string labelPrefix = "")
        {
            // var name = ReflectionHelper.GetPath(propertyFunc);
            // var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);

            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);
            var name = expc.Path;

            var topClasses = "row,unity-base-field,unity-base-text-field,unity-text-field";
            if (classes != null)
            {
                topClasses = topClasses + "," + classes;
            }

            VisualElement(name, topClasses, pr => pr
                .Label(name + "Label", _ => label ?? (labelPrefix + AddSpacesToSentence(expc.FinalPathSegment)), "unity-text-element,unity-label,unity-base-field__label,unity-base-text-field__label,unity-text-field__label")
                .VisualElement(name + "Value", "unity-base-text-field__input,unity-base-text-field__input--single-line,unity-base-field__input,unity-text-field__input,readonly", g => g
                    .Label(name + "Value", x => toStringFunc(expc.Getter(x)), "unity-text-element,unity-text-element--inner-input-field-component")
                )
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
            Action<FluiCreator<TContext, TextField>> updateAction = null,
            Action onModelChanged = null)
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
                                () => s.Element.value, v => s.Element.value = v,
                                onModelChanged));


                        s.ValueBinding = valueBinding;
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
            Action<FluiCreator<TContext, TextField>> updateAction = null,
            Action onModelChanged = null)
        {
            // var name = ReflectionHelper.GetPath(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);

            return TextField(
                expc.Path,
                AddSpacesToSentence(expc.FinalPathSegment),
                classes,
                propertyFunc,
                buildAction,
                initiateAction,
                updateAction,
                onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> TextField(
            string name,
            string label,
            string classes,
            Expression<Func<TContext, string>> propertyFunc,
            Action<FluiCreator<TContext, TextField>> buildAction = null,
            Action<FluiCreator<TContext, TextField>> initiateAction = null,
            Action<FluiCreator<TContext, TextField>> updateAction = null,
            Action onModelChanged = null)
        {
            // var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            // var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);

            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);

            return TextField(
                name,
                label,
                classes,
                expc.Getter,
                expc.Setter,
                buildAction,
                initiateAction,
                updateAction,
                onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> IntegerField(
            Expression<Func<TContext, int>> propertyFunc,
            string classes,
            Action<FluiCreator<TContext, IntegerField>> buildAction = null,
            Action<FluiCreator<TContext, IntegerField>> initiateAction = null,
            Action<FluiCreator<TContext, IntegerField>> updateAction = null,
            Action onModelChanged = null)
        {
            // var name = ReflectionHelper.GetPath(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);
            return IntegerField(
                expc.Path,
                AddSpacesToSentence(expc.FinalPathSegment),
                classes,
                propertyFunc,
                buildAction,
                initiateAction,
                updateAction,
                onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> IntegerField(
            string name,
            string label,
            string classes,
            Expression<Func<TContext, int>> propertyFunc,
            Action<FluiCreator<TContext, IntegerField>> buildAction = null,
            Action<FluiCreator<TContext, IntegerField>> initiateAction = null,
            Action<FluiCreator<TContext, IntegerField>> updateAction = null,
            Action onModelChanged = null)
        {
            // var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            // var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);

            return IntegerField(
                name,
                label,
                classes,
                expc.Getter,
                expc.Setter,
                buildAction,
                initiateAction,
                updateAction,
                onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> IntegerField(
            string name,
            string label,
            string classes,
            Func<TContext, int> getValue,
            Action<TContext, int> setValue,
            Action<FluiCreator<TContext, IntegerField>> buildAction = null,
            Action<FluiCreator<TContext, IntegerField>> initiateAction = null,
            Action<FluiCreator<TContext, IntegerField>> updateAction = null,
            Action onModelChanged = null)
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
                        s.ValueBinding = SetUpdateOnReturn(
                            s,
                            new ValueBinding<int>(
                                () => getValue(Context), v => setValue(Context, v),
                                () => s.Element.value, v => s.Element.value = v,
                                onModelChanged));
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
            Action<FluiCreator<TContext, FloatField>> updateAction = null,
            Action onModelChanged = null)
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
                        s.ValueBinding = SetUpdateOnReturn(
                            s, new ValueBinding<float>(
                                () => getValue(Context), v => setValue(Context, v),
                                () => s.Element.value, v => s.Element.value = v,
                                onModelChanged));
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
            Action<FluiCreator<TContext, FloatField>> updateAction = null,
            Action onModelChanged = null)
        {
            // var name = ReflectionHelper.GetPath(propertyFunc);
            // var name = CachedExpressionHelper.GetCachedExpression(propertyFunc).Path;
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);
            return FloatField(
                expc.Path,
                AddSpacesToSentence(expc.FinalPathSegment),
                classes,
                propertyFunc,
                buildAction,
                initiateAction,
                updateAction,
                onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> FloatField(
            string name,
            string label,
            string classes,
            Expression<Func<TContext, float>> propertyFunc,
            Action<FluiCreator<TContext, FloatField>> buildAction = null,
            Action<FluiCreator<TContext, FloatField>> initiateAction = null,
            Action<FluiCreator<TContext, FloatField>> updateAction = null,
            Action onModelChanged = null)
        {
            // var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            // var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);

            return FloatField(
                name,
                label,
                classes,
                expc.Getter,
                expc.Setter,
                buildAction,
                initiateAction,
                updateAction,
                onModelChanged);
        }

        public FluiCreator<TContext, TVisualElement> Button(
            Expression<Action<FluiCreator<TContext, Button>>> onClick,
            string classes,
            string text = null,
            string name = null,
            Func<TContext, bool> enabledFunc = null,
            Action<FluiCreator<TContext, Button>> buildAction = null,
            Action<FluiCreator<TContext, Button>> initiateAction = null,
            Action<FluiCreator<TContext, Button>> updateAction = null)
        {
            name ??= CachedExpressionHelper.GetMethodName(onClick);
            return Button(name, onClick, classes, text, enabledFunc, buildAction, initiateAction, updateAction);
        }

        public FluiCreator<TContext, TVisualElement> Button(
            string name,
            Expression<Action<FluiCreator<TContext, Button>>> onClick,
            string classes,
            string text = null,
            Func<TContext, bool> enabledFunc = null,
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
                    b.Element.text = text ?? AddSpacesToSentence(name);
                    if (onClick != null)
                    {
                        var compiled = onClick.Compile();
                        b.Element.clicked += () =>
                        {
                            if (enabledFunc == null || enabledFunc(b.Context))
                            {
                                compiled(b);
                            }
                        };
                    }

                    initiateAction?.Invoke(b);
                },
                b =>
                {
                    updateAction?.Invoke(b);
                    if (enabledFunc != null)
                    {
                        if (enabledFunc(b.Context))
                        {
                            b.Element.RemoveFromClassList("disabled");
                        }
                        else
                        {
                            b.Element.AddToClassList("disabled");
                        }
                    }
                });
            return this;
        }

        // public FluiCreator<TContext, TVisualElement> Button(
        //     string name,
        //     string text,
        //     string classes,
        //     Expression<Action<TContext>> onClick,
        //     Action<FluiCreator<TContext, Button>> buildAction = null,
        //     Action<FluiCreator<TContext, Button>> initiateAction = null,
        //     Action<FluiCreator<TContext, Button>> updateAction = null)
        // {
        //     RawCreate(
        //         name,
        //         classes,
        //         x => x,
        //         buildAction,
        //         b =>
        //         {
        //             b.Element.text = text;
        //             if (onClick != null)
        //             {
        //                 b.Element.clicked += () => onClick(b);
        //             }
        //
        //             initiateAction?.Invoke(b);
        //         },
        //         b => { updateAction?.Invoke(b); });
        //     return this;
        // }

        public FluiCreator<TContext, TVisualElement> ForEach<TChildContext>(
            Expression<Func<TContext, IEnumerable<TChildContext>>> itemsFunc,
            string groupClasses,
            string childClasses,
            Action<FluiCreator<TChildContext, VisualElement>> bindAction = null,
            Action<FluiCreator<TChildContext, VisualElement>> initiateAction = null,
            Action<FluiCreator<TChildContext, VisualElement>> updateAction = null)
        {
            // var name = ReflectionHelper.GetPath(itemsFunc);
            // var getFunc = ReflectionHelper.GetPropertyValueFunc(itemsFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(itemsFunc);
            ForEach(expc.Path, expc.Getter, groupClasses, childClasses, bindAction, initiateAction, updateAction);
            return this;
        }

        public FluiCreator<TContext, TVisualElement> ForEach<TChildContext>(
            string name,
            Func<TContext, IEnumerable<TChildContext>> itemsFunc,
            string groupClasses,
            string childClasses,
            Action<FluiCreator<TChildContext, VisualElement>> bindAction = null,
            Action<FluiCreator<TChildContext, VisualElement>> initiateAction = null,
            Action<FluiCreator<TChildContext, VisualElement>> updateAction = null)
        {
            RawCreate<TContext, VisualElement>(
                name,
                groupClasses,
                x => x,
                s =>
                {
                    // No bind action for the container (?)
                },
                s => { },
                s =>
                {
                    s.SynchronizeList(
                        childClasses,
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
            var children = itemsFunc(Context).ToList();
            HashSet<VisualElement> unvisited = new HashSet<VisualElement>(Element.Children());
            List<VisualElement> properSort = new();
            for (var index = 0; index < children.Count; index++)
            {
                var context = children[index];
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

                properSort.Add(flui.Element);
                unvisited.Remove(flui.Element);
            }

            RemoveUnused<TChildContext>(unvisited);
            SortChildren<TChildContext>(properSort);
        }

        private void RemoveUnused<TChildContext>(HashSet<VisualElement> unvisited)
        {
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

        private void SortChildren<TChildContext>(List<VisualElement> properSort)
        {
            for (int correctIndex = 0; correctIndex < properSort.Count; correctIndex++)
            {
                VisualElement elementToSort = properSort[correctIndex];
                int currentIndex = Element.IndexOf(elementToSort);
                if (currentIndex != correctIndex)
                {
                    FluiCreatorStats.FluisMoved++;
                    Element.RemoveAt(currentIndex);
                    Element.Insert(correctIndex, elementToSort);
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
            // var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            // var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            var expc = CachedExpressionHelper.GetCachedExpression(propertyFunc);
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
                            onClick: b => expc.Setter(b.Context, button.Value),
                            button.Classes,
                            button.Label,
                            buildAction: b => b
                                .OptionalClass(activeClass, ctx => Equals(expc.Getter(ctx), button.Value)));
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

        public FluiCreator<TContext, TVisualElement> SetScreenXy(
            Vector2 screenPosition,
            AlignX alignX,
            AlignY alignY)
        {
            var localPosition = RuntimePanelUtils.ScreenToPanel(Element.panel, new Vector2(screenPosition.x, Screen.height - screenPosition.y));
            switch (alignX)
            {
                case AlignX.Left:
                    break;
                case AlignX.Mid:
                    localPosition.x -= Element.resolvedStyle.width / 2;
                    break;
                case AlignX.Right:
                    localPosition.x -= Element.resolvedStyle.width;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignX), alignX, null);
            }

            switch (alignY)
            {
                case AlignY.Top:
                    break;
                case AlignY.Mid:
                    localPosition.y -= Element.resolvedStyle.height / 2;
                    break;
                case AlignY.Bottom:
                    localPosition.y -= Element.resolvedStyle.height;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(alignY), alignY, null);
            }

            Element.style.position = Position.Absolute;
            Element.style.left = localPosition.x;
            Element.style.top = localPosition.y;

            return this;
        }

        public FluiCreator<TContext, TVisualElement> SetPosition(Position position)
        {
            Element.style.position = position;
            return this;
        }

        public FluiCreator<TContext, TVisualElement> SetPickingMode(PickingMode pickingMode)
        {
            Element.pickingMode = pickingMode;
            return this;
        }

        public static string SanitizeUIToolkitName(string name)
        {
            // Remove characters that are not letters, digits, underscores, or hyphens
            var allowedChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789_-";
            var sanitized = new System.Text.StringBuilder();
            foreach (var c in name)
            {
                if (allowedChars.Contains(c))
                    sanitized.Append(c);
            }

            // Ensure the name does not start with a digit
            if (sanitized.Length > 0 && char.IsDigit(sanitized[0]))
            {
                sanitized[0] = '_'; // Replace leading digit with underscore
            }
            else if (sanitized.Length == 0)
            {
                return "defaultName"; // Return a default name if all characters were removed
            }

            return sanitized.ToString();
        }
    }
}