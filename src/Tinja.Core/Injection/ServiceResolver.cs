﻿using System;
using Tinja.Abstractions.Injection;
using Tinja.Abstractions.Injection.Dependencies;
using Tinja.Core.Injection.Activations;

namespace Tinja.Core.Injection
{
    public class ServiceResolver : IServiceResolver
    {
        /// <summary>
        /// <see cref="ServiceLifeScope"/>
        /// </summary>
        internal ServiceLifeScope Scope { get; }

        /// <summary>
        /// <see cref="ActivatorProvider"/>
        /// </summary>
        internal ActivatorProvider Provider { get; }

        /// <summary>
        /// create root service resolver
        /// </summary>
        /// <param name="factory"></param>
        internal ServiceResolver(ICallDependElementBuilderFactory factory)
        {
            if (factory == null)
            {
                throw new NullReferenceException(nameof(factory));
            }

            Scope = new ServiceLifeScope(this);
            Provider = new ActivatorProvider(Scope, factory);
        }

        /// <summary>
        /// create scoped service resolver
        /// </summary>
        /// <param name="serviceResolver"></param>
        internal ServiceResolver(ServiceResolver serviceResolver)
        {
            if (serviceResolver == null)
            {
                throw new NullReferenceException(nameof(serviceResolver));
            }

            Provider = serviceResolver.Provider;
            Scope = new ServiceLifeScope(this, serviceResolver.Scope);
        }

        public object ResolveService(Type serviceType)
        {
            return Provider.Get(serviceType)?.Invoke(this, Scope);
        }

        public void Dispose()
        {
            Scope.Dispose();
        }
    }
}
