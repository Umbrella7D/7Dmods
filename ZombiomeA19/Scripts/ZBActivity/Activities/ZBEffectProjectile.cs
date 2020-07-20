using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;
using CSutils;
using Iterating;
using ZBActivity;
using SdtdUtils;
using SdtdUtils.Blocks;

namespace ZBActivity.Projectile {

public class Fire : MultiEffect {
    /*
    Can be "permanent" fire at reproducible positions, if updated fast and long enough

    Could also control heigth for fosse enflammées
    how to define reference height ?
    */
    public Fire(Zone zone) : base(zone) {
        EffectType = EffectType.Environment;
        Printer.Log(60, "Fire() done");
    }
    public override void Effect1(EntityPlayer player, Emplacement place, OptionEffect opt) {
        // Vector3i at = Geo3D.Surface(Vectors.ToInt(place.position));
        Vector3i at = Geo3D.Surface(Vectors.ToInt(place.position), -1, Geo3D.IsGroundOrBuilding);
        float y = 1.1f; // air above surface (+1) + offset
        for (int x=0; x< opt.OptionShape.shape.x; x++) {
            for (int z=0; z< opt.OptionShape.shape.z; z++) {
                Vector3 shift = new Vector3(2*x, y, 2*z); // TODO randomize
                int echos = 1;
                if (ItemClass.GetItem(opt.OptionItem.item, false).ItemClass.Properties.Contains("Replicates")){
                    echos = 3;
                }
                // int echos = (opt.OptionItem.item == "") ? 1 : 3;
                Zombiome.Routines.Start(
                    ThrowItem(Vectors.ToFloat(at) + shift, opt.OptionItem.item, echos),
                    "Fire-ThrowItem"
                );
            }
        }
    }
    private static YieldInstruction echo = new WaitForSeconds(1f);
    public static IEnumerator ThrowItem(Vector3 pos, string item, int rep=1) {
        for (int k=0;k<rep;k++) {
            yield return EffectsItem.spawnItemGhost(item, pos, - Vectors.Float.UnitY);
            yield return echo;
        }
    }
    public override void Configure() {
        Cycler.Set(2.5f); // molotov duration => permanent
        Repeater.Set(-1f);

        // this.opt.OptionItem.item = "ZBProj_fire";
        this.opt.OptionItem.item = ZBiomeInfo.Weighted5(this.biome.envProj, Hashes.Rand(seed, "eproj"));
        this.name = String.Format("{0}-{1}", this.name, this.opt.OptionItem.item);
        // NB I have opt.OptionShape.direction
        opt.OptionShape.shape.x = Hashes.Rand(1, 3, seed,"len");
        opt.OptionShape.shape.y = Hashes.Rand(1, 3, seed,"heigth");
        opt.OptionShape.shape.z = Hashes.Rand(1, 3, seed,"width");

        Printer.Log(60, "Fire Configure: done");
    }
    protected override void Randomize(OptionEffect opt) {
        opt.OptionShape.shape.x = Math.Max(1, opt.OptionShape.shape.x + this.rand.RandomRange(-1, 2));
        opt.OptionShape.shape.y = Math.Max(1, opt.OptionShape.shape.y + this.rand.RandomRange(-1, 2));
        opt.OptionShape.shape.z = Math.Max(1, opt.OptionShape.shape.z + this.rand.RandomRange(-1, 2));
        opt.OptionShape.pace = opt.OptionShape.pace + this.rand.RandomRange(0f, 1f);
    }
}

public class BlockRain : MultiEffect {
    public BlockRain(Zone zone) : base(zone) {
        EffectType = EffectType.Environment;
        Printer.Log(60, "BlockRain() done");
    }
    public override void Effect1(EntityPlayer player, Emplacement place, OptionEffect opt) {
        Vector3i where = place.ipos;
        where = where + rand.RandomRange(30, 50) * Vectors.Up;
        BlockSetter setter = new BlockSetter(opt.OptionBlock);
        Block previous = setter.options.block;
        setter.Apply(where);
        setter.Push();
    }
    public override void Configure() { 
        Repeater.Set(10); // 10 blocks
        Cycler.Set(40); 
        opt.OptionBlock.avoidBlock = true;
        if (Hashes.Rand(0.1f, "rainSlime")) opt.OptionBlock.SetBlocks("waterSlime");
        else opt.OptionBlock.SetBlocks(ZBiomeInfo.GetBlock(seed, biome.ToString())); 
        Printer.Log(60, "BlockRain Configure: done");
    }
    protected override void Randomize(OptionEffect opt) {
        opt.OptionShape.pace = opt.OptionShape.pace + this.rand.RandomRange(0f, 1f);
    }
}

public class SlimeBlocks : MultiEffect {
    public SlimeBlocks(Zone zone) : base(zone) {
        EffectType = EffectType.Environment;
        Printer.Log(60, "SlimeBlocks() done");
    }
    public override void Effect1(EntityPlayer player, Emplacement place, OptionEffect opt) {
        Vector3i where = place.ipos;
        // where = where + rand.RandomRange(30, 50) * Vectors.Up;
        where = where + Vectors.Up;
        BlockSetter setter = new BlockSetter(opt.OptionBlock);
        Block previous = setter.options.block;
        setter.Apply(where);
        setter.Push();
    }
    public override void Configure() { 
        Repeater.Set(10); // 10 blocks
        Cycler.Set(40); 
        opt.OptionBlock.avoidBlock = true;
        opt.OptionShape.shape = new Vector3i(1,1,1);
        opt.OptionBlock.SetBlocks("waterSlime");
        Printer.Log(60, "BlockRain Configure: done");
    }
    protected override void Randomize(OptionEffect opt) {
        opt.OptionShape.pace = opt.OptionShape.pace + this.rand.RandomRange(0f, 1f);
    }
}

public class FireStorm : MultiEffect {
    /*
     Easier to have one enumerator in Effect1
     DONE: ghost dédié

     Lesson: que _Effet1 manage les clones pour tout le monde ?

    Can we rewrite this with copies ?
    */
    public FireStorm(Zone zone) : base(zone) {
        EffectType = EffectType.Environment;
        Printer.Log(60, "FireStorm() done");
    }
    public override void Effect1(EntityPlayer player, Emplacement place, OptionEffect opt) {
        Zombiome.Routines.Start(this._Effect1(player, place, opt), "FireStorm-_Effect1");
    }
    public IEnumerator _Effect1(EntityPlayer player, Emplacement place, OptionEffect opt) {
        // Careful, place.direction is the "to" (or I need a range option somewhere, or natural fadeaway)
        float y = 1.1f; // air above surface (+1) + offset
        // foreach (Emplacement place in Iter.On(Placer.Get(player.GetPosition()))) {
        Printer.Log(40, "FireStorm _Effect1:", place, opt.OptionItem.item); // line manage +5;
        if (place.valid) {
            // Vector3i at = Geo3D.Surface(Vectors.ToInt(place.position));
            Vector3i at = Geo3D.Surface(Vectors.ToInt(place.position), -1, Geo3D.IsGroundOrBuilding);
            for (int x=0; x< opt.OptionShape.shape.x; x++) {
                for (int z=0; z< opt.OptionShape.shape.z; z++) {
                    Vector3 shift = new Vector3(3*x, y, 3*z); // TODO randomize
                    int echos = 1;
                    if (ItemClass.GetItem(opt.OptionItem.item, false).ItemClass.Properties.Contains("Replicates")){
                        echos = 3;
                    }
                    yield return Zombiome.Routines.Start(
                        Fire.ThrowItem(Vectors.ToFloat(at) + shift, opt.OptionItem.item, echos),
                        "FireStorm-spawnItemGhost"
                    );
                } 
            }
        } else {
            Printer.Log(41, "Invalid place:", place, place.valid_msg);
        }
    }
    public override void ConfigurePlacesRepeat() {
        Positions line = new Positions.LineAround(2f);
        Placer = new Placer(line, Positions.Rand.NoCenter(1f));
        Options.Repeaters(this);
    }
    public override void Configure() {
        Repeater.Set(int.MaxValue, 1f); // Line enumerator limits it, vitesse propagation

        // this.opt.OptionItem.item = this.biome.envProj;
        this.opt.OptionItem.item = ZBiomeInfo.Weighted5(this.biome.envProj, Hashes.Rand(seed, "eproj"));
        this.name = String.Format("{0}-{1}", this.name, this.opt.OptionItem.item);

        // NB I have opt.OptionShape.direction
        opt.OptionShape.shape.x = Hashes.Rand(1, 3, seed,"len");
        opt.OptionShape.shape.y = Hashes.Rand(1, 3, seed,"heigth");
        opt.OptionShape.shape.z = Hashes.Rand(1, 3, seed,"width");

        Printer.Log(60, "FireStorm Configure: done");
    }
    protected override void Randomize(OptionEffect opt) {
        opt.OptionShape.shape.x = Math.Max(1, opt.OptionShape.shape.x + this.rand.RandomRange(-1, 2));
        opt.OptionShape.shape.y = Math.Max(1, opt.OptionShape.shape.y + this.rand.RandomRange(-1, 2));
        opt.OptionShape.shape.z = Math.Max(1, opt.OptionShape.shape.z + this.rand.RandomRange(-1, 2));
        // opt.OptionShape.shape = new Vector3i(1,1,1); // debug
        opt.OptionShape.pace = opt.OptionShape.pace + this.rand.RandomRange(0f, 1f);
    }
}


public class Meteorite : MultiEffect {

