using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using CSutils;
using SdtdUtils;

namespace SdtdUtils {

public class EntityCreation {
    /*

    ----
    Entity subclasses :

    EntityAlive
        EntityAnimal, EntityCar, EntityEnemy (Animal, Flying, Zombie, Dog), EntityNPC (Bandit, Survivor), EntityPlayer, EntitySupplyCrate, EntityTurret, EntityVehicle (Driveable)
    EntityFallingBlock
    EntityFallingTree
    EntityItem
        EntityBackpack, EntityLootContainer
    EntitySupplyPlane

    /* ******************** Spawn ******************** */

    public class Options {
        public Options Copy() {return this.MemberwiseClone() as Options;}
        public string entity = ""; 
        public string buff = "";
    }

    public static Entity SpawnEntity(Entity player, Emplacement place, Options options) {
        return Spawn(place.position, options.entity);
    }

    public static Entity Spawn(Vector3 pos, string entityName) { // fixme entityID useless here
        int entityClassID = EntityClass.FromString(entityName); // "zombieBoe" "animalSnake"
        Entity entity = EntityFactory.CreateEntity(entityClassID, pos);
        entity.SetSpawnerSource(EnumSpawnerSource.Dynamic);
        // entity.SetSpawnerSource(EnumSpawnerSource.StaticSpawner, 0, entityGroup);
        GameManager.Instance.World.SpawnEntityInWorld(entity);
        // Zombiome.Routines.Start(WaitEntity(entity));
        // Zombiome.Routines.Start(testWait());
        return entity;
    }
    public static Entity Spawn(int entityID, Emplacement place, string entityName) { // fixme entityID useless here
        return Spawn(place.position, entityName);
        // string entityGroup = "ZombiesWasteland"; // zombieFarmer
        // int ClassID = 0;
        // entityID = EntityGroups.GetRandomFromGroup(entityGroup, ref ClassID);
        // if (entityGroup != "") entityID = EntityGroups.GetRandomFromGroup(entityGroup, ref ClassID);
        // else entityID = this.entityIDs[UnityEngine.Random.Range(0, this.entityIDs.Count)]; */
    }


    public static IEnumerator WaitEntity(Entity initial, float extra_delay=-1f, Entity[] track = null) {
        int id = initial.entityId;
        return WaitEntity(id, extra_delay, track);
    }
    public static IEnumerator WaitEntity(int entityId, float extra_delay=-1f, Entity[] track = null) {
        if (track != null) track[0] = null;
        Entity found = null;
        while(true) {
            found = GameManager.Instance.World.GetEntity(entityId);
            if(found != null) {
                // Debug.Log(String.Format("WaitEntity got = {0} {1} -> {2}", entityId, entityId, found));
                break;
            }
            // Debug.Log(String.Format("WaitEntity {0} {1}", entityId, entityId));
            yield return new WaitForSeconds(1f);
        }
        // if (found == null) Printer.Print("WaitEntity missed ", entityId, found);
        if (extra_delay > 0) {
            yield return new WaitForSeconds(extra_delay); // maybe the entity is destroyed right away ?
            found = GameManager.Instance.World.GetEntity(entityId);
            // if (found == null) Printer.Print("WaitEntity lost after delay", entityId, found, extra_delay);
        }
        if (track != null) track[0] = found;
    }

    public static IEnumerator SpawnAndbuff(EntityPlayer player, Emplacement place,
                                            OptionEffect options, Entity[] track = null) {
        /** Spawn and wait for callback before applying buff

        NB: waiting extra dt before applying buff is important
        - Some entities just die, I don't know why. There is nothing at the spawn point,
          they are not too far away from player (terrain heigth is known, althoug it happens more for distant ghosts)
        - Waiting may or may not decrease the failure rate, but it helps the ghost activity to count them and
          avoid many additionnal creations
        - Because Ghost activity yield the waiter, this spread out creation and may help (too many spawn at once ?).
          But is is also costly to wait (more hanging coroutines)

        NB2) the problem is NOT caused by : MyAddParticle, ghost type, ghost small buf, invisibility, GetTerrainHeight check

        */
        Entity Requested = Spawn(place.position, options.OptionEntity.entity);
        int eid = Requested.entityId;
        yield return WaitEntity(Requested, 2f, track);
        Entity ent = GameManager.Instance.World.GetEntity(eid);
        if (ent == null) {
            Debug.Log(String.Format("SpawnAndbuff failed entity {0} {1}", eid, Requested));
            yield break;
        }
        EntityAlive entity = ent as EntityAlive;
        if (ent == null) {
            Printer.Print("Entity is not alive !");
            yield break;
        }
        string buffname = options.OptionEntity.buff;
        if (entity != null) {
            if (entity.Buffs == null) Printer.Print("Entity.buff == null !");
            else if(! entity.Buffs.HasBuff(buffname)) entity.Buffs.AddBuff(buffname);
        }
    }

    public static void Kill(EntityAlive entity) {
        entity.Kill(DamageResponse.New(true));
        // entity.SetDead();
        // entity.DamageEntity(new DamageSource(EnumDamageSource.Internal, EnumDamageTypes.None), 99999, false, 1f);
    }

    // If buff was on entity init, I won't need to wait its spawn. But then I need all combinations in xml
    public static IEnumerator LightningGhost(EntityPlayer player, Emplacement place, OptionEffect options) {
        options.OptionEntity.entity = "zombieMoeGhost";
        options.OptionEntity.buff = "buffZBShoking";
        yield return SpawnAndbuff(player, place, options);
    }

