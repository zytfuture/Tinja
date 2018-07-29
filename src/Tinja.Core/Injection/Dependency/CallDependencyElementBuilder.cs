﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Tinja.Abstractions.Injection.Configurations;
using Tinja.Abstractions.Injection.Dependency;
using Tinja.Abstractions.Injection.Dependency.Elements;
using Tinja.Abstractions.Injection.Descriptors;
using Tinja.Core.Injection.Descriptors;

namespace Tinja.Core.Injection.Dependency
{
    /// <summary>
    /// the default implementation for <see cref="ICallDependencyElementBuilder"/>
    /// </summary>
    public class CallDependencyElementBuilder : ICallDependencyElementBuilder
    {
        protected IInjectionConfiguration Configuration { get; }

        protected IServiceDescriptorFactory ServiceDescriptorFactory { get; set; }

        protected CallDependencyElementScope CallScope { get; set; }

        public CallDependencyElementBuilder(IServiceDescriptorFactory serviceDescriptorFactory, IInjectionConfiguration configuration)
        {
            Configuration = configuration;
            ServiceDescriptorFactory = serviceDescriptorFactory;
            CallScope = new CallDependencyElementScope();
        }

        public virtual CallDepenencyElement Build(Type serviceType)
        {
            var descriptor = ServiceDescriptorFactory.Create(serviceType);
            if (descriptor == null)
            {
                return null;
            }

            return BuildElement(descriptor);
        }

        protected virtual CallDepenencyElement BuildElement(ServiceDescriptor descriptor)
        {
            switch (descriptor)
            {
                case ServiceManyDescriptor many:
                    return BuildManyElement(many);

                case ServiceInstanceDescriptor instance:
                    return BuildInstanceElement(instance);

                case ServiceDelegateDescriptor @delegate:
                    return BuildDelegateElement(@delegate);

                case ServiceConstrcutorDescriptor constrcutor:
                    using (CallScope.Begin(constrcutor.ImplementationType))
                    {
                        return BuildConstrcutorElement(constrcutor);
                    }
            }

            throw new InvalidOperationException();
        }

        protected virtual CallDepenencyElement BuildDelegateElement(ServiceDelegateDescriptor descriptor)
        {
            return new DelegateCallDepenencyElement()
            {
                LifeStyle = descriptor.LifeStyle,
                ServiceType = descriptor.ServiceType,
                Delegate = descriptor.Delegate
            };
        }

        protected virtual CallDepenencyElement BuildInstanceElement(ServiceInstanceDescriptor descriptor)
        {
            return new InstanceCallDependencyElement()
            {
                LifeStyle = descriptor.LifeStyle,
                ServiceType = descriptor.ServiceType,
                Instance = descriptor.Instance
            };
        }

        protected virtual CallDepenencyElement BuildManyElement(ServiceManyDescriptor descriptor)
        {
            var elements = new List<CallDepenencyElement>();

            foreach (var item in descriptor.Elements)
            {
                var ele = BuildElement(item);
                if (ele == null)
                {
                    continue;
                }

                elements.Add(ele);
            }

            return new ManyCallDepenencyElement()
            {
                Elements = elements.ToArray(),
                LifeStyle = descriptor.LifeStyle,
                ServiceType = descriptor.ServiceType,
                ImplementionType = descriptor.CollectionType,
                ConstructorInfo = descriptor.CollectionType.GetConstructors().FirstOrDefault(i => i.GetParameters().Length == 0)
            };
        }

        protected virtual CallDepenencyElement BuildConstrcutorElement(ServiceConstrcutorDescriptor descriptor)
        {
            var parameterElements = new Dictionary<ParameterInfo, CallDepenencyElement>();

            foreach (var item in descriptor.Constrcutors.OrderByDescending(i => i.GetParameters().Length))
            {
                var parameterInfos = item.GetParameters();
                if (parameterInfos.Any(parameterInfo => !BuildParameterElement(parameterInfo, parameterElements)))
                {
                    parameterElements.Clear();
                }

                if (parameterElements.Count != parameterInfos.Length)
                {
                    parameterElements.Clear();
                    continue;
                }

                var element = new ConstructorCallDependencyElement()
                {
                    Parameters = parameterElements,
                    LifeStyle = descriptor.LifeStyle,
                    ServiceType = descriptor.ServiceType,
                    ImplementionType = descriptor.ImplementationType,
                    ConstructorInfo = item
                };

                return BuildProperty(element);
            }

            return null;
        }

        protected CallDepenencyElement BuildProperty(ConstructorCallDependencyElement element)
        {
            if (element == null || !Configuration.EnablePropertyInjection)
            {
                return element;
            }

            var propertieInfos = element
                .ImplementionType
                .GetTypeInfo()
                .DeclaredProperties
                .Where(i => i.CanRead && i.CanWrite && i.IsDefined(typeof(InjectAttribute)));

            var properties = new Dictionary<PropertyInfo, CallDepenencyElement>();

            foreach (var propertieInfo in propertieInfos)
            {
                BuildPropertyElement(propertieInfo, properties);
            }

            element.Properties = properties;

            return element;
        }

        protected void BuildPropertyElement(PropertyInfo propertyInfo, Dictionary<PropertyInfo, CallDepenencyElement> propertyElements)
        {
            var descriptor = ServiceDescriptorFactory.Create(propertyInfo.PropertyType);
            if (descriptor == null)
            {
                return;
            }

            CheckCircularDependency(descriptor as ServiceConstrcutorDescriptor);

            var propertyElement = BuildElement(descriptor);
            if (propertyElement == null)
            {
                return;
            }

            propertyElements[propertyInfo] = propertyElement;
        }

        protected bool BuildParameterElement(ParameterInfo parameterInfo, Dictionary<ParameterInfo, CallDepenencyElement> parameterElements)
        {
            var descriptor = ServiceDescriptorFactory.Create(parameterInfo.ParameterType);
            if (descriptor == null)
            {
                return false;
            }

            CheckCircularDependency(descriptor as ServiceConstrcutorDescriptor);

            var parameterElement = BuildElement(descriptor);
            if (parameterElement == null)
            {
                return false;
            }

            parameterElements[parameterInfo] = parameterElement;

            return true;
        }

        protected void CheckCircularDependency(ServiceConstrcutorDescriptor descriptor)
        {
            if (descriptor == null ||
                descriptor.ImplementationType == null)
            {
                return;
            }

            if (CallScope.Contains(descriptor.ImplementationType))
            {
                throw new CallCircularException(descriptor.ImplementationType, $"type:{descriptor.ImplementationType.FullName} exists circular dependencies!");
            }
        }
    }
}
