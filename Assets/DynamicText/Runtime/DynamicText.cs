using System;
using System.Collections.Generic;
using UnityEngine.Events;

namespace UnityEngine.UI
{
    /// <summary>
    /// 图片: <sprite="name"/>
    /// Prefab: <prefab="name"/>
    /// 下划线: <u>文本内容</u>
    /// 超链接: <link="id">文本内容</link>
    /// </summary>
    [AddComponentMenu("UI/Text - DynamicText")]
    public partial class DynamicText : Text
    {
        const int CHARACTER_VERTICE_NUMBER = 4;
        private List<TagData> tags;
        private bool resolved;
        private bool pending;
        private string sourceText;
        private int sourceFontSize;
        private int characterCount = 0;

        private List<Node> nodeCaches;
        [SerializeField]
        private List<SpriteAsset> m_FallbackSpriteAssets;
        [SerializeField]
        private List<PrefabAsset> m_FallbackPrefabAssets;

        [SerializeField]
        private HyperlinkEvent m_HyperlinkClick = new HyperlinkEvent();

        public HyperlinkEvent onHyperlinkClick
        {
            get => m_HyperlinkClick;
        }
        public List<SpriteAsset> FallbackSpriteAssets
        {
            get => m_FallbackSpriteAssets;
        }
        public List<PrefabAsset> FallbackPrefabAssets
        {
            get => m_FallbackPrefabAssets;
        }

        public override string text
        {
            set => SetText(value);
            get => base.text;
        }

        protected override void Start()
        {
            base.Start();
            if (Application.isPlaying && !resolved)
            {
                SetText(text);
            }
        }
        protected override void OnEnable()
        {
            base.OnEnable();
            Validate();
            if (Application.isPlaying)
            {
                DynamicTextUpdateTracker.Track(this);
            }
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            if (Application.isPlaying)
            {
                DynamicTextUpdateTracker.Untrack(this);
            }
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (Application.isPlaying)
            {
                DynamicTextUpdateTracker.Untrack(this);
                ClearTagNodes();
            }
        }
        protected override void OnPopulateMesh(VertexHelper toFill)
        {
            base.OnPopulateMesh(toFill);
            if (Application.isPlaying && this != null)
            {
                UpdateTagBounds();
                UpdateQuadVertexToEmpty(toFill);
                //UpdateTagNodes();
                pending = true;
            }
        }
        public virtual void ForceUpdate()
        {
            SetText(resolved ? sourceText : base.text);
        }
        public virtual void Validate()
        {
            if (!Application.isPlaying || this == null)
                return;
            if (sourceFontSize != fontSize)
            {
                ForceUpdate();
            }
            if (pending)
            {
                pending = false;
                UpdateTagNodes();
            }
        }

        void SetText(string value)
        {
            resolved = true;
            pending = true;
            tags?.Clear();
            sourceText = value;
            sourceFontSize = fontSize;
            characterCount = -1;

            if (!this.supportRichText)
                base.text = value;
            else
                base.text = TagUtils.ParseText(value, ref tags, fontSize);

            //UpdateTagBounds();
        }

