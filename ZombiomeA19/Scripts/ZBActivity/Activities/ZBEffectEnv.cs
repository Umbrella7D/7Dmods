using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;

using Iterating;
using ZBActivity;

using CSutils;
using SdtdUtils;

/* Connect to esthetic particles:
Create a utils MultiEffect to Spawn articles, called from here

*/

namespace ZBActivity.Environment {

public static class ZBSounds {
    /** Play: the returned IEnumerator is useless (avoid Routine.Call in start multi),
    but it starts the interesting one !
    */
    public static System.Random Rand = new System.Random();   
    private static long last = 0;
    public static IEnumerator Play(string sound, Vector3 pos, EntityPlayer player, World World = null,
                            int _SetSoundMode = 1, int reduce=0, float rate = 1f) {
        return Play(new string[]{sound}, pos, player, World, _SetSoundMode, reduce, rate);
    }
    public static IEnumerator Play(string[] sounds, Vector3 pos, EntityPlayer player, World World = null,
                            int _SetSoundMode = 1, int reduce=0, float rate = 1f) {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (now - last > 5) {
            last = now;
            Zombiome.Routines.Start(            
                Loop(sounds, pos, player, World, _SetSoundMode, reduce, rate),
                "ZBSounds"
            );
        }
        yield break;
    }

    private static YieldInstruction second = new WaitForSeconds(1f);
    private static IEnumerator Loop(string[] sounds, Vector3 pos, EntityPlayer player, World World = null,
                            int _SetSoundMode = 1, int reduce=0, float rate = 1f) {
        for (int k=0; k<5; k++) {
            if (rate < 1 && Rand.NextDouble() >= rate) {}
            else {
                string sound = sounds[Rand.Next(0,sounds.Length)];
                PlayZBSound(sound, pos, player, World, _SetSoundMode, reduce);
            }            
            yield return second;
        }       
    }

    public static void PlayZBSound(string sound, Vector3 pos, EntityPlayer player, World World = null,
                            int _SetSoundMode = 1, int reduce=0) {
        if (World==null) World = GameManager.Instance.World;
        if (EffectsItem.SetSoundMode > -1) _SetSoundMode = EffectsItem.SetSoundMode;
         // unnoisy
        if (_SetSoundMode == 0) World.GetGameManager().PlaySoundAtPositionServer(pos, sound, AudioRolloffMode.Custom, 300); 
        // less noisy
        else if (_SetSoundMode == 1) World.GetGameManager().PlaySoundAtPositionServer(pos, sound, AudioRolloffMode.Linear, 1 + reduce); 
         // too noisy
        else if (_SetSoundMode == 2) Audio.Manager.BroadcastPlay(player, sound);
        // Much less unnoisy
        else Audio.Manager.BroadcastPlay(pos, sound, 0f);     
    }

}

public class Particles : MultiEffect {
    private string particle;
    private Bounds colors;
    public Particles(Zone zone, string particle, Bounds colors) : base(zone) {
        EffectType = EffectType.Environment;
        this.particle = particle;
        this.colors = colors;
        this.ApplyConfigure();
    }
    private bool running = false;
    public override IEnumerator _Next(EntityPlayer player) {
        // yield break; // DEBUG desactivated for log pollution (big smoke particles require entity )
        if (running) yield break;
        running = true;
        yield return base._Next(player);
        running = false;
    }
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        Vector3 color = Vectors.Float.RandomIn(colors.min, colors.max);
        Printer.Log(31, "ZBParticles", particle, color);
        SdtdUtils.EffectsItem.SpawnParticle(place.position, particle, new Color(color.x, color.y, color.z, 0.2f));
    }
    public override void Configure() {
        Placer.positions = new Positions.Rand((float) (ZChunk.size) / 2f);
    }
}

public class Gravity : AtEntities {
    private string[] buffs = new string[]{"buffGravity", "buffGravityStrong", "buffGravity", "buffAntiGravity", "buffEjectable", "buffEjectable"};
    private Particles Particles;
    public Gravity(Zone zone) : base(zone) {
        EffectType = EffectType.Environment;
        Particles = new Particles(zone, "blockdestroy_boulder", BoundsUtils.BoundsForMinMax(
            0.5f, 0.1f, 0.1f,
            1f  , 0.2f, 0.2f
        ));
        Printer.Log(60, "Gravity() done");
    }
    public override IEnumerator Apply(EntityPlayer player, EntityAlive target, OptionEffect opt) {
        Zombiome.Routines.Start(Particles._Next(player), "Gravity-Particle");
        Printer.Log(40, "Gravity() AddBuff", target);
        target.Buffs.AddBuff(this.opt.OptionEntity.buff); // avant c'était juste player
        player.Buffs.AddBuff(this.opt.OptionEntity.buff); // easier debug
        yield break;
    }
    public override void Configure() {
        this.opt.OptionEntity.buff = Hashes.Rand(buffs, seed, "buff");
        Printer.Log(60, "Gravity Configure: done");
    }
}

