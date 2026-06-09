using System.Collections;
using UnityEngine;

namespace _Game.Scripts.Helper.Extensions.System
{
    [global::System.Serializable]
    public class InterfaceSerialization<T> : ISerializationCallbackReceiver, IEnumerable where T : class
    {
        public Object targetData;
        public T I { get => targetData as T; }
        public static implicit operator bool(InterfaceSerialization<T> ir) => ir.targetData != null;
        void OnValidate()
        {
            if (!(targetData is T)) 
            {
                if (targetData is GameObject go)
                {
                    targetData = null;
                    foreach (Component c in go.GetComponents<Component>())
                    { 
                        if (c is T){
                            targetData = c;
                            break;
                        }
                    }
                }
            }
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize() => this.OnValidate();
        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
        public IEnumerator GetEnumerator()
        {
            throw new global::System.NotImplementedException();
        }
    }
}