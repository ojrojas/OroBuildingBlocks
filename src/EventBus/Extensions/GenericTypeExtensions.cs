namespace OroBuildingBlocks.EventBus.Extensions;

public static class GenericTypeExtensions
{
    extension(Type type)
    {
        public string GetGenericTypeName()
        {
            var typeName = string.Empty;

            if (type.IsGenericType)
            {
                var genericTypes = string.Join(",", type.GetGenericArguments().Select(t => t.Name).ToArray());
                typeName = $"{type.Name.Remove(type.Name.IndexOf('`'))}<{genericTypes}>";
            }
            else
            {
                typeName = type.Name;
            }

            return typeName;
        }
    }

    extension(object @object)
    {
        public string GetGenericTypeName()
        {
            return @object.GetType().GetGenericTypeName();
        }
    }
}