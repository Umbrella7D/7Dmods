using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;


using SdtdUtils;
using CSutils;

namespace SdtdUtils {

public abstract class Positions {
    /* Generate random positions or directions for Zombiome activity

    Argument position of Get():
    - Center for random position
    - Ignored by direction, except for reproducibility reason (TODO)

    Positions: return absolute (near position)
    Direction: return near 0
    (both use shift)
    */

    public abstract IEnumerator<Vector3> Get(Vector3 position);
    public virtual Bounds Bounds(Vector3 position) {return new Bounds();} // Line uses it as default
    protected GameRandom rand;
    protected bool D2=true; // D2 <=> delta.y=0
    // public Func<Vector3,Vector3> constraint = null;

    public class Rand : Positions {
        /* Random Generator
        - Positions: Argument position (eg player pos) is used to shift the returned value 
        - Directions : Argument position is ignored (no shift), but could be used for reproducible randomness
        */

        public Vector3 shift;
        public bool nocenter = false;
        public float ray = 1f;
        bool normalized = false;
        public Rand(float ray, bool normalized=false, bool D2=true) {
            //TODO AtSurface, iniguess ?
            this.shift = shift; this.ray = ray;
            this.normalized = normalized; this.D2 = D2;
            this.rand = GameRandomManager.Instance.CreateGameRandom();
        }
        public Rand() : this((float) (2*ZChunk.size)) {}


        /* In order to reproduce direction (eg trap line), I need deterministic position -> direction,
        where position arg is selected location by position(player)
                player -> positions -> direction
        */
        public static Rand NoCenter(float ray= 1f, Vector3 shift= new Vector3(), bool normalized= false, bool D2= true) {
            /* Direction : Get() must not depend on arg position !
            */
            Rand nc = new Rand(ray, normalized, D2);
            nc.shift = shift;
            nc.nocenter = true;
            return nc;
        }
        public Vector3 Generate(Vector3 position) {
            Vector3 delta = ray * (2 * new Vector3(rand.RandomFloat, rand.RandomFloat, rand.RandomFloat) - Vectors.Float.One);
            if (D2) delta.y = 0f;
            // if (constraint != null) delta = constraint(delta);
            if (nocenter) return shift + delta;
            else return position + shift + delta;
        }
        public override IEnumerator<Vector3> Get(Vector3 position) {
            while (true) yield return Generate(position);
        }
        public override Bounds Bounds(Vector3 position) {
            if (nocenter) return new Bounds(shift, ray * Vectors.Float.One);
            return new Bounds(position + shift, ray * Vectors.Float.One);
        }
    }

    public class AtZChunk : Positions { 
        /* Generate reproducible positions (TODO can't handle directions yet ) */
        int n = 20;
        public override Bounds Bounds(Vector3 position) {
            Vector3i zchunk = ZChunk.TL4(position);
            return ZChunk.Bounds4(zchunk, (int) Math.Floor(position.y));
        }
        public override IEnumerator<Vector3> Get(Vector3 position) {
            int maxsize = 10;
            Vector3i zchunk = ZChunk.TL4(position);
            bool D0 = false; int L0 = ZChunk.Len(zchunk + ZChunk.adjacents[0], maxsize);
            bool D1 = false; int L1 = ZChunk.Len(zchunk + ZChunk.adjacents[1], maxsize);
            bool D2 = false; int L2 = ZChunk.Len(zchunk + ZChunk.adjacents[2], maxsize);
            bool D3 = false; int L3 = ZChunk.Len(zchunk + ZChunk.adjacents[3], maxsize);

            for (int step=0; step<maxsize; step++) {
                if (! D0 && step > L0) D0 = true;
                if (! D0) yield return ZChunk.Position(Zombiome.worldSeed, zchunk + ZChunk.adjacents[0], step);
                if (! D1 && step > L1) D1 = true;
                if (! D1) yield return ZChunk.Position(Zombiome.worldSeed, zchunk + ZChunk.adjacents[1], step);
                if (! D2 && step > L2) D2 = true;
                if (! D2) yield return ZChunk.Position(Zombiome.worldSeed, zchunk + ZChunk.adjacents[2], step);
                if (! D3 && step > L3) D3 = true;
                if (! D3) yield return ZChunk.Position(Zombiome.worldSeed, zchunk + ZChunk.adjacents[3], step);
                if (D0 && D1 && D2 && D3) break;
            }
        }
    } 

    // public static void LineNear(GameRandom rand, Vector3 pos, float ray, out Vector3 start, out Vector3 end) {
    //     Vector3 N1 = Vectors.Float.Randomize(rand, 1f).normalized;
    //     Vector3 N2 = Vectors.Float.Randomize(rand, 1f).normalized;
    //     start = pos + ray * N1;
    //     end = pos + ray * N2;
    // }

    // public class Line : Positions {
    //     public Vector3 start;
    //     public Vector3 end;
    //     public float dx = 1;
    //     public Line(Vector3 start, Vector3 end, float dx = 1f, bool D2=true) {
    //         this.start = start; this.end = end;
    //         this.D2 = D2;
    //         this.rand = GameRandomManager.Instance.CreateGameRandom();
    //     }
    //     public override IEnumerator<Vector3> Get(Vector3 position) {
    //         float dist = (end-start).magnitude;
    //         Vector3 unit = (end-start).normalized;
    //         int steps = (int) Math.Floor(dist / dx);
    //         for (int step=0; step<steps; step++) yield return start + unit * (dx * step);
    //     }
    // }

