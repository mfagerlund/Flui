﻿using System;
using System.Collections;
using System.Reflection;
using System.Text;
using Flui.Binder;
using UnityEngine;
using UnityEngine.UIElements;

namespace Flui
{
    public static class FluiHelper
    {
        public static string AddSpacesToSentence(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);

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
        
        public static void AddClasses(VisualElement ve, string classes)
        {
            if (!string.IsNullOrWhiteSpace(classes))
            {
                string[] classArray = classes.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var @class in classArray)
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