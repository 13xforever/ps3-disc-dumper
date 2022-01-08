using System;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IrdLibraryClient
{
    public class JsonContractResolver : DefaultContractResolver
    {
        private readonly Func<string, string> propertyNameResolver;

        public JsonContractResolver(): this(NamingStyles.Dashed)
        {
        }

        public JsonContractResolver(Func<string, string> propertyNameResolver)
        {
            if (propertyNameResolver == null)
                throw new ArgumentNullException(nameof(propertyNameResolver));

            this.propertyNameResolver = propertyNameResolver;
        }

        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);
            property.PropertyName = propertyNameResolver(property.PropertyName!);
            return property;
        }
    }
}