using System;
using System.Collections;
using System.Reflection;
using Flui.Binder;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flui
{
    public static class FluiHelper
    {
        public static VisualElement CloneVisualElementHierarchy(VisualElement original)
        {
            var clone = (VisualElement)Activator.CreateInstance(original.GetType());

            clone.name = original.name;
            CopyStyleProperties(clone, original);
            original.GetClasses().ForEach(x => clone.AddToClassList(x));

            foreach (var child in original.Children())
            {
                clone.Add(CloneVisualElementHierarchy(child));
            }

            if (original is TextElement te)
            {
                var teClone = (TextElement)clone;
                teClone.text = te.text;
            }
            else if (IsBaseFieldType(original))
            {
                ReflectionHelper.SetPropertyValue(clone, "label",
                    ReflectionHelper.GetPropertyValue<VisualElement, string>(original, "label"));
            }

            return clone;
        }

        public static bool IsBaseFieldType(VisualElement obj)
        {
            if (obj == null) return false;

            var type = obj.GetType();
            while (type != null && type != typeof(object))
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(BaseField<>))
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }

        public static string GetBaseFieldLabel(VisualElement obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            Type objType = obj.GetType();
            while (objType != null && objType != typeof(object))
            {
                if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(BaseField<>))
                {
                    PropertyInfo labelProperty = objType.GetProperty("label");
                    if (labelProperty != null && labelProperty.PropertyType == typeof(string))
                    {
                        return labelProperty.GetValue(obj) as string;
                    }

                    throw new InvalidOperationException("The 'label' property was not found or is not a string.");
                }

                objType = objType.BaseType;
            }

            throw new ArgumentException("The object is not a BaseField<?>.", nameof(obj));
        }

        public static string SetBaseFieldLabel(VisualElement obj, string label)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            Type objType = obj.GetType();
            while (objType != null && objType != typeof(object))
            {
                if (objType.IsGenericType && objType.GetGenericTypeDefinition() == typeof(BaseField<>))
                {
                    PropertyInfo labelProperty = objType.GetProperty("label");
                    if (labelProperty != null && labelProperty.PropertyType == typeof(string))
                    {
                        return labelProperty.GetValue(obj) as string;
                    }

                    throw new InvalidOperationException("The 'label' property was not found or is not a string.");
                }

                objType = objType.BaseType;
            }

            throw new ArgumentException("The object is not a BaseField<?>.", nameof(obj));
        }

        private static void CopyProperties<T>(T target, T source) where T : class
        {
            if (source == null || target == null)
                throw new ArgumentNullException("Source or target object is null");

            Type type = typeof(T);

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.PropertyType.IsValueType)
                {
                    if (property.CanRead && property.CanWrite)
                    {
                        try
                        {
                            object value = property.GetValue(source);
                            property.SetValue(target, value);
                            Debug.Log($"Copied {property.Name}");
                        }
                        catch (Exception)
                        {
                            Debug.LogError($"Failed to copy {property.Name}");
                        }
                    }
                }
                else
                {
                    Debug.Log($"Skipped {property.Name}");
                }
            }
        }

        public static void ExecuteAfterClassTransition(
            VisualElement visualElement,
            string @class,
            string expectedPropertyName,
            Action action)
        {
            visualElement.AddToClassList(@class);
            visualElement.RegisterCallback<TransitionEndEvent>(Callback);

            void Callback(TransitionEndEvent e)
            {
                if (e.stylePropertyNames.Contains(expectedPropertyName))
                {
                    action();
                    visualElement.UnregisterCallback<TransitionEndEvent>(Callback);
                }
            }
        }

        public static void RunDelayed(this MonoBehaviour monoBehaviour, int frameCount, Action action)
        {
            monoBehaviour.StartCoroutine(Coroutine());

            IEnumerator Coroutine()
            {
                for (int i = 0; i <= frameCount; i++)
                {
                    yield return 0;
                }

                action();
            }
        }

        private static void CopyStyleProperties(VisualElement clone, VisualElement original)
        {
            if (clone.resolvedStyle.alignContent != original.resolvedStyle.alignContent)
                clone.style.alignContent = original.resolvedStyle.alignContent;

            if (clone.resolvedStyle.alignItems != original.resolvedStyle.alignItems)
                clone.style.alignItems = original.resolvedStyle.alignItems;

            if (clone.resolvedStyle.alignSelf != original.resolvedStyle.alignSelf)
                clone.style.alignSelf = original.resolvedStyle.alignSelf;

            if (clone.resolvedStyle.backgroundColor != original.resolvedStyle.backgroundColor)
                clone.style.backgroundColor = original.resolvedStyle.backgroundColor;

            if (clone.resolvedStyle.backgroundImage != original.resolvedStyle.backgroundImage)
                clone.style.backgroundImage = original.resolvedStyle.backgroundImage;

            if (clone.resolvedStyle.backgroundPositionX != original.resolvedStyle.backgroundPositionX)
                clone.style.backgroundPositionX = original.resolvedStyle.backgroundPositionX;

            if (clone.resolvedStyle.backgroundPositionY != original.resolvedStyle.backgroundPositionY)
                clone.style.backgroundPositionY = original.resolvedStyle.backgroundPositionY;

            if (clone.resolvedStyle.backgroundRepeat != original.resolvedStyle.backgroundRepeat)
                clone.style.backgroundRepeat = original.resolvedStyle.backgroundRepeat;

            if (clone.resolvedStyle.backgroundSize != original.resolvedStyle.backgroundSize)
                clone.style.backgroundSize = original.resolvedStyle.backgroundSize;

            if (clone.resolvedStyle.borderBottomColor != original.resolvedStyle.borderBottomColor)
                clone.style.borderBottomColor = original.resolvedStyle.borderBottomColor;

            if (clone.resolvedStyle.borderBottomLeftRadius != original.resolvedStyle.borderBottomLeftRadius)
                clone.style.borderBottomLeftRadius = original.resolvedStyle.borderBottomLeftRadius;

            if (clone.resolvedStyle.borderBottomRightRadius != original.resolvedStyle.borderBottomRightRadius)
                clone.style.borderBottomRightRadius = original.resolvedStyle.borderBottomRightRadius;


            if (clone.resolvedStyle.borderTopColor != original.resolvedStyle.borderTopColor)
                clone.style.borderTopColor = original.resolvedStyle.borderTopColor;

            if (clone.resolvedStyle.borderTopLeftRadius != original.resolvedStyle.borderTopLeftRadius)
                clone.style.borderTopLeftRadius = original.resolvedStyle.borderTopLeftRadius;

            if (clone.resolvedStyle.borderTopRightRadius != original.resolvedStyle.borderTopRightRadius)
                clone.style.borderTopRightRadius = original.resolvedStyle.borderTopRightRadius;


            if (clone.resolvedStyle.color != original.resolvedStyle.color)
                clone.style.color = original.resolvedStyle.color;

            if (clone.resolvedStyle.display != original.resolvedStyle.display)
                clone.style.display = original.resolvedStyle.display;

            // var flexBasis = ReflectionHelper.GetPrivatePropertyValue<object, float>(computedStyle, "flexBasis");
            // if (clone.resolvedStyle.flexBasis != flexBasis)
            // {
            //     clone.style.flexBasis = flexBasis;
            // }

            if (clone.resolvedStyle.flexDirection != original.resolvedStyle.flexDirection)
                clone.style.flexDirection = original.resolvedStyle.flexDirection;

            if (clone.resolvedStyle.flexGrow != original.resolvedStyle.flexGrow)
                clone.style.flexGrow = original.resolvedStyle.flexGrow;

            if (clone.resolvedStyle.flexShrink != original.resolvedStyle.flexShrink)
                clone.style.flexShrink = original.resolvedStyle.flexShrink;

            if (clone.resolvedStyle.flexWrap != original.resolvedStyle.flexWrap)
                clone.style.flexWrap = original.resolvedStyle.flexWrap;

            if (clone.resolvedStyle.fontSize != original.resolvedStyle.fontSize)
                clone.style.fontSize = original.resolvedStyle.fontSize;


            if (clone.resolvedStyle.maxHeight != original.resolvedStyle.maxHeight)
                clone.style.maxHeight = original.resolvedStyle.maxHeight.value;

            if (clone.resolvedStyle.maxWidth != original.resolvedStyle.maxWidth)
                clone.style.maxWidth = original.resolvedStyle.maxWidth.value;

            if (clone.resolvedStyle.minHeight != original.resolvedStyle.minHeight)
                clone.style.minHeight = original.resolvedStyle.minHeight.value;

            if (clone.resolvedStyle.minWidth != original.resolvedStyle.minWidth)
                clone.style.minWidth = original.resolvedStyle.minWidth.value;

            if (clone.resolvedStyle.opacity != original.resolvedStyle.opacity)
                clone.style.opacity = original.resolvedStyle.opacity;

            if (clone.resolvedStyle.rotate != original.resolvedStyle.rotate)
                clone.style.rotate = original.resolvedStyle.rotate;

            if (clone.resolvedStyle.scale != original.resolvedStyle.scale)
                clone.style.scale = original.resolvedStyle.scale;

            if (clone.resolvedStyle.textOverflow != original.resolvedStyle.textOverflow)
                clone.style.textOverflow = original.resolvedStyle.textOverflow;


            // if (clone.resolvedStyle.transformOrigin != original.resolvedStyle.transformOrigin)
            //     clone.style.transformOrigin = original.resolvedStyle.transformOrigin;
            //
            // if (clone.resolvedStyle.transitionDelay != original.resolvedStyle.transitionDelay)
            //     clone.style.transitionDelay = original.resolvedStyle.transitionDelay;
            //
            // if (clone.resolvedStyle.transitionDuration != original.resolvedStyle.transitionDuration)
            //     clone.style.transitionDuration = original.resolvedStyle.transitionDuration;
            //
            // if (clone.resolvedStyle.transitionProperty != original.resolvedStyle.transitionProperty)
            //     clone.style.transitionProperty = original.resolvedStyle.transitionProperty;
            //
            // if (clone.resolvedStyle.transitionTimingFunction != original.resolvedStyle.transitionTimingFunction)
            //     clone.style.transitionTimingFunction = original.resolvedStyle.transitionTimingFunction;
            //
            // if (clone.resolvedStyle.translate != original.resolvedStyle.translate)
            //     clone.style.translate = original.resolvedStyle.translate;

            if (clone.resolvedStyle.unityBackgroundImageTintColor != original.resolvedStyle.unityBackgroundImageTintColor)
                clone.style.unityBackgroundImageTintColor = original.resolvedStyle.unityBackgroundImageTintColor;

            if (clone.resolvedStyle.unityFont != original.resolvedStyle.unityFont)
                clone.style.unityFont = original.resolvedStyle.unityFont;

            if (clone.resolvedStyle.unityFontDefinition != original.resolvedStyle.unityFontDefinition)
                clone.style.unityFontDefinition = original.resolvedStyle.unityFontDefinition;

            if (clone.resolvedStyle.unityFontStyleAndWeight != original.resolvedStyle.unityFontStyleAndWeight)
                clone.style.unityFontStyleAndWeight = original.resolvedStyle.unityFontStyleAndWeight;

            if (clone.resolvedStyle.unityParagraphSpacing != original.resolvedStyle.unityParagraphSpacing)
                clone.style.unityParagraphSpacing = original.resolvedStyle.unityParagraphSpacing;

            if (clone.resolvedStyle.unitySliceBottom != original.resolvedStyle.unitySliceBottom)
                clone.style.unitySliceBottom = original.resolvedStyle.unitySliceBottom;

            if (clone.resolvedStyle.unitySliceLeft != original.resolvedStyle.unitySliceLeft)
                clone.style.unitySliceLeft = original.resolvedStyle.unitySliceLeft;

            if (clone.resolvedStyle.unitySliceRight != original.resolvedStyle.unitySliceRight)
                clone.style.unitySliceRight = original.resolvedStyle.unitySliceRight;

            if (clone.resolvedStyle.unitySliceScale != original.resolvedStyle.unitySliceScale)
                clone.style.unitySliceScale = original.resolvedStyle.unitySliceScale;

            if (clone.resolvedStyle.unitySliceTop != original.resolvedStyle.unitySliceTop)
                clone.style.unitySliceTop = original.resolvedStyle.unitySliceTop;

            if (clone.resolvedStyle.unityTextAlign != original.resolvedStyle.unityTextAlign)
                clone.style.unityTextAlign = original.resolvedStyle.unityTextAlign;

            if (clone.resolvedStyle.unityTextOutlineColor != original.resolvedStyle.unityTextOutlineColor)
                clone.style.unityTextOutlineColor = original.resolvedStyle.unityTextOutlineColor;

            // if (clone.resolvedStyle.unityTextOutlineWidth != original.resolvedStyle.unityTextOutlineWidth)
            //     clone.style.unityTextOutlineWidth = original.resolvedStyle.unityTextOutlineWidth;

            if (clone.resolvedStyle.unityTextOverflowPosition != original.resolvedStyle.unityTextOverflowPosition)
                clone.style.unityTextOverflowPosition = original.resolvedStyle.unityTextOverflowPosition;

            if (clone.resolvedStyle.visibility != original.resolvedStyle.visibility)
                clone.style.visibility = original.resolvedStyle.visibility;

            if (clone.resolvedStyle.whiteSpace != original.resolvedStyle.whiteSpace)
                clone.style.whiteSpace = original.resolvedStyle.whiteSpace;

            if (clone.resolvedStyle.justifyContent != original.resolvedStyle.justifyContent)
                clone.style.justifyContent = original.resolvedStyle.justifyContent;
            
            if (clone.resolvedStyle.letterSpacing != original.resolvedStyle.letterSpacing)
                clone.style.letterSpacing = original.resolvedStyle.letterSpacing;
            
            var computedStyle =
                ReflectionHelper
                    .GetPropertyValue<VisualElement, object>(original, "m_Style");

            var type = computedStyle.GetType();

            var borderBottomWidth = ReflectionHelper.GetPropertyValue<float>(computedStyle, type, "borderBottomWidth");
            if (Math.Abs(clone.resolvedStyle.borderBottomWidth - borderBottomWidth) > 0.01f)
            {
                clone.style.borderBottomWidth = borderBottomWidth;
            }

            if (clone.resolvedStyle.borderLeftColor != original.resolvedStyle.borderLeftColor)
                clone.style.borderLeftColor = original.resolvedStyle.borderLeftColor;
            var borderLeftWidth = ReflectionHelper.GetPropertyValue<float>(computedStyle, type, "borderLeftWidth");
            if (Math.Abs(clone.resolvedStyle.borderLeftWidth - borderLeftWidth) > 0.01f)
            {
                clone.style.borderLeftWidth = borderLeftWidth;
            }

            if (clone.resolvedStyle.borderRightColor != original.resolvedStyle.borderRightColor)
                clone.style.borderRightColor = original.resolvedStyle.borderRightColor;
            var borderRightWidth = ReflectionHelper.GetPropertyValue<float>(computedStyle, type, "borderRightWidth");
            if (Math.Abs(clone.resolvedStyle.borderRightWidth - borderRightWidth) > 0.01f)
            {
                clone.style.borderRightWidth = borderRightWidth;
            }

            var borderTopWidth = ReflectionHelper.GetPropertyValue<float>(computedStyle, type, "borderTopWidth");
            if (Math.Abs(clone.resolvedStyle.borderTopWidth - borderTopWidth) > 0.01f)
            {
                clone.style.borderTopWidth = borderTopWidth;
            }

            var bottom = ReflectionHelper.GetPropertyValue<Length>(computedStyle, type, "bottom");
            if (clone.resolvedStyle.bottom != bottom)
            {
                clone.style.bottom = bottom;
            }

            var height = ReflectionHelper.GetPropertyValue<Length>(computedStyle, type, "height");
            if (Math.Abs(clone.resolvedStyle.height - height.value) > 0.01f && height.value > 0)
            {
                clone.style.height = height;
            }

            var width = ReflectionHelper.GetPropertyValue<Length>(computedStyle, type, "width");
            if (Math.Abs(clone.resolvedStyle.width - width.value) > 0.01f && width.value > 0)
            {
                clone.style.width = width;
            }
            
            var left = ReflectionHelper.GetPropertyValue<Length>(computedStyle, type, "left");
            if (clone.resolvedStyle.left != left)
            {
                clone.style.left = left;
            }

            var marginBottom = ReflectionHelper.GetPropertyValue<Length>(computedStyle, type, "marginBottom");
            if (clone.resolvedStyle.marginBottom != marginBottom)
            {
                clone.style.marginBottom = marginBottom;
            }

            var marginLeft = ReflectionHelper.GetPropertyValue<Length>(computedStyle, type, "marginLeft");
            if (clone.resolvedStyle.marginLeft != marginLeft)
            {
                clone.style.marginLeft = marginLeft;
            }

            var marginRight = ReflectionHelper.GetPropertyValue<Length>(computedStyle, type, "marginRight");
            if (clone.resolvedStyle.marginRight != marginRight)
            {
                clone.style.marginRight = marginRight;
            }

            var marginTop = ReflectionHelper.GetPropertyValue<Length>(computedStyle, type, "marginTop");
            if (clone.resolvedStyle.marginTop != marginTop)
            {
                clone.style.marginTop = marginTop;
            }

            var paddingBottom = ReflectionHelper.GetPropertyValue<Length>(computedStyle, type, "paddingBottom");
            if (clone.resolvedStyle.paddingBottom != paddingBottom)
            {
                clone.style.paddingBottom = paddingBottom;
            }

            var paddingLeft = ReflectionHelper.GetPropertyValue<Length>(computedStyle, type, "paddingLeft");
            if (clone.resolvedStyle.paddingLeft != paddingLeft)
            {
                clone.style.paddingLeft = paddingLeft;
            }

            var paddingRight = ReflectionHelper.GetPropertyValue<Length>(computedStyle, type, "paddingRight");
            if (clone.resolvedStyle.paddingRight != paddingRight)
            {
                clone.style.paddingRight = paddingRight;
            }

            var paddingTop = ReflectionHelper.GetPropertyValue<Length>(computedStyle, type, "paddingTop");
            if (clone.resolvedStyle.paddingTop != paddingTop)
            {
                clone.style.paddingTop = paddingTop;
            }

            if (clone.resolvedStyle.position != original.resolvedStyle.position)
                clone.style.position = original.resolvedStyle.position;
            var right = ReflectionHelper.GetPropertyValue<Length>(computedStyle, type, "right");
            if (clone.resolvedStyle.right != right)
            {
                clone.style.right = right;
            }

            var top = ReflectionHelper.GetPropertyValue<Length>(computedStyle, type, "top");
            if (clone.resolvedStyle.top != top)
            {
                clone.style.top = top;
            }

            // if (clone.resolvedStyle.wordSpacing != original.resolvedStyle.wordSpacing)
            //     clone.style.wordSpacing = original.resolvedStyle.wordSpacing;
            //
            // if (clone.resolvedStyle.unityBackgroundScaleMode != original.resolvedStyle.unityBackgroundScaleMode)
            //     clone.style.unityBackgroundScaleMode = original.resolvedStyle.unityBackgroundScaleMode;
        }
    }
}