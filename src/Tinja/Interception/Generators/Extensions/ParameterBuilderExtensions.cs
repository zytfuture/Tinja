﻿using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Tinja.Extensions;

namespace Tinja.Interception.Generators.Extensions
{
    public static class ParameterBuilderExtensions
    {
        public static ParameterBuilder SetCustomAttributes(this ParameterBuilder builder, ParameterInfo parameterInfo)
        {
            if (builder == null)
            {
                throw new NullReferenceException(nameof(builder));
            }

            if (parameterInfo == null)
            {
                throw new NullReferenceException(nameof(parameterInfo));
            }

            foreach (var customAttriute in parameterInfo
                .CustomAttributes
                .Where(item => !item.AttributeType.Is(typeof(InjectAttribute)) &&
                               !item.AttributeType.Is(typeof(InterceptorAttribute))))
            {
                var attrBuilder = GeneratorUtility.CreateCustomAttribute(customAttriute);
                if (attrBuilder != null)
                {
                    builder.SetCustomAttribute(attrBuilder);
                }
            }

            return builder;
        }
    }
}
