using System.Collections.Generic;

namespace UnityEngine.UI
{
    public partial class DynamicText
    {
        internal static class NodeManager
        {
            public static ObjectPool<HyperlinkNode> HyperlinkPool { get; private set; }
            public static ObjectPool<SpriteNode> SpritePool { get; private set; }
            public static ObjectPool<UnderlineNode> UnderlinePool { get; private set; }
            static Dictionary<string, ObjectPool<PrefabNode>> PrefabPools;

            static Transform root;
            static Transform GetNodeManagerRoot()
            {
                if (root == null)
                {
                    var go = new GameObject(nameof(NodeManager) + "Root").transform;
                    UnityEngine.Object.DontDestroyOnLoad(go);
                    root = go.transform;
                }
                return root;
            }

            static NodeManager()
            {
                InitPools();
            }
            static void InitPools()
            {
                //Hyperlink
                HyperlinkPool = new ObjectPool<HyperlinkNode>();
                HyperlinkPool.CreateEvent += () => HyperlinkNode.Create();
                HyperlinkPool.GetEvent += (item) =>
                {
                    item.SetActive(true);
                };
                HyperlinkPool.ReleaseEvent += (item) =>
                {
                    item.SetActive(false);
                    item.SetParent(GetNodeManagerRoot());
                    item.Release();
                };
                //Sprite
                SpritePool = new ObjectPool<SpriteNode>();
                SpritePool.CreateEvent += () => SpriteNode.Create();
                SpritePool.GetEvent += (item) =>
                {
                    item.SetActive(true);
                };
                SpritePool.ReleaseEvent += (item) =>
                {
                    item.SetActive(false);
                    item.SetParent(GetNodeManagerRoot());
                    item.Release();
                };
                //Underline
                UnderlinePool = new ObjectPool<UnderlineNode>();
                UnderlinePool.CreateEvent += () => UnderlineNode.Create();
                UnderlinePool.GetEvent += (item) =>
                {
                    item.SetActive(true);
                };
                UnderlinePool.ReleaseEvent += (item) =>
                {
                    item.SetActive(false);
                    item.SetParent(GetNodeManagerRoot());
                    item.Release();
                };
            }
            public static void Release(Node node)
            {
                if (node is HyperlinkNode node1)
                {
                    HyperlinkPool.Release(node1);
                }
                else if (node is SpriteNode node2)
                {
                    SpritePool.Release(node2);
                }
                else if (node is UnderlineNode node3)
                {
                    UnderlinePool.Release(node3);
                }
                else if (node is PrefabNode node4)
                {

                }
            }
        }
    }
}