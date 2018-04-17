﻿using Microsoft.Extensions.DependencyInjection;
using System;
using Tinja;
using Tinja.Annotations;
using Tinja.LifeStyle;

namespace Sample
{
    public interface IServiceA
    {

    }

    public class ServiceA : IServiceA
    {
        [Inject]
        public IService Service { get; set; }

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

        public ServiceB(IServiceA serviceA)
        {
            //Console.WriteLine("B" + GetHashCode());
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

        public ServiceXX(T t)
        {

        }
    }

    public class Service : IService
    {
        [Inject]
        public IServiceB ServiceA
        {
            get; set;
        }

        public ServiceB S { get; set; }

        public Service(IServiceB b, IServiceA s)
        {
            S = b as ServiceB;
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

    class Program
    {
        static void Main(string[] args)
        {
            var watch = new System.Diagnostics.Stopwatch();
            var container = new Container();
            var services = new ServiceCollection();

            container.AddService(typeof(IServiceA), typeof(ServiceA), ServiceLifeStyle.Scoped);
            container.AddService(typeof(IServiceB), typeof(ServiceB), ServiceLifeStyle.Transient);
            container.AddService(typeof(IService), typeof(Service), ServiceLifeStyle.Transient);
            container.AddService(typeof(IServiceXX<>), typeof(ServiceXX<>), ServiceLifeStyle.Scoped);

            services.AddTransient<IServiceA, ServiceA>();
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
            //var x = resolver.Resolve<Nullable<int>>();

            var serviceXX = resolver.Resolve<IServiceXX<IService>>();
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
    }
}
