using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;
using Iterating;

using SdtdUtils;
using SdtdUtils.Blocks;
using CSutils;


/* Intensity combination
TODO: combine
- ZBEffect class (normalize the various effects)
- Zone : some are light, some are tough, event for the same ZBEffect class 
- Time of day : pattern may be Zone/Effect based + global season
- Distance to Zone center
- Global seasonnality (day, weeks)
- Game difficulty (settings and GS/level)

*/


/*

GroundEffect
- reverse: ok, effet permanent limité, peut etre plus frequent
- non reverse
    - rdm: will increase the whole area
    - repeat: creates huge peaks (cool). just add a rare randomly "destroy / collapse"
        can use TH or surrounding surface (or player) as benchmark
        will make use collapse (cool, but perf ?)
*/
namespace ZBActivity {
public abstract class ZBEffect {
    /** ***** Iteration ***** 
    Next is called by ZombiomeManager, on the current Zone    
    **/
    public string name = "?";
    public override string ToString(){return "ZBE" + name;}
    private long last_call = 0;
    public long last_access = 0; // for memory optimization
    private float probaSkip = 0f; // rate in continuous time MC P(change) = p dt
    public World World;
    private Scaler Scaler= new Scaler();
    public Periodicity Periodicity;
    public virtual string Next(EntityPlayer player, Vector3 position, float intensity) {
        /*

        TODO: trigger
        - Not always trigger on last=0, to avoid systematic trigger as the player moves to new zones
        - Use absolute time (indep of day length ?)
        - Use intensity

        trigger <=> changement d'état, proba = rate * dt
        could also generate next time from clock
        */        
        long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        last_access = now;      
        if (! Zombiome.IgnoreScale) {
            if (intensity < 0.99f) { // Always active when close to center
                float per = this.Periodicity.Status(now);
                bool skip = Scaler.Skip(now, per * intensity);
                if (true && skip) { // called once, and long ago enough
                    // Printer.Log(49, "Skip effect: ", intensity, per, Scaler.rate);
                    return String.Format("+{0}[S{1}/{2}/{3}]", this.name, Scaler.rate, intensity, per);
                }
            }
        }
        string name = this.ToString(); // get str of ZBEffect
        Zombiome.Routines.Start(Routines.IfNotRunning(Lock, _Next(player)), name + "-_Next");
        last_call = now; // after so that _Next() may compute elapsed time
        return String.Format("+{0}[R]", this.name);
    }
    public abstract IEnumerator _Next(EntityPlayer player);
    /** ***** Options***** 
    Base and randomized versions
    TODO: varying based on amplitude
    **/
    protected virtual void Randomize(OptionEffect opt) {}
    public OptionEffect Randomize() {
        OptionEffect opt = this.opt.Copy();
        Randomize(opt);
        return opt;
    }

    public abstract void Configure();
    public virtual void ConfigurePlacesRepeat() {
        // TODO: randomize choice
        // Placer = new Placer(new Positions.Rand((float) (2*ZChunk.size)), Positions.Rand.NoCenter(1f));

        Options.Repeaters(this);
        Positions.Rand directions = Positions.Rand.NoCenter(1f);
        Positions pos;
        // pos = new Positions.LineAround(4f);
        if (true) {
            float u = Hashes.Rand(seed, "positionType");
            if (u < 0.4) pos = new Positions.AtZChunk();
            else if (u < 0.8) pos = new Positions.Rand((float) (2*ZChunk.size));
            else pos = new Positions.LineAround(4f);
        }
        Placer = new Placer(new Positions.Rand((float) (2*ZChunk.size)), Positions.Rand.NoCenter(1f));
        Printer.Log(40, "ZBEffect ConfigurePlacesRepeat: Repeater, Positions, Directions");
    }
    public ZBEffect ApplyConfigure() {
        // Now done after init, so that subclass init can alter it !
        Printer.Log(60, "new ZBEffect pre configure", seed, biome);
        this.ConfigurePlacesRepeat();
        this.Configure();
        return this;
        // should be moved to the 2 only instanciation points (rdm and command), so super init can interact before

        // Printer.Print("new ZBEffect post configure", opt.OptionBlock, opt.OptionShape); // not set for Wind !
        // not set for FireStorm !
        // Printer.Print("ZBEffect.Peak init done", seed, opt.OptionBlock.block, opt.OptionShape.shape);
    }

