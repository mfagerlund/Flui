using System.Collections.Generic;
using Flui.Binder;
using Flui.Creator;
using UnityEngine;
using UnityEngine.UIElements;

namespace FluiDemo.Mixed
{
    public class Mixed : Fadable
    {
        private FluiBinderRoot<ChildObjectWithProperties, VisualElement> _root = new();
        private ChildObjectWithProperties _exampleObjectWithProperties = new();
        [SerializeField] private bool _triggerOnValidate = false;

        private void OnValidate()
        {
            if (Application.isPlaying && !_triggerOnValidate)
            {
                return;
            }

            if (_triggerOnValidate)
            {
                _triggerOnValidate = false;
            }

            Update();
        }

        void Update()
        {
            _root
                .BindGui(_exampleObjectWithProperties, RootVisualElement, r => r
                    .Button("Close", _ => Hide())
                    .Create("Properties", x => x, r.Context.CreateGui)
                    .Create("PropertiesAgain", x => x, r.Context.CreateGui)
                    .ForEachCreate("List", ctx => ctx.Items, "row", i => i.Context.CreateGui(i))
                );
        }

        public class ObjectWithGui
        {
            public string StringProperty { get; set; } = "StringProppo";
            public ObjectSizeEnum ObjectSize { get; set; } = ObjectSizeEnum.Large;

            public void CreateGui<TObjectWithGui>(FluiCreator<TObjectWithGui, VisualElement> flui) where TObjectWithGui : ObjectWithGui
            {
                flui
                    .TextField(x => x.StringProperty, "")
                    .EnumField(x => x.ObjectSize, "");
            }
        }

        public class ChildObjectWithProperties : ObjectWithGui
        {
            public float RangeValue { get; set; } = 4;
            public float FloatValue { get; set; } = 8;
            public bool ToggleValue { get; set; } = false;
            public int IntValue { get; set; } = 20;

            public List<ListItem> Items { get; } = new List<ListItem>
            {
                new ListItem("Alpha", "Alpacka"),
                new ListItem("Beta", "Betamax"),
                new ListItem("Zeta", "Jones"),
                new ListItem("Offside", "Onside"),
            };

            public new void CreateGui<TObjectWithGui>(FluiCreator<TObjectWithGui, VisualElement> flui) where TObjectWithGui : ChildObjectWithProperties
            {
                base.CreateGui(flui);

                flui
                    .Slider(x => x.RangeValue, "", 0, 15)
                    .FloatField(x => x.FloatValue, "")
                    .Toggle(x => x.ToggleValue, "")
                    .IntegerField(x => x.IntValue, "");
            }
        }

        public class ListItem
        {
            public ListItem(string name, string value)
            {
                Name = name;
                Value = value;
            }

            public string Name { get; set; }
            public string Value { get; set; }

            public void CreateGui(FluiCreator<ListItem, VisualElement> fluiCreator)
            {
                fluiCreator
                    .Foldout("foldout", "Foldy", x => x, false, "", f => f
                        .VisualElement("row", "row", r => r
                            .TextField(l => l.Name, "")
                            .TextField(l => l.Value, "")));
            }
        }

        public enum ObjectSizeEnum
        {
            Small,
            Medium,
            Large,
            ExtraLarge
        }
    }
}