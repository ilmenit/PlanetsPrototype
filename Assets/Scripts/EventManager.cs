using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// cases to cover:
// 1. event starts and queue goes without waiting for finish
// 2. queue is waiting for finish of specific event
// 3. specific events can be completed on click

public enum EventState
{
    NotStarted,
    Executing,
    Finished
}

public abstract class GameEvent
{
    [HideInInspector]
    public EventState State = EventState.NotStarted;

    [HideInInspector]
    public int m_id;

    [HideInInspector]
    public bool FinishOnExecute = true;

#if UNITY_EDITOR
    public string StackTrace;
    public string CalledFrom; // shows GameEvent in which it was created using new()
#endif

    public GameEvent()
    {
#if UNITY_EDITOR
        StackTrace = UnityEngine.StackTraceUtility.ExtractStackTrace();
        List<string> StackList = StackTrace.Split(new[] { "\n" }, System.StringSplitOptions.None).ToList();
        if (StackList.Count > 2)
            CalledFrom = StackList[2];
#endif
    }

    public abstract void Execute();

    // this calls Execut of child class but assures that Finish is called too!
    public virtual void ExecuteAndFinish()
    {
        //Debug.Log("---> Executing " + this.GetType().Name);
        Execute();
        if (FinishOnExecute)
            Finish();
    }

    /*
    public virtual IEnumerator ExecuteAsync()
    {
        yield return null;
    }
    */

    public virtual void Finish()
    {
        State = EventState.Finished;
    }
}

public class EventManager : MonoSingleton<EventManager>
{
    protected EventManager()
    {
    } // guarantee this will be always a singleton only - can't use the constructor!

    private int m_lastID = 0;
    private LinkedListNode<GameEvent> m_currentNode;
    private LinkedList<GameEvent> m_eventList = new LinkedList<GameEvent>();

    public delegate void EventDelegate<T>(T e) where T : GameEvent;

    private delegate void EventDelegate(GameEvent e);

    private Dictionary<System.Type, EventDelegate> delegates = new Dictionary<System.Type, EventDelegate>();
    private Dictionary<System.Delegate, EventDelegate> delegateLookup = new Dictionary<System.Delegate, EventDelegate>();
    private Dictionary<System.Delegate, System.Delegate> onceLookups = new Dictionary<System.Delegate, System.Delegate>();

    private EventDelegate AddDelegate<T>(EventDelegate<T> del) where T : GameEvent
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

    public void AddListener<T>(EventDelegate<T> del) where T : GameEvent
    {
        //        UnityEngine.Debug.Log("AddListener " + typeof(T).ToString() + " : " + del.Method.ToString());
        AddDelegate<T>(del);
    }

    public void AddListenerOnce<T>(EventDelegate<T> del) where T : GameEvent
    {
        //        UnityEngine.Debug.Log("AddListenerOnce " + typeof(T).ToString() + " : " + del.Method.ToString());
        EventDelegate result = AddDelegate<T>(del);

        if (result != null)
        {
            // remember this is only called once
            onceLookups[result] = del;
        }
    }

    public void RemoveListener<T>(EventDelegate<T> del) where T : GameEvent
    {
        //       UnityEngine.Debug.Log("RemoveListener " + typeof(T).ToString() + " : " + del.Method.ToString());

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

    public void RemoveAll()
    {
        delegates.Clear();
        delegateLookup.Clear();
        onceLookups.Clear();
    }

    public bool HasListener<T>(EventDelegate<T> del) where T : GameEvent
    {
        return delegateLookup.ContainsKey(del);
    }

    public void TriggerEvent(GameEvent e)
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
        if (e.State != EventState.Finished)
            e.ExecuteAndFinish();
    }

    /// <summary>
    /// Joined event is executed at the same time, without waiting for the previous one to finish
    /// </summary>
    public void Join(GameEvent evt)
    {
        evt.m_id = m_lastID;
        m_eventList.AddLast(evt);
    }

    /// <summary>
    /// Local event is added after the currently processed event, not at the end of the queue!
    /// </summary>
    /// <param name="evt"></param>
    public void JoinCurrent(GameEvent evt)
    {
        if (m_eventList.Count == 0 || m_currentNode == null)
        {
            m_currentNode = m_eventList.First;
            if (m_currentNode == null)
            {
                m_eventList.AddLast(evt);
                return;
            }
        }
        evt.m_id = m_currentNode.Value.m_id;
        var previousNode = m_currentNode;
        var nextNode = previousNode.Next;
        // add after the last with the current ID
        while (nextNode != null && nextNode.Value.m_id == evt.m_id)
        {
            previousNode = nextNode;
            nextNode = previousNode.Next;
        }
        m_eventList.AddAfter(previousNode, evt);
    }

    public void Append(GameEvent evt)
    {
        m_lastID++;
        Join(evt);
    }

    public void AppendIfEmpty(GameEvent evt)
    {
        if (m_eventList.Count == 0)
            Append(evt);
    }

    /// <summary>
    /// If the previous was join then adds by Append and all next FastAppend as join
    /// </summary>
    public void GroupAppend(GameEvent evt)
    {
        JoinCurrent(evt);
    }

    private void ProcessEventList()
    {
        int currentID = m_lastID + 1;
        // start new node
        m_currentNode = m_eventList.First;
        while (m_currentNode != null)
        {
            var next = m_currentNode.Next;
            GameEvent evt = m_currentNode.Value;
            if (evt.m_id > currentID)
                break;
            switch (evt.State)
            {
                case EventState.Executing:
                    currentID = evt.m_id;
                    break;

                case EventState.NotStarted:
                    evt.State = EventState.Executing;
                    currentID = evt.m_id;
                    TriggerEvent(evt);
                    break;
            }
            m_currentNode = next;
        }
        // remove finished events
        m_currentNode = m_eventList.First;
        while (m_currentNode != null)
        {
            var next = m_currentNode.Next;
            if (m_currentNode.Value.State == EventState.NotStarted)
                break;
            if (m_currentNode.Value.State == EventState.Finished)
            {
                m_eventList.Remove(m_currentNode);
                if (m_eventList.Count == 0)
                {
                    m_lastID = 0; // revert lastID - we have no more events in queue
                    break; // stop removing
                }
            }
            m_currentNode = next;
        }
    }

    //Every update cycle the queue is processed, if the queue processing is limited,
    //a maximum processing time per update can be set after which the events will have
    //to be processed next update loop.
    private void Update()
    {
        if (m_eventList.Count == 0)
            return;

        ProcessEventList();
    }

    public void OnApplicationQuit()
    {
        RemoveAll();
        m_eventList.Clear();
    }
}