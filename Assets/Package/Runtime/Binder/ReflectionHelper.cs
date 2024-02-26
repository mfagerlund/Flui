// using System;
// using System.Linq.Expressions;
// using System.Reflection;
// using System.Runtime.CompilerServices;
//
// namespace Flui.Binder
// {
//     internal static class ReflectionHelper
//     {
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public static Func<T, TValue> GetPropertyValueFunc<T, TValue>(Expression<Func<T, TValue>> expr)
//         {
//             if (expr.Body is MemberExpression memberExpression)
//             {
//                 var propertyInfo = memberExpression.Member as PropertyInfo;
//                 if (propertyInfo != null)
//                 {
//                     if (!propertyInfo.DeclaringType.IsAssignableFrom(typeof(T)))
//                     {
//                         throw new InvalidOperationException($"Unhandled expression:{expr}");
//                     }
//     
//                     return x => (TValue)propertyInfo.GetValue(x);
//                 }
//     
//                 var fieldInfo = memberExpression.Member as FieldInfo;
//                 if (fieldInfo != null)
//                 {
//                     if (!fieldInfo.DeclaringType.IsAssignableFrom(typeof(T)))
//                     {
//                         throw new InvalidOperationException($"Unhandled expression:{expr}");
//                     }
//     
//                     return x => (TValue)fieldInfo.GetValue(x);
//                 }
//             }
//     
//             throw new InvalidOperationException($"Unable to get property getter for {expr}");
//         }
//     
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public static Action<T, TValue> SetPropertyValueFunc<T, TValue>(Expression<Func<T, TValue>> expr)
//         {
//             if (expr.Body is MemberExpression memberExpression)
//             {
//                 var propertyInfo = memberExpression.Member as PropertyInfo;
//                 if (propertyInfo != null)
//                 {
//                     if (!propertyInfo.DeclaringType.IsAssignableFrom(typeof(T)))
//                     {
//                         throw new InvalidOperationException($"Unhandled expression:{expr}");
//                     }
//     
//                     return (t, v) => propertyInfo.SetValue(t, v);
//                 }
//     
//                 var fieldInfo = memberExpression.Member as FieldInfo;
//                 if (fieldInfo != null)
//                 {
//                     if (!fieldInfo.DeclaringType.IsAssignableFrom(typeof(T)))
//                     {
//                         throw new InvalidOperationException($"Unhandled expression:{expr}");
//                     }
//     
//                     return (t, v) => fieldInfo.SetValue(t, v);
//                 }
//             }
//     
//             throw new InvalidOperationException($"Unable to get property setter for {expr}");
//         }
//     
//     
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public static string GetPath<T, U>(Expression<Func<T, U>> expr)
//         {
//             if (expr.Body is MemberExpression me)
//             {
//                 return me.Member.Name;
//             }
//     
//             throw new InvalidOperationException($"Expected a member expression of the type {typeof(U).Name}, but received {expr.Body.ToString()}");
//         }
//         
//         [MethodImpl(MethodImplOptions.AggressiveInlining)]
//         public static string GetMethodName<T>(Expression<Action<T>> func)
//         {
//             if (func.Body is MethodCallExpression methodCall)
//             {
//                 return methodCall.Method.Name;
//             }
//     
//             throw new ArgumentException("Expression does not represent a method call.");
//         }
//     }
// }