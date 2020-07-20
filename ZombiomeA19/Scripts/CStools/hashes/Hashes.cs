
using System;
using System.Linq;

namespace CSutils {


public static class Hashes {
    /** Reproducible sources


    **/
    public class Objects {
        /* Does not work very well, but I can live with strings */
        public static int ShiftAndWrap(int value, int positions) {
            positions = positions & 0x1F;
            // Save the existing bit pattern, but interpret it as an unsigned integer.
            uint number = BitConverter.ToUInt32(BitConverter.GetBytes(value), 0);
            // Preserve the bits to be discarded.
            uint wrapped = number >> (32 - positions);
            // Shift and wrap the discarded bits.
            return BitConverter.ToInt32(BitConverter.GetBytes((number << positions) | wrapped), 0);
        }
        public static int Hash(object arg0, params object[] args) {
            // cf https://docs.microsoft.com/en-us/dotnet/api/system.object.gethashcode?view=netframework-4.8
            // should I toString ?
            int aggreg = arg0.GetHashCode();
            foreach(object arg in args) aggreg = ShiftAndWrap(aggreg, 2) ^ arg.GetHashCode();
            return aggreg;
        }

        public static double HashFloat(object arg0, params object[] args) {
            int h = Hash(arg0, args);
            double x = (1.0 * h - 1.0 *int.MinValue) /  (1.0 * int.MaxValue - 1.0 *int.MinValue);
            return x;
        }
        public static int HashInt(int max, object arg0, params object[] args) {
            int h = Hash(arg0, args);
            return h % max;
        }
    }

    public static UInt64 CalculateHash(string data) {
        UInt64 hashedValue = 3074457345618258791ul;
        for(int i=0; i<data.Length; i++) {
            hashedValue += data[i];
            hashedValue *= 3074457345618258799ul;
        }
        return hashedValue;
    }
    public static UInt64 CalculateHash(params string[] datas) {
        /* Avoid strings concat*/
        UInt64 hashedValue = 3074457345618258791ul;
        foreach(string data in datas) {
            for(int i=0; i<data.Length; i++) {
                hashedValue += data[i];
                hashedValue *= 3074457345618258799ul;
            }
        }
        return hashedValue;
    }

    public static float Rand_GetHashCode(params string[] args) {
        int h = string.Join("", args).GetHashCode();
        double x = (1.0 * h - 1.0 * int.MinValue) /  (1.0 * int.MaxValue - 1.0 * int.MinValue);
        return (float) x;
    }
    public static float Rand(params string[] args) {
        UInt64 h = CalculateHash(args); // Avoid Join creating strings
        double x = (1.0 * h - 1.0 * UInt64.MinValue) /  (1.0 * UInt64.MaxValue - 1.0 * UInt64.MinValue);
        return (float) x;
    }
    public static float Rand(float low, float high, params string[] args) {
        return low + (high-low) * Rand(args);
    }
    public static int Rand(int low, int high, params string[] args) {
        // int h = string.Join("", args).GetHashCode();
        // UInt64 h = CalculateHash(string.Join("", args));
        return low + (int) Math.Floor(Rand(args) * (high-low));
    }
    public static bool Rand(float p, params string[] args) {
        return Rand(args)<=p;
    }
    public static T Rand<T>(T[] choices, params string[] args) {
        int i = Rand(0, choices.Length, args);
        return choices[i];
    }

    /* Random choices, weighted, reprod

    compile it into 100-array as %, cumulated, and fill it (for fast access) ?
    compilation needs be reprod
    1000 ?


    */
    public class Weighted<T> {
        private int size;
        private int nElem;
        private T[] Indexed;
        public static float[] nmlz(float[] weights) {
            float sum = weights.Sum(); // foreach(float weight in weights) sum = sum + weight;
            return weights.Select(x => x/sum).ToArray();
            // float[] w = (from weight in weights select ToWords(weight/sum)).ToArray;
        }
        public override String ToString() {
            return String.Format("Weighted({0} / {1})", nElem, size);
        }
        public Weighted(int size, T[] values, float[] weights) {
            this.size = size;
            this.nElem = values.Length;
            Indexed = new T[size + 10]; // keep some undrawn offset for rouding
            weights = nmlz(weights);
            int index = -1;
            for (int k=0; k<values.Length; k++) {
                int w = (int) Math.Round(weights[k] * size);
                w = Math.Max(1, w);
                for (int l=0; l<w; l++) {
                    index = index + 1;
                    Console.WriteLine(string.Join(" ", "Weighted Filling: ", this,
                        index, "<-", k, "/",values.Length,
                        l, "/", w
                    ));
                    Indexed[index] = values[k];
                }
            }
            if (index < size) {
                // Printer.Print("Weighted Filling: ", this, index, "<", size);
                Console.WriteLine(string.Join(" ", "Weighted ReFilling: ", this, index, "<", size));
                for (int k=index; k<values.Length; k++) Indexed[index] = values[k % values.Length];
            }
        }
        public T this[string seed] {
            get { return Indexed[Rand(0, size, seed)]; }
        }
        public T this[params string[] seed] {
            get { return Indexed[Rand(0, size, seed)]; }
        }

        public class Index : Weighted<int> {
            public Index(int size, float[] weights)
                    : base(size, Enumerable.Range(0, weights.Length).ToArray(), weights){}
        }

    }

