// ReSharper disable InconsistentNaming

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flui.Binder
{
    public partial class FluiBinder<TContext, TVisualElement> : IFluiBinder where TVisualElement : VisualElement
    {
        private readonly Dictionary<VisualElement, IFluiBinder> _childBinders = new();
        private Action<FluiBinder<TContext, TVisualElement>> _updateAction;
        private Action<FluiBinder<TContext, TVisualElement>> _bindAction;
        private bool _visited;
        private IValueBinding _valueBinding;
        private Func<TContext, bool> _hiddenFunc;
        private Func<TContext, bool> _invisibleFunc;

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

        public FluiBinder<TContext, TVisualElement> SetHidden(Func<TContext, bool> hiddenFunc)
        {
            _hiddenFunc = hiddenFunc;
            return this;
        }

        public FluiBinder<TContext, TVisualElement> SetInvisible(Func<TContext, bool> invisibleFunc)
        {
            _invisibleFunc = invisibleFunc;
            return this;
        }

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
            Action<TContext> clicked,
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
                    s.Element.clicked += () => clicked(s.Context);
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
                            ctx => setFunc(ctx, button.Value),
                            bindAction: b => b
                                .OptionalClass(activeClass, ctx => Equals(getFunc(ctx), button.Value)
                                )
                        );
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

        public FluiBinder<TContext, TVisualElement> EnumDropdown<TEnum>(
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

        public FluiBinder<TContext, TVisualElement> EnumDropdown<TEnum>(
            string query,
            Expression<Func<TContext, TEnum>> propertyFunc,
            Action<FluiBinder<TContext, EnumField>> bindAction = null,
            Action<FluiBinder<TContext, EnumField>> initiateAction = null,
            Action<FluiBinder<TContext, EnumField>> updateAction = null) where TEnum : Enum
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            return EnumDropdown(query, getFunc, setFunc, bindAction, initiateAction, updateAction);
        }

        public FluiBinder<TContext, TVisualElement> Dropdown(
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

        public FluiBinder<TContext, TVisualElement> Dropdown(
            string query,
            Expression<Func<TContext, int>> propertyFunc,
            Action<FluiBinder<TContext, DropdownField>> bindAction = null,
            Action<FluiBinder<TContext, DropdownField>> initiateAction = null,
            Action<FluiBinder<TContext, DropdownField>> updateAction = null)
        {
            var getFunc = ReflectionHelper.GetPropertyValueFunc(propertyFunc);
            var setFunc = ReflectionHelper.SetPropertyValueFunc(propertyFunc);
            return Dropdown(query, getFunc, setFunc, bindAction, initiateAction, updateAction);
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