public class Jumping : AtEntities {
    // Green smoke looks like poison ... better use leaves
    private Particles Particles;
    public  Jumping(Zone zone) : base(zone) {
        EffectType = EffectType.Environment;
        base.rate = 0.2f;
        Particles = new Particles(zone, "treeGib_birch_small", BoundsUtils.BoundsForMinMax( // treeGib_small_dust
            0.1f, 0.5f, 0.1f,
            0.2f, 1f  , 0.2f
        ));
        Printer.Log(60, "Jumping() done");
    }
    public override IEnumerator Apply(EntityPlayer player, EntityAlive target, OptionEffect opt) { 
        Zombiome.Routines.Start(Particles._Next(player), "Jumping-Particles");
        // EntityMover mover = new EntityMover(2, 0.2f, 1); // randomize
        SdtdUtils.EffectsItem.SpawnParticle(target.GetPosition(), "treeGib_birch_small", biome.groundColor);
        int len = 5 + (int) (this.rand.RandomFloat * 5);
        EntityMover mover = new EntityMover(len, 0.5f, 1); // tres bien : petit saut
        mover.Apply(target, Vectors.Float.UnitY);
        yield break;
    }
    public override void Configure() {
        Printer.Log(60, "Jumping Configure: done");
    }
}

public class DwarfPlayer : AtPlayer {
    // private Particles Particles;
    private ZBActivity.Entities.Ghost ParticleGhosts;
    public  DwarfPlayer(Zone zone) : base(zone) {
        // EffectType = EffectType.Environment;
        // Particles = new Particles(zone, "treefall", BoundsUtils.BoundsForMinMax(
        //     0.1f, 0.1f, 0.5f, 
        //     0.2f, 0.2f, 1f 
        // ));
        ParticleGhosts = new ZBActivity.Entities.Ghost(zone);
        ParticleGhosts.ApplyConfigure();
        Printer.Log(60, "DwarfPlayer() done");
    }
    public override void Effect1(EntityPlayer player, Emplacement place, OptionEffect opt) {
        // Zombiome.Routines.Start(Particles._Next(player)); 
        player.Buffs.AddBuff("buffZBDwarf");
        ParticleGhosts.ManageParticle(player, GhostData.csmokeEffect);
    }
    public override void Configure() {
        this.opt.OptionEntity.buff = "buffZBDwarf";
        Printer.Log(60, "DwarfPlayer Configure: done");
    }
}

public class AttractivePlayer : AtPlayer {

    private ZBActivity.Entities.Ghost ParticleGhosts;
    public AttractivePlayer(Zone zone) : base(zone) {
        // EffectType = EffectType.Environment;
        // Particles = new Particles(zone, "treefall", BoundsUtils.BoundsForMinMax(
        //     0.1f, 0.1f, 0.5f, 
        //     0.2f, 0.2f, 1f 
        // ));
        ParticleGhosts = new ZBActivity.Entities.Ghost(zone);
        ParticleGhosts.ApplyConfigure();
        Printer.Log(60, "AttractivePlayer() done");
    }
    public override void Effect1(EntityPlayer player, Emplacement place, OptionEffect opt) {
        // Zombiome.Routines.Start(Particles._Next(player)); 
        player.Buffs.AddBuff("buffZBLighten");
        ParticleGhosts.ManageParticle(player, GhostData.lightEffect);
    }
    public override void Configure() {
        this.opt.OptionEntity.buff = "buffZBLighten";
        Printer.Log(60, "DwarfPlayer Configure: done");
    }
}

