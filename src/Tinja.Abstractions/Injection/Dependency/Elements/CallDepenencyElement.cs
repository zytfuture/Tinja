﻿using System;

namespace Tinja.Abstractions.Injection.Dependency.Elements
{
    /// <summary>
    /// an element that representing an dependency point
    /// </summary>
    public abstract class CallDepenencyElement
    {
        public long ServiceId { get; set; }

        /// <summary>
        /// the service definition type
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// the service life style
        /// </summary>
        public ServiceLifeStyle LifeStyle { get; set; }

        public virtual TVisitResult Accept<TVisitResult>(CallDependencyElementVisitor<TVisitResult> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
