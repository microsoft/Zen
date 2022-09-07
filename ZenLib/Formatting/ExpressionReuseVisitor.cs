// <copyright file="ExpressionReuseVisitor.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.Generation
{
    using System.Collections.Generic;

    /// <summary>
    /// Class to identify sub-expressions that are repeated more than once.
    /// </summary>
    internal class ExpressionReuseVisitor : ZenExprActionVisitor
    {
        /// <summary>
        /// The subexpressions that are reused in this expression.
        /// </summary>
        private ISet<object> reusedSubexpressions;

        /// <summary>
        /// Creates a new instance of the <see cref="ExpressionReuseVisitor"/> class.
        /// </summary>
        public ExpressionReuseVisitor() : base(new Dictionary<long, object>())
        {
            this.reusedSubexpressions = new HashSet<object>();
        }

        /// <summary>
        /// The set of reused subexpressions in this expression.
        /// </summary>
        /// <param name="expression">The zen expression.</param>
        /// <returns>The expression as a formatted string.</returns>
        public ISet<object> GetReusedSubExpressions<T>(Zen<T> expression)
        {
            this.VisitCached(expression);
            return this.reusedSubexpressions;
        }

        /// <summary>
        /// Visit an expression.
        /// </summary>
        /// <param name="expression">The expression.</param>
        /// <returns></returns>
        public override void VisitCached<T>(Zen<T> expression)
        {
            if (this.Visited.Contains(expression.Id))
            {
                this.reusedSubexpressions.Add(expression);
            }
            else
            {
                this.Visited.Add(expression.Id);
                expression.Accept(this);
            }
        }
    }
}
