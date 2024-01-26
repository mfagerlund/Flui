using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Flui.Binder
{
    internal static class ReflectionHelper
    {
        public static Func<T, TValue> GetPropertyValueFunc<T, TValue>(Expression<Func<T, TValue>> memberLamda)
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

        public static Action<T, TValue> SetPropertyValueFunc<T, TValue>(Expression<Func<T, TValue>> memberLamda)
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