using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;

using CSutils;
using SdtdUtils.Blocks;

namespace SdtdUtils {

public class EffectsGround {

    public class Options {
        // Shape
        public Options Copy() {
            Options clone = this.MemberwiseClone() as Options;
            clone.shape = Vectors.Zero + shape;
            clone.direction = Vector3.zero + direction;
            return clone;
        }
        public float pace = 0f;
        public Vector3i shape = new Vector3i(1,1,1);
        public Vector3 direction = new Vector3(0,1,0);
        public string ground = "";
        public string reverse = ""; // U:up, O:at once
        public int offsetSurface = 0; 
        /*
        <triggered_effect trigger="onSelfPrimaryActionEnd" action="Hook, Mods" target="self" effect="EffectsGround.LineSurface" block="woodMaster" elastic="5" shape="20,1,5"/>
        <triggered_effect trigger="onSelfPrimaryActionEnd" action="Hook, Mods" target="self" effect="EffectsGround.LineSurface" block="airFull" elastic="5" shape="10,1,5"/>
         <triggered_effect trigger="onSelfPrimaryActionEnd" action="Hook, Mods" target="self" effect="EffectsGround.Rift" at="ray" block="brickNoUpgradeMaster" elastic="5"/>
        <triggered_effect trigger="onSelfPrimaryActionEnd" action="Hook, Mods" target="self" effect="EffectsGround.TrapLine" at="ray" block="trapSpikesWoodDmg0" avb="true"/>
        <triggered_effect trigger="onSelfPrimaryActionEnd" action="Hook, Mods" target="self" effect="EffectsGround.Peak"
                        block="water" at="ray" elastic="5"/>
                        <triggered_effect trigger="onSelfPrimaryActionEnd" action="Hook, Mods" target="self" effect="EffectsGround.Peak" block="water" at="ray" elastic="5" shape="1,5,1" reverse="U"/>

        <triggered_effect trigger="onSelfPrimaryActionEnd" action="Hook, Mods" target="self" effect="EffectsGround.Peak" avb="true" block="terrSnow" at="ray" shape="1,4,1" pace="0.05" elastic="5" reverse="R"/>
        <triggered_effect trigger="onSelfPrimaryActionEnd" action="Hook, Mods" target="self" effect="EffectsGround.Peak" block="terrDesertGround" at="ray" shape="3,7,3" pace="0.05" elastic="5" reverse="O"/>

        <triggered_effect trigger="onSelfPrimaryActionEnd" action="Hook, Mods" target="self" effect="EffectsGround.Peak" avb="true" ave="true" block="terrSnow" at="ray" shape="5,4,5" pace="0.05"/>

        <triggered_effect trigger="onSelfPrimaryActionEnd" action="Hook, Mods" target="self" effect="EffectsGround.CactusGrowth" block="air" at="ray"/>

        */
        public static Options Peak(string seed, string type) {
            Options opt = new Options();
            switch(type) {
              //  case ""
            }
            return opt;
        }
    }



    public static IEnumerable<int> LR(int n) {
        return Enumerable.Range(-n/2, n);
    }


    public static void setBlockAt(Emplacement place, string block, Func<Vector3i, Block, int> setter) {
        setter(place.ipos, Block.GetBlockByName(block, false));
    }

