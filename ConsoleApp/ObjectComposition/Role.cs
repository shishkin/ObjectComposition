using System;

namespace DataContextInteraction.ObjectComposition
{
    public abstract class Role
    {
        public Role(Entity entity)
        {
            Entity = entity;
        }

        protected dynamic Entity { get; private set; }

        public string Id
        {
            get { return Entity.Id; }
        }
    }
}
