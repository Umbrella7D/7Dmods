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

namespace ZBActivity.Collapse {

/*
ZChunk.Positions should be wrapped by a Position ?
if we treat all pos in the same Next

how to manage the 4 adjacent blocks ? c'est pas plus mal d'avoir 4 routines en parralèle (wait for entity for ghosts)

Permettrait de choisir rdm ou reproductible (from seed)

Clone and replicate should be reversed if FireStorm has memory (eg clones * 1 front de feu qui repart la ou il s'est arreté)
=> clones should be attribute ... (store their last position)

Currently:
flood: 1 replicate with Effect1 -> 4 blocks -> for all pos
multi effect : k replicates to EFfect1

how to have a distinct component using one or the other ?

Could call Effect1 once per ZChunk, in parralel ? 
*/

// default MultiEffect.configure should have 1 replicate
public class Flood : SingleChunked {   
    private float gen = 0.08f; // NB: 1% * 40*40 = 1%*1600 = 16 // Randomize !
    // private static int WaterId;

    private static bool IsWater(Block block) {
        MaterialBlock mat = block.blockMaterial;
        return mat.IsLiquid && mat.SurfaceCategory == "water";
    }
    public Flood(Zone zone) : base(zone) {
        EffectType = EffectType.Collapse;
        Printer.Log(60, "Flood() done");
        // WaterId = Block.GetBlockByName("water", false).blockID;
    }
    public override IEnumerator Regen(EntityPlayer player, Vector3i zchunk, int iniguess) {
        yield return ZBActivity.Environment.ZBSounds.Play(ZBiomeInfo.NoiseWater, player.GetPosition(), player, World, 2, 0, 0.5f);

        int gen = ZChunk.Size(this.gen);
        Vector3[] positions = ZChunk.Positions(Zombiome.worldSeed, zchunk, gen);

        foreach (Vector3 pos in positions) {
            Printer.Log(40, "Flood regen", zchunk, pos);
            Vector3i surfaced = Geo3D.Surface(Vectors.ToInt(pos), iniguess);
            Emplacement place = Emplacement.At(Vectors.ToFloat(surfaced), Vectors.Float.UnitY);
            /* Generate Emplacement => apply filter from gth */
            int th = GameManager.Instance.World.GetTerrainHeight(surfaced.x, surfaced.z);
            bool go = th > 5;
            if (go) go = surfaced.y < th + Placer.pOffSurface;
            if (go) go = surfaced.y > th - Placer.nOffSurface;
            if (go) go = ! (  IsWater(World.GetBlock(surfaced + Vectors.Up).Block)
                && IsWater(World.GetBlock(surfaced + 2 * Vectors.Up).Block));
            if (go) {
                Printer.Log(40, "Flood at", place);
                // Dont do it if already water, surtout qu'on affaisse la surface !!
                // Cave allows to bound water by ground
                yield return EffectsCollapse.Cave(player, place, opt);
            }
            yield return Repeater.Yield;
        }
    }
    public override void Configure() {
        opt.OptionShape.shape.x = Hashes.Rand(1,6, seed,"len");
        opt.OptionShape.shape.z = Hashes.Rand(1,6, seed,"width");
        opt.OptionShape.shape.y = Hashes.Rand(1,2, seed,"heigth");
        opt.OptionShape.pace = Hashes.Rand(0.01f,3f, seed,"pace");
        opt.OptionShape.reverse = "";

        // string blk = "water"; // not in desert  - use air instead ?
        // string blk = biome.waterBlock;
        string blk = Hashes.Rand(biome.waterBlock.Split(','), seed, "waterType");
        opt.OptionBlock.SetBlocks(blk);
        opt.OptionShape.ground = blk;

        opt.OptionBlock.avoidEntity = false; // TODO: don't push entity on (this) water
        opt.OptionBlock.avoidBlock = false;
        opt.OptionBlock.elastic = 0; //1 fait flotter du ground ... 

        Printer.Log(60, "Flood Configure: done");
    }
    protected override void Randomize(OptionEffect opt) {
        opt.OptionShape.shape.x = Math.Max(1, opt.OptionShape.shape.x + this.rand.RandomRange(0, 2));
        opt.OptionShape.shape.y = Math.Max(1, opt.OptionShape.shape.y + this.rand.RandomRange(0, 5));
        opt.OptionShape.shape.z = Math.Max(1, opt.OptionShape.shape.z + this.rand.RandomRange(0, 3));
    }
}

public class RiftCollapse : MultiEffect {
    public RiftCollapse(Zone zone) : base(zone) {
        EffectType = EffectType.Collapse;
    }
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        Printer.Log(40, "RiftCollapse Effect1", place.position, place.ipos, opt.OptionBlock.blocks, opt.OptionShape.shape);
        // Zombiome.Routines.Start(EffectsCollapse.Rift(player, place, opt), "RiftCollapse");
        Zombiome.Routines.Named("RiftCollapse").Start(
            Routines.Call(biome.groundParticleEffect, place.ipos),
            ZBActivity.Environment.ZBSounds.Play(ZBiomeInfo.NoiseCollapse, place.position, player, World, 1, 20, 0.2f),
            new WaitForSeconds(1f),
            EffectsCollapse.Rift(player, place, opt)
        );
    }
    public override void Configure() {
        // (longueur 1, hauteur (profonfeur), replicats)
        int w = Hashes.Rand(5, 15, seed,"length");
        opt.OptionShape.shape.x = Hashes.Rand(5, 20, seed,"length");
        opt.OptionShape.shape.y = Hashes.Rand(1, 4, seed,"depth");
        opt.OptionShape.shape.z = Hashes.Rand(1, 3, seed,"replicats");

        opt.OptionShape.pace = Hashes.Rand(new float[]{0.005f, 0.1f, 0.2f, 0.5f, 1f, 3f}, seed,"pace");
        opt.OptionShape.reverse = "";
        opt.OptionShape.reverse = opt.OptionShape.reverse + (Hashes.Rand(0.7f, seed, "reverse") ? "U" : "");
        Printer.Log(60, "RiftCollapse Configure: Shape done");


        Printer.Log(60, "RiftCollapse Configure: Block done");
        Options.Physics(seed, biomeDef, opt.OptionBlock);
        opt.OptionBlock.avoidEntity = false;
        opt.OptionBlock.avoidBlock = false;
        opt.OptionBlock.elastic = 0;

        string blk = "air";
        if (Hashes.Rand(0.3f, seed, "useWaters")) {
            blk = Hashes.Rand(biome.waterBlock.Split(','), seed, "waterType");

        }
        opt.OptionBlock.SetBlocks(blk); // todo water // biome.emptyBlock
        Printer.Log(60, "RiftCollapse Configure: Physics done");
    }
    protected override void Randomize(OptionEffect opt) {
        opt.OptionShape.shape.x = Math.Max(1, opt.OptionShape.shape.x + this.rand.RandomRange(-1, 3));
        opt.OptionShape.shape.y = Math.Max(1, opt.OptionShape.shape.y + this.rand.RandomRange(-1, 3));
        opt.OptionShape.shape.z = Math.Max(1, opt.OptionShape.shape.z + this.rand.RandomRange(-1, 3));
        opt.OptionShape.pace = opt.OptionShape.pace + this.rand.RandomRange(0f, 1f);
    }
}