        void UpdateCharacterCount()
        {
            if (this.text == null)
            {
                this.characterCount = 0;
                return;
            }
            this.cachedTextGeneratorForLayout.GetPreferredWidth(this.text, GetGenerationSettings(Vector2.zero));
            this.characterCount = this.cachedTextGeneratorForLayout.GetCharactersArray().Length;
        }
        /// <summary>
        /// 更新tag标签位置区域信息
        /// 字符全部显示时: 坐标序号忽略标签和空白字符;
        /// 字符被截断显示时: 坐标序号需计算标签和空白字符
        /// </summary>
        void UpdateTagBounds()
        {
            if (font == null || tags == null || tags.Count <= 0)
            {
                tags?.ForEach(tag => tag.bounds.Clear());
                return;
            }
            if (characterCount < 0) UpdateCharacterCount();

            UIVertex[] verts = this.cachedTextGenerator.GetVerticesArray();
            int count = verts.Length;
            int _characterCount = this.cachedTextGenerator.characterCount;
            foreach (var tag in tags)
            {
                tag.bounds.Clear();

                //获取字符序号, 并检查endIndex是否超出顶点列表(截断丶不渲染的字符)
                int startIndex = tag.StartIndex, endIndex = tag.EndIndex;
                if (_characterCount < characterCount)
                {
                    startIndex += tag.StartPadding;
                    endIndex += tag.EndPadding;
                }
                if (GetUVIndexLB(endIndex) >= count)
                    endIndex = count / 4 - 1;
                if (endIndex < startIndex)
                    continue;

                //计算属于同一行的UV信息
                bool move = true;
                while (move && startIndex <= endIndex)
                {
                    //获取开始uv序号和结束uv序号
                    if (GetUVIndexLT(startIndex) >= count)
                    {
                        move = false;
                        break;
                    }
                    int _index = endIndex;
                    Vector3 startLT = verts[GetUVIndexLT(startIndex)].position,
                        startLB = verts[GetUVIndexLB(startIndex)].position,
                        endRB = verts[GetUVIndexRB(_index)].position;
                    while (_index > startIndex && !Utils.InLine(startLB, endRB))
                    {
                        _index--;
                        endRB = verts[GetUVIndexRB(_index)].position;
                    }

                    Rect bound = new Rect(
                        startLB.x,
                        startLB.y,
                        endRB.x - startLB.x,
                        startLT.y - startLB.y
                    );
                    if (Utils.IsValidBounds(bound)) tag.bounds.Add(bound);
                    startIndex = _index + 1;
                }
            }
        }
        /// <summary> quad标签将显示乱码, 将其顶点重置 </summary>
        void UpdateQuadVertexToEmpty(VertexHelper toFill)
        {
            if (font == null || tags == null || tags.Count <= 0)
                return;

            if (characterCount < 0) UpdateCharacterCount();

            int count = toFill.currentVertCount;
            int _characterCount = this.cachedTextGenerator.characterCount;
            foreach (var tag in tags)
            {
                if (!tag.IsQuadTag)
                    continue;
                int index = tag.StartIndex;
                if (_characterCount < characterCount)
                {
                    index += tag.StartPadding;
                }
                int startUVIndex = GetUVIndexLT(index);
                if (startUVIndex >= count)
                    break;
                UIVertex uv = default(UIVertex);
                toFill.PopulateUIVertex(ref uv, startUVIndex);    //LT
                toFill.SetUIVertex(uv, startUVIndex + 1);         //RT
                toFill.SetUIVertex(uv, startUVIndex + 2);         //RB
                toFill.SetUIVertex(uv, startUVIndex + 3);         //LB
            }
        }
        void UpdateTagNodes()
        {
            ClearTagNodes();
            if (font == null || tags == null || tags.Count <= 0)
                return;

            if (nodeCaches == null)
                nodeCaches = new List<Node>();

            Vector3 basePosition = transform.position;
            foreach (var tag in tags)
            {
                if (tag.bounds.Count == 0)
                    continue;
                switch (tag.Type)
                {
                    case TagType.Sprite:
                        {
                            var data = AssetManager.GetSprite(tag.Extra, this);
                            Vector2 scale = data?.scale ?? Vector2.one,
                                offset = data?.offset ?? Vector2.zero;
                            foreach (var bound in tag.bounds)
                            {
                                var node = NodeManager.SpritePool.Get();
                                node.SetParent(transform);
                                node.SetSizeDelta(bound.size);
                                node.SetPosition(basePosition + (Vector3)bound.center + (Vector3)offset);
                                node.SetSprite(data?.sprite);
                                node.SetColor(Utils.HexToColor(tag.Color, color));

                                nodeCaches.Add(node);
                            }
                        }
                        break;
                    case TagType.Prefab:
                        foreach (var bound in tag.bounds)
                        {

                        }
                        break;
                    case TagType.Underline:
                        foreach (var bound in tag.bounds)
                        {
                            var node = NodeManager.UnderlinePool.Get();
                            node.SetParent(transform);
                            node.SetLocation(
                                basePosition + new Vector3(bound.min.x, bound.min.y),
                                basePosition + new Vector3(bound.max.x, bound.min.y)
                            );
                            node.SetColor(Utils.HexToColor(tag.Color, color));

                            nodeCaches.Add(node);
                        }
                        break;
                    case TagType.Hyperlink:
                        UnityAction callback = () =>
                        {
                            if (m_HyperlinkClick == null)
                                return;
                            m_HyperlinkClick.Invoke(tag.Extra);
                        };
                        foreach (var bound in tag.bounds)
                        {
                            var node = NodeManager.HyperlinkPool.Get();
                            node.SetParent(transform);
                            node.SetSizeDelta(bound.size);
                            node.SetPosition(basePosition + (Vector3)bound.center);
                            node.AddCallback(callback);

                            nodeCaches.Add(node);
                        }
                        break;
                }
            }
        }
        void ClearTagNodes()
        {
            if (nodeCaches == null)
                return;
            foreach (var node in nodeCaches)
            {
                NodeManager.Release(node);
            }
            nodeCaches.Clear();
        }


#if UNITY_EDITOR
        protected virtual void OnSceneGUI()
        {
        }
        protected virtual void OnDrawGizmos()
        {
            DrawTagsBounds();
        }
        void DrawTagsBounds()
        {
            //UnityEditor.Handles.Label(this.transform.position, "***");
            //Gizmos.color = Color.red;
            //Gizmos.DrawCube(transform.position, Vector3.one * 100f);
            if (tags == null || tags.Count <= 0)
                return;
            Gizmos.color = Color.green;
            Vector3 basePosition = transform.position;
            tags.ForEach(tag => tag.bounds.ForEach(bound =>
            {
                Vector2 min = bound.min, max = bound.max;
                Vector3 LB = basePosition + new Vector3(min.x, min.y),
                    RB = basePosition + new Vector3(max.x, min.y),
                    LT = basePosition + new Vector3(min.x, max.y),
                    RT = basePosition + new Vector3(max.x, max.y);
                Gizmos.DrawLine(LB, RB);
                Gizmos.DrawLine(LB, LT);
                Gizmos.DrawLine(RT, RB);
                Gizmos.DrawLine(RT, LT);
            }));
        }
#endif

