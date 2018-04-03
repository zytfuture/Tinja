﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using Tinja.Resolving.ReslovingContext;

namespace Tinja.Resolving.Builder
{
    public class ServiceFactoryBuilder : IServiceFactoryBuilder
    {
        public static ConcurrentDictionary<Type, Func<IContainer, ILifeStyleScope, object>> Cache { get; }

        static ServiceFactoryBuilder()
        {
            Cache = new ConcurrentDictionary<Type, Func<IContainer, ILifeStyleScope, object>>();
        }

        public Func<IContainer, ILifeStyleScope, object> Build(IServiceNode serviceNode)
        {
            return Cache.GetOrAdd(serviceNode.ResolvingContext.ReslovingType, (k) => BuildFactory(serviceNode, new HashSet<IServiceNode>()));
        }

        public Func<IContainer, ILifeStyleScope, object> Build(Type resolvingType)
        {
            if (Cache.TryGetValue(resolvingType, out var factory))
            {
                return factory;
            }

            return null;
        }

        public static Func<IContainer, ILifeStyleScope, object> BuildFactory(IServiceNode node, HashSet<IServiceNode> injectedProperties)
        {
            return new ServiceActivatorFacotry().CreateActivator(node);
        }

        private class ServiceActivatorFacotry
        {
            static ParameterExpression ParameterContainer { get; }

            static ParameterExpression ParameterLifeScope { get; }

            private Dictionary<IResolvingContext, HashSet<IResolvingContext>> _resolvedPropertyTypes;

            static ServiceActivatorFacotry()
            {
                ParameterContainer = Expression.Parameter(typeof(IContainer));
                ParameterLifeScope = Expression.Parameter(typeof(ILifeStyleScope));
            }

            public ServiceActivatorFacotry()
            {
                _resolvedPropertyTypes = new Dictionary<IResolvingContext, HashSet<IResolvingContext>>();
            }

            public Func<IContainer, ILifeStyleScope, object> CreateActivator(IServiceNode node)
            {
                var lambdaBody = BuildExpression(node);
                if (lambdaBody == null)
                {
                    throw new NullReferenceException(nameof(lambdaBody));
                }

                var factory = (Func<IContainer, ILifeStyleScope, object>)Expression
                       .Lambda(lambdaBody, ParameterContainer, ParameterLifeScope)
                       .Compile();

                if (node.ResolvingContext.Component.LifeStyle != LifeStyle.Transient ||
                    node.ResolvingContext.Component.ImplementionType.Is(typeof(IDisposable)))
                {
                    return BuildProperty(
                        (o, scoped) =>
                            scoped.GetOrAddLifeScopeInstance(node.ResolvingContext, (_) => factory(o, scoped)),
                        node
                    );
                }

                return BuildProperty(factory, node);
            }

            public Expression BuildExpression(IServiceNode serviceNode)
            {
                if (serviceNode.Constructor == null)
                {
                    return BuildImplFactory(serviceNode);
                }

                if (serviceNode is ServiceEnumerableNode enumerable)
                {
                    return BuildEnumerable(enumerable);
                }
                else
                {
                    return BuildConstructor(serviceNode as ServiceConstrutorNode);
                }
            }

            public Expression BuildImplFactory(IServiceNode node)
            {
                return
                    Expression.Invoke(
                        Expression.Constant(node.ResolvingContext.Component.ImplementionFactory),
                        ParameterContainer
                    );
            }

            public NewExpression BuildConstructor(ServiceConstrutorNode node)
            {
                var parameterValues = new Expression[node.Paramters?.Count ?? 0];

                for (var i = 0; i < parameterValues.Length; i++)
                {
                    var parameterValueFactory = CreateActivator(node.Paramters[node.Constructor.Paramters[i]]);
                    if (parameterValueFactory == null)
                    {
                        parameterValues[i] = Expression.Constant(null, node.Constructor.Paramters[i].ParameterType);
                    }
                    else
                    {
                        parameterValues[i] = Expression.Invoke(
                            Expression.Constant(parameterValueFactory),
                            ParameterContainer,
                            ParameterLifeScope
                        );
                    }
                }

                return Expression.New(node.Constructor.ConstructorInfo, parameterValues);
            }

