using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace UnityEngine.UI
{
    public partial class DynamicText
    {
        public static class AssetManager
        {
            private static List<SpriteAsset> buildinSpriteAssets;
            private static List<PrefabAsset> buildinPrefabAssets;

            public static SpriteAsset.SpriteData GetSprite(string name, DynamicText component = null)
            {
                SpriteAsset.SpriteData sprite = GetSpriteInAssets(component?.FallbackSpriteAssets, name);
                if (sprite != null)
                    return sprite;

                LoadBuildinSpriteAssets();
                sprite = GetSpriteInAssets(buildinSpriteAssets, name);
                return sprite;
            }
            public static GameObject GetPrefab(string name, DynamicText component = null)
            {
                LoadBuildinPrefabAssets();

                return null;
            }
            public static void AddFallbackAssets(SpriteAsset asset)
            {
                LoadBuildinSpriteAssets();
                if (asset != null && !buildinSpriteAssets.Contains(asset))
                {
                    buildinSpriteAssets.Add(asset);
                }
            }
            public static void AddFallbackAssets(PrefabAsset asset)
            {
                LoadBuildinPrefabAssets();
                if (asset != null && !buildinPrefabAssets.Contains(asset))
                {
                    buildinPrefabAssets.Add(asset);
                }
            }

            static void LoadBuildinSpriteAssets()
            {
                if (buildinSpriteAssets != null)
                    return;
                buildinSpriteAssets = new List<SpriteAsset>();

                var asset = Resources.Load<SpriteAsset>("DynamicText/SpriteAsset");
                if (asset != null)
                {
                    buildinSpriteAssets.Add(asset);
                }
            }
            static void LoadBuildinPrefabAssets()
            {
                if (buildinPrefabAssets != null)
                    return;
                buildinPrefabAssets = new List<PrefabAsset>();

                var asset = Resources.Load<PrefabAsset>("DynamicText/PrefabAsset");
                if (asset != null)
                {
                    buildinPrefabAssets.Add(asset);
                }
            }

            static SpriteAsset.SpriteData GetSpriteInAssets(IEnumerable<SpriteAsset> assets, string name, HashSet<SpriteAsset> resolved = null)
            {
                if (assets == null)
                    return null;

                SpriteAsset.SpriteData data = null;
                foreach (var asset in assets)
                {
                    if (resolved != null && resolved.Contains(asset))
                        continue;
                    if (asset.TryGetValue(name, out data) && data != null)
                        break;
                    if (asset.Fallbacks != null)
                    {
                        resolved ??= new HashSet<SpriteAsset>();

                        resolved.Add(asset);
                        data = GetSpriteInAssets(asset.Fallbacks, name, resolved);
                    }
                }
                return data;
            }
        }
    }
}
