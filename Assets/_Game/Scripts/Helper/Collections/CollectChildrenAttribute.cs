using System;

namespace _Game.Scripts.Helper.Collections
{
    public enum CollectChildrenMode
    {
        DirectChildren = 0,
        DepthFirstDescendants = 1,
    }

    [AttributeUsage(AttributeTargets.Field)]
    public sealed class CollectChildrenAttribute : Attribute
    {
        public string PoolRootFieldName { get; }
        public CollectChildrenMode Mode { get; }

        public CollectChildrenAttribute(string poolRootFieldName, CollectChildrenMode mode = CollectChildrenMode.DirectChildren)
        {
            PoolRootFieldName = poolRootFieldName;
            Mode = mode;
        }
    }
}
