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

namespace ZBActivity.Ground {

public class Peak : MultiEffect {

    public static float smokeFreq = 0.05f;
    public Peak(Zone zone) : base(zone) {
        EffectType = EffectType.Ground;
    }
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        Printer.Log(60, "Peak Effect1", place.position, place.ipos, opt.OptionBlock.blocks, opt.OptionShape.shape);
        // FIXME: this syntax cannot be merged !
        Zombiome.Routines.Named("Peak").Start(
            // treeGib_birch_15m creates falling leaves above smoke :(
            Routines.Call(biome.groundParticleEffect, place.ipos), // "treeGib_burnt_small"
            new WaitForSeconds(1f),
            EffectsGround.Peak(player, place, opt)
        );
    }
    public override void Configure() {
        int w = Hashes.Rand(1,4, seed,"width");
        opt.OptionShape.shape.x = opt.OptionShape.shape.z = w;
        opt.OptionShape.shape.y = Hashes.Rand(1,5, seed,"heigth");
        // opt.OptionShape.pace = Hashes.Rand(0.01f,3f, seed,"pace");
        opt.OptionShape.pace = Hashes.Rand(new float[]{0.005f, 0.1f, 0.2f, 0.5f, 1f, 3f}, seed,"pace");
        opt.OptionShape.reverse = "";
        opt.OptionShape.reverse = opt.OptionShape.reverse + (Hashes.Rand(0.7f, seed, "reverse") ? "R" : "");
        Printer.Log(60, "Peak Configure: Shape done");

        string blk = ZBiomeInfo.GetBlock(seed, biome.ToString());
        Printer.Log(60, "Peak Configure: Block=", blk);
        opt.OptionBlock.SetBlocks(blk); // manage multi blocks
        Printer.Log(60, "Peak Configure: Block done");
        // Options.MaybeFloatingObject(seed, opt);
        Options.Physics(seed, biomeDef, opt.OptionBlock);
        Printer.Log(60, "Peak Configure: Physics done");

        opt.OptionBlock.CallBack = biome.groundParticleEffect;
        opt.OptionBlock.RateCallBack = smokeFreq;
    }
    protected override void Randomize(OptionEffect opt) {
        opt.OptionShape.shape.x = Math.Max(1, opt.OptionShape.shape.x + this.rand.RandomRange(-1, 3));
        opt.OptionShape.shape.y = Math.Max(1, opt.OptionShape.shape.y + this.rand.RandomRange(-1, 3));
        opt.OptionShape.shape.z = Math.Max(1, opt.OptionShape.shape.z + this.rand.RandomRange(-1, 3));
        opt.OptionShape.pace = opt.OptionShape.pace + this.rand.RandomRange(0f, 1f);
    }
}

public class Geyser : MultiEffect {
    public Geyser(Zone zone) : base(zone) {
        EffectType = EffectType.Ground;
    }
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        Printer.Log(60, "Geyser Effect1", place.position, place.ipos, opt.OptionBlock.blocks, opt.OptionShape.shape);
        Zombiome.Routines.Named("Geyser").Start(
            // treeGib_birch_15m creates falling leaves above smoke :(
            Routines.Call(EffectsItem.SpawnParticle, place.position, "treeGib_burnt_small"),
            new WaitForSeconds(1f),
            EffectsGround.Peak(player, place, opt)
        );
    }
    public override void Configure() {
        int w = Hashes.Rand(1,2, seed,"width");
        opt.OptionShape.shape.x = opt.OptionShape.shape.z = w;
        opt.OptionShape.shape.y = Hashes.Rand(3,6, seed,"heigth");
        // opt.OptionShape.pace = Hashes.Rand(0.01f,3f, seed,"pace");
        // opt.OptionShape.pace = Hashes.Rand(new float[]{0.005f, 0.1f, 0.2f}, seed,"pace");
        opt.OptionShape.pace = 0.5f;
        // opt.OptionShape.reverse = "U";
        opt.OptionShape.reverse = "R";
        Printer.Log(60, "Geyser Configure: Shape done");

        // string blk = "water";
        // string blk = biome.waterBlock; // "terrWaterPOI"; // 
        string blk = Hashes.Rand(this.biome.waterBlock.Split(','), seed, "waterType");

        // cela s'evapore direct (mais pas de chapeau) - il faut l'autre qui transitionne !
        opt.OptionBlock.SetBlocks(blk);
        Options.Physics(seed, biomeDef, opt.OptionBlock);
        Printer.Log(60, "Geyser Configure: Physics done");
    }
    protected override void Randomize(OptionEffect opt) {
        opt.OptionShape.shape.x = Math.Max(1, opt.OptionShape.shape.x + this.rand.RandomRange(-1, 1));
        opt.OptionShape.shape.y = Math.Max(1, opt.OptionShape.shape.y + this.rand.RandomRange(-1, 1));
        opt.OptionShape.shape.z = Math.Max(1, opt.OptionShape.shape.z + this.rand.RandomRange(-1, 1));
    }
}

