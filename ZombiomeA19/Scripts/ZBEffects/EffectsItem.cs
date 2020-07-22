using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;

using CSutils;
using SdtdUtils;

namespace SdtdUtils {

public class EffectsItem {
    public static System.Random Rand = new System.Random();

    public class Options {
        public Options Copy() {return this.MemberwiseClone() as Options;}
        public string item ="";
        // TODO: ghost
    }

    public static void spawnItem(EntityPlayer player, Emplacement place, Options options) {
        spawnItem(player, options.item);
    }


    /* ******************** Item ******************** */
    public static void spawnItem(EntityPlayer player, string item) {
        //EntityPlayerLocal player = GameManager.Instance.World.GetPrimaryPlayer();
        // if(entityAlive is EntityPlayerLocal)

        // base.ItemController in ItemActionEntryResharpenSDX : BaseItemActionEntry

        // using System;using System.Globalization;using System.Xml;using UnityEngine;

        // item = "meleeToolClawHammer";
        ItemStack itemStack = new ItemStack(ItemClass.GetItem(item, false), 1); // this.CreateItemCount
        //player.world.gameManager.ItemDropServer(itemStack, player.GetPosition(), Vector3.zero, -1, 60f, false);

        //invData.gameManager.ItemDropServer(new ItemStack(holdingEntity.inventory.holdingItemItemValue, 1), vector, Vector3.zero,
        //        lookVector * _actionData.m_ThrowStrength, holdingEntity.entityId, 60f, true, -1);


/*         ItemDropServer(
            ItemStack _itemStack,
            Vector3 _dropPos, Vector3 _randomPosAdd, Vector3 _initialMotion,
            int _entityId = -1,
            float _lifetime = 60f
             bool _bDropPosIsRelativeToHead = false, int _clientInstanceId = 0); */


        player.world.gameManager.ItemDropServer(
            itemStack,
            player.GetPosition() + new Vector3(1.0f,0.2f,0.0f),  Vector3.zero, new Vector3(1,-1,0),
            player.entityId, // -1 plante (pour gagner XP ?)
            1f,
            false,
            0);




        //if (!LocalPlayerUI.GetUIForPlayer(player).xui.PlayerInventory.AddItem(itemStack, true)) // Argument 1: cannot convert from 'EntityPlayer' to 'EntityPlayerLocal'
        //{
        //    player.world.gameManager.ItemDropServer(itemStack, player.GetPosition(), Vector3.zero, -1, 60f, false);
        //}
    }


    public static void spawnItem(EntityPlayer player, string item, Vector3 position) {spawnItem(player, item, position, -Vectors.Float.UnitY);}
    public static void spawnItem(EntityPlayer player, string item, Vector3 position, Vector3 motion) {
        ItemStack itemStack = new ItemStack(ItemClass.GetItem(item, false), 1); // this.CreateItemCount
        player.world.gameManager.ItemDropServer(
            itemStack,
            position + new Vector3(0.0f,0.2f,0.0f), // initial pos
            Vector3.zero, /// rdm
            motion, ///  _initialMotion
            player.entityId, // -1 plante (pour gagner XP ?)
            1f,
            false,
            0
        );
    }
    public static IEnumerator spawnItemGhost(EntityAlive player, string item, Vector3 position, Vector3 motion) {
        return spawnItemGhost(item, position, motion);
    }

    public static IEnumerator spawnItemGhost(string item, Vector3 position, Vector3 motion) {
        /* TODO: speed per weight
        TODO: position up by multiblockdim of the item
        */
        ItemStack itemStack = new ItemStack(ItemClass.GetItem(item, false), 1); // this.CreateItemCount

        //yield return Zombiome.Routines.Start(SdtdUtils.EffectsEntity.Ghost.GetGhost(player));
        //Entity ghost = SdtdUtils.EffectsEntity.Ghost.Current;
        yield return Zombiome.Routines.Start(TheGhost.Ensure(), "TheGhost.Ensure");
        Entity ghost = TheGhost.Current;

        Printer.FLog(27, "spawnItemGhost ghost {0}", ghost);
        // Entity ghost = GameManager.Instance.World.GetEntity(ghostId);
        if (ghost == null) yield break;
        GameManager.Instance.ItemDropServer(
            itemStack,
            position + new Vector3(0.0f,0.75f,0.0f), // initial pos
            Vector3.zero, /// rdm
            motion, ///  _initialMotion
            ghost.entityId, // -1 plante (pour gagner XP ?)
            60f, // 1 second lifetime
            false,
            0
        );
    }

