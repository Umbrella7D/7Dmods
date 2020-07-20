using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using CSutils;

// using Ghosts = GhostData;

public class ZBiomeInfo{
    /*
    Biome                Elemental           no element
    =====================================================
    Desert                  F,L                 W

    Burnt                   F                   W

    Forest

    Snow                    W                   F

    Wasteland               R (L) ?
    =====================================================
    */
    public static class Blocks {
        public static string cactus = "treeCactus01,treeCactus02,treeCactus03,treeCactus04,treeCactus05,treeCactus06";
        public static string woodtrap = "trapSpikesWoodDmg2"; //"trapSpikesWoodDmg0,trapSpikesWoodDmg1,trapSpikesWoodDmg2"; // trapSpikesWoodMaster
        public static string irontrap = "trapSpikesIronDmg0,trapSpikesIronDmg1,trapSpikesIronDmg2"; // trapSpikesIronMaster
        // what is trapSpikesNew ?
    }

    public static string Weighted5(string choices, float u = -1f) {
        /* 50, 20, 15, 10, 5. Splitting below 50 might be more useful */
        /* https://www.keithschwarz.com/darts-dice-coins/ */
        if (u < -0.5f) u = Zombiome.rand.RandomFloat;
        string[] split = choices.Split(',');
        if (split.Length != 5) Printer.Write("Error Weighted5 ", split.Length, choices);
        if (u< 0.05) return split[4]; // TODO test most probable first !!
        if (u< 0.15) return split[3];
        if (u< 0.30) return split[2];
        if (u< 0.50) return split[1];
        return split[0];
    }
    public static string Weighted7(string choices, float u = -1f) {
        /* 20, 20, 20,
           15, 15,
           5, 5 
           */
        if (u < -0.5f) u = Zombiome.rand.RandomFloat;
        string[] split = choices.Split(',');
        if (split.Length != 5) Printer.Write("Error Weighted5 ", split.Length, choices);
        if (u< 0.05) return split[4]; // TODO test most probable first !!
        if (u< 0.15) return split[3];
        if (u< 0.30) return split[2];
        if (u< 0.50) return split[1];
        return split[0];
    }

    public static string GetBlock(string seed, string biome) {
        /*
        NB: cactus make no sense for peak etc - a la limite block superieur de la forme ? 
        */
        Printer.Log(50, "GetBlock", seed, biome);
        ZBiomeInfo Biome = ZBiomeInfo.Get(biome);
        Printer.Log(50, "GetBlock", seed, biome, "->", Biome);
        // FIXME! water seems to alter performance. Only use in fast reverse (eg geyser)
        // if (Hashes.Rand(seed, "water") <= 0.1 * Biome.water) return "water"; // only in reverse, maybe only in geyser ?
        if (Hashes.Rand(0.2f, seed, "rock")) return Biome.blockRock;
        // if (Hashes.Rand(0.1f, seed, "trap")) return Biome.blockTrap; // should not use multiblock cactus
        if (Hashes.Rand(0.1f, seed, "build")) return Biome.blockBuild;
        return Biome.blockSoil;
    }

    private static System.Random Random = new System.Random();
    public static string GetDecoProj(string seed, string biome) {
        Printer.Log(50, "GetDecoProj", seed, biome);
        ZBiomeInfo Biome = ZBiomeInfo.Get(biome);
        Printer.Log(50, "GetDecoProj", seed, biome, "->", Biome);
        String[] decoration = Biome.decoProj.Split(',');
        if (decoration.Length==0) return null; 
        if (decoration.Length==1) return decoration[0];
        return decoration[Random.Next(0,decoration.Length)];
    }

    public ZBiomeInfo(string name, float water=1f) {
        this.name = name;
        this.water = water;
        List[this.name] = this;
    }

    public override string ToString() {return this.name;}

    public string name;
    public string zombies;
    public string animals;
    public string blockSoil;
    public string blockRock;
    public string blockBuild;
    public string buffGhost; // ?
    public string[] trees; // get deco dynamically ?
    public string blockTrap; // trap, cactus ...
    public string decoProj;
    public float water= 1f;
    //public string waterBlock= "water"; // terrWaterPOI
    /*
    waterMovingBucket (Count=3):
        coule lentement. descend si support devient air (pas lateralement)
        initial geyser state -> poi
        pas possible pour geyser (s'effondre)

    terrWaterPOI (Count=8):
        Coule vite mais ne s'effondre pas. geyser laisse des chapeaux, ok pour flood (si pas trop couteux)

    */
    public static string force_water_block = "";
    private string _waterBlock;
    public string waterBlock {
        get{
            if (force_water_block != "") return force_water_block;
            return _waterBlock;
        }// "terrWaterPOI"  "waterMovingBucket";
        set{_waterBlock = value;}
    }
    public string fillBag;
    public string envProj;
    private string[] _groundSmokeProjectile;
    public Color groundColor;

    public void groundParticleEffect(Vector3i x) {
        SdtdUtils.EffectsItem.SpawnParticle(Vectors.ToFloat(x), this.groundSmokeProjectileGen(), this.groundColor);
    }