    public Meteorite(Zone zone) : base(zone) {
        EffectType = EffectType.Environment;
        Printer.Log(60, "Meteorite() done");
    }
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        // Ignore place.direction, regen it
        // Iter.EverySeconds(ref tMonitor, dtMonitor, this.Monitor(player.GetPosition()), place.position);

        Vector3i iwhere = place.ipos;
        iwhere = Geo3D.Surface(iwhere);
        Vector3 speed = - Vectors.Float.UnitY + Vectors.Float.Randomize(rand, 0.5f);
        speed = speed.normalized * (50 + 50 * rand.RandomFloat); // weight 100 
        speed = 0f * speed; // [!]
        float altitude = 25;
        Printer.Log(40, "Meteorite Effect1", iwhere, speed, altitude);
        Vector3 where = Vectors.ToFloat(iwhere) + new Vector3(0f,altitude,0f);
        where.y = 254f; // accelere trop ?
        Zombiome.Routines.Start(EffectsItem.spawnItemGhost(
            opt.OptionItem.item,
            where,
            speed
            // Vectors.Float.Randomize(GameManager.Instance.World.GetGameRandom(), 1f, speed)
        ), "Meteorite-spawnItemGhost");
    }
    public override void Configure() {
        // TODO: randomize from biome (if we convert everything ...)
        this.opt.OptionItem.item = Hashes.Rand(0.5f, "meteoreType", seed) ? "meteoreStone" : "meteoreIron" ;
    }
}

// // 
}  // end namespace
// //