using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;
using CSutils;
using ZBActivity;

using SdtdUtils;
// using SdtdUtils.EntityCreation;

namespace ZBActivity.Entities {
/* Size:
- for player, restoring size is important, so use buff
- for giant/boss, same
- for Zs it is not that important, and we dont want a buff on everyone, so just store base scale in memory ?
*/

public class Ghost : SingleChunked {
    /* Ghost
    Duration should come from buffZBRespiteKeep that is renew by the ZBEffect
    - renew < buffZBRespiteKeep.duration < activity interval
    - All other buffs should vanish on death

    Different (adjacent) GhostActivity instances don't share their ghost (various concrete classes)

    chicken is smaller (less visible between spawn and decrease size)
    */

    private float gen = 0.01f; // NB: 1% * 40*40 = 1%*1600 = 16
    private float regen = 0.005f;
    private int limit_new = 5;

    public static string ghost_type = ""; // for testing/commands
    public GhostData GhostData;
    private int concreteIdx;

    private bool[] lockRebuff = new bool[]{false};
    public Ghost(Zone zone) : base(zone) {
        EffectType = EffectType.Environment;
        Printer.Log(60, "Ghost() done");
        concreteIdx = EntityGhost.ConcreteIdx(this.centerx, this.centerz);
    }
    public override IEnumerator Regen(EntityPlayer player, Vector3i zchunk, int iniguess) {
        Bounds bounds = ZChunk.Bounds4(zchunk, iniguess);
        List<Entity> existing = GameManager.Instance.World.GetEntitiesInBounds(
            EntityGhost.Concretes[this.concreteIdx],
            bounds,
            new List<Entity>()
        );
        yield return Iterating.Repeater.Frame; // listent may be costly

        Vector3i min = Vectors.ToInt(bounds.min);
        Vector3i max = Vectors.ToInt(bounds.max);

        int current = existing.Count;
        int gen = ZChunk.Size(this.gen);
        // gen = 1; // DEBUG
        int regen = ZChunk.Size(this.regen);
        int iniy = (int) Math.Floor(player.GetPosition().y);

        Zombiome.Routines.Start(Routines.IfNotRunning(
            LockRenew,
            EffectExisting(player, existing)
        ), "Ghost-Existing");

        int nnew = Math.Min(limit_new, gen-current);
        Entity[] Tracker = new Entity[]{null};
        if (current < regen) {
            Printer.Log(45, "Ghost regen", regen, current, gen, "=>", gen-current);
            for (int k=0; k<nnew; k++) {
                Vector3i pos = new Vector3i(rand.RandomRange(min.x, max.x), 0, rand.RandomRange(min.z, max.z));
                pos = Geo3D.Surface(pos, iniy);
                if (GameManager.Instance.World.GetTerrainHeight(pos.x, pos.z) > 2) {
                    Printer.Log(40, "Ghost", pos, opt.OptionEntity.entity, opt.OptionEntity.buff);
                    Emplacement place = Emplacement.At(Vectors.ToFloat(pos) + 2f* Vectors.Float.UnitY, Vectors.Float.UnitY);
                    GhostData gdata = (ghost_type == "") ? this.GhostData : GhostData.Ghosts[ghost_type];
                    yield return EntityGhost.Create(gdata, place, opt.OptionEntity.entity);
                }
                yield return Repeater.Yield;
            }
        }
    }
    private int _existingIndex = -1;
    private bool[] LockRenew = new bool[]{false};
    private IEnumerator EffectExisting(EntityPlayer player, List<Entity> existing) {
        Printer.Log(91, "GhostDyn EffectExisting", existing.Count);
        for (int k=0; k<existing.Count; k++) { 
            yield return Repeater.Yield;
            _existingIndex = (_existingIndex + 1) % existing.Count;
            EntityAlive ghost = existing[_existingIndex] as EntityAlive;
            if (ghost==null) continue;
            Printer.Log(91, "GhostDyn EffectExisting Rebuff", ghost);
            ghost.Buffs.AddBuff("buffZBRespiteKeep"); // keep alive - so igniting buff should be infinite
        }
    } 
    public override void Configure() {
        opt.OptionEntity.entity = String.Format("ghost{0}", concreteIdx);
        Printer.Log(91, "Biome Ghosts: ", this.biome.Ghost);
        this.GhostData = this.biome.Ghost.Gen(Hashes.Rand(this.seed, "ghosttype"));
        this.name = String.Format("{0}-{1}", this.name, this.GhostData.name);
        Printer.Log(91, "GhostData: ", GhostData);
        Printer.Log(60, "Ghost Configure: done");
    }

