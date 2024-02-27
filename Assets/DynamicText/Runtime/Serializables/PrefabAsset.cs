using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    [CreateAssetMenu(fileName = "DynamicText_PrefabAsset", menuName = "DynamicText/PrefabAsset", order = 0)]
#endif
    public class PrefabAsset : ScriptableObject
    {
        [SerializeField]
        private List<PrefabAsset> m_Fallbacks;
        [SerializeField]
        private PrefabDataDictionary m_Data;

        public List<PrefabAsset> Fallbacks
        {
            get => m_Fallbacks;
        }
        public PrefabDataDictionary Data
        {
            get => m_Data;
        }
        public bool TryGetValue(string name, out PrefabData value)
        {
            if (m_Data == null)
            {
                value = null;
                return false;
            }
            return m_Data.TryGetValue(name, out value);
        }

        [System.Serializable]
        public class PrefabData
        {
            [SerializeField]
            public GameObject prefab;
            [SerializeField]
            public Vector2 offset;
            [SerializeField]
            public Vector2 scale = Vector2.one;
        }

        [System.Serializable]
        public class PrefabDataDictionary : Dictionary<string, PrefabData>, UnityEngine.ISerializationCallbackReceiver
        {
            [SerializeField]
            private List<string> names;
            [SerializeField]
            private List<PrefabData> prefabs;

            public void OnAfterDeserialize()
            {
                this.Clear();
                if (names == null || prefabs == null)
                    return;
                int count = Math.Min(names.Count, prefabs.Count);
                for (int i = 0; i < count; i++)
                {
                    if (names[i] == null)
                        continue;
                    this[names[i]] = prefabs[i];
                }
                names.Clear();
                prefabs.Clear();
            }

            public void OnBeforeSerialize()
            {
                ClearOrCreate(ref names);
                ClearOrCreate(ref prefabs);
                foreach (var pair in this)
                {
                    names.Add(pair.Key);
                    prefabs.Add(pair.Value);
                }
            }

            static void ClearOrCreate<T>(ref List<T> list)
            {
                if (list != null)
                    list.Clear();
                else
                    list = new List<T>();
            }
        }
    }
}
