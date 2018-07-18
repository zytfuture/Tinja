﻿using System.Collections.Generic;
using Tinja.Abstractions.DynamicProxy;

namespace Tinja.Abstractions.Configuration
{
    public class InterceptionConfiguration
    {
        public bool EnableInterception { get; set; } = true;

        public List<IInterceptorDefinitonProvider> Providers { get; }

        public InterceptionConfiguration()
        {
            Providers = new List<IInterceptorDefinitonProvider>();
        }
    }
}