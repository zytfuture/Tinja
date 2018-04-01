﻿using System;

namespace Tinja.Resolving.ReslovingContext
{
    public class ResolvingContext : IResolvingContext
    {
        public Type ReslovingType { get; }

        public Component Component { get; }

        public ResolvingContext(Type resolvingType, Component component)
        {
            Component = component;
            ReslovingType = resolvingType;
        }
    }
}