    private bool[] lockRunExternal = new bool[]{false};
    public void ManageParticle(EntityPlayer player, GhostData ghost) {
        this.GhostData = ghost;
        Zombiome.Routines.Start(Routines.IfNotRunning(lockRunExternal, _Next(player)), "Ghost-Particle");
    }

}


public class MovingGhost : MultiEffect {
    /*
    CURRENTLY DEPERACTED (not updated to new EntityPool) - adding motion buffs to Ghost should be enoug

    DONE: directions alternate in player, random, parralel random
    TODO: depending on inventory affinity ??
    */
    public static string[] ghosts = new string[]{"animalChickenGhostV2"}; 

    private EntityPool EntityPool;
    public MovingGhost(Zone zone) : base(zone) {
        EffectType = EffectType.Environment;
        Printer.Log(60, "MovingGhost() done");
    }
    // private int cursor = -1;
    public override void Effect1(EntityPlayer player, Emplacement place, OptionEffect opt) {
        Vector3 ppos = player.GetPosition();
        Vector3 where = place.position;
        Bounds area = new Bounds(ppos, new Vector3(40, 40, 40));
        EntityPool.Update(area);

        Vector3 sharedDir = place.direction;
        sharedDir.y = 0.2f; // Math.Abs(sharedDir.y); // TODO: constraint in Directions
        EntityMover mover = new EntityMover(3, 0.3f); // .Config(1);
        int mode = 0; // 0nothing, 1random, 2parralel, 3 player
        float u = rand.RandomFloat;
        if (u < 0.4f) mode = 3;
        else if (u < 0.6f) mode = 2;
        else if (u < 0.8f) mode = 1;

        foreach(Entity entity in EntityPool.Entities) {
            if (entity == null) continue;
            // if (entity.IsDead()) continue;
            EntityAlive alive = (EntityAlive) entity;
            if (! alive.Buffs.HasBuff(opt.OptionEntity.buff)) {
                Printer.Log(40, "MovingGhost buffing", opt.OptionEntity.buff, alive);
                alive.Buffs.AddBuff(opt.OptionEntity.buff);
            }
            Vector3 toward = Vectors.Float.Zero;
            if (false) {
                if (mode == 3) {
                    toward = (ppos - alive.GetPosition()).normalized;
                    // toward.y = rand.RandomRange(0.05f, 0.2f);
                    toward.y = 0.2f;
                } else if (u < 0.6) {
                    toward = sharedDir;
                }
                else if (u < 0.8) {
                    toward.x = rand.RandomRange(-1f, 1f); 
                    toward.z = rand.RandomRange(-1f, 1f); 
                    // toward.y = rand.RandomRange(0.05f, 0.2f);
                    toward = toward.normalized;
                    toward.y = 0.2f;
                }
            }
            toward = (ppos - alive.GetPosition()).normalized;
            // toward.y = rand.RandomRange(0.05f, 0.2f);
            toward.y = 0.2f; 
            if (toward.magnitude > 0) {
                // Can we have a single routine that manages all motions ?
                Printer.Log(40, "MovingGhost motion", entity, toward);
                Zombiome.Routines.Start(mover.Move(alive, toward), "MovingGhost");
            }
        }
    }
    public override void Configure() {
        string[] buffs = biome.buffGhost.Split(',');
        // buffs = new string[]{"buffZBShoking"}; // "buffZBFiring"

        // TODO: randomize from biome (if we convert everything ...) 
        opt.OptionEntity.entity = Hashes.Rand(ghosts, seed, "ghost");
        opt.OptionEntity.buff = Hashes.Rand(buffs, seed, "buff");
        EntityPool = new EntityPool(opt.OptionEntity.entity, 30);
        Printer.Log(60, "MovingGhost Configure: done");
    }
}


public class Slippery : AtPlayer {
    public Slippery(Zone zone) : base(zone) {
        EffectType = EffectType.Inventory;
        Printer.Log(60, "Slippery() done");
    }
    public override void Effect1(EntityPlayer player, Emplacement place, OptionEffect opt) {
        player.Buffs.AddBuff("buffSlippery");
    }
    public override void Configure() {
        opt.OptionEntity.buff = "buffSlippery";
        Printer.Log(60, "Slippery Configure: done");
    }
}

public class FillBag : AtPlayer {
    private Ghost ParticleGhosts;
    public FillBag(Zone zone) : base(zone) {
        EffectType = EffectType.Inventory;
        ParticleGhosts = new Ghost(zone);
        ParticleGhosts.ApplyConfigure();
        Printer.Log(60, "FillBag() done");
    }
    public override void Effect1(EntityPlayer player, Emplacement place, OptionEffect opt) {
        // EffectsBag.AddItem(Bag bag, ItemStack stack, bool merge=true);
        // TODO randomize bool
        EffectsInventory.AddToBag(player, opt.OptionItem.item, false);
        ParticleGhosts.ManageParticle(player, biome.particleStorm);
    }
    public override void Configure() {
        opt.OptionItem.item = Hashes.Rand(biome.fillBag.Split(','), seed, "fillbag");
        Printer.Log(60, "FillBag Configure: done");
    }
}

public class Giant : SingleChunked {
    /* 
    DONE: when giant killed immediatly, NPE

    DONE: aura qui détruit block et dmg player, sinon il est stuck dans maison et il pietine pas le j ...
    */

