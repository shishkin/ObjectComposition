using System;
using System.Collections.Generic;
using System.Dynamic;

namespace DataContextInteraction.ObjectComposition
{
    public class Entity : DynamicObject
    {
        private readonly Dictionary<string, object> data = new Dictionary<string, object>();

        public Entity(string id)
        {
            Id = id;
        }

        public string Id { get; private set; }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return data.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            data[binder.Name] = value;
            return true;
        }
    }
}