    /** Internals **/
    public ZBEffect(Zone zone) { 
        string seed = zone.seed;
        this.World = GameManager.Instance.World;
        this.biomeDef = zone.biomeDef;
        this.biome = zone.biome;
        this.seed = seed;
        this.centerx = zone.x;
        this.centerz = zone.z;
        this.rand = Zombiome.rand; //  GameRandomManager.Instance.CreateGameRandom();
        this.opt = new OptionEffect();
        this.Lock = new bool[]{false};
        this.Periodicity = new Periodicity(seed);
    }

    private bool[] Lock;
    public EffectType EffectType;
    public string seed;
    public BiomeDefinition biomeDef;
    public ZBiomeInfo biome;
    public OptionEffect opt;
    public GameRandom rand; // TODO: use a single instance on Zombiome. This one is not reproducible.
    public Repeater Cycler;
    public Repeater Repeater;
    public Placer Placer;
    protected int centerx;
    protected int centerz;
}

public enum EffectType
{
    Ground,
    Collapse,
    Inventory,
    Environment
}

public class Options {
    public static void MaybeFloatingObject(string seed, OptionEffect Options, float rate=1f) {
        /* Ground effect use airFull below surface to make ground/decoration float

        FIXME: ground becomes the inserted air, so this ends up digging via offset !
        I could also have a special effect that spawns the decoration and make it float

        */
        if (true) return; // DISACTIVATED
        if (Hashes.Rand(seed, "FloatingObject") > rate) return;
        Options.OptionBlock.SetBlocks("airFull");
        Options.OptionBlock.avoidBlock = false;
        Options.OptionBlock.elastic = 10;
        Options.OptionShape.offsetSurface = 0;   // -2 is too deep. 0 with elastic is enough to have 1 block !
    }

    public static void Physics(string seed, BiomeDefinition biome, BlockSetter.Options OptionBlock) {
        OptionBlock.avoidEntity = Hashes.Rand(0.5f, seed, "avoidEntity") ? true : false;
        OptionBlock.avoidBlock = Hashes.Rand(0.5f,seed, "avoidBlock") ? true : false;
        OptionBlock.elastic = Hashes.Rand(0.5f,seed, "elastic") ? 10 : 0;
    }
    private static float[] dtRepeats = new float[]{0.005f, 0.1f, 0.2f, 0.5f, 1f, 3f};
    public static void Repeaters(ZBEffect effect) {
        // "n"s are scalable for ground effects
        // Let's use 1 repeat and increase frequency of calls from Zone
        effect.Cycler = new Repeater(1).Set(Hashes.Rand(2f,5f, effect.seed, "cycle.duration"));
        effect.Repeater = new Repeater(6);
        if (Hashes.Rand(effect.seed, "repeat.frame") < 0.2) effect.Repeater.Set(-1f);
        else effect.Repeater.Set(Hashes.Rand(dtRepeats, effect.seed, "repeat.duration"));
    }


}

/*
 ME should call both per ZChunk, and random

 But signature different:

 * ZChunk -> zc index -> gen positions

flood: for ZChunk.Positions
ghost: random in ZChunk

 * random -> rectangle / positionner -> gen positions



*/

/*
Use cases: 
- MultiEffect : generate effects at positions around players, either random or repro
- AtEntities : generate effects at existing entities (buff, motion, inventory)
                can use the selected entity positition to generate block effect at entities
- SingleChunked: chunks used to control effects (eg ghost: count existing)
                they are single effect/ single replicate

NB: these distinction could be hidden in the Placer (so we might have both at positions and at player/ents)
*/

public abstract class MultiEffect : ZBEffect {
    /* Simple repetition, the base effect should be smart
    TODO: position memory
     */

