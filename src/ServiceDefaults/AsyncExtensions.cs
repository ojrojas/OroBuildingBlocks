// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.ServicesDefaults;

/// <summary>
/// Extension methods for async enumerable sequences.
/// </summary>
public static class AsyncExtensions
{
    /// <summary>
    /// Materializes an <see cref="IAsyncEnumerable{T}"/> into a <see cref="List{T}"/>.
    /// </summary>
    /// <typeparam name="T">The element type.</typeparam>
    /// <param name="source">The async-enumerable source.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> that resolves to the materialized list.</returns>
    public static ValueTask<List<T>> ToListExtensionsAsync<T>(this IAsyncEnumerable<T> source)
    {
        ArgumentNullException.ThrowIfNull(source);

        return ExecutionAsync();

        async ValueTask<List<T>> ExecutionAsync()
        {
            var list = new List<T>();

            await foreach (var i in source)
            {
                list.Add(i);
            }

            return list;
        }
    }
}