     public static IEnumerator LineSurface(Emplacement place, OptionEffect options) { 
        Vector3i shape = options.OptionShape.shape; /// (avancée, hauteur, largeur)
        Vector3i above = Vectors.Up;
        Vector3i pos = place.ipos;
        Vector3i offsetSurface = new Vector3i(0, options.OptionShape.offsetSurface, 0);
        bool reverse = options.OptionShape.reverse != "";
        float pace = options.OptionShape.pace;
        bool collapse_once = options.OptionShape.reverse == "once";
        Block air = Block.GetBlockByName("air", false);
        Block blk = options.OptionBlock.block;
        BlockSetter setter = new BlockSetter(options.OptionBlock);

        /// IntLine traj = new IntLine(place.ipos, Vectors.Float.UnitX);
        IntLine traj = new IntLine(place.ipos, Emplacement.Truncate(place.direction, true, true));
        for (int avance=0; avance<shape.x; avance++) {
            Vector3i where = traj.Get(avance);
            Debug.Log("LineSurface " + avance.ToString());
            IntLine orth = new IntLine(Vectors.ToFloat(where), Vectors.Float.UnitZ);
            foreach(int ligne in LR(shape.z)) {
                Vector3i at = orth.Get(ligne);
                //Debug.Log("Line Surface inner " + p.ToString());
                at = Geo3D.Surface(at); // I don't need surface before orthogonal ... surface made after
                setter.Apply(at + above + offsetSurface);
            }
            setter.Push();
            yield return new WaitForEndOfFrame();
        }
    }


/* 
    public static string Get(IDictionary<string, string> dico, string key, string def) { // UTILS
        if (dico.ContainsKey(key)) return dico[key];
        return def;
    } */
    public static IEnumerator Peak(Entity player, Emplacement place, OptionEffect options) {
        Vector3i offsetSurface = new Vector3i(0, options.OptionShape.offsetSurface, 0);
        Vector3i pos = Geo3D.Surface(place.ipos) + Vectors.Up + offsetSurface;

        Vector3i shape = options.OptionShape.shape;

        float pace = options.OptionShape.pace;
        Block air = Block.GetBlockByName("air", false); // The air instance could prolly be shared ...
        Block blk = options.OptionBlock.block;
        BlockSetter setter = new BlockSetter(options.OptionBlock);

        Printer.Log(20, "Peak (position/pos=Surface+Up):", place.ipos, pos);
        Printer.Log(20, "              (blk/pace/shape)", options.OptionBlock.block, options.OptionShape.pace, options.OptionShape.shape);
        Printer.Log(20, "    setter   (avB/avE,elastic):", options.OptionBlock.avoidBlock, options.OptionBlock.avoidEntity, options.OptionBlock.elastic);
        // Start at -1 only if air below
        for(int h = 0; h <shape.y; h++) {
            foreach(int e in LR(shape.x)) {
                foreach(int n in LR(shape.z)) {
                    Vector3i where = new Vector3i(pos.x+e, pos.y+h, pos.z+n);
                    setter.Apply(where);
                }
                setter.Push();
                yield return new WaitForSeconds(pace);
            }
            // 
        }
        if (options.OptionShape.reverse == "") yield break;
        // unset if exists
        options.OptionBlock.block = air;
        options.OptionBlock.avoidBlock = false; // protected by testing blk.blockID, and preventing us from actually deleting ! 
        setter = new BlockSetter(options.OptionBlock);
        // IEnumerable<int> heights = Enumerable.Range(0, shape.y-1);
        IEnumerable<int> heights = Enumerable.Range(0, shape.y);
        if (options.OptionShape.reverse.Contains('U')) {}
        else heights = Enumerable.Reverse(heights);
        foreach(int h in heights) { // when destroyed from below, collapse => destroy top to bottom
            foreach(int e in LR(shape.x)) {
                foreach(int n in LR(shape.z)) {
                    Vector3i where = new Vector3i(pos.x+e, pos.y+h, pos.z+n); 
                    BlockValue existing = GameManager.Instance.World.GetBlock(where);
                    if (existing.type == blk.blockID) { // only erase the type I just inserted
                        setter.Apply(where);
                        // TODO: reverse once option
                        // yield return new WaitForSeconds(sleep);
                    }
                }
            }
            if (! options.OptionShape.reverse.Contains('O')) { /// not once: update progressively
                setter.Push();
                yield return new WaitForSeconds(pace);
            }
        }
    }

