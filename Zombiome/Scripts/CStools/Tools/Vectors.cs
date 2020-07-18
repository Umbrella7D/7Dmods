using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using Harmony;


using System.Linq;
using System.Reflection.Emit;


namespace CSutils {

public static class Vectors {

    public static readonly Vector3i East = new Vector3i(1,0,0);
    public static readonly Vector3i West = new Vector3i(-1,0,0);
    public static readonly Vector3i North = new Vector3i(0,0,1);
    public static readonly Vector3i South = new Vector3i(0,0,-1);
    public static readonly Vector3i Up = new Vector3i(0,1,0);
    public static readonly Vector3i Down = new Vector3i(0,-1,0);
    public static readonly Vector3i One = new Vector3i(1,1,1);
    public static readonly Vector3i Zero = new Vector3i(0,0,0);

    public static readonly Vector3i UnitX = new Vector3i(1,0,0);
    public static readonly Vector3i UnitY = new Vector3i(0,1,0);
    public static readonly Vector3i UnitZ = new Vector3i(0,0,1);

    public static class Float {
        public static readonly Vector3 One = new Vector3(1,1,1);
        public static readonly Vector3 Zero = new Vector3(0,0,0);
        public static readonly Vector3 UnitX = new Vector3(1,0,0);
        public static readonly Vector3 UnitY = new Vector3(0,1,0);
        public static readonly Vector3 UnitZ = new Vector3(0,0,1);

        public static Vector3 Randomize(GameRandom rand, float ray, Vector3 center) {
            /// GameRandom random = GameRandomManager.Instance.CreateGameRandom();
            // GameManager.Instance.World.GetGameRandom().RandomFloat
            return new Vector3(
                center.x + ray * (2 * rand.RandomFloat -1),
                center.y + ray * (2 * rand.RandomFloat -1),
                center.z + ray * (2 * rand.RandomFloat -1)
            );
        }
        public static Vector3 Randomize(GameRandom rand, float ray) {
            return Randomize(rand, ray, Zero);
        }

        public static Vector3 Randomize(GameRandom rand, Vector3 center, Vector3 radius) {
            return new Vector3(
                center.x + radius.x * (-1 + 2 * rand.RandomFloat),
                center.y + radius.y * (-1 + 2 * rand.RandomFloat),
                center.z + radius.z * (-1 + 2 * rand.RandomFloat)
            );
        }

        public static Vector3 RandomIn(Vector3 min, Vector3 max) {
            return new Vector3(
                min.x + Zombiome.rand.RandomFloat * (max.x - min.x),
                min.y + Zombiome.rand.RandomFloat * (max.y - min.y),
                min.z + Zombiome.rand.RandomFloat * (max.z - min.z)
            );
        }


        public static Vector3 Divide(Vector3 a, Vector3 b) {
            return new Vector3(a.x / b.x, a.y / b.y, a.z / b.z);
        }
        public static Vector3 Mult(Vector3 a, Vector3 b) {
            return new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        }

    }

    public static Vector3i Copy(Vector3i arg) {
        return new Vector3i(arg.x, arg.y, arg.z);
    }
    public static Vector3 Copy(Vector3 arg) {
        return new Vector3(arg.x, arg.y, arg.z);
    }

    // FIXME: some conversion are simple cast, others need shift for the entity center/ bbox
    public static Vector3i ToInt(Vector3 v) {
        // return new Vector3i(v.x, v.y, v.z);
        // return new Vector3i( (int)Math.Ceiling(v.x),  (int)Math.Round(v.y),  (int)Math.Ceiling(v.z)); // semble decalé de 1 vers le NE
        //return new Vector3i( (int)Math.Round(v.x+0.5),  (int)Math.Round(v.y),  (int)Math.Round(v.z+0.5)); // dec +1 N seulement
        // return new Vector3i( (int)Math.Round(v.x+0.5),  (int)Math.Round(v.y),  (int)Math.Round(v.z-0.5));
        return new Vector3i( Utils.Fastfloor(v.x), Utils.Fastfloor(v.y),  Utils.Fastfloor(v.z));
    }
    public static Vector3 ToFloat(Vector3i v) {
        // return new Vector3i(v.x, v.y, v.z);
        // return new Vector3i( (int)Math.Ceiling(v.x),  (int)Math.Round(v.y),  (int)Math.Ceiling(v.z)); // semble decalé de 1 vers le NE
        //return new Vector3i( (int)Math.Round(v.x+0.5),  (int)Math.Round(v.y),  (int)Math.Round(v.z+0.5)); // dec +1 N seulement
        return new Vector3( 1.0f * v.x,  1.0f * v.y,  1.0f * v.z);
    } 


    public static Vector3i Toward(Vector3 pos, Vector3 dir, float toward) {return ToInt(pos + ( toward  ) * dir);}
    public static Vector3i Toward(Vector3 pos, Vector3 dir, int toward) {return Toward(pos, dir, 1.0f * toward);}

    public static int D1(Vector3i a, Vector3i b) {
        return Math.Abs(a.x - b.x) + Math.Abs(a.y - b.y) + Math.Abs(a.z - b.z);
    }
    public static int D1(Vector3i a) {
        return Math.Abs(a.x) + Math.Abs(a.y) + Math.Abs(a.z);
    }


}


} // END namespace