using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Flui.Binder
{
    internal static class ReflectionHelper
    {
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
    }
}