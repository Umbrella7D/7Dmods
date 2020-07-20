using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace CSutils {

public class Routines {
    /* A stoppable group of co-routines

    TODO: start another group 
    */
    public static YieldInstruction WaitFrame = new WaitForEndOfFrame();
    public static IEnumerator Merge(params object[] enums) {
        /* Accepts IEnumerator and YieldInstruction
        TODO: accept and call Action<> ? -> Use Call wrapper below instead
        */
        foreach(object obj in enums) yield return obj;
    }
    // Should we wrap Call ?
    public static IEnumerator Call(Action action, float delay=-1f) {
        if (delay >=0) yield return new WaitForSeconds(delay);
        action();
        yield break;
    }
    public static IEnumerator Call<T>(Action<T> action, T arg, float delay=-1f) {
        if (delay >=0) yield return new WaitForSeconds(delay);
        action(arg);
        yield break;
    }
    public static IEnumerator Call<T1,T2>(Action<T1,T2> action, T1 a1, T2 a2, float delay=-1f) {
        if (delay >=0) yield return new WaitForSeconds(delay);
        action(a1, a2);
        yield break;
    }
    public static IEnumerator Call<T1,T2,T3>(Action<T1,T2,T3> action, T1 a1, T2 a2, T3 a3, float delay=-1f) {
        if (delay >=0) yield return new WaitForSeconds(delay);
        action(a1, a2, a3);
        yield break;
    }

    public static IEnumerator IfNotRunning(bool[] Lock, IEnumerator iter) {
        /* Would be more efficient to skip starting ! */
        if (Lock[0]) yield break;
        Lock[0] = true;
        yield return iter;
        Lock[0] = false;
    }

    private Dictionary<long,Coroutine> Group = new Dictionary<long,Coroutine>();
    private long nworker = 0;
    private bool stopped = false;
    public Routines() {}
    private long NewId() {
        long index = nworker;
        nworker = nworker + 1;
        return index;
    }
    private IEnumerator fake() {
        yield break;
    }
    public Coroutine Start(IEnumerator routine, string name="", params object[] args) {
        if (stopped) routine = fake();
        long index = NewId();
        routine = AutoRemoved(index, routine);
        string wname = (name=="") ? "Routine" : name;
        wname = String.Format(name, args);
        routine = new Catcher(String.Format("[{0}{1}]", wname, index), routine);
        Coroutine started = GameManager.Instance.StartCoroutine(routine);
        Group.Add(index, started);
        Printer.Log(10, "Routine started", index, started);
        return started;
    }
    public Coroutine Start(params object[] enums) {
        return Start(Merge(enums));
    }

    public class _Named {
        public Routines Base;
        public string name;
        public void Start(params object[] enums) {
            Base.Start(Merge(enums), name);
        }
    }
    public _Named Named(string name) {
        _Named named = new _Named();
        named.name = name; named.Base = this;
        return named;
    }

    public void Stop(bool forever=false) {
        if (forever) stopped = true;
        foreach (Coroutine started in Group.Values) GameManager.Instance.StopCoroutine(started);
        Group.Clear();
    }

    public Coroutine Stop(long index) {
        if (Group.ContainsKey(index)) {
            Coroutine started = Group[index];
            Group.Remove(index);
            GameManager.Instance.StopCoroutine(started);
            return started;
        }
        return null;
    }
    public void Start() {stopped = false;}

    private IEnumerator AutoRemoved(long index, IEnumerator routine) {
        yield return routine;
        Group.Remove(index);
    }

}

class Catcher : IEnumerator {
    /** Basically a debug tool to print stacktrace on error

    Also allows to swallow exceptions. When Harmony is not available, this is the only way to 
    avoid a lot of NPE when we log out of a game, so let's use this not just for debug.
    And then, better to prevent them (check GameManager/World) than to catch em

    There is a performance penalty because
    - MoveNext() is overriden 
    - Every yielded IEnumerator is wrapped
    */
    private IEnumerator wrapped;
    private string name;
    private object _Current;
    private bool recursive = true;
    public Func<Exception,bool> OnError = null; // true <=> reraise


    public static bool SwallowErrors = false;

    public static bool ReRaise(Exception e) {

        return true;
    }
    public Catcher(string name, IEnumerator routine, bool recursive=true) {
        this.name = name;
        this.wrapped = routine;
        _Current = null;
    }
    public object Current {
        get {return _Current;}
    }
    public void Reset() {wrapped.Reset();}

    public bool MoveNext() {
        bool res = false;
        try {res = wrapped.MoveNext();}
        catch(Exception ex) {
            if (GameManager.Instance == null) Printer.Print("Routines Catcher with null GameManager");
            if (GameManager.Instance.World == null) Printer.Print("Routines Catcher with null World");
            else {
                if (SwallowErrors) {
                    Printer.WriteError(name, ex);
                }
                else {
                    Printer.WriteError(name, ex);
                    Printer.Print("ERROR in Routine:", name);
                    Printer.Print(ex); // prints the stack trace, even better than adding actual names ?
                    throw ex;
                }
            }
        }
        // TODO: wrap Current into Catcher - except if already Catcher
        _Current = wrapped.Current;
        if (recursive) {
            if (_Current is IEnumerator) {
                IEnumerator _sub = _Current as IEnumerator;
                if (!(_Current is Catcher) ) {
                    _Current = new Catcher(String.Format("{0}{1}", name, "+sub"), _sub);
                }
            }
        }
        return res;
    }

}

} // END namespace