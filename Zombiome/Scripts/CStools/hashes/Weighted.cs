using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace CSutils {

public class Weighted<T> {
    /* Let's leave with 0(N) for small N as long as it's only at ZBEffect.configure time
    */

    private static Random Random = new Random();
    public static Dictionary<K,V> Dico<K,V>(params object[] args) {
        /* I need ordered dict not to break reproducibility */
        Dictionary<K,V> dico = new Dictionary<K,V>();
        for (int n=0; n<args.Length; n=n+2) dico[(K) args[n]] = (V) args[n+1];
        return dico;
    }
    public static List<KeyValuePair<K,V>> ODico<K,V>(params object[] args) {
        // Dictionary<K,V> dico = Dico<K,V>(args);
        /* I need ordered dict not to break reproducibility */
        List<KeyValuePair<K,V>> lst = new List<KeyValuePair<K,V>>();
        // Console.WriteLine(String.Format("{0}{1}",args.Length, args.Length/2));
        for (int n=0; n<(int) args.Length/2; n++) {
            // Console.WriteLine(String.Format("{0}{1}{2}", n, 2*n, 2*n+1));
            lst.Add(new KeyValuePair<K,V>((K) args[2*n], (V) args[2*n+1]));
        }
        return lst;
    }

    private T[] keys;
    private float[] probs;

    public static Weighted<T> Of(params object[] args) {
        // return new Weighted<T>(Dico<T,float>(args));
        return new Weighted<T>(ODico<T,float>(args));
    }
    // public Weighted(Dictionary<T,float> Pairs) {
    public Weighted(ICollection<KeyValuePair<T, float>> Pairs) {
        float sum = 0f;
        foreach(KeyValuePair<T,float> pair in Pairs) sum = sum + pair.Value;
        int index=-1;
        float cum = 0f;
        keys = new T[Pairs.Count];
        probs = new float[Pairs.Count];
        foreach(KeyValuePair<T,float> pair in Pairs) {
            index++;
            cum = cum + pair.Value;
            keys[index] = pair.Key;
            probs[index] = cum / sum;
        }
        Console.WriteLine(String.Format("{0}{1}", keys[0], keys[1]));
        Console.WriteLine(String.Format("{0}{1}", probs[0], probs[1]));
    }
    public T Gen(float u) {
        for (int k=0; k<keys.Length; k++) if (u <= probs[k]) return keys[k];
        return keys[0];
    }
     public T Gen() {return Gen((float) Random.NextDouble());}


    public static void _Main(params string[] args) {
        Weighted<string> W = Weighted<string>.Of("A", 0.1f, "B",0.3f, "C",0.6f);
        Console.WriteLine(W.Gen(0.09f));
        Console.WriteLine(W.Gen(0.31f));
        Console.WriteLine(W.Gen(0.41f));
        Console.WriteLine(W.Gen(0.99f));
        for (int k=0; k< 30; k++) Console.WriteLine(W.Gen());
    }

}

} // END namespace