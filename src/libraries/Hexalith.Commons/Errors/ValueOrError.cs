// <copyright file="ValueOrError.cs" company="ITANEO">
// Copyright (c) ITANEO (https://www.itaneo.com). All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Hexalith.Commons.Errors;

/// <summary>
/// Conditional value class.
/// </summary>
public class ValueOrError
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ValueOrError" /> class.
    /// Initializes a new valued instance of the <see cref="ValueOrError" /> class.
    /// </summary>
    /// <param name="value">The value.</param>
    public ValueOrError(object? value)
    {
        HasValue = true;
        Value = value;
        Error = null;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValueOrError"/> class.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <param name="error">The error.</param>
    /// <param name="hasValue">if set to <c>true</c> [has value].</param>
    protected ValueOrError(object? value, ApplicationError error, bool hasValue)
    {
        HasValue = hasValue;
        Value = value;
        Error = error;
    }

    /// <summary>
    /// Gets a value indicating whether this instance is success.
    /// </summary>
    /// <value><c>true</c> if this instance is success; otherwise, <c>false</c>.</value>
    /// <exception cref="InvalidOperationException">No error.</exception>
    public ApplicationError Error =>
        HasValue || field == null ? throw new InvalidOperationException("No error") : field;

    /// <summary>
    /// Gets a value indicating whether this instance is success.
    /// </summary>
    /// <value><c>true</c> if this instance is success; otherwise, <c>false</c>.</value>
    public bool HasValue { get; }

    /// <summary>
    /// Gets the value.
    /// </summary>
    /// <value>The value.</value>
    /// <exception cref="InvalidOperationException">No value.</exception>
    public object? Value => HasValue ? field : throw new InvalidOperationException("No value");

    /// <summary>
    /// Withes the error.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>ValueOrError&lt;T&gt;.</returns>
    public static ValueOrError<T> WithError<T>(ApplicationError error) => new(default!, error, false);
}