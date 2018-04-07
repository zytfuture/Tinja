﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Tinja;

namespace Sample
{
    public interface IServiceA
    {

    }

    public class ServiceA : IServiceA
    {
        public ServiceA()
        {

        }
    }

    public interface IServiceB
    {
        void Up();
    }

    public class ServiceB : IServiceB
    {
        public IService Service { get; set; }

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
        public ServiceXX(T t)
        {

        }
    }

    public class Service : IService
    {
        public Service(IServiceA ServiceA)
        {

        }

        public void Dispose()
        {

        }

        public void Give()
        {
            Console.WriteLine("Give");

        }
    }

    public class ServiceC : IService
    {
        public void Give()
        {

        }
    }

    public class ServiceD : ServiceC
    {

    }

    class Program
    {
        static void Main(string[] args)
        {
            var ioc = new Container();

            ioc.Register(typeof(IServiceA), typeof(ServiceA), LifeStyle.Transient);
            ioc.Register(typeof(IServiceB), typeof(ServiceB), LifeStyle.Transient);
            ioc.Register(typeof(IService), typeof(Service), LifeStyle.Transient);

            var st = new System.Diagnostics.Stopwatch();
            var services = new ServiceCollection();

            services.AddScoped<IServiceA, ServiceA>();
            services.AddScoped<IServiceB, ServiceB>();
            services.AddScoped<IService, Service>();

            var provider = services.BuildServiceProvider();
            //provider.GetService(typeof(IEnumerable<IEnumerable<IService>>));
            //provider.GetService<IService>();

            st.Reset();
            st.Start();

            for (var i = 0; i < 1000_00000; i++)
            {
                provider.GetService(typeof(IService));
            }

            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds);

            var service = ioc.Resolve(typeof(IService));

            st.Reset();
            st.Start();

            for (var i = 0; i < 1000_00000; i++)
            {
                ioc.Resolve(typeof(IService));
            }

            st.Stop();
            Console.WriteLine(st.ElapsedMilliseconds);

            Console.WriteLine("Hello World!");
            Console.ReadKey();
        }
    }
}
