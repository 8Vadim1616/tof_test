/*
* Copyright 2017 Ben D'Angelo
*
* MIT License
*
* Permission is hereby granted, free of charge, to any person obtaining a copy of this
* software and associated documentation files (the "Software"), to deal in the Software
* without restriction, including without limitation the rights to use, copy, modify, merge,
* publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons
* to whom the Software is furnished to do so, subject to the following conditions:
*
* The above copyright notice and this permission notice shall be included in all copies or
* substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
* INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
* PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
* FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR
* OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
* DEALINGS IN THE SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.Utils;
using UnityEngine;

namespace Assets.Scripts.Events
{
    public class EventController : Singleton<EventController>
    {
        public static bool LogNoListeners = false;

        public bool LimitQueueProcesing = false;
        public float QueueProcessTime = 0.0f;
        private static EventController s_Instance = null;
        private Queue m_eventQueue = new Queue();

        public delegate void EventDelegate<T>(T e) where T : Event;
        private delegate void EventDelegate(Event e);

        private Dictionary<System.Type, EventDelegate> delegates = new Dictionary<System.Type, EventDelegate>();
        private Dictionary<System.Delegate, EventDelegate> delegateLookup = new Dictionary<System.Delegate, EventDelegate>();
        private Dictionary<System.Delegate, System.Delegate> onceLookups = new Dictionary<System.Delegate, System.Delegate>();

        private EventDelegate AddDelegate<T>(EventDelegate<T> del) where T : Event
        {
            // Early-out if we've already registered this delegate
            if (delegateLookup.ContainsKey(del))
                return null;

            // Create a new non-generic delegate which calls our generic one.
            // This is the delegate we actually invoke.
            EventDelegate internalDelegate = (e) => del((T)e);
            delegateLookup[del] = internalDelegate;

            EventDelegate tempDel;
            if (delegates.TryGetValue(typeof(T), out tempDel))
            {
                delegates[typeof(T)] = tempDel += internalDelegate;
            }
            else
            {
                delegates[typeof(T)] = internalDelegate;
            }

            return internalDelegate;
        }

        public void addListener<T>(EventDelegate<T> del) where T : Event
        {
            AddDelegate(del);
        }

        public void addListenerOnce<T>(EventDelegate<T> del) where T : Event
        {
            EventDelegate result = AddDelegate<T>(del);

            if (result != null)
            {
                // remember this is only called once
                onceLookups[result] = del;
            }
        }

        public void removeListener<T>(EventDelegate<T> del) where T : Event
        {
            EventDelegate internalDelegate;
            if (delegateLookup.TryGetValue(del, out internalDelegate))
            {
                EventDelegate tempDel;
                if (delegates.TryGetValue(typeof(T), out tempDel))
                {
                    tempDel -= internalDelegate;
                    if (tempDel == null)
                    {
                        delegates.Remove(typeof(T));
                    }
                    else
                    {
                        delegates[typeof(T)] = tempDel;
                    }
                }

                delegateLookup.Remove(del);
            }
        }

        public void removeAll()
        {
            delegates.Clear();
            delegateLookup.Clear();
            onceLookups.Clear();
        }

        public bool hasListener<T>(EventDelegate<T> del) where T : Event
        {
            return delegateLookup.ContainsKey(del);
        }

        public void triggerEvent<T>(T e) where T : Event
        {
            EventDelegate del;
            if (delegates.TryGetValue(e.GetType(), out del))
            {
                del.Invoke(e);

                // remove listeners which should only be called once
                foreach (EventDelegate k in delegates[e.GetType()].GetInvocationList())
                {
                    if (onceLookups.ContainsKey(k))
                    {
                        delegates[e.GetType()] -= k;

                        if (delegates[e.GetType()] == null)
                        {
                            delegates.Remove(e.GetType());
                        }

                        delegateLookup.Remove(onceLookups[k]);
                        onceLookups.Remove(k);
                    }
                }
            }
            else
            {
                if (LogNoListeners)
                    GameLogger.warning("Event: " + e.GetType() + " has no listeners");
            }
        }

        //Inserts the event into the current queue.
        public bool QueueEvent<T>(T evt) where T : Event
        {
            if (!delegates.ContainsKey(evt.GetType()))
            {
                if (LogNoListeners)
                    GameLogger.warning("EventManager: QueueEvent failed due to no listeners for event: " + evt.GetType());
                return false;
            }

            m_eventQueue.Enqueue(evt);
            return true;
        }

        //Every update cycle the queue is processed, if the queue processing is limited,
        //a maximum processing time per update can be set after which the events will have
        //to be processed next update loop.
        void Update()
        {
            float timer = 0.0f;
            while (m_eventQueue.Count > 0)
            {
                if (LimitQueueProcesing)
                {
                    if (timer > QueueProcessTime)
                        return;
                }

                Event evt = m_eventQueue.Dequeue() as Event;
                TriggerEvent(evt);

                if (LimitQueueProcesing)
                    timer += Time.deltaTime;
            }
        }

        public void OnApplicationQuit()
        {
            RemoveAll();
            m_eventQueue.Clear();
            s_Instance = null;
        }

        public static void AddListener<T>(EventDelegate<T> del) where T : Event
        {
            Instance.addListener(del);
        }

        public static void AddListenerOnce<T>(EventDelegate<T> del) where T : Event
        {
            Instance.addListenerOnce(del);
        }

        public static void TriggerEvent<T>(T e) where T : Event
        {
            Instance.triggerEvent(e);
        }

        public static void RemoveListener<T>(EventDelegate<T> del) where T : Event
        {
            Instance.removeListener(del);
        }

        public static void RemoveAll()
        {
            Instance.removeAll();
        }

        public static bool HasListener<T>(EventDelegate<T> del) where T : Event
        {
            return Instance.hasListener(del);
        }
    }
}