        static int GetUVIndexLT(int index)
        {
            return index * CHARACTER_VERTICE_NUMBER;
        }
        static int GetUVIndexRT(int index)
        {
            return index * CHARACTER_VERTICE_NUMBER + 1;
        }
        static int GetUVIndexRB(int index)
        {
            return index * CHARACTER_VERTICE_NUMBER + 2;
        }
        static int GetUVIndexLB(int index)
        {
            return index * CHARACTER_VERTICE_NUMBER + 3;
        }


        [Serializable]
        public class HyperlinkEvent : UnityEngine.Events.UnityEvent<string>
        {
        }

        class DynamicTextUpdateTracker : UnityEngine.MonoBehaviour
        {
            private List<DynamicText> Texts { get; } = new List<DynamicText>();


            private void Update()
            {
                for (int i = Texts.Count - 1; i >= 0; i--)
                {
                    if (Texts[i] == null)
                    {
                        Texts.RemoveAt(i);
                        continue;
                    }
                    Texts[i].Validate();
                }
            }

            static DynamicTextUpdateTracker _instance;
            static DynamicTextUpdateTracker GetInstance(bool create)
            {
                if (_instance == null)
                {
                    if (!create)
                        return null;

                    var go = new GameObject(nameof(DynamicTextUpdateTracker));
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<DynamicTextUpdateTracker>();
                }
                return _instance;
            }

            public static void Track(DynamicText text)
            {
                GetInstance(true).Texts.Add(text);
            }
            public static void Untrack(DynamicText text)
            {
                GetInstance(false)?.Texts.Remove(text);
            }
        }
    }
}
