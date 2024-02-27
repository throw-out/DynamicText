using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    public static class SpriteAssetMenu
    {
        [MenuItem("Assets/Create/DynamicText/Default SpriteAsset")]
        static void CreateDefaultSpriteAssets()
        {
            var asset = Resources.Load<SpriteAsset>("DynamicText/SpriteAssets");
            if (asset != null)
            {
                Selection.activeObject = asset;
                return;
            }
            string localPath = $"Assets/Resources/DynamicText/SpriteAsset.asset",
                file = $"{Path.GetDirectoryName(UnityEngine.Application.dataPath)}/{localPath}",
                dir = Path.GetDirectoryName(file);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            asset = UnityEngine.ScriptableObject.CreateInstance<SpriteAsset>();
            UnityEditor.AssetDatabase.CreateAsset(asset, localPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Selection.activeObject = asset;
        }
        [MenuItem("Assets/Create/DynamicText/Default PrefabAsset")]
        static void CreateDefaultPrefabAssets()
        {
            var asset = Resources.Load<PrefabAsset>("DynamicText/PrefabAssets");
            if (asset != null)
            {
                Selection.activeObject = asset;
                return;
            }

            string localPath = $"Assets/Resources/DynamicText/PrefabAsset.asset",
                file = $"{Path.GetDirectoryName(UnityEngine.Application.dataPath)}/{localPath}",
                dir = Path.GetDirectoryName(file);
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            asset = UnityEngine.ScriptableObject.CreateInstance<PrefabAsset>();
            UnityEditor.AssetDatabase.CreateAsset(asset, localPath);
            UnityEditor.AssetDatabase.SaveAssets();
            UnityEditor.AssetDatabase.Refresh();

            Selection.activeObject = asset;
        }
    }
}
