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

public static class EffectsCollapse {
    /*

    Options:
    - ground (water, traps)
    - recursion
    - size / depth (puis avant)
    - other content : Z, animal, torch, lights ...

    */


     public static IEnumerator Rift(EntityPlayer player, Emplacement place, OptionEffect options) {
         /*
         Laisse des blocks tomber au dessus ? just changed  erase="yes"
         (longueur 1, hauteur (profonfeur), replicats)
         */
        EntityPlayerLocal epl = player as EntityPlayerLocal;
        epl.cameraTransform.SendMessage("ShakeBig");
        yield return new WaitForSeconds(1f);

        BlockSetter setter = new BlockSetter(options.OptionBlock);
        Vector3 direction = Vectors.Copy(place.direction);
        direction.y = 0;
        direction = direction.normalized;

        Vector3i start = Geo3D.Surface(place.ipos);
        for (int k=0; k< options.OptionShape.shape.z; k++) {
            Vector3 kdirection = direction + Vectors.Float.Randomize(GameManager.Instance.World.GetGameRandom(), 0.2f);
            // IntLine traj = new IntLine(start, direction); //east
            IEnumerable<Vector3i> segment = IntLine.Segment(Vectors.ToFloat(start), kdirection, 0, options.OptionShape.shape.x);
            Vector3i prev = new Vector3i();
            bool hasprev = false;
            foreach (Vector3i where in segment) {
                Vector3i Swhere = Geo3D.Surface(where);
                setter.Apply(Swhere);
                if(hasprev) {
                    for (int creuse=1; creuse<options.OptionShape.shape.y; creuse++) setter.Apply(prev + creuse * Vectors.Down);
                }
                setter.Push();
                start = Swhere;
                yield return new WaitForEndOfFrame();
                hasprev = true; prev = Swhere;
            }
            yield return new WaitForSeconds(1f);
        }

     }

    public static IEnumerator Cave(EntityPlayer player, Emplacement place, OptionEffect options) {
        /// TODO: enumérer les colonnes et s'arreter à surface
        BlockSetter setter = new BlockSetter(options.OptionBlock);
        Vector3i shape = options.OptionShape.shape;
        Vector3i start = Geo3D.Surface(place.ipos);

        int depth = shape.y;
        Vector3 direction = Vectors.Float.UnitY; // cannot use negative, so positive and get(_k) !
        IntLine colonne = new IntLine(Vectors.ToFloat(start), direction);

        // Debug.Log(String.Format("Cave: pos={0} start={1} dir={2} ground={3}", place.position, start, direction, ground));

        for (int d=0; d<depth; d++) {
            // Debug.Log(String.Format("cave {0} {1}", d, setter));
            if (options.OptionShape.ground != "" && d==depth-1)
                setter.options.block = Block.GetBlockByName(options.OptionShape.ground, false);
            Vector3i where = colonne.Get(- d);
            Vector3i dxy = new Vector3i(0,0,0);
            foreach(int p in SdtdUtils.EffectsGround.LR(shape.x)) foreach(int q in SdtdUtils.EffectsGround.LR(shape.z)) {
                    dxy.x = p; dxy.z = q;
                    Printer.Log(20, "Cave Apply (d,p) =", d, p, "where, dxy=", where, dxy);
                    setter.Apply(where + dxy);
            }
            Printer.FLog(20, "cave Push {0} {1}", d, where);
            setter.Push();
        }
        yield return new WaitForEndOfFrame();


     }

    public static IEnumerator Puit(EntityPlayer player, Emplacement place, OptionEffect options) {
        /// crache des rochers
        yield return Cave(player, place, options);
        // EffectsItem.SpawnParticle(place.position, "big_smoke");// warning !!
        yield return new WaitForSeconds(0.1f);
        string item = options.OptionItem.item;
        for (int k=0; k<4; k++) {
            Vector3 motion = Vectors.Float.Randomize(player.world.GetGameRandom(), 1f, 3* Vectors.Float.UnitY);
            motion = motion.normalized * 5f;
            yield return EffectsItem.spawnItemGhost(item, place.position + 3.5f * Vectors.Float.UnitY, motion);
            yield return new WaitForSeconds(2f);
        }
    }
}


////////////////////
} // End Namespace
////////////////////