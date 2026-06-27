// OroBuildingBlocks
// Copyright (C) 2026 Oscar Rojas
// Licensed under the GNU AGPL v3.0 or later.
// See the LICENSE file in the project root for details.
namespace OroBuildingBlocks.EventBus.Extensions;

/// <summary>
/// Extension methods for obtaining human-readable generic type names.
/// </summary>
public static class GenericTypeExtensions
{
    /// <summary>
    /// Returns the name of the type, including its generic parameters in angle-bracket notation.
    /// For example, <c>List&lt;String&gt;</c> for <see cref="List{T}"/> of <see cref="string"/>.
    /// </summary>
    /// <param name="type">The type to get the name for.</param>
    /// <returns>The formatted generic type name.</returns>
    public static string GetGenericTypeName(this Type type)
    {
        if (type.IsGenericType)
        {
            var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
            return $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
        }

        return type.Name;
    }

    /// <summary>
    /// Returns the generic type name of the object's runtime type.
    /// </summary>
    /// <param name="object">The object to get the type name for.</param>
    /// <returns>The formatted generic type name.</returns>
    public static string GetGenericTypeName(this object @object)
    {
        return @object.GetType().GetGenericTypeName();
    }
}
