using System;
using System.Linq;

namespace Gk.Core.Utilities
{
    public static class TypeExtensions
    {
        public static string GetReadableName(this Type type)
        {
            if (!type.GetGenericArguments().Any())
            {
                return type.Name;
            }

            var genericTypeName = type.Name.Split('`').First();
            var genericArguments = type.GetGenericArguments().Select(x => x.IsGenericParameter ? "" : x.GetReadableName());
            var delimiter = type.GetGenericArguments().All(x => x.IsGenericParameter) ? "," : ", ";

            return string.Format("{0}<{1}>", genericTypeName, string.Join(delimiter, genericArguments.ToArray()));
        }
    }
}
