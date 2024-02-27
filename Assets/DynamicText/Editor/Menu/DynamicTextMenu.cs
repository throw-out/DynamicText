using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    public static class DynamicTextMenu
    {
        const BindingFlags Flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        [MenuItem("GameObject/UI/Text - DynamicText")]
        static void Create()
        {
            var selectObj = Selection.activeGameObject;
            if (selectObj == null)
                return;
            //当前为资源文件
            var assetPath = AssetDatabase.GetAssetPath(selectObj);
            if (!string.IsNullOrEmpty(assetPath))
                return;

            Transform parent = null;
            //从当前节点向上遍历,寻找Canvas节点
            if (selectObj.GetComponentInParent<Canvas>() != null)
            {
                parent = selectObj.transform;
            }
            //从全局Find一个Canvas节点
            if (parent == null)
            {
                parent = UnityEngine.Object.FindObjectOfType<Canvas>()?.transform;
            }
            //创建一个新的Canvas节点
            if (parent == null)
            {
                var canvasGO = new GameObject("Canvas");
                canvasGO.transform.SetParent(null);
                canvasGO.transform.position = Vector2.zero;
                var canvas = canvasGO.AddComponent<Canvas>();
                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
                canvas.worldCamera = Camera.main;
                canvas.renderMode = RenderMode.ScreenSpaceCamera;
                parent = canvasGO.transform;
                //EventSystem
                if (UnityEngine.Object.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
                {
                    var systemGO = new GameObject("EventSystem");
                    systemGO.transform.SetParent(null);
                    systemGO.transform.position = Vector2.zero;
                    systemGO.AddComponent<UnityEngine.EventSystems.EventSystem>();
                    systemGO.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                }
            }
            //创建节点
            var go = new GameObject(nameof(DynamicText));
            go.AddComponent<DynamicText>();
            go.transform.SetParent(parent);
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;

            Selection.activeGameObject = go;
        }

        [MenuItem("CONTEXT/Text/-> DynamicText")]
        static void Convert(MenuCommand command)
        {
            if (command == null || command.context == null || !(command.context is UnityEngine.Component))
                return;

            Convert<Text, DynamicText>(((UnityEngine.Component)command.context).gameObject);
        }

        [MenuItem("CONTEXT/Text/-> Text")]
        static void Restore(MenuCommand command)
        {
            if (command == null || command.context == null || !(command.context is UnityEngine.Component))
                return;

            Restore<Text, DynamicText>(((UnityEngine.Component)command.context).gameObject);
        }

        [MenuItem("CONTEXT/Text/-> DynamicText", true)]
        static bool ConvertValidate(MenuCommand command)
        {
            return command != null &&
                command.context != null &&
                typeof(UnityEngine.UI.Text).Equals(command.context.GetType());
        }
        [MenuItem("CONTEXT/Text/-> Text", true)]
        static bool RestoreValidate(MenuCommand command)
        {
            return command != null &&
                command.context != null &&
                typeof(UnityEngine.UI.DynamicText).Equals(command.context.GetType());
        }

        static void Convert<TSource, TTarget>(GameObject go)
            where TSource : UnityEngine.Component
            where TTarget : TSource
        {
            var source = go.GetComponent<TSource>();
            if (source == null)
            {
                return;
            }
            var values = GetMembers<TSource>(source);
            GameObject.DestroyImmediate(source);
            var target = go.AddComponent<TTarget>();
            SetMembers<TSource>(target, values);
            EditorUtility.SetDirty(go);
        }
        static void Restore<TSource, TTarget>(GameObject go)
            where TSource : UnityEngine.Component
            where TTarget : TSource
        {
            var target = go.GetComponent<TTarget>();
            if (target == null)
            {
                return;
            }

            var values = GetMembers<TSource>(target);
            GameObject.DestroyImmediate(target);
            var source = go.AddComponent<TSource>();
            SetMembers<TSource>(source, values);
            EditorUtility.SetDirty(go);
        }

        static Dictionary<MemberInfo, object> GetMembers<T>(T obj)
        {
            var members = new Dictionary<MemberInfo, object>();
            foreach (var field in typeof(T).GetFields(Flags))
            {
                members.Add(field, field.GetValue(obj));
            }
            foreach (var property in typeof(T).GetProperties(Flags))
            {
                if (!property.CanRead || !property.CanWrite)
                    continue;
                members.Add(property, property.GetValue(obj));
            }
            return members;
        }
        static void SetMembers<T>(T obj, Dictionary<MemberInfo, object> values)
        {
            foreach (var value in values)
            {
                if (value.Key is FieldInfo)
                {
                    ((FieldInfo)value.Key).SetValue(obj, value.Value);
                }
                else if (value.Key is PropertyInfo)
                {
                    ((PropertyInfo)value.Key).SetValue(obj, value.Value);
                }
            }
        }
    }
}
