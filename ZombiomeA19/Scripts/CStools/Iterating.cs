using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;


namespace Iterating {

public class Iter {
    /** Allows foreach on an IEnumerator */
    //public static Iter<T> On(IEnumerator<T> enumerator) {return new Iter<T>(enumerator);}
    public static IEnumerable<T> On<T>(IEnumerator<T> enumerator) {return new IterWrap<T>(enumerator);}

    public static bool EverySeconds<T>(ref long last, float dt, Action<T> action, T arg) {
        // -> did execute
        // TODO: if last = 0, randomize first call ?
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (last + (long) (1000*dt) >= now) return false;
        last = now;
        action(arg);
        return true;
    }
    public static bool EverySeconds(ref long last, float dt, Action action) {
        // TODO: if last = 0, randomize first call ?
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Printer.Log(15, "EverySeconds ?", last, " + ", dt, "<", now, "?");
        if (last + (long) (1000*dt) >= now) return false;
        last = now;
        action();
        return true;
    }
}

class IterWrap<T> : IEnumerable<T> {
    /** Allows foreach on an IEnumerator */
    private IEnumerator<T> enumerator;
    public IterWrap(IEnumerator<T> enumerator) {this.enumerator = enumerator;}
    IEnumerator IEnumerable.GetEnumerator() {return enumerator;}
    public IEnumerator<T> GetEnumerator() {return enumerator;}
}

public class Repeater {
    public static WaitForEndOfFrame Frame = new WaitForEndOfFrame();
    public float dt = -1; // use at least the recall time Zone-> Effect, in seconds
    public int n; // Only limits external loop
    public YieldInstruction Yield = null;

    public static YieldInstruction Wait(float dt) {
        if (dt <= 0) return Frame;
        return new WaitForSeconds(dt);
    }
    public Repeater(int n) {
        this.n = n;
        dt = Zombiome.FrequencyManager;
    }
    public Repeater Set(int n) {this.n = n; return this;}
    public Repeater Set(float dt) {this.Yield = Wait(dt); return this;}
    public Repeater Set(int n, float dt) {this.n = n; this.Yield = Wait(dt); return this;}
    public IEnumerable<int> Over() {
        return Enumerable.Range(0, n);
    }
     public override String ToString() {
        return String.Format("Repeater({0}*{1})", n, Yield);
    }
}



} // END namespace Iterating 