public class PeakProjecting : Peak {
    // Project items (block like)
    public  PeakProjecting(Zone zone) : base(zone) {
        EffectType = EffectType.Ground;
    }
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        Printer.Log(40, "PeakProjecting Effect1", place.position, place.ipos, opt.OptionBlock.blocks, opt.OptionShape.shape);
        Zombiome.Routines.Named("PeakProjecting").Start(
            Routines.Call(biome.groundParticleEffect, place.ipos),
            new WaitForSeconds(1f),
            PeakProj(player, place, opt)
        );
    }
    private IEnumerator PeakProj(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        yield return EffectsGround.Peak(player, place, opt);
        for (int k=0; k< 10; k++) {
            Vector3 pos = place.position + (opt.OptionShape.shape.y + 2) * Vectors.Float.UnitY;
            yield return EffectsItem.spawnItemGhost(opt.OptionItem.item, 
                pos,
                3 * (Vectors.Float.UnitY + Placer.directions.Generate(pos).normalized)
            );
            yield return new WaitForSeconds(0.5f);
        }
    }
    public override void Configure() {
        base.Configure();
        string blk = ZBiomeInfo.GetBlock(seed, biome.ToString());
        opt.OptionBlock.SetBlocks("trapSpikesWoodDmg0"); // manage multi blocks
        opt.OptionItem.item = "woodSpikes"; // was using trapSpikesWoodDmg0 which is a block, not an item !
        // opt.OptionItem.item = "" ironSpikes
        opt.OptionBlock.CallBack = biome.groundParticleEffect;
        opt.OptionBlock.RateCallBack = Peak.smokeFreq;
    }
}



public class Rift : Peak {
    // shape; /// (longueur, hauteur, largeur)
    public  Rift(Zone zone) : base(zone) {
        EffectType = EffectType.Ground;
    }
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        Printer.Log(40, "Rift Effect1", place.position, place.ipos, opt.OptionBlock.blocks, opt.OptionShape.shape);
        Zombiome.Routines.Named("Rift").Start(
            Routines.Call(biome.groundParticleEffect, place.ipos),
            new WaitForSeconds(1f),
            EffectsGround.Rift(player, place, opt)
        );
    }
    public override void Configure() {
        opt.OptionShape.shape.x = Hashes.Rand(10,30, seed,"len");
        opt.OptionShape.shape.z = Hashes.Rand(1,4, seed,"width");
        opt.OptionShape.shape.y = Hashes.Rand(1,3, seed,"heigth");
        opt.OptionShape.pace = Hashes.Rand(0.01f,3f, seed,"pace");
        opt.OptionShape.reverse = "";
        opt.OptionShape.reverse = opt.OptionShape.reverse + (Hashes.Rand(0.7f, seed, "reverse") ? "R" : "");

        string blk = ZBiomeInfo.GetBlock(seed, biome.ToString());
        opt.OptionBlock.SetBlocks(blk); // manage multi blocks
        Options.MaybeFloatingObject(seed, opt);
        Options.Physics(seed, biomeDef, opt.OptionBlock);

        opt.OptionBlock.CallBack = biome.groundParticleEffect;
        opt.OptionBlock.RateCallBack = Peak.smokeFreq;
    }
}

