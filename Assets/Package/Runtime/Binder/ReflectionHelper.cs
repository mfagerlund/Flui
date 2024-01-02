using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Flui.Binder
{
    internal static class ReflectionHelper
    {
        private static readonly Dictionary<string, Func<object, object>> Getters = new Dictionary<string, Func<object, object>>();

        internal static TValue GetPropertyValue<TValue>(object instance, Type type, string propertyName)
        {
            var key = $"{type.Name}.{propertyName}";
            if (!Getters.TryGetValue(key, out var getter))
            {
                var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

                PropertyInfo propertyInfo = type.GetProperty(propertyName, bindingFlags);
                if (propertyInfo != null)
                {
                    getter = (obj) => propertyInfo.GetValue(obj);
                }
                else
                {
                    FieldInfo fieldInfo = type.GetField(propertyName, bindingFlags);
                    if (fieldInfo != null)
                    {
                        getter = (obj) => fieldInfo.GetValue(obj);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(propertyName), $"No property or field found named {propertyName} for {type}.");
                    }
                }

                Getters[key] = getter;
            }

            try
            {
                return (TValue)getter(instance);
            }
            catch
            {
                throw new InvalidOperationException($"Unable to retrieve value for {propertyName} as {typeof(TValue).Name}");
            }
        }
        
        internal static TValue GetPropertyValue<TObject, TValue>(TObject instance, string propertyName)
        {
            var type = typeof(TObject);
            
            if (typeof(TObject) == typeof(object))
            {
                type = instance.GetType();
            }

            var key = $"{type.Name}.{propertyName}";
            if (!Getters.TryGetValue(key, out var getter))
            {
                var bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
                PropertyInfo propertyInfo = type.GetProperty(propertyName, bindingFlags);
                if (propertyInfo != null)
                {
                    getter = (obj) => propertyInfo.GetValue(obj);
                }
                else
                {
                    FieldInfo fieldInfo = type.GetField(propertyName, bindingFlags);
                    if (fieldInfo != null)
                    {
                        getter = (obj) => fieldInfo.GetValue(obj);
                    }
                    else
                    {
                        throw new ArgumentOutOfRangeException(nameof(propertyName), $"No property or field found named {propertyName} for {type}.");
                    }
                }

                Getters[key] = getter;
            }

            try
            {
                return (TValue)getter(instance);
            }
            catch
            {
                throw new InvalidOperationException($"Unable to retrieve value for {propertyName} as {typeof(TValue).Name}");
            }
        }

        private static readonly Dictionary<string, Action<object, object>> Setters = new Dictionary<string, Action<object, object>>();

        internal static void SetPropertyValue<TValue>(object instance, string propertyName, TValue newValue)
        {
            var type = instance.GetType();
            var key = $"{type.FullName}.{propertyName}";

            if (!Setters.TryGetValue(key, out var setter))
            {
                PropertyInfo propertyInfo = instance.GetType().GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
                setter = (obj, val) => propertyInfo.SetValue(obj, val);
                Setters[key] = setter;
            }

            setter(instance, newValue);
        }

        internal static Func<T, TValue> GetPropertyValueFunc<T, TValue>(Expression<Func<T, TValue>> memberLamda)
        {
            if (memberLamda.Body is MemberExpression memberSelectorExpression)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null)
                {
                    return x => (TValue)property.GetValue(x);
                }

                var field = memberSelectorExpression.Member as FieldInfo;
                if (field != null)
                {
                    return x => (TValue)field.GetValue(x);
                }
            }

            throw new InvalidOperationException($"Unable to set value to {memberLamda}");
        }

        internal static Action<T, TValue> SetPropertyValueFunc<T, TValue>(Expression<Func<T, TValue>> memberLamda)
        {
            if (memberLamda.Body is MemberExpression memberSelectorExpression)
            {
                var property = memberSelectorExpression.Member as PropertyInfo;
                if (property != null)
                {
                    return (t, v) => property.SetValue(t, v, null);
                }

                var field = memberSelectorExpression.Member as FieldInfo;
                if (field != null)
                {
                    return (t, v) => field.SetValue(t, v);
                }
            }

            throw new InvalidOperationException($"Unable to set value to {memberLamda}");
        }

        public static string GetPath<T, U>(Expression<Func<T, U>> expr)
        {
            var txt = expr.Body.ToString();
            var resultString = Regex.Match(txt, @"Convert\(value\(.*?\).(.*?), Object\)").Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(resultString))
            {
                return resultString;
            }

            txt = txt.Substring(txt.IndexOf(".") + 1);
            if (txt.EndsWith(", Object)"))
            {
                txt = txt.Substring(0, txt.Length - ", Object)".Length);
            }

            return txt;
        }
    }
}