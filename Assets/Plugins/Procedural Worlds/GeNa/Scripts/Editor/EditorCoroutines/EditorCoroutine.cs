using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
namespace GeNa.Core
{
    public class EditorCoroutine
    {
        private struct YieldProcessor
        {
            private enum DataType : byte
            {
                None = 0,
                WaitForSeconds = 1,
                EditorCoroutine = 2,
                AsyncOP = 3,
            }
            private struct ProcessorData
            {
                public DataType type;
                public double targetTime;
                public object current;
            }
            private ProcessorData data;
            public void Set(object yield)
            {
                if (yield == data.current)
                    return;
                Type type = yield.GetType();
                DataType dataType = DataType.None;
                double targetTime = -1;
                if (type == typeof(EditorCoroutine))
                    dataType = DataType.EditorCoroutine;
                else if (type == typeof(AsyncOperation) || type.IsSubclassOf(typeof(AsyncOperation)))
                    dataType = DataType.AsyncOP;
                data = new ProcessorData {current = yield, targetTime = targetTime, type = dataType};
            }
            public bool MoveNext(IEnumerator enumerator)
            {
                bool advance = false;
                switch (data.type)
                {
                    case DataType.WaitForSeconds:
                        advance = data.targetTime <= EditorApplication.timeSinceStartup;
                        break;
                    case DataType.EditorCoroutine:
                        advance = (data.current as EditorCoroutine).m_IsDone;
                        break;
                    case DataType.AsyncOP:
                        advance = (data.current as AsyncOperation).isDone;
                        break;
                    default:
                        advance = data.current == enumerator.Current; //a IEnumerator or a plain object was passed to the implementation
                        break;
                }
                if (advance)
                {
                    data = default;
                    return enumerator.MoveNext();
                }
                return true;
            }
        }
        internal WeakReference m_Owner;
        private IEnumerator m_Routine;
        private YieldProcessor m_Processor;
        private bool m_IsDone;
        internal EditorCoroutine(IEnumerator routine)
        {
            m_Owner = null;
            m_Routine = routine;
            EditorApplication.update += MoveNext;
        }
        internal EditorCoroutine(IEnumerator routine, object owner)
        {
            m_Processor = new YieldProcessor();
            m_Owner = new WeakReference(owner);
            m_Routine = routine;
            EditorApplication.update += MoveNext;
        }
        internal void MoveNext()
        {
            if (m_Owner != null && !m_Owner.IsAlive)
            {
                EditorApplication.update -= MoveNext;
                return;
            }
            bool done = ProcessIEnumeratorRecursive(m_Routine);
            m_IsDone = !done;
            if (m_IsDone)
                EditorApplication.update -= MoveNext;
        }
        private static Stack<IEnumerator> kIEnumeratorProcessingStack = new Stack<IEnumerator>(32);
        private bool ProcessIEnumeratorRecursive(IEnumerator enumerator)
        {
            IEnumerator root = enumerator;
            while (enumerator.Current is IEnumerator current)
            {
                kIEnumeratorProcessingStack.Push(enumerator);
                enumerator = current;
            }

            //process leaf
            m_Processor.Set(enumerator.Current);
            bool result = m_Processor.MoveNext(enumerator);
            while (kIEnumeratorProcessingStack.Count > 1)
            {
                if (!result)
                    result = kIEnumeratorProcessingStack.Pop().MoveNext();
                else
                    kIEnumeratorProcessingStack.Clear();
            }
            if (kIEnumeratorProcessingStack.Count > 0 && !result && root == kIEnumeratorProcessingStack.Pop())
                result = root.MoveNext();
            return result;
        }
        internal void Stop()
        {
            m_Owner = null;
            m_Routine = null;
            EditorApplication.update -= MoveNext;
        }
    }
}