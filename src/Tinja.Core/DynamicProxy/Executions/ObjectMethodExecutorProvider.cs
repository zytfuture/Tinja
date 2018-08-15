﻿using System;
using System.Collections.Generic;
using System.Reflection;
using Tinja.Abstractions.DynamicProxy.Executions;

namespace Tinja.Core.DynamicProxy.Executions
{
    public class ObjectMethodExecutorProvider : IObjectMethodExecutorProvider
    {
        private readonly Dictionary<MethodInfo, IObjectMethodExecutor> _executors;

        public ObjectMethodExecutorProvider()
        {
            _executors = new Dictionary<MethodInfo, IObjectMethodExecutor>();
        }

        public IObjectMethodExecutor GetExecutor(MethodInfo methodInfo)
        {
            if (methodInfo == null)
            {
                throw new NullReferenceException(nameof(methodInfo));
            }

            if (_executors.TryGetValue(methodInfo, out var executor))
            {
                return executor;
            }

            return _executors[methodInfo] = new ObjectMethodExecutor(methodInfo);
        }
    }
}