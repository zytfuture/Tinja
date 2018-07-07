﻿using System;
using System.Threading.Tasks;
using Tinja.Interception;

namespace ConsoleApp
{
    public class UserServiceInterceptor : IInterceptor
    {
        public async Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            Console.WriteLine($"InterceptorType:{GetType().FullName} :::: { invocation.Method.Name }:executing");

            await next(invocation);

            Console.WriteLine($"InterceptorType:{GetType().FullName}::::{invocation.Method.Name }:executed :return{invocation.ReturnValue}->{invocation.ReturnValue.ToString() + "Interceptor"}");

            invocation.SetReturnValue(invocation.ReturnValue.ToString() + "Interceptor");
        }
    }

    public class UserServiceDataAnnotationInterceptor : IInterceptor
    {
        public async Task InvokeAsync(IMethodInvocation invocation, Func<IMethodInvocation, Task> next)
        {
            Console.WriteLine($"InterceptorType:{GetType().FullName} :::: { invocation.Method.Name }:executing");

            await next(invocation);

            Console.WriteLine($"InterceptorType:{GetType().FullName}::::{invocation.Method.Name }:executed");
        }
    }
}