public class Cave : MultiEffect {
    public Cave(Zone zone) : base(zone) {
        EffectType = EffectType.Collapse;
    }
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        Printer.Log(40, "Cave Effect1", place.position, place.ipos, opt.OptionBlock.blocks, opt.OptionShape.shape);
        Zombiome.Routines.Named("Cave").Start(
            Routines.Call(biome.groundParticleEffect, place.ipos),
            ZBActivity.Environment.ZBSounds.Play("light_pipebomb", place.position, player, World, 1, 0, 0.2f),
            new WaitForSeconds(1f),
            EffectsCollapse.Cave(player, place, opt)
        );
    }
    public override void Configure() {
        // (dx,depth,dz)
        // direction ??
        // is the game cslowed down by water ?
        opt.OptionShape.shape.x = Hashes.Rand(1, 5, seed,"dx");
        opt.OptionShape.shape.y = Hashes.Rand(1, 6, seed,"depth");
        opt.OptionShape.shape.z = Hashes.Rand(1, 5, seed,"dz");

        opt.OptionShape.pace = Hashes.Rand(new float[]{0.005f, 0.1f, 0.2f, 0.5f, 1f, 3f}, seed,"pace");
        opt.OptionShape.reverse = "";
        opt.OptionShape.reverse = opt.OptionShape.reverse + (Hashes.Rand(0.7f, seed, "reverse") ? "U" : "");
        Printer.Log(60, "Cave Configure: Shape done");

        opt.OptionBlock.SetBlocks("air");
        opt.OptionShape.ground = (Hashes.Rand(0.8f, seed, "hasGroundTrap")) ? "air" 
                                : Hashes.Rand(biome.blockTrap.Split(','), seed, "groundTrap");
        Printer.Log(60, "Cave Configure: Block done");
        Options.Physics(seed, biomeDef, opt.OptionBlock);
        Printer.Log(60, "Cave Configure: Physics done");
    }
    protected override void Randomize(OptionEffect opt) {
        opt.OptionShape.shape.x = Math.Max(1, opt.OptionShape.shape.x + this.rand.RandomRange(-1, 3));
        opt.OptionShape.shape.y = Math.Max(1, opt.OptionShape.shape.y + this.rand.RandomRange(-1, 3));
        opt.OptionShape.shape.z = Math.Max(1, opt.OptionShape.shape.z + this.rand.RandomRange(-1, 3));
        opt.OptionShape.pace = opt.OptionShape.pace + this.rand.RandomRange(0f, 1f);
    }
}

public class Puit : MultiEffect {
    public Puit(Zone zone) : base(zone) {
        EffectType = EffectType.Collapse;
    }
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        Printer.Log(40, "Puit Effect1", place.position, place.ipos, opt.OptionBlock.blocks, opt.OptionShape.shape);
        Zombiome.Routines.Named("Puit").Start(
            Routines.Call(biome.groundParticleEffect, place.ipos),
            new WaitForSeconds(1f),
            EffectsCollapse.Puit(player, place, opt)
        );
    }
    public override void Configure() {
        // (dx,depth,dz)
        /** */
        opt.OptionShape.shape.x = Hashes.Rand(5, 20, seed,"dx");
        opt.OptionShape.shape.y = Hashes.Rand(1, 6, seed,"depth");
        opt.OptionShape.shape.z = Hashes.Rand(1, 3, seed,"dz");

        opt.OptionShape.pace = Hashes.Rand(new float[]{0.005f, 0.1f, 0.2f, 0.5f, 1f, 3f}, seed,"pace");
        opt.OptionShape.reverse = "";
        opt.OptionShape.reverse = opt.OptionShape.reverse + (Hashes.Rand(0.7f, seed, "reverse") ? "U" : "");
        Printer.Log(60, "Puit Configure: Shape done");

        opt.OptionItem.item = "Boulder";
        opt.OptionBlock.SetBlocks("air"); 
        opt.OptionShape.ground = "water";
        Printer.Log(60, "Puit Configure: Block done");
        Options.Physics(seed, biomeDef, opt.OptionBlock);
        Printer.Log(60, "Puit Configure: Physics done");
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