public class RandomSize : AtEntities {
    private Particles Particles;
    private string[] buffs = new string[]{"buffSmall50", "buffSmall80", "buffBig30", "buffBig70"};
    public RandomSize(Zone zone) : base(zone) {
        Particles = new Particles(zone, "wire_tool_sparks", BoundsUtils.BoundsForMinMax( // treeGib_birch_small
            0.1f, 0.6f, 0.5f, 
            0.2f, 0.7f, 1f 
        ));
        EffectType = EffectType.Environment;
        Printer.Log(60, "RandomSize() done");
    }
    public override IEnumerator Apply(EntityPlayer player, EntityAlive target, OptionEffect opt) {
        Zombiome.Routines.Start(Particles._Next(player), "RandomSize-Particles");
        if (target is EntityZombie || target is EntityAnimal) {
            if (! target.IsDead()) {
                SdtdUtils.EffectsItem.SpawnParticle(target.GetPosition(), "wire_tool_sparks",
                                                    biome.groundColor, "electric_fence_impact");
                int index = rand.RandomRange(buffs.Length);
                // target.Buffs.AddBuff(buffs[index]);
                // rm others, otherwise they would set back scaling=1 when gone but the neew buff still runs
                for (int k=0; k<buffs.Length; k++) {
                    if (k != index && target.Buffs.HasBuff(buffs[k])) target.Buffs.RemoveBuff(buffs[k]);
                }
                target.Buffs.AddBuff(buffs[index]);
            }
        }
        yield break;
    }
    public override void Configure() {
        this.opt.OptionEntity.buff = string.Join(",", buffs); //"buffSmallest,buffGiant";
        Printer.Log(60, "RandomSize Configure: done");
    }
}




public class Slip : AtEntities {
    private string buff = "buffRagdoll";
    public Slip(Zone zone) : base(zone) {
        EffectType = EffectType.Environment;
        base.rate = 0.2f;
        Printer.Log(60, "Slip() done");
    }
    public override IEnumerator Apply(EntityPlayer player, EntityAlive target, OptionEffect opt) {
        // TODO: randomize chance, and only on ice or snow ? 
        // check is block below is snow
        Vector3 dir = Vectors.Float.Randomize(Zombiome.rand, 1f);
        dir = dir.normalized;
        dir.y = 0.1f;
        Vector3i at = Vectors.ToInt(target.GetPosition());
        if (World.GetBlock(at).type == BlockValue.Air.type && World.GetBlock(at - Vectors.Up).type == BlockValue.Air.type)
            yield break; // target is not on ground  // todo: snow only

        Vector3 motion = EffectsEntity.MoveDir(target);
        if (motion.magnitude <= 0.3f) yield break;
        Vector3 slideDir = Vectors.Float.Randomize(rand, 1f, motion.normalized).normalized;
        slideDir.y = 0.05f;
        EntityMover mover = new EntityMover(2, 0.2f, 1); // .Config(1);
        yield return mover.Move(target, slideDir);
        target.Buffs.AddBuff("buffRagdoll");
        if (target is EntityPlayerLocal) GameManager.ShowTooltip((EntityPlayerLocal) target, "You slipped !");
    }
    public override void Configure() {
        Printer.Log(60, "Slip Configure: done");
    }
}


public class MovingSands0 : AtEntities {
    /* Use buff to avoid loop

    */
    private string buff = "buffMarkerMovingSands";
    public MovingSands0(Zone zone) : base(zone) {
        EffectType = EffectType.Environment;
        Printer.Log(60, "MovingSands() done");
    }
    public override IEnumerator Apply(EntityPlayer player, EntityAlive target, OptionEffect opt) {
        if (target.Buffs.HasBuff(buff)) yield break;
        Vector3 dir = -0.5f * Vectors.Float.UnitY;
        EntityMover mover = new EntityMover(1); // .Config(1);
        yield return mover.Move(target, dir);
        target.Buffs.AddBuff(buff);
    }
    public override void Configure() {
        Printer.Log(60, "MovingSands Configure: done");
    }
}

public class MovingSands : AtEntities {
    /* Use ground check to avoid loop

    */
    public  MovingSands(Zone zone) : base(zone) {
        EffectType = EffectType.Environment;
        Printer.Log(60, "MovingSands() done");
    }
    public override IEnumerator Apply(EntityPlayer player, EntityAlive target, OptionEffect opt) {
        Vector3 tpos = target.GetPosition();
        Vector3 s = Geo3D.Surface(tpos);
        if (s.y <= 1) yield break;
        float dy = tpos.y - s.y;
        if (dy >= 0.5f) yield break;
        if (dy <= -0.5f) yield break;
        Vector3 dir = -0.3f * Vectors.Float.UnitY;
        EntityMover mover = new EntityMover(1); // .Config(1);
        yield return mover.Move(target, dir);
    }
    public override void Configure() {
        Printer.Log(60, "MovingSands Configure: done");
    }
}

// // 
}  // end namespace
// //