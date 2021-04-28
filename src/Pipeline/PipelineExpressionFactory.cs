using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;

namespace Pipeline
{
    internal static class PipelineExpressionFactory
    {
        private static readonly ConcurrentDictionary<Type, Func<object, IServiceProvider, object>> _messageContextConstructorCache =
            new();

        public static Func<object, IServiceProvider, object> GetMessagecontextConstructor(Type messageType)
        {
            if (!_messageContextConstructorCache.TryGetValue(messageType, out var lambda))
            {
                var messageContextType = typeof(MessageContext<>).MakeGenericType(messageType);
                var parameters = new Type[] { typeof(object), typeof(IServiceProvider) };
                var methodInfo = messageContextType.GetMethod("Create", parameters);
                var parametersExpression = new ParameterExpression[] { Expression.Parameter(typeof(object)), Expression.Parameter(typeof(IServiceProvider)) };
                lambda = Expression.Lambda<Func<object, IServiceProvider, object>>(Expression.Call(methodInfo, parametersExpression), parametersExpression).Compile();
                _messageContextConstructorCache.TryAdd(messageType, lambda);
            }
            return lambda;
        }


        private static readonly ConcurrentDictionary<Type, Func<object, Func<object, Func<Task>, Task>>> _middlewareInstanceAcessorCache =
            new();

        public static Func<object, Func<Task>, Task> GetMiddlewareExecutor(object middleware, Type messageType)
        {
            var middlewareType = middleware.GetType();
            if (!_middlewareInstanceAcessorCache.TryGetValue(middlewareType, out var middlewareInstanceAcessor))
            {
                middlewareInstanceAcessor = CreateLambda(middlewareType, messageType).Compile();
                _middlewareInstanceAcessorCache.TryAdd(middlewareType, middlewareInstanceAcessor);
            }
            return middlewareInstanceAcessor(middleware);
        }

        private static Expression<Func<object, Func<object, Func<Task>, Task>>> CreateLambda(Type middlewareType, Type messageType)
        {
            var (messageContextType, invokeMethodInfo) = GetTypeData(messageType, middlewareType);
            var (objectInstanceParameter, objectContextParameter, objectNextParameter) = parametersDefinition;

            return Expression.Lambda<Func<object, Func<object, Func<Task>, Task>>>
            (
                Expression.Lambda<Func<object, Func<Task>, Task>>
                (
                    Expression.Call(Expression.TypeAs(objectInstanceParameter, middlewareType), invokeMethodInfo, Expression.TypeAs(objectContextParameter, messageContextType), objectNextParameter),
                    objectContextParameter, objectNextParameter
                ),
                objectInstanceParameter
            );
        }

        private static (Type messageContextType, MethodInfo invokeMethodInfo) GetTypeData(Type messageType, Type middlewareType)
        {
            var messageContextType = typeof(MessageContext<>).MakeGenericType(messageType);
            var invokeMethodInfo = middlewareType.GetMethod("Invoke", new Type[] { messageContextType, typeof(Func<Task>) });

            return (messageContextType, invokeMethodInfo);
        }

        private static readonly (ParameterExpression, ParameterExpression, ParameterExpression) parametersDefinition
            = (Expression.Parameter(typeof(object), "obj"), Expression.Parameter(typeof(object), "ctx"), Expression.Parameter(typeof(Func<Task>), "next"));
    }
}