public class _Line : Peak {
    public  _Line(Zone zone) : base(zone) {
        EffectType = EffectType.Ground;
    }
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        Printer.Log(40, "_Line Effect1", place.position, place.ipos, opt.OptionBlock.blocks, opt.OptionShape.shape);
        Zombiome.Routines.Named("_Line").Start(
            Routines.Call(biome.groundParticleEffect, place.ipos),
            new WaitForSeconds(1f),
            EffectsGround.LineSurface(place, opt)
        );
    }
    public override void Configure() {
        // shape; /// (avancée, hauteur, largeur)
        opt.OptionShape.shape.x = Hashes.Rand(5, 50, seed,"len");
        opt.OptionShape.shape.y = Hashes.Rand(1,3, seed,"heigth");
        opt.OptionShape.shape.z = Hashes.Rand(3,20, seed,"width");

        opt.OptionShape.pace = Hashes.Rand(0.01f,3f, seed,"pace");
        opt.OptionShape.reverse = "";
        opt.OptionShape.reverse = opt.OptionShape.reverse + (Hashes.Rand(0.7f, seed, "reverse") ? "R" : "");

        string blk = ZBiomeInfo.GetBlock(seed, biome.ToString());
        opt.OptionBlock.SetBlocks(blk); // manage multi blocks
        Options.MaybeFloatingObject(seed, opt);
        Options.Physics(seed, biomeDef, opt.OptionBlock);

        opt.OptionBlock.CallBack = biome.groundParticleEffect;
        opt.OptionBlock.RateCallBack = Peak.smokeFreq;
    }
}

// FIXME
// wave and trapline have hard coded length and pace 

// FIXME: falling blocks
// TODO: select should clear Zone cache
// FIXME: air levitating block
public class Wave : Peak {
    public  Wave(Zone zone) : base(zone) {
        EffectType = EffectType.Ground;
    }

    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        Printer.Log(40, "Wave Effect1", place.position, place.ipos, opt.OptionBlock.blocks, opt.OptionShape.shape);
        Zombiome.Routines.Named("Wave").Start(
            Routines.Call(biome.groundParticleEffect, place.ipos),
            // Routines.Call(__BUG), // TEST
            new WaitForSeconds(1f),
            EffectsGround.Wave(player, place, opt)
        );
    }
    public override void Configure() {
        //  size = E/W=largeur, hauteur, N/S profondeur (portee)
        opt.OptionShape.shape.x = Hashes.Rand(1, 4, seed,"len");
        opt.OptionShape.shape.y = Hashes.Rand(1, 3, seed,"heigth");
        opt.OptionShape.shape.z = Hashes.Rand(3, 20, seed,"width");

        opt.OptionShape.pace = Hashes.Rand(0.01f,3f, seed,"pace");
        opt.OptionShape.reverse = "";
        opt.OptionShape.reverse = opt.OptionShape.reverse + (Hashes.Rand(0.7f, seed, "reverse") ? "R" : "");

        string blk = ZBiomeInfo.GetBlock(seed, biome.ToString());
        opt.OptionBlock.SetBlocks(blk); // manage multi blocks
        Options.MaybeFloatingObject(seed, opt);
        Options.Physics(seed, biomeDef, opt.OptionBlock);

        opt.OptionBlock.CallBack = biome.groundParticleEffect;
        opt.OptionBlock.RateCallBack = Peak.smokeFreq;
    }
}