    protected bool isFollowing = true;
    protected MultiEffect(Zone zone) : base(zone) {}
    /* TODO:possible yield from Effect1 ? I don't really want to block
    - return null / not null
    - pass ref null object
    */
    public abstract void Effect1(EntityPlayer player, Emplacement place, OptionEffect opt);
    public override IEnumerator _Next(EntityPlayer player) {
        /* Over should keep some memory (position), and regenerate toward player
        */
        long start_time = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        Emplacement placeBase = new Emplacement(player, opt);
        placeBase.direction = Vectors.Float.Randomize(this.rand, 1f); // useless for peak
        Printer.Log(46, "MultiEffect._Next() placeBase", player, placeBase.position, placeBase.direction);
        foreach (int p in Cycler.Over()) {
            if (Cycler.dt >= 0 && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - start_time > (long) (Cycler.dt*1000)) yield break;
            Printer.Log(41, "_Next iterating Over"); 
            Vector3 maybeFollowing = (this.isFollowing) ? player.GetPosition() : placeBase.position;
            int q = -1;
            foreach (Emplacement place in Iter.On(Placer.Get(maybeFollowing))) {
                q = q+1;
                if (q >= Repeater.n) break;
                if (place.valid) {
                    Printer.Log(40, "MultiEffect._Next iterating Clones", place); // last before null (firestorm) 
                    // float state = ((float) q) / Repeater.copies;
                    OptionEffect rdm = Randomize();
                    Printer.Log(40, "_Next Randomized", p, q, opt);
                    Printer.Log(40, "_Next Place", p, q, place);
                    this.Effect1(player, place, rdm);
                } else {
                    Printer.Log(40, "Invalid place:", q, place, place.valid_msg);
                }
                yield return Repeater.Yield;
            }
            yield return Cycler.Yield;
        }
    }
}

// TODO: Bounds from ZChunk should be centered at player (or at their input position y) !
// FIXME: this is not SingleChunked1 (replicates allowed)
// FIXME: check is not dead
public abstract class AtEntities : ZBEffect {
    /* Scaling depends on
    - Permanent effects (gravity, randomsize) -> refresh rate.
      Both scaling and resfresh rate don't matter much
      Having active/inactive periods is fine enough
    - Spot effects (jumping, slip, peakat, moving sands) -> event rate
      We want to control scaling with nEntities (dont repeat twice more when there are twice
      less ents, although downscale is important to performance)

    Avoid too much repeat when list is small; while still updating new ents ?
    For spot effect (hence both), the nb of updates (wether random or cycling) is min(rate * len, performance)
    ensures constant rate per entity. => Repeater.n becomes useless 
    TODO: use cycler when performance limit ?
    */

    /* Manage list of entities */
    private List<Entity> entities = new List<Entity>();
    private long last_entities_update = 0; 
    static private float updateEvery = 5; // seconds
    /* Run through entities list */
    public float rate = 1f; // could also be expressed per time unit ?
    private int _indexEnt = -1; // -1 is cycle, -2 is random

    protected AtEntities(Zone zone) : base(zone) {}
    // protected bool cycle = true; // false <=> random
    // cycle is tough when list is refreshed. Risk to always select the first ones
    public override void ConfigurePlacesRepeat() {
        // Placer = new Placer(new Positions.Rand(30f), Positions.Rand.NoCenter(1f));
        // Placer = new Placer(new Positions.Rand(4f), Positions.Rand.NoCenter(1f));
        base.ConfigurePlacesRepeat();
        Repeater.Set(20, -1f); // 10 entities at frame rate
        Printer.Log(60, "ZBEffect ConfigurePlacesRepeat: Repeater, Positions, Directions");
    }

    public abstract IEnumerator Apply(EntityPlayer player, EntityAlive target, OptionEffect opt);
    public override IEnumerator _Next(EntityPlayer player) {
        Vector3 ppos = player.GetPosition();

        // if (entities.Count == 0) {
        //     Printer.Print("AtEntities empty at ", bounds, player);
        //     yield break;
        // }
        foreach (int p in Repeater.Over()) { 
            Bounds bounds = BoundToPosition(ppos, Placer.Bounds(ppos));
            Iter.EverySeconds(ref last_entities_update, updateEvery, this.UpdateEntities, bounds);

            // Printer.Print("AtEntities_Next Over");
            // for (int draw=0; draw< Repeater.n; draw++) { 
            // Downscale only if all entities can't be treated at once. Don't upscale above. 
            int nUpdates = Math.Min(entities.Count, (int) Math.Ceiling(rate * entities.Count));
            for (int draw=0; draw< nUpdates; draw++) {
                // EntityAlive target = entities[this.NextIndex()] as EntityAlive;
                // Printer.Print("AtEntities_Next Clones", draw, target); // last before null (firestorm)
                EntityAlive target = SdtdUtils.Cycling.Next(entities, ref _indexEnt) as EntityAlive;
                if (target != null) {
                    OptionEffect rdm = Randomize();
                    Printer.Log(40, "AtEntities_Next Randomized", draw, target, opt);
                    yield return this.Apply(player, target, rdm);
                } else Printer.Log(40, "AtEntities_Next null Entity", draw);
                yield return Repeater.Yield;
            }
            yield return Cycler.Yield; // a single cycle
        }
    }
    private int NextIndex() {
        if (_indexEnt == -2) return rand.RandomRange(entities.Count);
        return (_indexEnt + 1) % entities.Count;
    }
    public static Bounds BoundToPosition(Vector3 pos, Bounds bounds) {
        Vector3 bcenter = Vectors.Copy(bounds.center);
        Vector3 bsize = Vectors.Copy(bounds.size);
        bcenter.y = pos.y;
        bsize.y = 30;
        return new Bounds(bcenter, bsize); // take size, not ray
    }
    private void UpdateEntities(Bounds bounds) {
        this.entities.Clear();
        this.entities = GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityAlive), bounds, this.entities);
        Printer.Log(46, "AtEntities.UpdateEntities found ", entities.Count, bounds, entities);
    } // scrapMetalPile
}

