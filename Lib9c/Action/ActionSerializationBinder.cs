using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace Nekoyume.Action
{
    public class ActionSerializationBinder: SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            var listOfAllowedTypeNames = Assembly
                .GetAssembly(typeof(ActionBase))
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(ActionBase)))
                .Select(x => x.FullName)
                .ToArray();
 
            if (Array.Exists(listOfAllowedTypeNames, e => e == typeName))
            {
                return null;
            }
            throw new ArgumentException($"Unexpected type {typeName}", nameof(typeName));
        }
    }
}