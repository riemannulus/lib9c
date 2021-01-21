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
            try
            {
                return Assembly
                    .GetAssembly(typeof(ActionBase))
                    .GetTypes()
                    .Where(t => t.IsSubclassOf(typeof(ActionBase)))
                    .Single(t => t.FullName == typeName);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Unexpected type {typeName}", e);
            }
        }
    }
}