    private EntityPool EntityPool;

    private Vector3 center;
    public Giant(Zone zone) : base(zone) {
        EffectType = EffectType.Environment;
        Printer.Log(60, "Giant() done");
    }
    public override IEnumerator Regen(EntityPlayer player, Vector3i zchunk, int iniguess) {
        Vector3 ppos = player.GetPosition();
        Vector3 where = new Vector3(this.centerx, ppos.y, this.centerz);
        if ((ppos - where).magnitude > 50f) {
            Printer.Log(45, "Giant too far ", this.centerx, this.centerz, "from player ", ppos);
            yield break;
        }
        Bounds area = new Bounds(where, new Vector3(20, 20, 20));
        Printer.Log(40, "EntityPool.Update");
        EntityPool.Update(area);
        if (EntityPool.Entities[0] != null) {
            EntityAlive giant = (EntityAlive) EntityPool.Entities[0];
            // could be null if not alive ?!
            if (giant == null) {
                Printer.Print("giant is not alive !"); 
            } else {
                Printer.Log(40, "giant buff", giant);
                if (giant.IsDead()) yield break;
                if (! giant.Buffs.HasBuff(opt.OptionEntity.buff)) giant.Buffs.AddBuff(opt.OptionEntity.buff);
            }
            yield return new WaitForSeconds(2f);
        }
    }
    public override void Configure() {
        opt.OptionEntity.buff = "buffGiant"; // todo: from biome
        string za = string.Join(",", biome.zombies, biome.animals);
        opt.OptionEntity.entity = Hashes.Rand(za.Split(','), seed, "giantZombie"); // todo: from biome
        EntityPool = new EntityPool(opt.OptionEntity.entity);  // TODO: ref to reflect attibut change ?
        Printer.Log(60, "Giant Configure: done");
    }
}

// // 
}  // end namespace
// //