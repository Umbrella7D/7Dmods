using Harmony;
using System.Reflection;
using UnityEngine;
using DMT;
using System;
using System.Collections.Generic;

using CSutils;

[HarmonyPatch(typeof(BlockModelTree))]
[HarmonyPatch("startToFall")] // private bool startToFall(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, int _entityId)
public class RandomTreeFall : IHarmony {
    /// Trees fall toward player or in a random direction, depending on perkMiner69r (TODO: and agility)
    // private static GameRandom gameRandom = GameManager.Instance.World.GetGameRandom(); // too early

    public void Start() {
        Debug.Log(" Loading Patch: " + GetType().ToString());
        var harmony = HarmonyInstance.Create(GetType().ToString());
        // harmony.PatchAll(Assembly.GetExecutingAssembly());
        CSutils.Harmonys.ApplyOnce(harmony, Assembly.GetExecutingAssembly(), GetType());
    }
    /// TODO: modify entity.position in prefix an re set in postfix
   // static bool Prefix(BlockModelTree _instance, bool __result, ref int _entityId) { // Returns true for the default PlaceBlock code to execute. If it returns false, it won't execute it at all.
    static bool v0_Prefix(bool __result, ref int _entityId) { 
       Debug.Log("RandomTreeFall startToFall");
       _entityId = 1;
       return true;
    }


    static int ShowSkillLevel(Entity entity) {
        EntityPlayer player = entity as EntityPlayer;
        if (player==null) return -1;
        int k=-1;
        foreach(KeyValuePair<string, ProgressionValue> entry in player.Progression.ProgressionValues) {
            Debug.Log(String.Format("SkillLevel of {0}: {1} {2}", player, entry.Key, entry.Value));
            Debug.Log(String.Format("      ->      {0}: {1} {2}", entry.Value.Name, entry.Value.ProgressionClass, entry.Value.Level));
            Debug.Log(String.Format("      ->      {0}: {1} {2}", entry.Value.ProgressionClass.IsPerk, entry.Value.ProgressionClass.IsSkill, entry.Value.ProgressionClass.IsAttribute));
            k++;
            if (k> 20) break;
            // Key and Name are eg "perkhiddenstrike" for perks 
            // "attrstrength"
        }
        return 18;
    }

    static int SkillLevel(Entity entity) {
        EntityPlayer player = entity as EntityPlayer;
        if (player==null) return -1;
        int lvl = player.Progression.ProgressionValues["perkMiner69r"].Level;
        return lvl;
    }


    private static Vector3 Altered(int level, Vector3i blockPos, Vector3 entPos) {
        Vector3 pos = Vectors.ToFloat(blockPos) + new Vector3(0.5f, 0f, 0.5f); // grid V3i to V3 center ???
        Vector3 baseDir = entPos - pos;
        baseDir.y = 0f;
        baseDir = baseDir.normalized;
        level = Math.Max(0, level);
        GameRandom gameRandom = GameManager.Instance.World.GetGameRandom();
        if (gameRandom.RandomRange(0f, 1f) < 0.4 / (1+level)) return pos - baseDir * 2f; // opposite
        if (gameRandom.RandomRange(0f, 1f) < 0.4 / (1+level)) {
            Vector3 rdm = new Vector3(gameRandom.RandomRange(0f, 1f), 0f, gameRandom.RandomRange(0f, 1f));
            return pos + rdm.normalized * 2f;
        }
        return entPos;
    }

   static void Prefix(bool __result, ref Tuple<Entity, Vector3> __state, WorldBase _world, Vector3i _blockPos, int _entityId) { 
       Debug.Log("RandomTreeFall Prefix");
       __state = Tuple.Create<Entity, Vector3>(null, new Vector3());
       if (_entityId >= 0) {
            Entity entity = _world.GetEntity(_entityId);
            if (entity != null) {
                ShowSkillLevel(entity);
                __state = new Tuple<Entity, Vector3>(entity, entity.GetPosition());
                Vector3 altered = Altered(SkillLevel(entity), _blockPos, entity.GetPosition());
                entity.SetPosition(altered);
            }
       }
   }

   static void Postfix(bool __result, Tuple<Entity, Vector3> __state) { 
       Debug.Log("RandomTreeFall Postfix");
       if (__state != null && __state.Item1 != null) {
           __state.Item1.SetPosition( __state.Item2);
       }
   }

}