using System;
using System.Collections;
// using System.Collections.ObjectModel;
// using UnityEngine;
using System.Collections.Generic;
using System.Linq; // https://stackoverflow.com/questions/32997696/why-dictionary-does-not-contain-a-definition-for-elementat

namespace SdtdUtils {

public static class Cycling {
    // private static GameRandom rand = new GameRandom();
    private static Random rand = new Random();

    // -2 fixme for idx is changed
    /** Next value in a container. Robust to container changes, yet tries to cycle.
    Also circumvent "Collection was modified; enumeration operation may not execute". */
    public static T Next<T>(T[] col, ref int index) {
        index = (index == -2) ? rand.Next(col.Length) : (index+1) % col.Length;
        return col[index]; // what if empty ?
    }
    public static T Next<T>(List<T> col, ref int index) { // Collection
        index = (index == -2) ? rand.Next(col.Count) : (index+1) % col.Count;
        return col[index]; // what if empty ?
    }
    public static KeyValuePair<K,V> Next<K,V>(System.Collections.Generic.Dictionary<K,V> col, ref int index) {
        /** ElementAt from IEnumerable will create enumerator and advance it : very inefficient */
        index = (index == -2) ? rand.Next(col.Count) : (index+1) % col.Count;
        return col.ElementAt(0); // if it does what I think, this is very slow !!!!
        // return Enumerable.ElementAt(col, 0); // does not require linq
    }

    public static KeyValuePair<K,V> Pop<K,V>(Dictionary<K,V> dict) {
        IEnumerator<KeyValuePair<K,V>> denum = dict.GetEnumerator();
        denum.MoveNext();
        KeyValuePair<K,V> popped = denum.Current;
        dict.Remove(popped.Key);
        return popped;
    }

    // public class DQueue<K,V> {
    //     private Dictionary<K,V> dict = new Dictionary<K,V>();
    //     private Queue<K> queue = new Queue<K>();

    //     public V this[K key] {     // Indexer declaration
    //         get {
    //             try {return dict[key];}
    //             catch(KeyNotFoundException e) {
    //                 Printer.Print("KeyNotFoundException: ", key, e);
    //                 throw;
    //             }
    //         }
    //         set {
    //             if (! dict.ContainsKey(key)) queue.Enqueue(key);
    //             dict[key] = value;
    //         }
    //     }

    //     public KeyValuePair<K,V> Pop() {
    //         // Do we need pop from dict ?
    //         K key = queue.Dequeue();
    //         return new KeyValuePair<K,V>(key, dict[key]);
    //     }
    // }

    // }

    public static void Main(string[] args) {
        Dictionary<int,int> d = new Dictionary<int,int>(); 
        for (int k=0; k<10; k++) d[k] = k;
        for (int k=0; k<10; k++) Console.WriteLine(String.Format("{0}:{1} left={2}", k, Pop(d), d.Count));
    }
}

} // END namespace