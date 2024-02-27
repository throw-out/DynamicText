using System;
using System.Collections.Generic;

namespace UnityEngine.UI
{
    public partial class DynamicText
    {
        internal class ObjectPool<T>
            where T : Node
        {
            //事件接口
            public event Func<T> CreateEvent;
            public event Action<T> GetEvent;
            public event Action<T> ReleaseEvent;
            //计数器
            public int Count { get { return queue.Count; } }

            //节点池
            private readonly Stack<T> queue = new Stack<T>();

            public T Get()
            {
                T item = default(T);
                while (queue.Count > 0 && (item == null || item.IsDestroy()))
                {
                    item = queue.Pop();
                }
                //创建节点/创建事件
                if (item == null || item.IsDestroy())
                {
                    item = CreateEvent();
                }
                //Get事件
                GetEvent?.Invoke(item);

                return item;
            }
            public void Release(T item)
            {
                if (item == null || item.IsDestroy())
                    return;

                ReleaseEvent?.Invoke(item);
                if (!queue.Contains(item))
                {
                    queue.Push(item);
                }
            }
            public void ReleaseAll(IEnumerable<T> lst)
            {
                foreach (var item in lst)
                {
                    Release(item);
                }
            }

            public void Clear()
            {
                queue.Clear();
            }
        }
    }
}