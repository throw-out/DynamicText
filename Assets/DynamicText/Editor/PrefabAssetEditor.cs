using System;
using System.Linq;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(PrefabAsset))]
    internal class PrefabAssetEditor : Editor
    {
        private PrefabAsset component;
        private SerializedProperty m_Fallbacks;
        private SerializedProperty m_Data;
        private ReorderableList fallbacksList;
        private int pageIndex = 0;
        private int pageSize = 10;

        private void OnEnable()
        {
            Reset();

            component = target as PrefabAsset;
            m_Fallbacks = serializedObject.FindProperty("m_Fallbacks");
            m_Data = serializedObject.FindProperty("m_Data");

            if (m_Fallbacks != null) m_Fallbacks.isExpanded = true;
            if (m_Data != null) m_Data.isExpanded = true;
        }
        private void OnDisable()
        {
            if (component == null)
                return;
        }

        void Reset()
        {
            component = null;
            m_Fallbacks = null;
            m_Data = null;
            fallbacksList = null;
            pageIndex = 0;
        }

        public override void OnInspectorGUI()
        {
            if (component == null)
            {
                base.OnInspectorGUI();
                return;
            }
            //EditorGUILayout.PropertyField(m_Fallbacks);
            m_Fallbacks.isExpanded = EditorGUILayout.Foldout(m_Fallbacks.isExpanded, "Fallbacks");
            if (m_Fallbacks.isExpanded)
            {
                RendererFallbacks();
            }

            m_Data.isExpanded = EditorGUILayout.Foldout(m_Data.isExpanded, "Prefabs");
            if (m_Data.isExpanded)
            {
                RendererData();
            }

            AssetDatabase.SaveAssetIfDirty(component);
        }
        void RendererFallbacks()
        {
            if (fallbacksList == null)
            {
                fallbacksList = new ReorderableList(serializedObject, m_Fallbacks, true, false, true, true)
                {
                    drawElementCallback = (rect, index, selected, focused) =>
                    {
                        if (component.Fallbacks == null || index >= component.Fallbacks.Count)
                            return;
                        var newFallback = EditorGUI.ObjectField(rect, component.Fallbacks[index], typeof(PrefabAsset), false) as PrefabAsset;
                        if (newFallback != component.Fallbacks[index])
                        {
                            component.Fallbacks[index] = newFallback;
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
                        m_Fallbacks.DeleteArrayElementAtIndex(list.index);
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(component);
                    },
                    onAddCallback = (list) =>
                    {
                        m_Fallbacks.arraySize++;
                        serializedObject.ApplyModifiedProperties();
                        EditorUtility.SetDirty(component);
                    }
                };
            }
            fallbacksList.DoLayoutList();
        }
        void RendererData()
        {
            bool dirty = false;

            //渲染头部菜单
            EditorGUILayout.BeginVertical("box");
            dirty = RendererDataMenu() || dirty;
            RendererDataPage(component.Data.Count);
            EditorGUILayout.EndVertical();

            //渲染成员信息
            int startIndex = pageIndex * pageSize, endIndex = (pageIndex + 1) * pageSize;
            string[] keys = component.Data.Keys.ToArray();
            for (int i = startIndex; i < keys.Length && i < endIndex; i++)
            {
                dirty = RendererDataMember(i, keys[i], component.Data[keys[i]]) || dirty;
            }

            //渲染底部翻页菜单
            EditorGUILayout.BeginVertical("box");
            RendererDataPage(component.Data.Count);
            EditorGUILayout.EndHorizontal();

            if (dirty)
            {
                EditorUtility.SetDirty(component);
            }
        }
        bool RendererDataMenu()
        {
            bool dirty = false;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Add"))
            {
                component.Data.Add(GetDefaultKey(), new PrefabAsset.PrefabData());
                dirty = true;
            }
            if (GUILayout.Button("Clear"))
            {
                if (EditorUtility.DisplayDialog("Tip", "Want to delete all empty sprites, do you want to continue?", "Continue", "Cancel"))
                {
                    string[] keys = component.Data.Keys.ToArray();
                    for (int i = 0; i < keys.Length; i++)
                    {
                        string key = keys[i]; PrefabAsset.PrefabData data = component.Data[key];
                        if (data == null || data.prefab == null && IsDefaultKey(key))
                        {
                            component.Data.Remove(key);
                            dirty = true;
                        }
                    }
                }
            }
            EditorGUILayout.EndHorizontal();

            return dirty;
        }
        //翻页菜单
        void RendererDataPage(int total)
        {
            int pageTotal = total / pageSize;
            if (total % pageSize > 0)
            {
                pageTotal++;
            }
            if (pageIndex >= pageTotal && pageTotal > 0)
            {
                pageIndex = pageTotal - 1;
            }

            EditorGUILayout.BeginHorizontal();
            using (new EditorGUI.DisabledScope(pageIndex <= 0))
            {
                if (GUILayout.Button("Previous Page"))
                {
                    pageIndex--;
                }
            }
            GUILayout.Label($"{pageIndex + 1}/{pageTotal}");
            using (new EditorGUI.DisabledScope(pageIndex >= pageTotal - 1))
            {
                if (GUILayout.Button("Next Page"))
                {
                    pageIndex++;
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        bool RendererDataMember(int index, string name, PrefabAsset.PrefabData data)
        {
            if (data == null)
            {
                data = component.Data[name] = new PrefabAsset.PrefabData();
            }

            bool dirty = false;
            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.BeginHorizontal();
            //略缩图
            EditorGUILayout.BeginVertical(GUILayout.MaxWidth(50f), GUILayout.MaxHeight(50f));
            if (data != null && data.prefab != null)
            {
                GUILayout.Label(AssetDatabase.GetCachedIcon(AssetDatabase.GetAssetPath(data.prefab)), GUILayout.Width(50f), GUILayout.Height(50f));
            }
            else
            {
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical();
            GUILayout.Label($"Index {index}");
            //渲染名称
            string newName = EditorGUILayout.TextField(name);
            if (newName != name && newName != null && !component.Data.ContainsKey(newName))
            {
                component.Data.Remove(name);
                component.Data.Add(newName, data);
                name = newName;
                dirty = true;
            }
            //渲染精灵
            GameObject newPrefab = EditorGUILayout.ObjectField(data.prefab, typeof(GameObject), false) as GameObject;
            if (newPrefab != data.prefab)
            {
                data.prefab = newPrefab;
                if (IsDefaultKey(name) && newPrefab != null && !component.Data.ContainsKey(newPrefab.name))
                {
                    newName = newPrefab.name;
                    component.Data.Remove(name);
                    component.Data.Add(newName, data);
                    name = newName;
                }
                dirty = true;
            }

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();

            //渲染scale
            Vector2 newScale = EditorGUILayout.Vector2Field("Scale", data.scale);
            if (newScale != data.scale)
            {
                data.scale = newScale;
                dirty = true;
            }
            //渲染offset
            Vector2 newOffset = EditorGUILayout.Vector2Field("Offset", data.offset);
            if (newOffset != data.offset)
            {
                data.offset = newOffset;
                dirty = true;
            }
            //删除按钮
            if (GUILayout.Button("Delete"))
            {
                component.Data.Remove(name);
                dirty = true;
            }
            EditorGUILayout.EndVertical();

            return dirty;
        }

        bool IsDefaultKey(string name)
        {
            return name != null && name.StartsWith("key");
        }
        string GetDefaultKey()
        {
            int i = 0;
            while (i < 10000)
            {
                string key = $"key{i++}";
                if (!component.Data.ContainsKey(key))
                    return key;
            }
            throw new Exception("The key is too large");
        }
    }
}