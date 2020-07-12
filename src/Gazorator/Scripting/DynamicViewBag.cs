using System;
using System.Collections.Generic;
using System.Dynamic;

namespace Gazorator.Scripting
{
    public class DynamicViewBag : DynamicObject
    {
        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (!_properties.TryGetValue(binder.Name, out result))
            {
                result = GetDefault(binder.ReturnType);
            }

            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            _properties[binder.Name] = value;
            return true;
        }

        private static object GetDefault(Type type)
            => type.IsValueType ? Activator.CreateInstance(type) : null;
    }
}