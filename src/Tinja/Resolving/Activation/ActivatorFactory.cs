﻿using System;
using Tinja.Resolving.Activation.Builder;
using Tinja.Resolving.Dependency;
using Tinja.ServiceLife;

namespace Tinja.Resolving.Activation
{
    public class ActivatorFactory : IActivatorFactory
    {
        private readonly ICallDependencyElementBuilderFactory _callDependencyElementBuilderFactory;

        public ActivatorFactory(ICallDependencyElementBuilderFactory factory)
        {
            _callDependencyElementBuilderFactory = factory;
        }

        public Func<IServiceResolver, IServiceLifeScope, object> CreateActivator(Type serviceType)
        {
            var elementBuilder = _callDependencyElementBuilderFactory.CreateBuilder();
            if (elementBuilder == null)
            {
                throw new NullReferenceException(nameof(elementBuilder));
            }

            var element = elementBuilder.Build(serviceType);
            if (element == null)
            {
                return null;
            }

            return ExpressionActivatorBuilder.Default.Build(element);
        }
    }
}