    public static IEnumerator Rift(EntityPlayer player, Emplacement place, OptionEffect options) {
         /*
         Laisse des blocks tomber au dessus ? just changed  erase="yes"
         */
        Vector3i offsetSurface = new Vector3i(0, options.OptionShape.offsetSurface, 0);

        EntityPlayerLocal epl = player as EntityPlayerLocal;
        epl.cameraTransform.SendMessage("ShakeBig");
        yield return new WaitForSeconds(1f);

        // Vector3i shape = options.OptionShape.shape; /// (longueur, hauteur, largeur)
        BlockSetter setter = new BlockSetter(options.OptionBlock);
        Vector3i start = Geo3D.Surface(place.ipos);
        for (int k=0; k<3; k++) {
            // Vector3 direction = Vectors.Float.UnitX + Vectors.Float.Randomize(GameManager.Instance.World.GetGameRandom(), 0.1f);
            Vector3 direction = place.direction + Vectors.Float.Randomize(GameManager.Instance.World.GetGameRandom(), 0.1f);
            direction.y = 0;
            direction = direction.normalized;
            // IntLine traj = new IntLine(start, direction); //east
            IEnumerable<Vector3i> segment = IntLine.Segment(Vectors.ToFloat(start), direction, 1, 5); // skip 0 intersecting with the previous
            foreach (Vector3i where in segment) { 
                Vector3i Swhere = Geo3D.Surface(where) + offsetSurface;
                setter.Apply(Swhere);
                setter.Apply(Swhere + Vectors.Up);
                setter.Apply(Swhere + 2* Vectors.Up);
                setter.Push();
                start = Swhere;
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(1f);
        }

     }



    public static System.Random Random = new System.Random();

    public static IEnumerator Chaos(Entity _player, Emplacement place, OptionEffect options) {
        // ca fait rien pr l'instant. essayer de yield les sous call plutot que de les startcoroutine ??
        Vector3i inipos = place.ipos;
        for (int repeat=0; repeat< 5; repeat++) {
            for (int clone=0; clone< 10; clone++) {
                OptionEffect copy = options.Copy();
                // FIXME: copy Place too ()
                Vector3i rdm = new Vector3i(Random.Next(-20,20), 0, Random.Next(-20,20));
                place.ipos = inipos + rdm; // [!}] in-place with past coroutine, should be ok (get pos at start, we reassign)
                copy.OptionShape.shape = new Vector3i(Random.Next(0,3), Random.Next(2,8), Random.Next(0,3));
                // copy["size"] = String.Format("{0},{1},{2}", Random.Next(0,3), Random.Next(2,8), Random.Next(0,3));
                Zombiome.Routines.Start(Peak(_player, place, copy), "Chaos--Peak");
                yield return new WaitForSeconds(0.3f);
            }
            yield return new WaitForSeconds(4.0f);
        }
    }


    public static void __BUG() {
        throw new NullReferenceException();
    }

    public static IEnumerator Wave(Entity _player, Emplacement place, OptionEffect options) {
        /*  NB: position ou ray peuvent etre ds le vide. Il faut partir une case dessous et not erase ?
        */
        Vector3i offsetSurface = new Vector3i(0, options.OptionShape.offsetSurface, 0);

        Vector3i pos = place.ipos;
        // size = E/W=largeur, hauteur, N/S profondeur (portee)
        Vector3i size = options.OptionShape.shape;
        Vector3 direction = Emplacement.Truncate(place.direction, true, true);

        float pace = 0.1f; // TODO pace in option
        Block air = Block.GetBlockByName("air", false); // The air instance could prolly be shared ...
        Block blk = options.OptionBlock.block;

        int portee = 10; // [!]

        BlockSetter setter = new BlockSetter(options.OptionBlock);
        BlockSetter setterAir = new BlockSetter(options.OptionBlock.Copy());
        setterAir.options.block = air;

        Vector3 posf = Vectors.ToFloat(pos + offsetSurface);

        for(int forward = 1; forward <=portee; forward++) {
            foreach(int width in LR(size.x)) {
                setter.Apply(Vectors.Toward(posf, direction, forward));  // avant de la vague
                setter.Push();
                yield return new WaitForSeconds(pace);
                setter.Apply(Vectors.Toward(posf, direction, forward-1));
                setter.Apply(Vectors.Toward(posf, direction, forward-1) + Vectors.Up); // if not exist material to speed up?
                setter.Push();
                yield return new WaitForSeconds(pace); 
                if(forward > 3) { // TODO: test existing ?
                    setterAir.Apply(Vectors.Toward(posf, direction, forward-1-2));
                    setterAir.Apply(Vectors.Toward(posf, direction, forward-1-2) + Vectors.Up);
                    setterAir.Push();
                }
            }
            // __BUG(); // FIXME test
        }
    }


    public static IEnumerator CactusGrowth(Entity player, Emplacement place, OptionEffect options) {
        Vector3i where = Geo3D.Surface(place.ipos) + Vectors.Up;
        string[] cactuses = new String[]{"treeCactus04","treeCactus05","treeCactus06","treeCactus03","treeCactus02","treeCactus01"};
        foreach(string blk in cactuses) {
            BlockSetter.SetBlockAt(where, Block.GetBlockByName(blk, false), options);
            yield return new WaitForSeconds(1f);
        }
    }

    public static IEnumerator TrapLine(Entity player, Emplacement place, OptionEffect options) {
        /// TODO: recoder ca avec rift
        Vector3i offsetSurface = new Vector3i(0, options.OptionShape.offsetSurface, 0);

        Vector3i pos = place.ipos;
        // size = E/W=largeur, hauteur, N/S profondeur (portee)
        Vector3i size = options.OptionShape.shape;
        Vector3 base_direction = Emplacement.Truncate(place.direction, true, true);

        float pace = 0.1f; // TODO pace in option
        Block air = Block.GetBlockByName("air", false); // The air instance could prolly be shared ...
        // Block blk = options.OptionBlock.block;

        int portee = 100;

        BlockSetter setter = new BlockSetter(options.OptionBlock);
        BlockSetter setterAir = new BlockSetter(options.OptionBlock.Copy());
        setterAir.options.block = air;

        Vector3 posf = Vectors.ToFloat(pos + offsetSurface);
        // string[] random_blocks = new string[]{"trapSpikesWoodDmg0", "trapSpikesWoodDmg1", "trapSpikesWoodDmg2"};
        // Block[] random_blocks = options.OptionBlock.blocks;
        Vector3i start = Geo3D.Surface(place.ipos);
        for (int k=0; k<10; k++) {
            Vector3 direction = base_direction + Vectors.Float.Randomize(GameManager.Instance.World.GetGameRandom(), 0.1f);
            direction.y = 0;
            direction = direction.normalized;
            // IntLine traj = new IntLine(start, direction); //east
            IEnumerable<Vector3i> segment = IntLine.Segment(Vectors.ToFloat(start), direction, 1, 10); // skip 0 intersecting with the previous
            foreach (Vector3i where in segment) { 
                Vector3i Swhere = Geo3D.Surface(where);
                // randomisation : "trapSpikesWoodDmg0-2"
                // string rdm = random_blocks[(int) Math.Floor(GameManager.Instance.World.GetGameRandom().RandomFloat*3)];
                // setter.options.block = Block.GetBlockByName(rdm, false);
                // Block rdm = 

                setter.Apply(Swhere + Vectors.Up);
                setter.Push();
                start = Swhere;
                yield return new WaitForEndOfFrame();
            }
            yield return new WaitForSeconds(0.5f);
        }
    }



}

////////////////////
} // End Namespace
////////////////////