    public class LineAround : Positions {
        public Vector3 current;
        public float dx = 1;
        public float ray = 50f;
        bool restart = false;
        public LineAround(float dx = 1f) {
            this.dx = dx;
            this.rand = GameRandomManager.Instance.CreateGameRandom();
        }
        public override IEnumerator<Vector3> Get(Vector3 pos) {
            Vector3 start;
            if (restart || current == Vectors.Float.Zero) {
                Vector3 N1 = Vectors.Float.Randomize(rand, 1f).normalized;
                start = pos + ray * N1;
            } else {
                start = current;
            }
            Vector3 N2 = Vectors.Float.Randomize(rand, 1f).normalized;
            Vector3 end = pos + ray * N2;

            float dist = (end-start).magnitude;
            Vector3 unit = (end-start).normalized;
            int steps = (int) Math.Floor(dist / dx);
            for (int step=0; step<steps; step++) {
                current = start + unit * (dx * step); 
                yield return start + unit * (dx * step);
            }
        }
    }

}


public class MapPersistence {
    /* TODO: Persistent single execution, using flag-block at y=1 */ 
    /* Use block "terrBedrockZBMarker" at y1=1 or 0 to mark that an effect has been done
    and should not be redone

    can I set at y=0 ? has my bedrock the same properies (undestruct ?)

    in effect, some part are unique and persistent (cratere), then some are cyclic (erupting)
 
    */
}


public class Placer {
    /*  Generate random positions and directions for Zombiome activity

    direction take the unnormalized position as argument (for reproducibility reason)

    Add the tracked player (or entity)
    - iniguess
    - limit range, check for is loaded chunk
    - Randomly tweaking position toward player
    - random collapse of ground effects
    TODO:
    limit from basic GTH
    */
    public Positions positions;
    public Positions.Rand directions;
    public bool AtSurface = true;
    // Limits of the starting point of the effect
    public int nOffSurface = 5;
    public int pOffSurface = 20;

    public Placer(Positions positions, Positions.Rand directions) {
        this.positions = positions;
        this.directions = directions;

        ulong wt = GameManager.Instance.World.GetWorldTime();
        int day = GameUtils.WorldTimeToDays(wt);
        int week = (int) (day / 7) + 1;
        this.nOffSurface = week * 3;
        this.pOffSurface = week * 10;
    }
    public IEnumerator<Emplacement> Get(Vector3 position) {
        int iniy = (int) Math.Floor(position.y);
        IEnumerator<Vector3> pos = positions.Get(position);
        while(true) {
            string valid = "";
            /* Extra point near or at player. NB: adds an extra yield */
            if (rate_around > 0) {
                float u = Zombiome.rand.RandomFloat;
                if (u <= rate_around) {
                    float ray = (u <= rate_at) ? 1f : 5f;
                    yield return Emplacement.At(
                        player.GetPosition() + Vectors.Float.Randomize(Zombiome.rand, ray),
                        directions.Generate(pos.Current)
                    );
                }
            }
            /* Basic point */
            bool continuing = pos.MoveNext(); // false if forward failed
            if (! continuing) {
                // Printer.Print("Placer.Get(): stop iteration");
                yield break;
            }
            int th = GameManager.Instance.World.GetTerrainHeight((int) Math.Floor(pos.Current.x), (int) Math.Floor(pos.Current.z));
            if (th == 0) {
                // Printer.Print("Placer.Get(): Chunk not loaded at", pos.Current);
                // Printer.Print("              position", position, "->", pos.Current);
                valid = "th=0";
            }
            // Vector3 where = (this.AtSurface) ? Geo3D.Surface(pos.Current, iniy) : pos.Current; 
            Vector3 where = (this.AtSurface) ? Geo3D.Surface(pos.Current, th) : pos.Current;


            /* Altitude check */
            // if (Math.Abs(th - where.y) > 60) valid = String.Format("y={0} / th={1} < 60", where.y, th); // initial surface
            if (where.y > th + pOffSurface) valid = String.Format("Surface Offset y={0} / th={1} > pD = {2}", where.y, th, pOffSurface); // initial surface
            if (where.y < th - nOffSurface) valid = String.Format("Surface Offset y={0} / th={1} < nD = 60", where.y, th, nOffSurface); // initial surface
            if (where.y <= 1) valid = "y<1";
            if (where.y >= 255) valid = "y>254";
            // TODO: just stop if too far from player (eg teleport) or Zone center 

            Emplacement place = Emplacement.At(where, directions.Generate(pos.Current));
            if (valid.Length > 0) place.Invalid(valid);
            yield return place;
        }
    }
    public virtual Bounds Bounds(Vector3 position) {return positions.Bounds(position);} // Line uses it as default

    /* Trigger some at player or very near */
    private float rate_at = -1;
    private float rate_around = -1;
    private EntityPlayer player;
    public void AtPlayerRate(EntityPlayer player, float at, float around) {
        this.player = player;
        this.rate_at = at;
        this.rate_around = around;
    }
}


} // END namespace