public class TrapLine : Peak {
    public TrapLine(Zone zone) : base(zone) {
        EffectType = EffectType.Ground;
    }
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        Printer.Log(40, "TrapLine Effect1", place.position, place.ipos, opt.OptionBlock.blocks, opt.OptionShape.shape);
        Zombiome.Routines.Named("TrapLine").Start(
            Routines.Call(biome.groundParticleEffect, place.ipos),
            new WaitForSeconds(1f),
            EffectsGround.TrapLine(player, place, opt)
        );
    }
    public override void Configure() {
        //  // size = E/W=largeur, hauteur, N/S profondeur (portee)
        opt.OptionShape.shape.x = Hashes.Rand(1, 4, seed,"len");
        opt.OptionShape.shape.y = Hashes.Rand(1, 3, seed,"heigth");
        opt.OptionShape.shape.z = Hashes.Rand(3, 20, seed,"width");

        opt.OptionShape.pace = Hashes.Rand(0.01f,3f, seed,"pace");
        opt.OptionShape.reverse = "";
        opt.OptionShape.reverse = opt.OptionShape.reverse + (Hashes.Rand(0.7f, seed, "reverse") ? "R" : "");

        opt.OptionBlock.SetBlocks(biome.blockTrap); // manage multi blocks
        Options.MaybeFloatingObject(seed, opt);
        Options.Physics(seed, biomeDef, opt.OptionBlock);
        opt.OptionBlock.avoidBlock = true; // avoid escaldataion

        opt.OptionBlock.CallBack = biome.groundParticleEffect;
        opt.OptionBlock.RateCallBack = Peak.smokeFreq;
    }
}


public class FloatingDeco : MultiEffect {
    // private string[] decorationsList;
    private HashSet<string> decorationsSet = new HashSet<string>();
    public FloatingDeco(Zone zone) : base(zone) {
        EffectType = EffectType.Ground;
        BiomeDefinition biomeDef = Zone.GetBiomeProvider(centerx, centerz);
        List<string> _blockDeco = new List<string>();
        foreach(BiomeBlockDecoration bbd in biomeDef.m_DistantDecoBlocks) {
            // Printer.Print("m_DistantDecoBlocks:", bbd.m_sBlockName, bbd.m_BlockValue, bbd.m_Prob); 
            string dname = bbd.m_sBlockName; // m_BlockValue
            if (dname.StartsWith("tree") || dname.StartsWith("rock") ) decorationsSet.Add(dname);// _blockDeco.Append(dname);
        }
        if (decorationsSet.Contains("water")) decorationsSet.Remove("water");
        Printer.Log(60, "FloatingDeco Effect1, ndeco=", decorationsSet.Count);
    }
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        World World = GameManager.Instance.World;
        Vector3i ipos = place.ipos;
        Vector3i surf = Geo3D.Surface(ipos, (int) player.GetPosition().y);
        BlockValue existingB = World.GetBlock(surf + Vectors.Up);

        string existing = existingB.Block.ToString();
        if (existingB.type == 0) { // air
            BlockValue insert = GenBV();
            World.SetBlockRPC(0, surf + Vectors.Up, insert);
        } else {
            if (! decorationsSet.Contains(existing)) return;
        }
        Zombiome.Routines.Start(EffectsGround.Peak(player, place, opt), "FloatingDeco-Peak");
    }
    private BlockValue GenBV() {
        int i = rand.RandomRange(decorationsSet.Count);
        string name = decorationsSet.ElementAt(i);
        Block block = Block.GetBlockByName(name, false);
        return new BlockValue((uint) block.blockID);
    }
    public override void Configure() {
        opt.OptionShape.shape.x = opt.OptionShape.shape.z = 1;
        opt.OptionShape.shape.y = Hashes.Rand(1,5, seed,"heigth");
        opt.OptionShape.pace = Hashes.Rand(new float[]{0.5f, 1f, 3f, 5f}, seed,"pace"); // slower bc reverse
        opt.OptionShape.reverse = "R"; // try not to leave airSupport
        Printer.Log(60, "FloatingDeco Configure: Shape done");

        Options.Physics(seed, biomeDef, opt.OptionBlock);
        opt.OptionBlock.SetBlocks("airFull");
        opt.OptionBlock.avoidBlock = false;
        opt.OptionBlock.elastic = 10;
        Printer.Log(60, "FloatingDeco Configure: Physics done");
    }
    protected override void Randomize(OptionEffect opt) {
        opt.OptionShape.shape.x = Math.Max(1, opt.OptionShape.shape.y + this.rand.RandomRange(-1, 1));
        opt.OptionShape.shape.z = Math.Max(1, opt.OptionShape.shape.y + this.rand.RandomRange(-1, 3));
        opt.OptionShape.shape.y = Math.Max(1, opt.OptionShape.shape.y + this.rand.RandomRange(-1, 3));
        opt.OptionShape.offsetSurface = this.rand.RandomRange(-2, 0);
        opt.OptionShape.pace = opt.OptionShape.pace + this.rand.RandomRange(0f, 1f);
    }
}


