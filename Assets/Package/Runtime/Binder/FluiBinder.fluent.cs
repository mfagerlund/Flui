// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Flui.Creator;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flui.Binder
{
    public partial class FluiBinder<TContext, TVisualElement> : IFluiBinder where TVisualElement : VisualElement
    {
        public FluiBinder<TContext, TVisualElement> EnumSwitch<TEnum>(
            string query,
            Func<TContext, TEnum> enumGetter,
            Action<Switcher<TEnum>> bindAction)
        {
            var switcher = new Switcher<TEnum>(this, enumGetter(Context));
            bindAction(switcher);
            return this;
        }

        public class Switcher<TEnum>
        {
            private readonly FluiBinder<TContext, TVisualElement> _fluiBinder;
            private readonly TEnum _currentValue;

            public Switcher(FluiBinder<TContext, TVisualElement> fluiBinder, TEnum currentValue)
            {
                _fluiBinder = fluiBinder;
                _currentValue = currentValue;
            }

            public Switcher<TEnum> Case<TChildContext>(TEnum value,
                Func<TContext, TChildContext> contextFunc,
                Action<FluiBinder<TChildContext, VisualElement>> bindAction = null,
                Action<FluiBinder<TChildContext, VisualElement>> initiateAction = null,
                Action<FluiBinder<TChildContext, VisualElement>> updateAction = null)
            {
                return Case(value.ToString(), value, contextFunc, bindAction, initiateAction, updateAction);
            }

            public Switcher<TEnum> Case<TChildContext>(
                string query,
                TEnum value,
                Func<TContext, TChildContext> contextFunc,
                Action<FluiBinder<TChildContext, VisualElement>> bindAction = null,
                Action<FluiBinder<TChildContext, VisualElement>> initiateAction = null,
                Action<FluiBinder<TChildContext, VisualElement>> updateAction = null)
            {
                var @case =
                    _fluiBinder
                        .RawBind<TChildContext, VisualElement>(
                            query,
                            contextFunc,
                            bindAction,
                            initiateAction,
                            updateAction);

                @case.SetHiddenFunc(ctx => !Equals(_currentValue, value));

                return this;
            }
        }

        // public FluiBinder<TContext, TVisualElement> ListView<TChildContext>(
        //     string query,
        //     Func<TContext, IList<TChildContext>> contextFunc,
        //     Func<VisualElement> visualElementFunc,
        //     Action<FluiBinder<TChildContext, VisualElement>> bindAction = null,
        //     Action<FluiBinder<TChildContext, VisualElement>> initiateAction = null,
        //     Action<FluiBinder<TChildContext, VisualElement>> updateAction = null)
        // {
        //     RawBind<TContext, ListView>(
        //         query,
        //         x => x,
        //         null,
        //         initiateAction: s =>
        //         {
        //             s.Element.makeItem = visualElementFunc;
        //             s.Element.itemsSource = (IList)contextFunc(s.Context);
        //         },
        //         null
        //     );
        //
        //     return this;
        // }

        public FluiBinder<TContext, TVisualElement> Group<TChildContext>(
            string query,
            Func<TContext, TChildContext> contextFunc,
            Action<FluiBinder<TChildContext, VisualElement>> bindAction = null,
            Action<FluiBinder<TChildContext, VisualElement>> initiateAction = null,
            Action<FluiBinder<TChildContext, VisualElement>> updateAction = null)
        {
            RawBind<TChildContext, VisualElement>(
                query,
                contextFunc,
                bindAction,
                initiateAction,
                updateAction);

            return this;
        }

        public FluiBinder<TContext, TVisualElement> Foldout<TChildContext>(
            string query,
            string text,
            Func<TContext, TChildContext> contextFunc,
            bool initialOpen,
            Action<FluiBinder<TChildContext, Foldout>> bindAction = null,
            Action<FluiBinder<TChildContext, Foldout>> initiateAction = null,
            Action<FluiBinder<TChildContext, Foldout>> updateAction = null)
        {
            RawBind<TChildContext, Foldout>(
                query,
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

        public FluiBinder<TContext, TVisualElement> Label(
            string query,
            Func<TContext, string> getLabel,
            Action<FluiBinder<TContext, Label>> bindAction = null,
            Action<FluiBinder<TContext, Label>> initiateAction = null,
            Action<FluiBinder<TContext, Label>> updateAction = null)
        {
            RawBind<TContext, Label>(
                query,
                x => x,
                bindAction,
                initiateAction,
                s =>
                {
                    s.Element.text = getLabel(Context);
                    updateAction?.Invoke(s);
                });

            return this;
        }

        public FluiBinder<TContext, TVisualElement> Button(
            string query,
            Action<FluiBinder<TContext, Button>> clicked,
            Action<FluiBinder<TContext, Button>> bindAction = null,
            Action<FluiBinder<TContext, Button>> initiateAction = null,
            Action<FluiBinder<TContext, Button>> updateAction = null)
        {
            RawBind<TContext, Button>(
                query,
                x => x,
                bindAction,
                s =>
                {
                    s.Element.clicked += () => clicked(s);
                    initiateAction?.Invoke(s);
                },
                updateAction);

            return this;
        }

        public FluiBinder<TContext, TVisualElement> Toggle(
            string query,
            Func<TContext, bool> getValue,
            Action<TContext, bool> setValue,
            Action<FluiBinder<TContext, Toggle>> bindAction = null,
            Action<FluiBinder<TContext, Toggle>> initiateAction = null,
            Action<FluiBinder<TContext, Toggle>> updateAction = null)
        {
            RawBind<TContext, Toggle>(
                query,
                x => x,
                bindAction,
                s =>
                {
                    s.Element.value = getValue(Context);
                    s._valueBinding = new ValueBinding<bool>(
                        () => getValue(Context), v => setValue(Context, v),
                        () => s.Element.value, v => s.Element.value = v);
                    initiateAction?.Invoke(s);
                },
                updateAction);

            return this;
        }

        public FluiBinder<TContext, TVisualElement> Toggle(
            string query,
            Expression<Func<TContext, bool>> propertyFunc,
            Action<FluiBinder<TContext, Toggle>> bindAction = null,
            Action<FluiBinder<TContext, Toggle>> initiateAction = null,
            Action<FluiBinder<TContext, Toggle>> updateAction = null)
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            return Toggle(query, getFunc, setFunc, bindAction, initiateAction, updateAction);
        }

        public FluiBinder<TContext, TVisualElement> EnumButtons<TEnum>(
            string query,
            Expression<Func<TContext, TEnum>> propertyFunc,
            Action<EnumButtonBinder<TEnum>> buttonsAction,
            string activeClass = "active",
            Action<FluiBinder<TContext, VisualElement>> bindAction = null,
            Action<FluiBinder<TContext, VisualElement>> initiateAction = null,
            Action<FluiBinder<TContext, VisualElement>> updateAction = null) where TEnum : Enum
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            var bb = new EnumButtonBinder<TEnum>();
            buttonsAction(bb);
            var buttons = bb.Buttons;
            RawBind<TContext, VisualElement>(
                query,
                x => x,
                f =>
                {
                    foreach (var button in buttons)
                    {
                        f.Button(
                            button.Query,
                            f => setFunc(f.Context, button.Value),
                            bindAction: b => b
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

            public EnumButtonBinder<TEnum> EnumButton(TEnum value)
            {
                Buttons.Add(new EnumButton<TEnum>(value));
                return this;
            }

            public EnumButtonBinder<TEnum> EnumButton(string query, TEnum value)
            {
                Buttons.Add(new EnumButton<TEnum>(query, value));
                return this;
            }
        }

        public class EnumButton<TEnum> where TEnum : Enum
        {
            public string Query { get; }
            public TEnum Value { get; }

            public EnumButton(string query, TEnum value)
            {
                Query = query;
                Value = value;
            }

            public EnumButton(TEnum value) : this(value.ToString(), value)
            {
            }
        }

        public FluiBinder<TContext, TVisualElement> FloatField(
            string query,
            Func<TContext, float> getValue,
            Action<TContext, float> setValue,
            Action<FluiBinder<TContext, FloatField>> bindAction = null,
            Action<FluiBinder<TContext, FloatField>> initiateAction = null,
            Action<FluiBinder<TContext, FloatField>> updateAction = null)
        {
            RawBind<TContext, FloatField>(
                query,
                x => x,
                bindAction,
                s =>
                {
                    s.Element.value = getValue(Context);
                    var valueBinding = new ValueBinding<float>(
                        () => getValue(Context), v => setValue(Context, v),
                        () => s.Element.value, v => s.Element.value = v);
                    s._valueBinding = valueBinding;
                    initiateAction?.Invoke(s);
                },
                updateAction);

            return this;
        }

        public FluiBinder<TContext, TVisualElement> FloatField(
            string query,
            Expression<Func<TContext, float>> propertyFunc,
            Action<FluiBinder<TContext, FloatField>> bindAction = null,
            Action<FluiBinder<TContext, FloatField>> initiateAction = null,
            Action<FluiBinder<TContext, FloatField>> updateAction = null)
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            return FloatField(query, getFunc, setFunc, bindAction, initiateAction, updateAction);
        }

        public FluiBinder<TContext, TVisualElement> IntegerField(
            string query,
            Func<TContext, int> getValue,
            Action<TContext, int> setValue,
            Action<FluiBinder<TContext, IntegerField>> bindAction = null,
            Action<FluiBinder<TContext, IntegerField>> initiateAction = null,
            Action<FluiBinder<TContext, IntegerField>> updateAction = null)
        {
            RawBind<TContext, IntegerField>(
                query,
                x => x,
                bindAction,
                s =>
                {
                    s.Element.value = getValue(Context);
                    var valueBinding = new ValueBinding<int>(
                        () => getValue(Context), v => setValue(Context, v),
                        () => s.Element.value, v => s.Element.value = v);
                    s._valueBinding = valueBinding;
                    initiateAction?.Invoke(s);
                },
                updateAction);

            return this;
        }

        public FluiBinder<TContext, TVisualElement> IntegerField(
            string query,
            Expression<Func<TContext, int>> propertyFunc,
            Action<FluiBinder<TContext, IntegerField>> bindAction = null,
            Action<FluiBinder<TContext, IntegerField>> initiateAction = null,
            Action<FluiBinder<TContext, IntegerField>> updateAction = null)
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            return IntegerField(query, getFunc, setFunc, bindAction, initiateAction, updateAction);
        }

        public FluiBinder<TContext, TVisualElement> TextField(
            string query,
            Func<TContext, string> getValue,
            Action<TContext, string> setValue,
            bool updateOnExit = true,
            Action<FluiBinder<TContext, TextField>> bindAction = null,
            Action<FluiBinder<TContext, TextField>> initiateAction = null,
            Action<FluiBinder<TContext, TextField>> updateAction = null)
        {
            RawBind<TContext, TextField>(
                query,
                x => x,
                bindAction,
                s =>
                {
                    s.Element.value = getValue(Context);
                    var valueBinding = new ValueBinding<string>(
                        () => getValue(Context), v => setValue(Context, v),
                        () => s.Element.value, v => s.Element.value = v);

                    if (updateOnExit)
                    {
                        valueBinding.SetLockedFunc(() =>
                        {
                            if (!s.IsFocused)
                            {
                                return false;
                            }

                            if (Input.GetKeyDown(KeyCode.Return))
                            {
                                // We're focused and the user pressed enter.
                                return false;
                            }

                            return s.IsFocused;
                        });
                    }

                    s._valueBinding = valueBinding;
                    initiateAction?.Invoke(s);
                },
                updateAction);

            return this;
        }

        public FluiBinder<TContext, TVisualElement> TextField(
            string query,
            Expression<Func<TContext, string>> propertyFunc,
            bool updateOnExit = true,
            Action<FluiBinder<TContext, TextField>> bindAction = null,
            Action<FluiBinder<TContext, TextField>> initiateAction = null,
            Action<FluiBinder<TContext, TextField>> updateAction = null)
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            return TextField(query, getFunc, setFunc, updateOnExit, bindAction, initiateAction, updateAction);
        }

        public FluiBinder<TContext, TVisualElement> EnumField<TEnum>(
            string query,
            Func<TContext, TEnum> getValue,
            Action<TContext, TEnum> setValue,
            Action<FluiBinder<TContext, EnumField>> bindAction = null,
            Action<FluiBinder<TContext, EnumField>> initiateAction = null,
            Action<FluiBinder<TContext, EnumField>> updateAction = null) where TEnum : Enum
        {
            RawBind<TContext, EnumField>(
                query,
                x => x,
                bindAction,
                s =>
                {
                    s.Element.Init(getValue(Context));
                    var valueBinding = new ValueBinding<TEnum>(
                        () => getValue(Context), v => setValue(Context, v),
                        () => (TEnum)s.Element.value, v => s.Element.value = v);
                    s._valueBinding = valueBinding;
                    initiateAction?.Invoke(s);
                },
                updateAction);

            return this;
        }

        public FluiBinder<TContext, TVisualElement> EnumField<TEnum>(
            string query,
            Expression<Func<TContext, TEnum>> propertyFunc,
            Action<FluiBinder<TContext, EnumField>> bindAction = null,
            Action<FluiBinder<TContext, EnumField>> initiateAction = null,
            Action<FluiBinder<TContext, EnumField>> updateAction = null) where TEnum : Enum
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            return EnumField(query, getFunc, setFunc, bindAction, initiateAction, updateAction);
        }

        public FluiBinder<TContext, TVisualElement> DropdownField(
            string query,
            Func<TContext, int> getValue,
            Action<TContext, int> setValue,
            Action<FluiBinder<TContext, DropdownField>> bindAction = null,
            Action<FluiBinder<TContext, DropdownField>> initiateAction = null,
            Action<FluiBinder<TContext, DropdownField>> updateAction = null)
        {
            RawBind<TContext, DropdownField>(
                query,
                x => x,
                bindAction,
                s =>
                {
                    s.Element.index = getValue(Context);
                    var valueBinding = new ValueBinding<int>(
                        () => getValue(Context), v => setValue(Context, v),
                        () => s.Element.index, v => s.Element.index = v);
                    s._valueBinding = valueBinding;
                    initiateAction?.Invoke(s);
                },
                updateAction);

            return this;
        }

        public FluiBinder<TContext, TVisualElement> DropdownField(
            string query,
            Expression<Func<TContext, int>> propertyFunc,
            Action<FluiBinder<TContext, DropdownField>> bindAction = null,
            Action<FluiBinder<TContext, DropdownField>> initiateAction = null,
            Action<FluiBinder<TContext, DropdownField>> updateAction = null)
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            return DropdownField(query, getFunc, setFunc, bindAction, initiateAction, updateAction);
        }

        public FluiBinder<TContext, TVisualElement> Slider(
            string query,
            Func<TContext, float> getValue,
            Action<TContext, float> setValue,
            float lowValue,
            float highValue,
            Action<FluiBinder<TContext, Slider>> bindAction = null,
            Action<FluiBinder<TContext, Slider>> initiateAction = null,
            Action<FluiBinder<TContext, Slider>> updateAction = null)
        {
            RawBind<TContext, Slider>(
                query,
                x => x,
                bindAction,
                s =>
                {
                    s.Element.lowValue = lowValue;
                    s.Element.highValue = highValue;
                    s.Element.value = getValue(Context);

                    s._valueBinding = new ValueBinding<float>(
                        () => getValue(Context), v => setValue(Context, v),
                        () => s.Element.value, v => s.Element.value = v);
                    ;
                    initiateAction?.Invoke(s);
                },
                updateAction);

            return this;
        }

        public FluiBinder<TContext, TVisualElement> Slider(
            string query,
            Expression<Func<TContext, float>> propertyFunc,
            float lowValue,
            float highValue,
            Action<FluiBinder<TContext, Slider>> bindAction = null,
            Action<FluiBinder<TContext, Slider>> initiateAction = null,
            Action<FluiBinder<TContext, Slider>> updateAction = null)
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            return Slider(query, getFunc, setFunc, lowValue, highValue, bindAction, initiateAction, updateAction);
        }

        public FluiBinder<TContext, TVisualElement> ForEach<TChildContext>(
            string query,
            Func<TContext, IEnumerable<TChildContext>> itemsFunc,
            Func<VisualElement> visualElementFunc,
            Action<FluiBinder<TChildContext, VisualElement>> bindAction = null,
            Action<FluiBinder<TChildContext, VisualElement>> initiateAction = null,
            Action<FluiBinder<TChildContext, VisualElement>> updateAction = null)
        {
            RawBind<TContext, VisualElement>(
                query,
                x => x,
                s =>
                {
                    // No bind action for the container (?)
                },
                s => { },
                s =>
                {
                    s.SynchronizeList(
                        itemsFunc,
                        visualElementFunc,
                        bindAction,
                        initiateAction,
                        updateAction);
                });

            return this;
        }

        public FluiBinder<TContext, TVisualElement> Create<TChildContext>(
            string query,
            Func<TContext, TChildContext> contextFunc,
            Action<FluiCreator<TChildContext, VisualElement>> createAction = null)
        {
            RawBind<TChildContext, VisualElement>(
                query,
                contextFunc,
                flui =>
                {
                    flui.Data ??= new FluiCreatorRoot<TChildContext, VisualElement>();
                    var root = (FluiCreatorRoot<TChildContext, VisualElement>)flui.Data;
                    root.CreateGui(flui.Context, flui.Element, createAction);
                },
                null,
                null);

            return this;
        }
        
        public FluiBinder<TContext, TVisualElement> ForEachCreate<TChildContext>(
            string query,
            Func<TContext, IEnumerable<TChildContext>> itemsFunc,
            string classes,
            Action<FluiCreator<TChildContext, VisualElement>> createAction = null)
        {
            RawBind<TContext, VisualElement>(
                query,
                x => x,
                s => { },
                s => { },
                s =>
                {
                    s.SynchronizeList(
                        itemsFunc,
                        () =>
                        {
                            var ve = new VisualElement();
                            FluiHelper.AddClasses(ve, classes);
                            return ve;
                        },
                        flui =>
                        {
                            flui.Data ??= new FluiCreatorRoot<TChildContext, VisualElement>();
                            var root = (FluiCreatorRoot<TChildContext, VisualElement>)flui.Data;
                            root.CreateGui(flui.Context, flui.Element, createAction);
                        },
                        null,
                        null);
                });

            return this;
        }

        public FluiBinder<TContext, TVisualElement> ForEach<TChildContext>(
            string query,
            Func<TContext, IEnumerable<TChildContext>> itemsFunc,
            Action<FluiBinder<TChildContext, VisualElement>> bindAction = null,
            Action<FluiBinder<TChildContext, VisualElement>> initiateAction = null,
            Action<FluiBinder<TChildContext, VisualElement>> updateAction = null)
        {
            var template = _fluiBinderRoot.GetOrCreateTemplate(Element, query);
            RawBind<TContext, VisualElement>(
                query,
                x => x,
                s => { },
                s => { },
                s =>
                {
                    s.SynchronizeList(
                        itemsFunc,
                        () => template.templateSource.CloneTree(),
                        bindAction,
                        initiateAction,
                        updateAction);
                });

            return this;
        }

        private void SynchronizeList<TChildContext>(
            Func<TContext, IEnumerable<TChildContext>> itemsFunc,
            Func<VisualElement> visualElementFunc,
            Action<FluiBinder<TChildContext, VisualElement>> bindAction,
            Action<FluiBinder<TChildContext, VisualElement>> initiateAction,
            Action<FluiBinder<TChildContext, VisualElement>> updateAction)
        {
            var children = itemsFunc(Context);
            HashSet<VisualElement> unvisited = new HashSet<VisualElement>(Element.Children());
            foreach (var context in children)
            {
                // Could be slow - need two way dictionary
                var rawFlui = _childBinders.Values.SingleOrDefault(x => Equals(x.Context, context));
                var veName = "";
                if (rawFlui == null)
                {
                    var visualElement = visualElementFunc();
                    veName = visualElement.name = Guid.NewGuid().ToString().Replace("-", "");
                    Element.Add(visualElement);
                }
                else
                {
                    veName = rawFlui.Query;
                }

                var flui = RawBind<TChildContext, VisualElement>(
                    veName,
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


                var childBinder = _childBinders.SafeGetValue(visualElement);
                if (childBinder != null)
                {
                    FluiBinderStats.FluiBinderRemoved += childBinder.GetHierarchyChildCount();
                    _childBinders.Remove(visualElement);
                }
            }
        }
    }
}