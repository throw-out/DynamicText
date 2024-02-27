using System;
using System.Collections.Generic;
using UnityEngine;

namespace UnityEngine.UI
{
#if UNITY_EDITOR
    [UnityEditor.InitializeOnLoad]
    [CreateAssetMenu(fileName = "DynamicText_SpriteAsset", menuName = "DynamicText/SpriteAsset", order = 0)]
#endif
    public class SpriteAsset : ScriptableObject
    {
        [SerializeField]
        private List<SpriteAsset> m_Fallbacks;
        [SerializeField]
        private SpriteDataDictionary m_Data;

        public List<SpriteAsset> Fallbacks
        {
            get => m_Fallbacks;
        }
        public SpriteDataDictionary Data
        {
            get => m_Data;
        }

        public bool TryGetValue(string name, out SpriteData value)
        {
            if (m_Data == null)
            {
                value = null;
                return false;
            }
            return m_Data.TryGetValue(name, out value);
        }

        [System.Serializable]
        public class SpriteData
        {
            [SerializeField]
            public Sprite sprite;
            [SerializeField]
            public Vector2 offset;
            [SerializeField]
            public Vector2 scale = Vector2.one;
        }

        
        [System.Serializable]
        public class SpriteDataDictionary : Dictionary<string, SpriteData>, UnityEngine.ISerializationCallbackReceiver
        {
            [SerializeField]
            private List<string> names;
            [SerializeField]
            private List<SpriteData> sprites;

            public void OnAfterDeserialize()
            {
                this.Clear();
                if (names == null || sprites == null)
                    return;
                int count = Math.Min(names.Count, sprites.Count);
                for (int i = 0; i < count; i++)
                {
                    if (names[i] == null)
                        continue;
                    this[names[i]] = sprites[i];
                }
                names.Clear();
                sprites.Clear();
            }

            public void OnBeforeSerialize()
            {
                ClearOrCreate(ref names);
                ClearOrCreate(ref sprites);
                foreach (var pair in this)
                {
                    names.Add(pair.Key);
                    sprites.Add(pair.Value);
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
