// <copyright file="ValueOrError{T}.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Errors;

/// <summary>
/// Conditional value class.
/// </summary>
/// <typeparam name="T">The type of the value.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="ValueOrError{T}" /> class.
/// Initializes a new valued instance of the <see cref="ValueOrError{T}" /> class.
/// </remarks>
public class ValueOrError<T> : ValueOrError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueOrError{T}"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    public ValueOrError(T value)
        : base(value)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueOrError{T}"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="error">The error.</param>
    /// <param name="hasValue">if set to <c>true</c> [has value].</param>
    internal ValueOrError(object value, ApplicationError error, bool hasValue)
        : base(value, error, hasValue)
    {
    }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>The value.</value>
    /// <exception cref="InvalidOperationException">No value.</exception>
    public new T? Value => base.Value is null ? default : (T)base.Value;
}