﻿// <copyright file="IModelChecker.cs" company="Microsoft">
// Copyright (c) Microsoft. All rights reserved.
// </copyright>

namespace ZenLib.ModelChecking
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface for a model checker.
    /// </summary>
    internal interface IModelChecker
    {
        /// <summary>
        /// Find an input satisfying the expression via model checking.
        /// </summary>
        /// <param name="expression">The boolean expression.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        ///     Mapping from zen arbitrary expression to value.
        ///     Null if there is no input.
        /// </returns>
        Dictionary<object, object> ModelCheck(Zen<bool> expression, Dictionary<long, object> arguments);

        /// <summary>
        /// Find an input satisfying the expression via model checking.
        /// </summary>
        /// <param name="maximize">The expression to maximize.</param>
        /// <param name="subjectTo">The constraints for the problem.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        ///     Mapping from zen arbitrary expression to value.
        ///     Null if there is no input.
        /// </returns>
        Dictionary<object, object> Maximize<T>(Zen<T> maximize, Zen<bool> subjectTo, Dictionary<long, object> arguments);

        /// <summary>
        /// Find an input satisfying the expression via model checking.
        /// </summary>
        /// <param name="minimize">The expression to minimize.</param>
        /// <param name="subjectTo">The constraints for the problem.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        ///     Mapping from zen arbitrary expression to value.
        ///     Null if there is no input.
        /// </returns>
        Dictionary<object, object> Minimize<T>(Zen<T> minimize, Zen<bool> subjectTo, Dictionary<long, object> arguments);
    }
}
