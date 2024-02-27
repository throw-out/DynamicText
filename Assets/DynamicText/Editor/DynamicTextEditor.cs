using System;
using UnityEditorInternal;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(DynamicText))]
    internal class DynamicTextEditor : TextEditor
    {
        private SerializedProperty m_HyperlinkClick;
        private SerializedProperty m_FallbackSpriteAssets;
        private SerializedProperty m_FallbackPrefabAssets;
        private ReorderableList fallbackSpriteAssetsList;
        private ReorderableList fallbackPrefabAssetsList;

        private DynamicText component;

        protected override void OnEnable()
        {
            base.OnEnable();
            Reset();

            component = target as DynamicText;
            m_HyperlinkClick = serializedObject.FindProperty("m_HyperlinkClick");
            m_FallbackSpriteAssets = serializedObject.FindProperty("m_FallbackSpriteAssets");
            m_FallbackPrefabAssets = serializedObject.FindProperty("m_FallbackPrefabAssets");

            if (m_FallbackSpriteAssets != null) m_FallbackSpriteAssets.isExpanded = true;
            if (m_FallbackPrefabAssets != null) m_FallbackPrefabAssets.isExpanded = true;
        }

        void Reset()
        {
            component = null;

            m_HyperlinkClick = null;
            m_FallbackSpriteAssets = null;
            m_FallbackPrefabAssets = null;
            fallbackSpriteAssetsList = null;
            fallbackPrefabAssetsList = null;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            //TODO: implement hyperlinkClick callback edit

            m_FallbackSpriteAssets.isExpanded = EditorGUILayout.Foldout(m_FallbackSpriteAssets.isExpanded, "Fallbacks SpriteAssets");
            if (m_FallbackSpriteAssets.isExpanded)
            {
                RendererFallbackSpriteAssets();
            }

            m_FallbackPrefabAssets.isExpanded = EditorGUILayout.Foldout(m_FallbackPrefabAssets.isExpanded, "Fallbacks PrefabAssets");
            if (m_FallbackPrefabAssets.isExpanded)
            {
                RendererFallbackPrefabAssets();
            }

            AssetDatabase.SaveAssetIfDirty(component);
        }

        void RendererFallbackSpriteAssets()
        {
            if (fallbackSpriteAssetsList == null)
            {
                fallbackSpriteAssetsList = new ReorderableList(serializedObject, m_FallbackSpriteAssets, true, false, true, true)
                {
                    drawElementCallback = (rect, index, selected, focused) =>
                    {
                        if (component.FallbackSpriteAssets == null || index >= component.FallbackSpriteAssets.Count)
                            return;
                        var newFallback = EditorGUI.ObjectField(rect, component.FallbackSpriteAssets[index], typeof(SpriteAsset), false) as SpriteAsset;
                        if (newFallback != component.FallbackSpriteAssets[index])
                        {
                            component.FallbackSpriteAssets[index] = newFallback;
                            EditorUtility.SetDirty(component);
                        }
                    },
                    onChangedCallback = (list) =>
                    {
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(component);
                    },
                    onRemoveCallback = (list) =>
                    {
                        m_FallbackSpriteAssets.DeleteArrayElementAtIndex(list.index);
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(component);
                    },
                    onAddCallback = (list) =>
                    {
                        m_FallbackSpriteAssets.arraySize++;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(component);
                    }
                };
            }
            fallbackSpriteAssetsList.DoLayoutList();
        }
        void RendererFallbackPrefabAssets()
        {
            if (fallbackPrefabAssetsList == null)
            {
                fallbackPrefabAssetsList = new ReorderableList(serializedObject, m_FallbackPrefabAssets, true, false, true, true)
                {
                    drawElementCallback = (rect, index, selected, focused) =>
                    {
                        if (component.FallbackPrefabAssets == null || index >= component.FallbackPrefabAssets.Count)
                            return;
                        var newFallback = EditorGUI.ObjectField(rect, component.FallbackPrefabAssets[index], typeof(PrefabAsset), false) as PrefabAsset;
                        if (newFallback != component.FallbackPrefabAssets[index])
                        {
                            component.FallbackPrefabAssets[index] = newFallback;
                            EditorUtility.SetDirty(component);
                        }
                    },
                    onChangedCallback = (list) =>
                    {
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(component);
                    },
                    onRemoveCallback = (list) =>
                    {
                        m_FallbackPrefabAssets.DeleteArrayElementAtIndex(list.index);
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(component);
                    },
                    onAddCallback = (list) =>
                    {
                        m_FallbackPrefabAssets.arraySize++;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(component);
                    }
                };
            }
            fallbackPrefabAssetsList.DoLayoutList();
        }
    }
}