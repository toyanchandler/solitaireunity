using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.Helper.Pooling
{
    public static class GameObjectPool
    {
        private static readonly Dictionary<GameObject, Queue<GameObject>> Pools = new Dictionary<GameObject, Queue<GameObject>>();
        private static readonly Dictionary<GameObject, GameObject> PrefabsByInstance = new Dictionary<GameObject, GameObject>();
        private static Transform root;

        public static GameObject Spawn(GameObject prefab)
        {
            if (prefab == null)
            {
                Debug.LogError("GameObjectPool.Spawn called with a null prefab.");
                return null;
            }

            return Spawn(prefab, prefab.transform.position, prefab.transform.rotation);
        }

        public static GameObject Spawn(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null)
            {
                Debug.LogError("GameObjectPool.Spawn called with a null prefab.");
                return null;
            }

            var instance = GetOrCreateInstance(prefab);
            instance.transform.SetPositionAndRotation(position, rotation);
            instance.transform.SetParent(null, true);
            instance.SetActive(true);
            instance.SendMessage("OnSpawn", SendMessageOptions.DontRequireReceiver);
            return instance;
        }

        public static void Despawn(GameObject instance)
        {
            if (instance == null)
            {
                return;
            }

            instance.SendMessage("OnDespawn", SendMessageOptions.DontRequireReceiver);

            if (!PrefabsByInstance.TryGetValue(instance, out var prefab) || prefab == null)
            {
                Object.Destroy(instance);
                return;
            }

            instance.SetActive(false);
            instance.transform.SetParent(Root, false);
            GetPool(prefab).Enqueue(instance);
        }

        private static GameObject GetOrCreateInstance(GameObject prefab)
        {
            var pool = GetPool(prefab);

            while (pool.Count > 0)
            {
                var instance = pool.Dequeue();
                if (instance != null)
                {
                    return instance;
                }
            }

            var created = Object.Instantiate(prefab);
            created.name = prefab.name;
            PrefabsByInstance[created] = prefab;
            return created;
        }

        private static Queue<GameObject> GetPool(GameObject prefab)
        {
            if (!Pools.TryGetValue(prefab, out var pool))
            {
                pool = new Queue<GameObject>();
                Pools[prefab] = pool;
            }

            return pool;
        }

        private static Transform Root
        {
            get
            {
                if (root != null)
                {
                    return root;
                }

                root = new GameObject("GameObjectPool").transform;
                Object.DontDestroyOnLoad(root.gameObject);
                return root;
            }
        }
    }
}
