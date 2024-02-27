using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Flui.Binder
{
    public class ExpressionHelper
    {
        private static readonly StringBuilder Sb = new();
        private static readonly Stack<string> Stack = new();

        // public static Action<TSource, TValue> CreateSetter<TSource, TValue>(Expression<Func<TSource, TValue>> getterExpression)
        // {
        //     // Get the member expression (e.g., x => x.Property)
        //     if (getterExpression.Body is MemberExpression memberExpression)
        //     {
        //         // Ensure that the member is a property or field that can be set
        //         if (memberExpression.Member is PropertyInfo propertyInfo && propertyInfo.CanWrite || memberExpression.Member is FieldInfo)
        //         {
        //             // Create a parameter expression for the source object (x)
        //             ParameterExpression sourceParameter = Expression.Parameter(typeof(TSource), "source");
        //             // Create a parameter expression for the value to be set
        //             ParameterExpression valueParameter = Expression.Parameter(typeof(TValue), "value");
        //
        //             // Create an expression to represent assigning the value to the property/field
        //             Expression assignExpression = Expression.Assign(
        //                 Expression.MakeMemberAccess(sourceParameter, memberExpression.Member),
        //                 valueParameter);
        //
        //             // Create a lambda expression representing the whole assignment operation
        //             var lambdaExpression = Expression.Lambda<Action<TSource, TValue>>(
        //                 assignExpression, sourceParameter, valueParameter);
        //
        //             // Compile the lambda expression into a delegate and return it
        //             return lambdaExpression.Compile();
        //         }
        //         else
        //         {
        //             throw new ArgumentException("The member selected by the getter expression is not writable.");
        //         }
        //     }
        //     else
        //     {
        //         throw new ArgumentException("The expression does not specify a property or field.");
        //     }
        // }
        
        public static string GetMethodName<T>(Expression<Action<T>> func)
        {
            if (func.Body is MethodCallExpression methodCall)
            {
                return methodCall.Method.Name;
            }

            throw new ArgumentException("Expression does not represent a method call.");
        }

        public static string GetPath<TSource, TValue>(Expression<Func<TSource, TValue>> expression)
        {
            Stack.Clear();
            var current = expression.Body;

            while (current != null)
            {
                switch (current.NodeType)
                {
                    case ExpressionType.MemberAccess:

                        var memberExpression = (MemberExpression)current;
                        Stack.Push(memberExpression.Member.Name);
                        current = memberExpression.Expression;
                        break;
                    case ExpressionType.Call:
                        var methodCallExpression = (MethodCallExpression)current;
                        Stack.Push(methodCallExpression.Method.Name + "()");
                        current = methodCallExpression.Object;
                        break;
                    case ExpressionType.Parameter:
                        current = null;
                        break;
                }
            }

            return CombineStrings();
        }

        public static TValue GetPropertyValue<TSource, TValue>(Expression<Func<TSource, TValue>> expression, TSource source) =>
            (TValue)GetPropertyValue(expression.Body, source);

        public static void SetPropertyValue<TSource, TValue>(Expression<Func<TSource, TValue>> expression, TSource source, TValue newValue) =>
            SetPropertyValue(expression.Body, source, newValue);

        public static Action<TSource, TValue> SetPropertyValueFunc<TSource, TValue>(Expression<Func<TSource, TValue>> expression) =>
            (source, newValue) => SetPropertyValue(expression, source, newValue);

        public static Func<TSource, TValue> GetPropertyValueFunc<TSource, TValue>(Expression<Func<TSource, TValue>> expression) =>
            source => GetPropertyValue(expression, source);

        private static void SetPropertyValue(Expression expression, object source, object newValue)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpr = (MemberExpression)expression;
                    var parent = GetPropertyValue(memberExpr.Expression, source);
                    SetPropertyValue(memberExpr, parent, newValue);
                    break;

                default:
                    throw new NotSupportedException($"Unsupported expression type: {expression.NodeType}");
            }
        }

        private static object GetPropertyValue(Expression expression, object source)
        {
            switch (expression.NodeType)
            {
                case ExpressionType.MemberAccess:
                    var memberExpr = (MemberExpression)expression;
                    var parent = GetPropertyValue(memberExpr.Expression, source);
                    return GetPropertyValue(memberExpr, parent);

                case ExpressionType.Call:
                    var methodCallExpr = (MethodCallExpression)expression;
                    var instance = GetPropertyValue(methodCallExpr.Object, source);
                    return GetPropertyValue(methodCallExpr, instance);

                case ExpressionType.Parameter:
                    return source;

                default:
                    throw new NotSupportedException($"Unsupported expression type: {expression.NodeType}");
            }
        }

        private static object GetPropertyValue(MemberExpression memberExpr, object instance)
        {
            switch (memberExpr.Member.MemberType)
            {
                case MemberTypes.Property:
                    var propertyInfo = (PropertyInfo)memberExpr.Member;
                    return propertyInfo.GetValue(instance, null);

                case MemberTypes.Field:
                    var fieldInfo = (FieldInfo)memberExpr.Member;
                    return fieldInfo.GetValue(instance);

                default:
                    throw new NotSupportedException($"Unsupported member type: {memberExpr.Member.MemberType}");
            }
        }

        private static void SetPropertyValue(MemberExpression memberExpr, object instance, object newValue)
        {
            switch (memberExpr.Member.MemberType)
            {
                case MemberTypes.Property:
                    var propertyInfo = (PropertyInfo)memberExpr.Member;
                    propertyInfo.SetValue(instance, newValue);
                    break;

                case MemberTypes.Field:
                    var fieldInfo = (FieldInfo)memberExpr.Member;
                    fieldInfo.SetValue(instance, newValue);
                    break;

                default:
                    throw new NotSupportedException($"Unsupported member type: {memberExpr.Member.MemberType}");
            }
        }

        private static object GetPropertyValue(MethodCallExpression methodCallExpr, object instance)
        {
            var methodInfo = methodCallExpr.Method;
            if (methodInfo.GetParameters().Length == 0) // Ensure it's a parameter-less method
            {
                return methodInfo.Invoke(instance, null);
            }

            throw new NotSupportedException("Method calls with parameters are not supported.");
        }

        private static string CombineStrings()
        {
            Sb.Clear();
            while (true)
            {
                Sb.Append(Stack.Pop());
                if (Stack.Any())
                {
                    Sb.Append(".");
                }
                else
                {
                    break;
                }
            }

            return Sb.ToString();
        }
    }
}