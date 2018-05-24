﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;
using Tinja;
using Tinja.Interception;
using Tinja.Interception.Generators;
using Tinja.Interception.Internal;
using Tinja.ServiceLife;

namespace Sample
{
    public interface IServiceA
    {

    }

    public class ServiceA : IServiceA
    {
        public ServiceA()
        {
            //Console.WriteLine("A" + GetHashCode());
        }
    }

    public interface IServiceB
    {
        void Up();
    }

    public class ServiceB : IServiceB
    {
        [Inject]
        public IServiceA Service { get; set; }

        public ServiceB()
        {
            Console.WriteLine("B" + GetHashCode());
        }

        public void Up()
        {
            Console.WriteLine("Up");
        }
    }

    public interface IService
    {
        void Give();
    }

    public interface IServiceXX<T>
    {

    }

    public class ServiceXX<T> : IServiceXX<T>
    {
        [Inject]
        public IServiceXX<T> Instance { get; set; }

        public T t;

        public ServiceXX(T t)
        {
            this.t = t;
        }
    }

    public class Service : IService
    {
        public Service(IServiceA b)
        {
            //B = b;
            //Console.WriteLine("A" + b.GetHashCode());
            ////Console.WriteLine("A" + serviceA.GetHashCode());
            //Console.WriteLine(GetHashCode());
        }

        public void Dispose()
        {

        }

        public void Give()
        {
            Console.WriteLine("Give");
        }
    }

    public class A
    {
        [Inject]
        public A A2 { get; set; }
    }

    public class InterceptorTest : IInterceptor
    {
        public Task InvokeAsync(MethodInvocation invocation, Func<MethodInvocation, Task> next)
        {
            return next(invocation);
            Console.WriteLine("brefore InterceptorTest             ");
            var task = next(invocation);
            Console.WriteLine("after InterceptorTest");
            return task;
        }
    }

    public class InterceptorTest2 : IInterceptor
    {
        public Task InvokeAsync(MethodInvocation invocation, Func<MethodInvocation, Task> next)
        {
            invocation.ReturnValue = 10000;
            Console.WriteLine("brefore InterceptorTest2222222222222222");
            var task = next(invocation);
            Console.WriteLine("after InterceptorTest222222222222222222");
            return task;
        }
    }

    public class InterceptorTest3 : IInterceptor
    {
        public Task InvokeAsync(MethodInvocation invocation, Func<MethodInvocation, Task> next)
        {
            Console.WriteLine("brefore InterceptorTest2222222222222222");
            var task = next(invocation);
            Console.WriteLine("after InterceptorTest222222222222222222");
            return task;
        }
    }

    [Interceptor(typeof(InterceptorTest))]
    public class Abc : IAbc
    {
        public event Action OnOk;

        public virtual object M()
        {
            Console.WriteLine("方法执行 执行");
            return 6;
        }

        public virtual void M2()
        {

        }

        [Interceptor(typeof(InterceptorTest))]
        public virtual object Id { get; set; }
    }

    [Interceptor(typeof(InterceptorTest3))]
    public class Abc2 : Abc
    {
        public override object M()
        {
            Console.WriteLine("方法执行 执行");
            return 6;
        }
    }

    public interface IAbc
    {
        event Action OnOk;

        object M();
    }

    class Program
    {
        static void Main(string[] args)
        {
            var x = new Tinja.Interception.TypeMembers.ClassTypeMemberCollector(typeof(Abc), typeof(Abc2)).Collect();

            var watch = new System.Diagnostics.Stopwatch();
            var container = new Container();
            var services = new ServiceCollection();

            container.AddService(typeof(IServiceA), _ => new ServiceA(), ServiceLifeStyle.Transient);
            container.AddService(typeof(IServiceB), typeof(ServiceB), ServiceLifeStyle.Scoped);
            container.AddService(typeof(IService), typeof(Service), ServiceLifeStyle.Scoped);
            container.AddService(typeof(IServiceXX<>), typeof(ServiceXX<>), ServiceLifeStyle.Scoped);
            container.AddTransient<InterceptorTest, InterceptorTest>();
            container.AddTransient<InterceptorTest2, InterceptorTest2>();
            container.AddTransient<IMethodInvocationExecutor, MethodInvocationExecutor>();
            container.AddTransient<IMethodInvokerBuilder, MethodInvokerBuilder>();
            container.AddTransient(typeof(Abc), typeof(Abc));
            container.AddTransient(typeof(IInterceptorCollector), typeof(InterceptorCollector));
            container.AddTransient(typeof(IInterceptionTargetProvider), typeof(InterceptionTargetProvider));
            container.AddTransient(typeof(IObjectMethodExecutorProvider), typeof(ObjectMethodExecutorProvider));
            container.AddTransient(typeof(IMemberInterceptorFilter), typeof(MemberInterceptorFilter));
            var proxyType = new ProxyClassTypeGenerator(typeof(Abc), typeof(Abc)).CreateProxyType();

            container.AddTransient(proxyType, proxyType);

            services.AddScoped<IServiceA, ServiceA>();
            services.AddTransient<IServiceB, ServiceB>();
            services.AddTransient<IService, Service>();

            var provider = services.BuildServiceProvider();
            var resolver = container.BuildResolver();

            watch.Reset();
            watch.Start();

            //for (var i = 0; i < 1000_0000; i++)
            //{
            //    provider.GetService(typeof(IService));
            //}

            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);
            var xx = resolver.Resolve<Abc>();

            var proxyService = resolver.Resolve(proxyType) as Abc;

            watch.Reset();
            watch.Start();
            for (var i = 0; i < 1000000; i++)
            {
                proxyService.M();
            }

            watch.Start();
            Console.WriteLine("Inter:" + watch.ElapsedMilliseconds);
            watch.Reset();
            var xxxxxx = new Abc();
            watch.Start();
            for (var i = 0; i < 1000000; i++)
            {
                xxxxxx.Id = 2;
            }

            watch.Start();
            Console.WriteLine("Inter2:" + watch.ElapsedMilliseconds);

            var id = proxyService.Id;

            var y = resolver.Resolve(typeof(IServiceA));
            var b = resolver.Resolve(typeof(IServiceB));
            var service = resolver.Resolve(typeof(IService));

            watch.Reset();
            watch.Start();

            for (var i = 0; i < 10000_000; i++)
            {
                service = resolver.Resolve(typeof(IService));
            }

            watch.Stop();
            Console.WriteLine(watch.ElapsedMilliseconds);

            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }

        private static void ProxyService_OnOk()
        {

        }
    }
}
