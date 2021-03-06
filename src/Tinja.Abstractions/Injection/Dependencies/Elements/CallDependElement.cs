﻿using System;

namespace Tinja.Abstractions.Injection.Dependencies.Elements
{
    /// <summary>
    /// an element that representing an dependency point
    /// </summary>
    public abstract class CallDependElement
    {
        /// <summary>
        /// the service definition type
        /// </summary>
        public Type ServiceType { get; set; }

        public int ServiceCacheId { get; set; }

        /// <summary>
        /// the service life style
        /// </summary>
        public ServiceLifeStyle LifeStyle { get; set; }

        public virtual TVisitResult Accept<TVisitResult>(CallDependElementVisitor<TVisitResult> visitor)
        {
            throw new NotImplementedException();
        }
    }
}