public abstract class AtPlayer : MultiEffect {
    /* Do we list all player, or let each local ZB process deal with local players ? */
    protected AtPlayer(Zone zone) : base(zone) {}
    public override void ConfigurePlacesRepeat() {
        Placer = new Placer(new Positions.AtZChunk(), Positions.Rand.NoCenter(1f));
        Options.Repeaters(this);
        Repeater.Set(1);
    }
}

public abstract class SingleChunked : MultiEffect {
    /* Effect1() runs Regen() on each 4 ZChunk

    Dont extend MultiEffect ? Placer/Repeater not used by Effect1?
    */
    protected SingleChunked(Zone zone) : base(zone) {}
    public abstract IEnumerator Regen(EntityPlayer player, Vector3i zchunk, int iniguess);
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        Printer.Log(46, "SingleChunked Effect1", place.position, place.ipos, opt.OptionItem.item); 
        Vector3i where = Vectors.ToInt(player.GetPosition()); 
        int y0 = Geo3D.Surface(where).y;
 
        Vector3i nw4 = ZChunk.TL4(place.ipos);
        Zombiome.Routines.Start(Regen(player, nw4, y0), name + "-SingleChunked0");
        Zombiome.Routines.Start(Regen(player, nw4 + new Vector3i(1,0,0), y0), name + "-SingleChunked1");
        Zombiome.Routines.Start(Regen(player, nw4 + new Vector3i(0,0,1), y0), name + "-SingleChunked2");
        Zombiome.Routines.Start(Regen(player, nw4 + new Vector3i(1,0,1), y0), name + "-SingleChunked3");
    }
    public override void ConfigurePlacesRepeat() {
        base.ConfigurePlacesRepeat();
        Repeater.Set(1);
        Cycler.Set(1);
    }
}

public abstract class SingleChunked1 : MultiEffect {
    /* Entity list are chunk based, so I risk non empty intersections/duplicates when mapping
    each ZChunk -> ents
    so let's act on the global bbox

    */
    protected SingleChunked1(Zone zone) : base(zone) {}

    public abstract IEnumerator Regen(EntityPlayer player, Bounds bounds, int iniguess);

    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        Printer.Print("SingleChunked Effect1", place.position, place.ipos, opt.OptionItem.item); 
        Vector3i where = Vectors.ToInt(player.GetPosition()); 
        int y0 = Geo3D.Surface(where).y;
        Vector3i nw4 = ZChunk.TL4(place.ipos);
        Bounds b4 = ZChunk.Bounds4(nw4, y0);
        Zombiome.Routines.Start(Regen(player, b4, y0), name + "-SingleChunked");
    }
    public override void ConfigurePlacesRepeat() {
        base.ConfigurePlacesRepeat();
        Repeater.Set(1);
    }
}


    /* Entity list are chunk based, so I risk non empty intersections/duplicates when mapping
    each ZChunk -> ents
    so let's act on the global bbox

    Do I need an entity selector object (similar to Positions ?)

    GetEntityInBound is inefficient: if (_class.IsAssignableFrom(entity.GetType()) && entity.boundingBox.Intersects(_bb)) _list.Add(entity);
    - I dont need exact bounds + adding
    - Let's draw from full list directly. The reject rate should stabilize because Alive Entities are temporary
    */
    /*
    float rate = 1f; // all entities

     Do I need select all, or is random enough ?
     -> buff should be all, but jump ?
     Also, draw dynamically adapts to changing list
     the  list update should not happen at every iteration... but drawing can 

     go with random: easier
     TODO: restrict to players ? (is there a list of, beside local)
          each Local ZB process could focus on the localPla, enough ?
     Each chunk  has 16 lists for dy =16
     I can also draw sub bounds
     I should recode has enumerator ?
    */


// // 
}  // end namespace
// //
