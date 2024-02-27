using System;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    public partial class DynamicText
    {
        internal abstract class Node
        {
            public readonly GameObject gameObject;
            public readonly RectTransform transform;
            public Node(GameObject go)
            {
                this.gameObject = go;
                this.transform = go.transform as RectTransform;
            }

            public bool IsDestroy()
            {
                return gameObject == null;
            }
            public virtual void SetActive(bool active)
            {
                ThrowErrorIfDestroy();
                gameObject.SetActive(active);
            }
            public virtual void SetParent(Transform parent)
            {
                ThrowErrorIfDestroy();
                transform.SetParent(parent);
                transform.localScale = Vector3.one;
                transform.localRotation = Quaternion.identity;
                transform.pivot = Vector2.one * 0.5f;
                transform.anchorMin = Vector2.zero;
                transform.anchorMax = Vector2.zero;
            }
            public virtual void SetPosition(Vector3 position)
            {
                this.transform.position = position;
            }
            public virtual void SetSizeDelta(Vector2 size)
            {
                this.transform.sizeDelta = size;
            }

            public virtual void Release()
            {
            }

            protected void ThrowErrorIfDestroy()
            {
                if (IsDestroy())
                    throw new System.Exception("Obejct is destroyed.");
            }
        }
        internal sealed class HyperlinkNode : Node
        {
            private Button Button { get; set; }
            public HyperlinkNode(GameObject go) : base(go)
            {
            }
            public override void Release()
            {
                base.Release();
                Button.onClick.RemoveAllListeners();
            }

            public void AddCallback(UnityAction callback)
            {
                ThrowErrorIfDestroy();
                Button.onClick.AddListener(callback);
            }

            public static HyperlinkNode Create(string name = null)
            {
                var go = new GameObject(name ?? "ButtonNode");
                var image = go.AddComponent<Image>();
                var button = go.AddComponent<Button>();
                image.color = Color.clear;
                button.targetGraphic = image;

                return new HyperlinkNode(go)
                {
                    Button = button,
                };
            }
        }
        internal sealed class SpriteNode : Node
        {
            private Image Image { get; set; }
            public SpriteNode(GameObject go) : base(go)
            {
            }

            public void SetColor(Color color)
            {
                ThrowErrorIfDestroy();
                Image.color = color;
            }
            public void SetSprite(Sprite sprite)
            {
                ThrowErrorIfDestroy();
                Image.sprite = sprite;
            }
            public static SpriteNode Create(string name = null)
            {
                var go = new GameObject(name ?? "SpriteNode");
                var image = go.AddComponent<Image>();
                image.color = Color.white;
                image.raycastTarget = false;
                return new SpriteNode(go)
                {
                    Image = image,
                };
            }
        }
        internal sealed class PrefabNode : Node
        {
            private Image Image { get; set; }
            public PrefabNode(GameObject go) : base(go)
            {
            }
        }
        internal sealed class UnderlineNode : Node
        {
            public Image Image { get; private set; }
            public UnderlineNode(GameObject go) : base(go)
            {

            }
            public override void SetParent(Transform parent)
            {
                ThrowErrorIfDestroy();
                transform.SetParent(parent);
                transform.localScale = Vector3.one;
                transform.localRotation = Quaternion.identity;
                transform.pivot = new Vector2(0f, 0.5f);
                transform.anchorMin = Vector2.zero;
                transform.anchorMax = Vector2.zero;
            }
            public void SetColor(Color color)
            {
                ThrowErrorIfDestroy();
                Image.color = color;
            }
            public void SetLocation(Vector3 start, Vector3 end)
            {
                ThrowErrorIfDestroy();
                transform.position = start;
                transform.sizeDelta = new Vector2((end - start).magnitude, 4f);
            }

            public static UnderlineNode Create(string name = null)
            {
                var go = new GameObject(name ?? "UnderlineNode");
                var image = go.AddComponent<Image>();
                image.color = Color.white;
                image.raycastTarget = false;
                return new UnderlineNode(go)
                {
                    Image = image,
                };
            }
        }
    }
}