            public ListInitExpression BuildEnumerable(ServiceEnumerableNode node)
            {
                var newExpression = BuildConstructor(node);
                var elementInits = new ElementInit[node.Elements.Length];
                var addElement = node.ResolvingContext.Component.ImplementionType.GetMethod("Add");

                for (var i = 0; i < elementInits.Length; i++)
                {
                    var elementValueFactory = CreateActivator(node.Paramters[node.Constructor.Paramters[i]]);
                    if (elementValueFactory == null)
                    {
                        continue;
                    }

                    elementInits[i] = Expression.ElementInit(
                        addElement,
                        Expression.Convert(
                            Expression.Invoke(
                                Expression.Constant(elementValueFactory),
                                ParameterContainer,
                                ParameterLifeScope
                            ),
                            node.Elements[i].ResolvingContext.ReslovingType
                        )
                    );
                }

                return Expression.ListInit(newExpression, elementInits);
            }

            public Expression BuildPropertyInfo(Expression instance, IServiceNode node)
            {
                instance = Expression.Convert(instance, node.Constructor.ConstructorInfo.DeclaringType);

                var vars = new List<ParameterExpression>();
                var statements = new List<Expression>();
                var instanceVar = Expression.Parameter(instance.Type);
                var assignInstance = Expression.Assign(instanceVar, instance);

                var label = Expression.Label(instanceVar.Type);

                vars.Add(instanceVar);
                statements.Add(assignInstance);
                statements.Add(
                    Expression.IfThen(
                        Expression.Equal(Expression.Constant(null), instanceVar),
                        Expression.Return(label, instanceVar)
                    )
                );

                foreach (var item in node.Properties)
                {
                    if (IsPropertyCircularDependeny(node, item.Value))
                    {
                        continue;
                    }

                    var property = Expression.MakeMemberAccess(instanceVar, item.Key);
                    var propertyVar = Expression.Variable(item.Key.PropertyType, item.Key.Name);

                    var propertyValue = Expression.Convert(
                            Expression.Invoke(
                                Expression.Constant(CreateActivator(item.Value)),
                                ParameterContainer,
                                ParameterLifeScope
                            ),
                            item.Value.ResolvingContext.ReslovingType
                        );

                    var setPropertyVarValue = Expression.Assign(propertyVar, propertyValue);
                    var setPropertyValue = Expression.IfThen(
                        Expression.NotEqual(Expression.Constant(null), propertyVar),
                        Expression.Assign(property, propertyVar)
                    );

                    vars.Add(propertyVar);
                    statements.Add(setPropertyVarValue);
                    statements.Add(setPropertyValue);
                }

                statements.Add(Expression.Return(label, instanceVar));
                statements.Add(Expression.Label(label, instanceVar));

                return Expression.Block(vars, statements);
            }

            public Func<IContainer, ILifeStyleScope, object> BuildProperty(Func<IContainer, ILifeStyleScope, object> factory, IServiceNode node)
            {
                if (node.Properties != null && node.Properties.Count != 0)
                {
                    var lambdaBody = BuildPropertyInfo(
                        Expression.Invoke(
                            Expression.Constant(factory),
                            ParameterContainer,
                            ParameterLifeScope
                        ),
                        node);

                    return (Func<IContainer, ILifeStyleScope, object>)Expression
                       .Lambda(lambdaBody, ParameterContainer, ParameterLifeScope)
                       .Compile();
                }

                return factory;
            }

            public bool IsPropertyCircularDependeny(IServiceNode instance, IServiceNode propertyNode)
            {
                if (!_resolvedPropertyTypes.ContainsKey(instance.ResolvingContext))
                {
                    _resolvedPropertyTypes[instance.ResolvingContext] = new HashSet<IResolvingContext>()
                {
                   propertyNode.ResolvingContext
                };

                    return false;
                }

                var properties = _resolvedPropertyTypes[instance.ResolvingContext];
                if (properties.Contains(propertyNode.ResolvingContext))
                {
                    return true;
                }

                properties.Add(propertyNode.ResolvingContext);

                return false;
            }
        }
    }
}