    public static IEnumerator FireGhost(EntityPlayer player, Emplacement place, OptionEffect options) {
        options.OptionEntity.entity = "animalChickenGhostV2";
        options.OptionEntity.buff = "buffZbesRFireEst";
        yield return SpawnAndbuff(player, place, options);
    }

    public static IEnumerator RadiatedGhost(EntityPlayer player, Emplacement place, OptionEffect options) {
        options.OptionEntity.entity = "zombieMoeGhost";
        options.OptionEntity.buff = "buffZBRadiating";
        yield return SpawnAndbuff(player, place, options);
    }

    public static IEnumerator SmokeGhost(EntityPlayer player, Emplacement place, OptionEffect options) {
        // onFire p_onFire
        Entity Requested = Spawn(place.position, "zombieMoeGhost"); // invisibleGhost zombieBoe
        int eid = Requested.entityId;
        yield return WaitEntity(Requested);
        Entity ent = GameManager.Instance.World.GetEntity(eid);
        if (ent == null) {
            Debug.Log(String.Format("SmokeGhost failed entity {0} {1}", eid, Requested));
            yield break;
        }
        EntityAlive entity = ent as EntityAlive;
        string buffname = "buffInfiniteSmoke0";
        if (entity != null) {
            if(! entity.Buffs.HasBuff(buffname)) entity.Buffs.AddBuff(buffname);
        }
    }
}

public class EntityPool {
    /* Update() ip in Entities - may be null !
     Can I have static func w ref arg, and use same code for Pool and Ghost ?
     or multiple call from different places ?
     how to ensure one and wait on it ??
    */
    public Entity[] Entities;
    private string entity;
    private int size;
    // public int currentSize; // can be exposed
    private int indexUpdate = -1;
    private int[] ids; // found? <=> Entities[index]=null? (as long as we keep em sync)
    private bool[] found; // already found once
    private int[] reqTimes;

    public EntityPool(string entity, int size=1) {
        this.entity = entity;
        this.size = size;
        ids = new int[size];
        found = new bool[size];
        reqTimes = new int[size];
        Entities = new Entity[size];
        for (int k=0; k<size; k++) Invalidate(k,"Initialize");
    }
    public void Update(Bounds area) {
        indexUpdate = (indexUpdate + 1) % size;
        Update(area, indexUpdate);
    }
    private void Update(Bounds area, int index) {
        /* Who is assigning null to Entities[0] without setting ids[0] to -1 ?
        check that by copying the public Entities on access ??
        or is it a ref somewhere ??
        */
        string strent = (Entities[index] == null) ? "null" : Entities[index].ToString();
        Printer.Log(26, "EntityPool.Update", index, area, " -> ", ids[index], strent, reqTimes[index], Entities[index] != null);
        // Printer.Print("                 ", strent, "null?", Entities[index] == null, "not-null?", Entities[index] != null);
        // Printer.Print("                 ", Entities[index].ToString());

        // Debug.Log(String.Format(
        //     "EntityPool.Update {0} {1} -> {2} {3} {4}. ent = {5} {6}",
        //      index, area,
        //      ids[index], reqTimes[index], found[index],
        //      strent, Entities[index] == null
        // ));

        if (Entities[index] != null) { // Already found once
            Printer.Log(26, "- Found starting");
            // Entity check = GameManager.Instance.World.GetEntity(Entities[index].entityId);
            Entity check = GameManager.Instance.World.GetEntity(ids[index]);
            Printer.Log(26, "- Found, has:", Entities[index], " check: ", check);
            if (check == null) {Invalidate(index, "LostTrack");} // Lost track of Entity
            else if (check.IsDead()) {Invalidate(index, "IsDead");} // ask a new one
            else if (Entities[index].entityId != ids[index]) {Invalidate(index, "IdMismatch");} // ask a new one
            return; // Entity still there, or just invalidated
        }
        else if (ids[index] != -1) { // Waiting for entity
            Entity hello = GameManager.Instance.World.GetEntity(ids[index]);
            Printer.Log(26, "EntityPool.Update waits", ids[index], hello);
            if (hello != null) {
                Entities[index] = hello;
                found[index] = true;
            }
            else if (found[index]) Invalidate(index, "NowNull"); // null but already found
            else if (DateTime.Now.Millisecond - reqTimes[index] > 1000 * 20) { // 20 sec
                Invalidate(index, "TimeOut");
            }
            return;
        }
        else Request(area, index);
    }
    private void Invalidate(int index, string reason="?") {
        Printer.Log(26, "EntityPool Invalidate", index, "reason=", reason);
        Entities[index] = null;
        ids[index] = -1;
        reqTimes[index] = -1;
        found[index] = false;
    }
    private void Request(Bounds area, int index) {
        Printer.Log(26, "EntityPool Request", index, area);
        Vector3 pos = Vectors.Float.RandomIn(area.min, area.max);
        Vector3 surf = Geo3D.Surface(pos, (int) area.center.y);
        if (surf.y == 0) {
            Printer.Log(26, "EntityPool.Request th =0", area.center);
            return;
        }
        Entity request = EntityCreation.Spawn(
            surf + 1.5f * Vectors.Float.UnitY,
            this.entity
        );
        Entities[index] = null;
        found[index] = false;
        ids[index] = request.entityId;
        reqTimes[index] = DateTime.Now.Millisecond; // TODO: add timeout
        // found[index] = false;
    }
}


////////////////////
} // End Namespace
////////////////////