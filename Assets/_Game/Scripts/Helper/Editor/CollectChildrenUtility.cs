#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Reflection;
using _Game.Scripts.Helper.Collections;
using UnityEditor;
using UnityEngine;

namespace _Game.Scripts.Helper.Editor
{
    public static class CollectChildrenUtility
    {
        public static void Collect(MonoBehaviour host)
        {
            if (host == null)
                throw new ArgumentNullException(nameof(host));

            var serializedObject = new SerializedObject(host);
            serializedObject.Update();

            FieldInfo[] fields = host.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            bool changed = false;

            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                CollectChildrenAttribute attribute = field.GetCustomAttribute<CollectChildrenAttribute>();

                if (attribute == null)
                    continue;

                SerializedProperty arrayProperty = serializedObject.FindProperty(field.Name);

                if (arrayProperty == null || !arrayProperty.isArray)
                    throw new InvalidOperationException($"{host.name}.{field.Name} must be a serialized array for CollectChildren.");

                Transform poolRoot = ResolvePoolRoot(host, attribute.PoolRootFieldName);
                Type elementType = field.FieldType.GetElementType();

                if (elementType == null || !typeof(UnityEngine.Object).IsAssignableFrom(elementType))
                    throw new InvalidOperationException($"{host.name}.{field.Name} must be a UnityEngine.Object array.");

                UnityEngine.Object[] collected = CollectComponents(poolRoot, elementType, attribute.Mode);
                WriteArray(arrayProperty, collected);
                changed = true;
            }

            if (!changed)
                return;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(host);

            if (host.gameObject.scene.IsValid())
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(host.gameObject.scene);
        }

        private static Transform ResolvePoolRoot(MonoBehaviour host, string poolRootFieldName)
        {
            FieldInfo poolRootField = host.GetType().GetField(
                poolRootFieldName,
                BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);

            if (poolRootField == null)
                throw new InvalidOperationException($"{host.name} is missing pool root field {poolRootFieldName}.");

            object value = poolRootField.GetValue(host);

            switch (value)
            {
                case Transform transform:
                    return transform != null
                        ? transform
                        : throw new InvalidOperationException($"{host.name}.{poolRootFieldName} is not assigned.");
                case GameObject gameObject:
                    return gameObject != null
                        ? gameObject.transform
                        : throw new InvalidOperationException($"{host.name}.{poolRootFieldName} is not assigned.");
                default:
                    throw new InvalidOperationException($"{host.name}.{poolRootFieldName} must be a Transform or GameObject.");
            }
        }

        private static UnityEngine.Object[] CollectComponents(Transform poolRoot, Type componentType, CollectChildrenMode mode)
        {
            var collected = new List<UnityEngine.Object>();

            switch (mode)
            {
                case CollectChildrenMode.DirectChildren:
                    CollectDirectChildren(poolRoot, componentType, collected);
                    break;
                case CollectChildrenMode.DepthFirstDescendants:
                    CollectDepthFirst(poolRoot, componentType, collected);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }

            return collected.ToArray();
        }

        private static void CollectDirectChildren(Transform poolRoot, Type componentType, List<UnityEngine.Object> collected)
        {
            for (int i = 0; i < poolRoot.childCount; i++)
            {
                Component component = poolRoot.GetChild(i).GetComponent(componentType);

                if (component != null)
                    collected.Add(component);
            }
        }

        private static void CollectDepthFirst(Transform current, Type componentType, List<UnityEngine.Object> collected)
        {
            Component component = current.GetComponent(componentType);

            if (component != null)
                collected.Add(component);

            for (int i = 0; i < current.childCount; i++)
                CollectDepthFirst(current.GetChild(i), componentType, collected);
        }

        private static void WriteArray(SerializedProperty arrayProperty, UnityEngine.Object[] values)
        {
            arrayProperty.arraySize = values.Length;

            for (int i = 0; i < values.Length; i++)
                arrayProperty.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
        }
    }
}
#endif
