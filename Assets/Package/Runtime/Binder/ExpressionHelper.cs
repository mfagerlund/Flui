using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace Flui.Binder
{
    public static class CachedExpressionHelper
    {
        private static readonly StringBuilder Sb = new();
        private static readonly Stack<string> Stack = new();

        public static Dictionary<long, ICachedExpression> CachedExpressions { get; } = new();

        public static CachedExpression<TSource, TValue> GetCachedExpression<TSource, TValue>(Expression<Func<TSource, TValue>> expression)
        {
            var code = GetExpressionCode(expression);
            return GetCachedExpression(expression, code);
        }

        public static CachedExpression<TSource, TValue> GetCachedExpression<TSource, TValue>(Expression<Func<TSource, TValue>> expression, long code)
        {
            return (CachedExpression<TSource, TValue>)
                CachedExpressions.GetOrCreate(code, () => new CachedExpression<TSource, TValue>
                {
                    Code = code,
                    Path = GetPath(expression),
                    Getter = expression.Compile(),
                    Setter = CreateSetterOrNull(expression)
                });
        }

        public static Action<TSource, TValue> CreateSetterOrNull<TSource, TValue>(Expression<Func<TSource, TValue>> getterExpression)
        {
            try
            {
                return CreateSetter(getterExpression);
            }
            catch
            {
                return null;
            }
        }

        public static long GetExpressionCode<TSource, TValue>(Expression<Func<TSource, TValue>> expression)
        {
            unchecked
            {
                const int prime1 = 486187739;
                const int prime2 = 16777619;

                long hash = prime1;
                hash = hash * prime2 + typeof(TSource).GetHashCode();
                var current = expression.Body;
                while (current != null)
                {
                    switch (current.NodeType)
                    {
                        case ExpressionType.MemberAccess:

                            var memberExpression = (MemberExpression)current;
                            hash = hash * prime2 + memberExpression.Member.Name.GetHashCode();
                            current = memberExpression.Expression;
                            break;
                        case ExpressionType.Call:
                            var methodCallExpression = (MethodCallExpression)current;
                            hash = hash * prime2 + methodCallExpression.Method.Name.GetHashCode();
                            current = methodCallExpression.Object;
                            break;
                        case ExpressionType.Parameter:
                            current = null;
                            break;
                    }
                }

                return hash;
            }
        }

        public static Action<TSource, TValue> CreateSetter<TSource, TValue>(Expression<Func<TSource, TValue>> getterExpression)
        {
            if (getterExpression.Body is MemberExpression memberExpression)
            {
                ParameterExpression sourceParameter = Expression.Parameter(typeof(TSource), "source");
                ParameterExpression valueParameter = Expression.Parameter(typeof(TValue), "value");

                // Build the expression chain to access the final member
                Expression targetExpression = BuildMemberAccessExpression(sourceParameter, memberExpression);

                // Ensure that the member is writable
                if (memberExpression.Member is PropertyInfo property && !property.CanWrite)
                {
                    throw new ArgumentException("The target property is read-only.", nameof(getterExpression));
                }

                // Create an expression to represent assigning the value to the property/field
                BinaryExpression assignExpression = Expression.Assign(targetExpression, valueParameter);

                // Create a lambda expression representing the whole assignment operation
                var lambdaExpression = Expression.Lambda<Action<TSource, TValue>>(assignExpression, sourceParameter, valueParameter);

                // Compile the lambda expression into a delegate and return it
                return lambdaExpression.Compile();
            }
            else
            {
                throw new ArgumentException("The expression does not specify a property or field.", nameof(getterExpression));
            }
        }

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

        private static Expression BuildMemberAccessExpression(Expression sourceParameter, MemberExpression memberExpression)
        {
            // This method recursively builds the chain of member access expressions
            Stack<MemberExpression> memberStack = new Stack<MemberExpression>();
            Expression currentExpression = memberExpression;
            while (currentExpression is MemberExpression currentMemberExpression)
            {
                memberStack.Push(currentMemberExpression);
                currentExpression = currentMemberExpression.Expression;
            }

            Expression resultExpression = sourceParameter;
            while (memberStack.Count > 0)
            {
                var member = memberStack.Pop();
                resultExpression = Expression.MakeMemberAccess(resultExpression, member.Member);
            }

            return resultExpression;
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

        public interface ICachedExpression
        {
            long Code { get; }
            string Path { get; }
        }

        public class CachedExpression<TSource, TValue> : ICachedExpression
        {
            public long Code { get; set; }
            public string Path { get; set; }
            public Func<TSource, TValue> Getter { get; set; }
            public Action<TSource, TValue> Setter { get; set; }
        }
    }
}