    // public static float HashFloat(params string[] args) {
    //     int h = string.Join("", args).GetHashCode();
    //     double x = (1.0 * h - 1.0 *int.MinValue) /  (1.0 * int.MaxValue - 1.0 *int.MinValue);
    //     return (float) x;
    // }
    // public static bool HashBool(float p, params string[] args) {
    //     int h = string.Join("", args).GetHashCode();
    //     double x = (1.0 * h - 1.0 *int.MinValue) /  (1.0 * int.MaxValue - 1.0 *int.MinValue);
    //     return x<=p;
    // }
    // public static int HashInt(int max, string arg0, params string[] args) {
    //     int h = String.Format("{0}{1}", arg0, string.Join("", args)).GetHashCode();
    //     // Console.WriteLine(String.Format("{0} / {1}", h, max));
    //     return Math.Abs(h) % max;
    // }

    static void Main2(string[] args) {
        float[] wgt = Enumerable.Range(0, 20).Select(x => (float) x).ToArray();
        Hashes.Weighted<int>.Index index = new Hashes.Weighted<int>.Index(1000, wgt);
        for (int k=0; k< 30; k++) Console.WriteLine(index[k.ToString()]);


    }
    static void Main0(string[] args) {
        if (false) {
            Console.WriteLine(String.Format("{0}", Rand("oui", "non")));
            Console.WriteLine(String.Format("{0}", Rand("oui", "nonn")));
            Console.WriteLine(String.Format("{0}", Rand("oui", "non")));
            Console.WriteLine(String.Format("{0}", Rand("non", "oui")));
            Console.WriteLine(String.Format("{0}", Rand("ouinon")));
            Console.WriteLine(String.Format("{0}", Rand("ouinon1")));
            Console.WriteLine(String.Format("{0}", Rand("ouinon2")));
            Console.WriteLine(String.Format("{0}", Rand("oui", "non", "oui")));
        }
        if (false) {
            // int.hash = identity
            // for(int k=0; k< 10; k++) Console.WriteLine(String.Format("{0}", HashFloat(k)));
            for(int k=0; k< 30; k++) Console.WriteLine(String.Format("{0}", Rand(k.ToString())));
            // for(int k=0; k< 10; k++) Console.WriteLine(String.Format("{0}", HashFloat(k, "a", k, "b")));
            for(int k=0; k< 30; k++) Console.WriteLine(String.Format("{0}", Rand(String.Format("a{0}", k))));
        }
        if (false) {
            Console.WriteLine(String.Format("{0}", Rand(4, "oui", "non")));
            Console.WriteLine(String.Format("{0}", Rand(4, "oui", "nonn")));
            Console.WriteLine(String.Format("{0}", Rand(4, "oui", "non")));
            Console.WriteLine(String.Format("{0}", Rand(4, "non", "non")));
        }
        if (false) {
            for(int k=0; k< 30; k++) Console.WriteLine(String.Format("{0}", Rand(10, String.Format("a{0}", k))));
        }
        int kk;
        for(kk=0; kk< 1; kk++) {

        }
        Console.WriteLine(String.Format("After loop 10 kk : {0}", kk));
        // for(kk=0; kk< 0; kk++) => k=0 after loop
        // for(kk=0; kk< 10; kk++) => k=10 after loop
    } 

    static void Main1(string[] args) {
        if (true) {
            Console.WriteLine(String.Format("{0}", Rand("oui", "non")));
            Console.WriteLine(String.Format("{0}", Rand("oui", "nonn")));
            Console.WriteLine(String.Format("{0}", Rand("oui", "non")));
            Console.WriteLine(String.Format("{0}", Rand("non", "oui")));
            Console.WriteLine(String.Format("{0}", Rand("ouinon")));
            Console.WriteLine(String.Format("{0}", Rand("ouinon1")));
            Console.WriteLine(String.Format("{0}", Rand("ouinon2")));
            Console.WriteLine(String.Format("{0}", Rand("oui", "non", "oui")));
        }
        if (true) {
            // int.hash = identity
            // for(int k=0; k< 10; k++) Console.WriteLine(String.Format("{0}", HashFloat(k)));
            for(int k=0; k< 30; k++) Console.WriteLine(String.Format("{0}", Rand(k.ToString())));
            // for(int k=0; k< 10; k++) Console.WriteLine(String.Format("{0}", HashFloat(k, "a", k, "b")));
            for(int k=0; k< 30; k++) Console.WriteLine(String.Format("{0}", Rand(String.Format("a{0}", k))));
        }
        if (true) {
            Console.WriteLine(String.Format("{0}", Rand(0,4, "oui", "non")));
            Console.WriteLine(String.Format("{0}", Rand(0,4, "oui", "nonn")));
            Console.WriteLine(String.Format("{0}", Rand(0,4, "oui", "non")));
            Console.WriteLine(String.Format("{0}", Rand(0,4, "non", "non")));
        }
        if (true) {
            for(int k=0; k< 30; k++) Console.WriteLine(String.Format("{0}", Rand(0,10, String.Format("a{0}", k))));
        }

        if (false) {
            int kk;
            for(kk=0; kk< 1; kk++) {

            }
            Console.WriteLine(String.Format("After loop 10 kk : {0}", kk));
            // for(kk=0; kk< 0; kk++) => k=0 after loop
            // for(kk=0; kk< 10; kk++) => k=10 after loop
        }
    } 

}

}