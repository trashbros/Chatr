using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Runtime.CompilerServices;

namespace Chatr
{
    public class SettingsChain : DynamicObject
    {
        private readonly Dictionary<string, object> _dictionary = new Dictionary<string, object>();

        public SettingsChain Parent { get; set; }

        public int Count => _dictionary.Count;

        public SettingsChain(SettingsChain parent = null)
        {
            Parent = parent;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var name = binder.Name;

            if (name.StartsWith("Own"))
            {
                name = name.Substring(3);

                if (_dictionary.ContainsKey(name))
                {
                    return _dictionary.TryGetValue(name, out result);
                }

                result = null;
                return true;
            }

            // Try to return own value
            if (_dictionary.ContainsKey(name))
            {
                return _dictionary.TryGetValue(name, out result);
            }

            // Try to return parent's value
            if (Parent != null)
            {
                return Parent.TryGetMember(binder, out result);
            }

            // Can't find it
            result = null;
            return true;
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            var name = binder.Name;

            if (name.StartsWith("Own"))
            {
                return false;
            }

            if (value != null)
            {
                _dictionary[name] = value;
            }
            else if (_dictionary.ContainsKey(name))
            {
                _dictionary.Remove(name);
            }

            return true;
        }
    }
}