public class PeakAt : AtEntities {

    // TODO: emprison with avoidEnt=true
    public PeakAt(Zone zone) : base(zone) {
        EffectType = EffectType.Ground;
        Printer.Log(60, "PeakAt() done");
    }
    public override IEnumerator Apply(EntityPlayer player, EntityAlive target, OptionEffect opt) {
        Vector3 pos = target.GetPosition();
        Emplacement place = Emplacement.At(pos, Placer.directions.Generate(pos));
        Printer.Log(40, "Peak Effect1", place.position, place.ipos, opt.OptionBlock.blocks, opt.OptionShape.shape);
        Zombiome.Routines.Named("PeakAt").Start(
            Routines.Call(biome.groundParticleEffect, place.ipos),
            new WaitForSeconds(1f),
            EffectsGround.Peak(player, place, opt)
        );
        yield break;
    }
    public override void Configure() {
        int w = Hashes.Rand(1,4, seed,"width");
        opt.OptionShape.shape.x = opt.OptionShape.shape.z = w;
        opt.OptionShape.shape.y = Hashes.Rand(1,5, seed,"heigth");
        // opt.OptionShape.pace = Hashes.Rand(0.01f,3f, seed,"pace");
        opt.OptionShape.pace = Hashes.Rand(new float[]{0.005f, 0.1f, 0.2f, 0.5f, 1f, 3f}, seed,"pace");
        opt.OptionShape.reverse = "";
        opt.OptionShape.reverse = opt.OptionShape.reverse + (Hashes.Rand(0.7f, seed, "reverse") ? "U" : "");
        Printer.Log(60, "PeakAt Configure: Shape done");

        string blk = ZBiomeInfo.GetBlock(seed, biome.ToString());
        Printer.Log(60, "PeakAt Configure: Block=", blk);
        opt.OptionBlock.SetBlocks(blk); // manage multi blocks
        Printer.Log(60, "PeakAt Configure: Block done");
        Options.Physics(seed, biomeDef, opt.OptionBlock);
        Printer.Log(60, "PeakAt Configure: Physics done");

        // seems the random index is evaluated only once in the lambda
        opt.OptionBlock.CallBack = biome.groundParticleEffect;
        opt.OptionBlock.RateCallBack = Peak.smokeFreq;
    }
    protected override void Randomize(OptionEffect opt) {
        opt.OptionShape.shape.x = Math.Max(1, opt.OptionShape.shape.x + this.rand.RandomRange(-1, 3));
        opt.OptionShape.shape.y = Math.Max(1, opt.OptionShape.shape.y + this.rand.RandomRange(-1, 3));
        opt.OptionShape.shape.z = Math.Max(1, opt.OptionShape.shape.z + this.rand.RandomRange(-1, 3));
        opt.OptionShape.pace = opt.OptionShape.pace + this.rand.RandomRange(0f, 1f);
    }
}



// // 
}  // end namespace
// //