    public static void Explosion(EntityPlayer player, Emplacement place, OptionEffect options) {
        /// effet graphique seul (mm si molo contient des degats, ils concernent le onImpact, pas l'explosion elle meme)
        ///
        ItemClass itemClass = ItemClass.GetItemClass("thrownAmmoMolotovCocktail", false);
        Debug.Log(String.Format("Explosion --> {0}", itemClass));
        // GameManager.Instance.ExplosionServer(0, place.position, place.position, Quaternion.identity, new ExplosionData(itemClass.Properties), player.entityId, 0.1f, false, null); // try -1
        GameManager.Instance.ExplosionServer(0, place.position, place.ipos, Quaternion.identity, new ExplosionData(itemClass.Properties), -1, 0.1f, false, null);
        // try in the air
        // altérer particule pour toutes les essayer
    }

 
  /*
sounds: avalanche
machete_swinglight
  */
    public static void SpawnParticle(Vector3 pos, string name, Color color, string sound = null) { // "ember_pile"
        /*
        Load: removes prefix "p_" before hashcode
        Constructor: does not remove
        */

        string pfx = "p_"; // ParticleEffect.prefix
        if (name.StartsWith(pfx)) name = name.Substring(pfx.Length);
        float lightValue = GameManager.Instance.World.GetLightBrightness(Vectors.ToInt(pos)) / 2f;
        ParticleEffect pe = new ParticleEffect(
            name,
            pos,
            lightValue,
            color, // new Color(1f, 0f, 0f, 0.3f),
            sound, // "electric_fence_impact",
            null,
            false
        );
        GameManager.Instance.SpawnParticleEffectServer(pe, -1);
    }
    public static void SpawnParticle(Vector3 pos, string name, string sound = null) { // "ember_pile"
        SpawnParticle(pos, name, Color.white, sound);
    }
    public static void SpawnParticle(Vector3 pos, string name) { // "ember_pile"
        SpawnParticle(pos, name, Color.white, null); // "electric_fence_impact"
    }

    public static int SetSoundMode = -1;
    private static YieldInstruction _WaitFrame = new WaitForEndOfFrame();
    public static IEnumerator PlayZBSound(string sound, Vector3 pos, EntityPlayer player, World World = null,
                            int _SetSoundMode = 1, int reduce=0, float rate= 1f) {
        if (rate < 1 && Rand.NextDouble() >= rate) return Iterating.Iter.Empty(); //(new object[]{_WaitFrame}).GetEnumerator();
        if (World==null) World = GameManager.Instance.World;
        if (SetSoundMode > -1) _SetSoundMode = SetSoundMode;
        if (_SetSoundMode == 0) return Routines.Call(
            World.GetGameManager().PlaySoundAtPositionServer,
            pos, sound, AudioRolloffMode.Custom, 300 // unnoisy
        );
        if (_SetSoundMode == 1) return Routines.Call(
            World.GetGameManager().PlaySoundAtPositionServer,
            pos, sound, AudioRolloffMode.Linear, 1 + reduce // less noisy
        );
        if (_SetSoundMode == 2) return Routines.Call(Audio.Manager.BroadcastPlay, player, sound); // too noisy
        return Routines.Call(Audio.Manager.BroadcastPlay, pos, sound, 0f); // Much less unnoisy        
    }
    
}



public static class TheGhost {
    /* Ghost used by SpawnItemGhost(). This Ghost is an implementation detail of biome projectile

    - Biome projectile are dropped ItemStack, with controler = Ghost
        (Using controlerId = -1 makes a NPE For what reason does the game need it ? xp, faction check ... )
    - Could use projectile, but then we need i) an item (could be ghost's hand) and ii) move the ghost for the holding
      animation to start at the correct place (can we avoid it ? can we use a custom entity controler for easier moves ?)
    - This ghost should not be interacted with (damage, collision, actions...)

    TODO: unicize running enumerator ?
    */
    private static EntityPool pool = new EntityPool("zombieMoeGhost");
    private static YieldInstruction Yield = new WaitForSeconds(1f);
    public static EntityAlive Current {
        get {return pool.Entities[0] as EntityAlive;}
    }
    public static IEnumerator Ensure() {
        /* Call this and wait for finish whenever prior to using the global ghost */
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0];
        Bounds area = BoundsUtils.BoundsForMinMax( -2,-1,-2,  2,1,2  );
        while (true) {
            Vector3 ppos = Vectors.Copy(player.GetPosition());
            area.center = ppos; // will be surfaced anyway 
            pool.Update(area); // update in any case to invalidate
            if(pool.Entities[0] != null) yield break;
            yield return Yield;
        }
    }
}

////////////////////
} // End Namespace
////////////////////