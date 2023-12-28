using System;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using Flui;
using UnityEngine;
using UnityEngine.UIElements;
using Debug = UnityEngine.Debug;

// ReSharper disable InconsistentNaming

namespace FluiDemo
{
    public class CloneTest : MonoBehaviour
    {
        [SerializeField] private bool _forceUpdate;
        [SerializeField] private bool _runPerformanceTest;
        [SerializeField, Header("Clone Code")] private bool _generate;
        [SerializeField] private string _code;
        

        void OnValidate()
        {
            if (_runPerformanceTest)
            {
                _runPerformanceTest = false;
                RunPerformanceTest();
                return;
            }
            
            if (_generate)
            {
                GenerateCloneCode();
                _generate = false;
                return;
            }

            var uiDocument = GetComponent<UIDocument>();
            if (uiDocument.rootVisualElement == null)
            {
                Debug.Log("uiDocument.rootVisualElement is null");
                return;
            }
            uiDocument.rootVisualElement.Clear();
            var original = uiDocument.visualTreeAsset.CloneTree();
            uiDocument.rootVisualElement.Add(original);
            uiDocument.rootVisualElement.Add(FluiHelper.CloneVisualElementHierarchy(original));
        }

        private void RunPerformanceTest()
        {
            var uiDocument = GetComponent<UIDocument>();
            
            // Warmup
            for (int i = 0; i < 10; i++)
            {
                var original = uiDocument.visualTreeAsset.CloneTree();
            }
            var orig = uiDocument.visualTreeAsset.CloneTree();
            for (int i = 0; i < 10; i++)
            {
                FluiHelper.CloneVisualElementHierarchy(orig);
            }

            var sw = Stopwatch.StartNew();
            for (int i = 0; i < 1000; i++)
            {
                var original = uiDocument.visualTreeAsset.CloneTree();
            }
            Debug.Log($"Clone tree time:{sw.Elapsed.TotalMilliseconds}");

            sw.Restart();
            for (int i = 0; i < 1000; i++)
            {
                FluiHelper.CloneVisualElementHierarchy(orig);
            }
            Debug.Log($"Clone tree time:{sw.Elapsed.TotalMilliseconds}");
        }

        private void GenerateCloneCode()
        {
            // This code is left here because we don't know how IStyle and IResolvedStyle will change
            // in the future, so we may need to re-generate the code.
            var sb = new StringBuilder();
            Type sourceType = typeof(IResolvedStyle);
            Type targetType = typeof(IStyle);

            foreach (PropertyInfo sourceProperty in sourceType.GetProperties())
            {
                // if (!sourceProperty.Name.EndsWith("Width"))
                // {
                //     continue;
                // }

                bool isYoga = _useYoga.Contains("IResolvedStyle." + sourceProperty.Name);


                PropertyInfo targetProperty = targetType.GetProperty(sourceProperty.Name);
                if (targetProperty != null && targetProperty.CanWrite)
                {
                    // sb.AppendLine($"target.{sourceProperty.Name} = source.{sourceProperty.Name};");
                    // sb.AppendLine("if (clone.resolvedStyle.% != original.resolvedStyle.%)".Replace("%", sourceProperty.Name));
                    // sb.AppendLine("    clone.style.% = original.resolvedStyle.%;".Replace("%", sourceProperty.Name));
                    var nonYogaTemplate = @"
                        if (clone.resolvedStyle.PROPERTYNAME != original.resolvedStyle.PROPERTYNAME)
                            clone.style.PROPERTYNAME = original.resolvedStyle.PROPERTYNAME;";

                    var yogaTemplate =
                        @"          var PROPERTYNAME = ReflectionHelper.GetPrivatePropertyValue<object, TYPE>(computedStyle, ""PROPERTYNAME"");
            if (clone.resolvedStyle.PROPERTYNAME != PROPERTYNAME)
            {
                clone.style.PROPERTYNAME = PROPERTYNAME;
            }";

                    var template = isYoga ? yogaTemplate : nonYogaTemplate;

                    template = template
                        .Replace("PROPERTYNAME", targetProperty.Name)
                        .Replace("TYPE", targetProperty.PropertyType.Name)
                        .Replace("StyleFloat", "float")
                        .Replace("StyleLength", "float");

                    sb.AppendLine(template);
                }
            }

            _code = sb.ToString();
        }

        private string _useYoga =
            @"    
    float IResolvedStyle.borderBottomWidth => this.yogaNode.LayoutBorderBottom;
    float IResolvedStyle.borderLeftWidth => this.yogaNode.LayoutBorderLeft;
    float IResolvedStyle.borderRightWidth => this.yogaNode.LayoutBorderRight;
    float IResolvedStyle.borderTopWidth => this.yogaNode.LayoutBorderTop;
    float IResolvedStyle.bottom => this.yogaNode.LayoutBottom;
    StyleFloat IResolvedStyle.flexBasis => new StyleFloat(this.yogaNode.ComputedFlexBasis);
    float IResolvedStyle.height => this.yogaNode.LayoutHeight;
    float IResolvedStyle.left => this.yogaNode.LayoutX;
    float IResolvedStyle.marginBottom => this.yogaNode.LayoutMarginBottom;
    float IResolvedStyle.marginLeft => this.yogaNode.LayoutMarginLeft;
    float IResolvedStyle.marginRight => this.yogaNode.LayoutMarginRight;
    float IResolvedStyle.marginTop => this.yogaNode.LayoutMarginTop;
    float IResolvedStyle.paddingBottom => this.yogaNode.LayoutPaddingBottom;
    float IResolvedStyle.paddingLeft => this.yogaNode.LayoutPaddingLeft;
    float IResolvedStyle.paddingRight => this.yogaNode.LayoutPaddingRight;
    float IResolvedStyle.paddingTop => this.yogaNode.LayoutPaddingTop;
    float IResolvedStyle.right => this.yogaNode.LayoutRight;
    float IResolvedStyle.top => this.yogaNode.LayoutY;
    float IResolvedStyle.width => this.yogaNode.LayoutWidth;";
    }
}