    public string groundSmokeProjectile {
        get{return _groundSmokeProjectile[Random.Next(_groundSmokeProjectile.Length)];}
        set{_groundSmokeProjectile = value.Split(',');}
    }
    public string groundSmokeProjectileGen() {
        return _groundSmokeProjectile[Random.Next(_groundSmokeProjectile.Length)];
    }

    public Weighted<GhostData> Ghost;
    public GhostData particleStorm;

    private static bool Defined = false;
    private static Dictionary<string,ZBiomeInfo> List = new Dictionary<string,ZBiomeInfo>();
    /*
    TODO: buffZBRadiating on ghost disactivated due to Mesh warning
    test on chickenV2 ?
    */
    public static void _Define() {
        Printer.Print("BiomeInfo._Define()");
        ZBiomeInfo desert = new ZBiomeInfo("desert", 0f);
        desert.blockSoil = "terrDesertGround,terrSand,terrSandStone";
        desert.blockTrap = ZBiomeInfo.Blocks.cactus;
        desert.buffGhost = "buffZBShoking";
        desert.decoProj = "rockFragment,ZBProj_treePlainsTree,Boulder,ZBProj_treeCactus03";
        desert.blockRock = "terrStone,terrGravel,terrSandStoneUnstable";
        desert.blockBuild = "brickBlock";
        desert.zombies = "zombieYo";
        desert.animals = "animalZombieVulture,animalSnake";
        desert.fillBag = "resourceBrokenGlass,resourceCrushedSand,terrDirt,terrSand";
        desert.envProj = "ZBProj_sand,ZBProj_electric,ZBProj_eject,ZBProj_ragdoll,ZBProj_poison";
        desert.groundSmokeProjectile = "p_impact_stone_on_plant,p_impact_stone_on_earth";
        desert.groundColor = new Color(218f/255,165f/255,32f/255);
        desert.particleStorm = GhostData.ssandEffect;
        desert.waterBlock = "waterBoil,waterBoil";
        desert.Ghost = Weighted<GhostData>.Of(
            GhostData.fbolt,0.2f,   GhostData.fghost,0.2f,   GhostData.fbomb,0.2f,
            GhostData.ebolt,2f,   GhostData.eghost,2f,   GhostData.ebomb,2f,
            GhostData.ybolt,0.5f,   GhostData.yghost,0.5f,   GhostData.ybomb,0.5f,
            GhostData.lbolt,0.5f,   GhostData.lbomb,0.5f,
            GhostData.bbolt,1f 
        );

        ZBiomeInfo burnt_forest = new ZBiomeInfo("burnt_forest", 0.5f);
        burnt_forest.blockSoil = "terrBurntForestGround,terrDirt";
        burnt_forest.blockTrap = "woodLogSpike1";
        burnt_forest.buffGhost = "buffZBFiring";
        burnt_forest.decoProj = "rockFragment,ZBProj_treePlainsTree,Boulder,ZBProj_treeShrub,ZBProj_treeDeadTree01";
        burnt_forest.blockRock = "terrStone,terrDirt";
        burnt_forest.blockBuild = "burntWoodBlock1,burntWoodBlock2,burntWoodBlock3,burntWoodBlock4";
        burnt_forest.zombies = "zombieMoe";
        burnt_forest.animals = "animalZombieBear";
        burnt_forest.fillBag = "terrDirt,terrSand";
        burnt_forest.envProj = "ZBProj_fire,ZBProj_fireHuman,ZBProj_fireHuman,ZBProj_eject,ZBProj_ragdoll";
        burnt_forest.groundSmokeProjectile = "p_blockdestroy_stone,p_impact_stone_on_earth";
        burnt_forest.particleStorm = GhostData.ssmokeEffect;
        burnt_forest.groundColor = new Color(139f/255,69f/255,19f/255);
        burnt_forest.waterBlock = "waterBoil,waterBoil";
        burnt_forest.Ghost = Weighted<GhostData>.Of(
            GhostData.fbolt,2f,   GhostData.fghost,2f,   GhostData.fbomb,2f,
            GhostData.ebolt,0.1f,
            GhostData.ybolt,0.5f,   GhostData.yghost,0.5f,   GhostData.ybomb,0.5f,
            GhostData.lbolt,0.3f,   GhostData.lbomb,0.3f,
            GhostData.bbolt,1f
        );

        ZBiomeInfo pine_forest = new ZBiomeInfo("pine_forest");
        pine_forest.blockSoil = "terrForestGround";
        pine_forest.blockTrap = ZBiomeInfo.Blocks.woodtrap;
        pine_forest.buffGhost = "buffZBRadiating,buffZBShoking,buffZBFiring";
        pine_forest.decoProj = "rockFragment,ZBProj_treePlainsTree,Boulder,ZBProj_treeShrub,ZBProj_treeOakSml01,ZBProj_treeOakLrg01,ZBProj_treePlantedOak41m,ZBProj_treeMountainPine12m";
        pine_forest.blockRock = "terrStone";
        pine_forest.blockBuild = "woodFrameMaster";
        pine_forest.zombies = "zombieFarmer";
        pine_forest.animals = "animalWolf,animalMountainLion";
        pine_forest.fillBag = "resourceWood,resourceClayLump";
        pine_forest.envProj = "ZBProj_fire,ZBProj_eject,ZBProj_sand,ZBProj_ragdoll,ZBProj_poison"; 
        pine_forest.groundSmokeProjectile = "p_treeGib_birch_small,p_impact_stone_on_plant,p_impact_stone_on_plant";  // p_treeGib_birch_6m is too big ?
        pine_forest.particleStorm = GhostData.ssmokeEffect;
        pine_forest.groundColor = new Color(34f/255,139f/255,34f/255); 
        pine_forest.waterBlock = "terrWaterPOI,terrWaterPOI,terrWaterPOI,terrWaterPOI,waterBoil";
        pine_forest.Ghost = Weighted<GhostData>.Of(
            GhostData.fbolt,1f,   GhostData.fghost,1f,   GhostData.fbomb,1f,
            GhostData.ybolt,1f,   GhostData.yghost,1f,   GhostData.ybomb,1f,
            GhostData.lbolt,1f,   GhostData.lbomb,1f,
            GhostData.bbolt,1f 
        );

        ZBiomeInfo snow = new ZBiomeInfo("snow", 2f);
        snow.blockSoil = "terrSnow";
        snow.blockTrap = "snowStalagmite";
        snow.buffGhost = "buffZBRadiating,buffZBShoking";
        snow.decoProj = "rockFragment,ZBProj_treePlainsTree,Boulder,ZBProj_treeShrub,ZBProj_treeWinterPine13m,ZBProj_treeWinterPine28m";
        snow.blockRock = "terrStone,terrAsphalt";
        snow.blockBuild = "flagstoneBlock";
        snow.zombies = "zombieSnow";
        snow.animals = "animalBear";
        snow.fillBag = "resourceSnowBall";
        snow.envProj = "ZBProj_freeze,ZBProj_electric,ZBProj_eject,ZBProj_ragdoll,ZBProj_freeze";
        snow.groundSmokeProjectile= "p_treeGib_winter01,p_paint_splash2,p_impact_bullet_on_snow,p_blockdestroy_snow";
        snow.particleStorm = GhostData.ssnowEffect;
        snow.groundColor = new Color(176f/255,224f/255,230f/255); 
        snow.waterBlock = "waterFreeze,waterFreeze,waterFreeze,waterBoil,terrWaterPOI";
        snow.Ghost = Weighted<GhostData>.Of(
            GhostData.ebolt,1f,   GhostData.eghost,1f,   GhostData.ebomb,1f,
            GhostData.ybolt,1f,   GhostData.yghost,1f,   GhostData.ybomb,1f,
            GhostData.lbolt,1f,   GhostData.lbomb,1f 
        );


        ZBiomeInfo wasteland = new ZBiomeInfo("wasteland");
        wasteland.blockSoil = "terrDestroyedStone"; 
        wasteland.blockTrap = string.Join(",", ZBiomeInfo.Blocks.irontrap, "barbedWireSheet"); 
        wasteland.buffGhost = "buffZBRadiating";
        wasteland.decoProj = "rockFragment,ZBProj_treePlainsTree,Boulder,ZBProj_treeShrub,ZBProj_treeOakSml01,ZBProj_treeMountainPine12m,ZBProj_treeDeadTree01";
        wasteland.blockRock = "terrStone,terrDestroyedStone,terrConcrete";
        wasteland.blockBuild = "concreteBlock,scrapMetalPile";
        wasteland.zombies = "zombieLab";
        wasteland.animals = "animalZombieDog";
        wasteland.fillBag = "resourceScrapPolymers";
        wasteland.envProj = "ZBProj_spark,ZBProj_poison,ZBProj_spark,ZBProj_eject,ZBProj_sand";
        wasteland.groundSmokeProjectile = "p_impact_wood_on_earth,p_blockdestroy_metal";
        wasteland.particleStorm = GhostData.ssandEffect;
        wasteland.groundColor = new Color(188f/255,143f/255,143f/255); 
        wasteland.waterBlock = "terrWaterPOI,terrWaterPOI,waterSlime";
        wasteland.Ghost = Weighted<GhostData>.Of(
            GhostData.fbolt,0.2f,   GhostData.fghost,0.2f,   GhostData.fbomb,0.2f,
            GhostData.ebolt,1f,   GhostData.eghost,1f,   GhostData.ebomb,1f,
            GhostData.ybolt,1f,   GhostData.yghost,1f,   GhostData.ybomb,1f,
            GhostData.lbolt,1f,   GhostData.lbomb,1f,
            GhostData.bbolt,1f 
        );

        Defined = true;
        Printer.Print("ZBiomeInfo keys:", string.Join(",", List.Keys));

    }
    public static ZBiomeInfo Get(string key) {
        if (! ZBiomeInfo.List.ContainsKey(key)) {
            Printer.Print("BiomeInfo Error !", key, " ? ", String.Join("+", ZBiomeInfo.List.Keys));
        } 
        return List[key];
    }
    public static void Define() {
        if (! Defined) _Define();
    }
}

