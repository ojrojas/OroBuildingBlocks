// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.ServicesDefaults;

public static class AsyncExtensions
{
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
