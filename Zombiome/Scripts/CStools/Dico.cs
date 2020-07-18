//using System.Xml;
using System;
using System.Collections;
using UnityEngine; // DEBUG
using System.Collections.Generic;

//using Harmony;


//using System.Linq;
//using System.Reflection.Emit;

//;

namespace CSutils {

public class Dico<K,V> : Dictionary<K,V> {
    /* Prints KeyError */

    public Dico() : base() {}

    public new V this[K index] {     // Indexer declaration
        get {
            try {return base[index];}
            catch(KeyNotFoundException e) {
                Debug.Log(String.Format("KeyNotFoundException: {0} ", index));
                throw;
            }
        }
        set {
            base[index] = value;
        }
    }
}

public class ForgettingDict<K,V> {
    /* Sorted by insertion-order and allowed to forget */
    private static System.Random rand = new System.Random();
    private List<K> keys = new List<K>();
    private Dictionary<K,V> items = new Dictionary<K,V>();
    private Func<K,V,bool> CanForget;
    private int _indexFirst = -1;
    private int _maxFirst = 10;
    public ForgettingDict(Func<K,V,bool> canForget) {
        this.CanForget = canForget;
    }

    public bool ContainsKey(K key) { return items.ContainsKey(key); }
    public V this[K key] {     // Indexer declaration
        get {
            try {return items[key];}
            catch(KeyNotFoundException e) {
                Printer.Print("KeyNotFoundException: ", key, e);
                throw;
            }
        }
        set {
            if (! items.ContainsKey(key)) keys.Add(key);
            items[key] = value;
        }
    } 
    public int Count {
        get {return items.Count;}
    }
    public void Clear() {
        items.Clear();
        keys.Clear();
    }
    public bool Forget(List<KeyValuePair<K,V>> popped = null) {
        int len = items.Count;
        if (len == 0) return false;
        // Try oldest inserted
        _indexFirst = _indexFirst + 1;
        _indexFirst = (_indexFirst % _maxFirst) % len;
        int idx = _indexFirst;
        K key = keys[idx];
        V val = items[key];
        if (CanForget(key, val)) {
            Printer.Log(10, "Forgetting ", idx, key, val); 
            if (popped != null) popped.Add(new KeyValuePair<K, V>(key, val));
            this.Pop(idx, key);
            return true;
        }
        // Try random
        if (len == 1) return false;
        idx = rand.Next(1, len);
        key = keys[idx];
        val = items[key];
        if (CanForget(key, val)) {
            Printer.Log(10, "Forgetting ", idx, key, val); 
            if (popped != null) popped.Add(new KeyValuePair<K, V>(key, val));
            this.Pop(idx, key);
            return true;
        }
        return false;
    }
    private void Pop(int index, K key) {
        // dont expose Pop(key k), it would be inefficent (find index)
        keys.RemoveAt(index);
        items.Remove(key);
    }
}


////////////////////
} // End Namespace
////////////////////