using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class Loom : MonoBehaviour
{
    public int maxThreads = 1;
    private int numThreads;

    private bool m_HasLoaded = false;

    private List<Action> _actions = new List<Action>();
    private List<Action> _currentActions = new List<Action>();
    private List<CoroutineItem> _coroutines = new List<CoroutineItem>();
    private List<CoroutineItem> _currentCoroutines = new List<CoroutineItem>();

    public class ValueWrapper<T> where T : struct
    {
        public T Value { get; set; }

        public ValueWrapper(T value)
        {
            this.Value = value;
        }
    }

    public struct CoroutineItem
    {
        public CoroutineItem(IEnumerator a_coroutine, ValueWrapper<bool> a_finished)
        {
            this.coroutine = a_coroutine;
            this.finished = a_finished;
        }

        public IEnumerator coroutine;
        public ValueWrapper<bool> finished;
    }

    private static Loom _instance;

    public static Loom Instance
    {
        get
        {
            if (_instance == null) _instance = GameObject.FindObjectOfType<Loom>();
            if (_instance == null) _instance = new GameObject("Loom").AddComponent<Loom>();
            return _instance;
        }
    }

    protected virtual void Start()
    {
        m_HasLoaded = true;
        DontDestroyOnLoad(gameObject);
    }

    public void Clear()
    {
        lock (_actions) { _actions.Clear(); }
        lock (_coroutines) { _coroutines.Clear(); }
    }

    public void QueueOnMainThread(Action action)
    {
        lock (_actions)
        {
            _actions.Add(action);
        }
    }

    public void QueueCoroutine(CoroutineItem coroutineItem)
    {
        lock (_coroutines)
        {
            _coroutines.Add(coroutineItem);
        }
    }

    public Thread RunAsync(Action a)
    {
        while (numThreads >= maxThreads) Thread.Sleep(1);

        //if (numThreads >= maxThreads)
        //    return null;

        Interlocked.Increment(ref numThreads);
        ThreadPool.QueueUserWorkItem(RunAction, a);

        return null;
    }

    private void RunAction(object action)
    {
        try
        {
            ((Action)action)();
        }
        catch (System.Threading.ThreadAbortException)
        {
            // Ignore this Exception as we are using multi-threading
        }
        catch (Exception ex)
        {
            Log.Error("[EXCEPTION] " + ex.ToString());
            throw;
        }
        finally
        {
            Interlocked.Decrement(ref numThreads);
        }
    }

    protected virtual void Update()
    {
        if (m_HasLoaded == false)
            Start();

        HandleActions();
        HandleCoroutines();
    }

    private void HandleActions()
    {
        lock (_actions)
        {
            _currentActions.Clear();
            _currentActions.AddRange(_actions);
            _actions.Clear();
        }
        foreach (var a in _currentActions)
        {
            a();
        }
    }

    private void HandleCoroutines()
    {
        lock (_coroutines)
        {
            _currentCoroutines.Clear();
            _currentCoroutines.AddRange(_coroutines);
            _coroutines.Clear();
        }
        foreach (var coroutineItem in _currentCoroutines)
        {
            StartCoroutine(StartCoroutineHelper(coroutineItem.coroutine, coroutineItem.finished));
        }
    }

    public IEnumerator StartCoroutineHelper(IEnumerator coroutine, ValueWrapper<bool> _waitUntilTrue)
    {
        yield return StartCoroutine(coroutine);
        _waitUntilTrue.Value = true;
    }

    public void CallCoroutineOnMainThread(IEnumerator coroutine)
    {
        ValueWrapper<bool> _waitUntilTrue = new ValueWrapper<bool>(false);
        var coroutineItem = new CoroutineItem(coroutine, _waitUntilTrue);
        Loom.Instance.QueueCoroutine(coroutineItem);
        SpinWait.SpinUntil(() => _waitUntilTrue.Value);
    }

    public void Call<T1>(Action<T1> a, T1 t1)
    {
        a.Invoke(t1);
    }

    public void Call<T1, T2>(Action<T1, T2> a, T1 t1, T2 t2)
    {
        a.Invoke(t1, t2);
    }

    public void Call<T1, T2, T3>(Action<T1, T2, T3> a, T1 t1, T2 t2, T3 t3)
    {
        a.Invoke(t1, t2, t3);
    }

    public T1 Return<T1>(Func<T1> method)
    {
        T1 retVal = method();
        return retVal;
    }
}