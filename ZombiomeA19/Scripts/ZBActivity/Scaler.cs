using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;


using SdtdUtils;

using CSutils;

public class Scaler {
    /*
    2 component - let's start with 2 so we can experiment
    1) how to compute the global factor
    2) how to apply it

    - Instance
    - Distance to center (for reprod emplacement, I need the source and distance of the chunk, not player ?)
    - Time serie
    - Random periodicity

    Effects: intensity and strength
    */

    private long last_exec = 0;
    private long last_try = 0;
    public float rate = 1f;

    /*
    Skip intensity should be product of
    */
    public bool Skip(long now, float intens = 1f) {
        float u = Zombiome.rand.RandomFloat;
        // Printer.Print("Skip ?", u, ">", rate * intens, u > rate * intens);
        return u > rate * intens; // P(skip) = 1-intensity 
    }


}

public class Periodicity {
    /* Generate 0/1 function of t, whith on average P(1) = rate, and n episode per hour

    location : #active / #total time
    shape : len of each episode / nb episode

    do I need reprod ? not really, but if GCed, it will generate again

    account for difficulty


    */
    /* Generation parameters
    How to rescale with difficulty/progression ?
    */
    private float rate = 0.5f;
    private int n = 5; // nb episode per (real) hour

    public float day = 1f;
    public float night = 1f;
    private Periodicity(float rate = 0.5f, int n = 5) {
        this.rate = rate;
        this.n = n;
        Renew(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }
    public Periodicity(string seed) {
        this.rate = CSutils.Hashes.Rand(0.1f, 0.7f, seed, "intensity"); // entre 10 et 70% du temps
        this.n = CSutils.Hashes.Rand(1, 30, seed, "frequency"); // phases de 2mn / 1h
        // this.n = 60;// debug
        Renew(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

    /* The current single activity period [astart,aend] in [start,end] */
    private long start;
    private long end;
    private long astart;
    private long aend;

    public override string ToString() {
        return String.Format("Periodic({0} * {1}): [{2}, {3}] from [{4}, {5}]", rate, n,
                                                                astart, aend, start, end );
    }
    private void Renew(long now) {
        /*  dt from shape : 1h / n
            adt covers rate % of dt */
        start = now;
        float dt0 = (float) (60 * 60 * 1000) / n;
        long dt = (long) rMult(Zombiome.rand, 0.2f, dt0);
        end = start + dt;

        float rate = rMult(Zombiome.rand, 0.2f, this.rate, 0.99f);
        rate = 0.5f; // DEBUG
        long adt = (long) (rate * dt);
        astart = start + (long) (Zombiome.rand.RandomFloat * (dt - adt));
        aend = Math.Min(astart + adt, start + dt);
        Printer.Log(45, "Renew-", this.ToString());
    }
    public float Status(long now) {
        if (now >= end) Renew(now);
        // use extrapolator dt/adt and return float ? Mais le 0 c'est sympa pour le ressenti
        if (now >= astart && now <= aend) return 1f;
        // if (Global.Status(now) > 0.5f) return 1f;
        return 0f;
    }
    private static float rMult(GameRandom rand, float magnitude, float center=1f, float tresh=float.MaxValue) {
        return Math.Min(center * (1f + - magnitude + 2f * magnitude * rand.RandomFloat), tresh);
    }

    private static Periodicity Global;
    static Periodicity() {
        /* Not frequent because it overrides zone Periodicity, and trigger 4 */
        Global = new Periodicity();
        Global.rate = 0.1f;
        Global.n = 1; // phases de 2mn / 1h // adjust with nday/gamsetage
        // TODO: float : - d'1 action par h en moyenne
        // this.n = 60;// debug
        Global.Renew(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }

}

/*

Honnete pour un début de partie
- this.rate = CSutils.Hashes.Rand(0.1f, 0.7f, seed, "intensity"); // entre 10 et 70% du temps
- this.n = CSutils.Hashes.Rand(1, 30, seed, "frequency");
- radius = CSutils.Hashes.Rand(20, ZoneSize/2, seed, "radius"); un peu trop faible

(trop de trapline, pas vu de geant)

*/