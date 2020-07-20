using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;


using CSutils;
using ZBActivity;

public class Zone {
    /** Geometric unit on the map that contains a unique Zombiome activity

    - Squares. Activity is generated only at the 4 zones adjacent to the player.
    - Center + distance to the center


    */
    public static int ZoneSize = 196;  // width of Zone, in blocks
    private static int nZone = 1000; // must be above mapsize / ZoneSize
    private static long dtForget = 30; // seconds
    private static bool _CanForget(int key, Zone value) {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        return value.effect.last_access < now - dtForget;
    } 
    private static CSutils.ForgettingDict<int,Zone> AllZones = new CSutils.ForgettingDict<int,Zone>(_CanForget);
    public static void ClearCache() { // For command ZBselect
        AllZones.Clear();
    }
    public static Zone GetSingle(Vector3 position) { 
        // Printer.Print("Zone Get", position);
        int xi = (int) Math.Floor(position.x / ZoneSize); // coordonnées du bloc
        int zi = (int) Math.Floor(position.z / ZoneSize);
        int _index = xi + nZone * zi;
        if (! AllZones.ContainsKey(_index)) {
            Zone created = new Zone(xi, zi);
            Printer.Log(62, "GetSingle returned", created);
            AllZones[_index] = created;
        }
        Printer.Log(62, "Zone Get dict _index", _index);
        Printer.Log(62, "Zone Get in dict", AllZones[_index]);
        return AllZones[_index];
    }
    public static Zone[] GetFour(Vector3 position) { 
        Vector3i cp = ZChunk.TL4(position, ZoneSize);
        Zone[] zones = new Zone[4];
        for (int adj=0; adj<4; adj++) {
            // foreach (Vector3i offset in ZChunk.adjacents) {
            Vector3i zca = cp + ZChunk.adjacents[adj];
            int _index = zca.x + nZone * zca.z;
            if (! AllZones.ContainsKey(_index)) {
                Zone created = new Zone(zca.x, zca.z);
                Printer.Log(62, "GetFour returned", created);
                AllZones[_index] = created;
            }
            zones[adj] = AllZones[_index];
            Printer.Log(62, "Zone Get dict _index", _index);
            Printer.Log(62, "Zone Get in dict", AllZones[_index]);
        }
        return zones;
    }
    public static Func<Vector3,Zone[]> Get = position => new Zone[]{GetSingle(position)};
    // public static Func<Vector3,Zone[]> Get = GetFour;

    public static BiomeDefinition GetBiomeProvider(int x, int z) {
        /* World.GetBiome(x,z) returns null when chunk isn't loaded.
        I could default to player pos (maybe until biome is known), but breaks reproducibility 
        So let's use GetBiomeProvider instead. I don't think biomes change dynamically ? 
        */
        BiomeDefinition bd = GameManager.Instance.World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt(x, z);
        return bd; // Seems to be null only if outside world boundaries
    }


    /* Zone: non-static */
    public string seed;
    public int x;
    public int z;
    public int radius = 50;
    public BiomeDefinition biomeDef;
    public ZBiomeInfo biome;
    bool biomeFromPlayer = false;
    float intensity = 0f;
    float difficulty = 0f;
    ZBEffect effect;

    public Zone(int xi, int zi) { 
        /** Zone generated with reproducible randomness from: seed, size and (x,z)-position 
        - x,z are NW corner of the zone
        - biome arg should be null (force it in debug mode)
        **/
        this.seed = String.Format("Zone{0}_{1}_{2}_{3}", Zombiome.worldSeed, Zombiome.worldSize, xi, zi); // receives center
        /* Geometry */
        this.x = Hashes.Rand(xi * ZoneSize, (xi + 1) * ZoneSize, seed, "xcenter");
        this.z = Hashes.Rand(zi * ZoneSize, (zi + 1) * ZoneSize, seed, "zcenter");
        this.radius = CSutils.Hashes.Rand(20, ZoneSize/2, seed, "radius");
        /* Biome (TODO:robustify: get at a few other points) */
        biomeFromPlayer = false;
        if (true) { // (biome == null) {
            this.biomeDef = GetBiomeProvider(this.x, this.z);
            if(this.biomeDef == null) {
                Printer.Write("Error NULL BiomeDefinition at", this.x, this.z, "   ", xi, zi);
                Printer.Print("Error NULL BiomeDefinition at", this.x, this.z, "   ", xi, zi);
                Printer.Print("player at ", GameManager.Instance.World.GetLocalPlayers()[0].GetPosition());
            } else {
                Printer.Print("GetBiomeProvider", this.x, this.z, this.biomeDef, this.biomeDef==null);
            }
        }
        this.biome = ZBiomeInfo.Get(this.biomeDef.ToString());

        effect = ZombiomeActivitySelector.Random(this);
        Printer.Log(64, "Zone instantiated:",this.x, this.z, biome, "seed:",seed, "effect:", effect, " dif/int:", difficulty, intensity);
        this.Log();
    }

    public string Next(EntityPlayer player, Vector3 position) {
        // Printer.Print("Zone Next", player, position, effect);
        Vector3 ppos = player.GetPosition();
        float dist = (float) Math.Sqrt( Math.Pow(ppos.x - this.x, 2f) + Math.Pow(ppos.z - this.z, 2f) );
        dist = Math.Max(dist, radius) - radius;
        float posIntensity = 1f / (1f + 4 * dist / Zone.ZoneSize); // in [1, 1/5], 1/2 at d/D = 1/4
        // in [1, 1/10], 1/2 at d/D = 1/9
        return effect.Next(player, position, posIntensity);
    }

    public override string ToString() {
        return String.Format("Zone({0},{1} @{2} -> {3})", x, z, biomeDef, effect.name);
    }

    public static void LogEffect(ZBEffect effect, ref string data) {
        data = Printer.Cat(data, "\n", "--- EFFECT ---");
        data = Printer.Cat(data, "\n", "effect=", effect);
        data = Printer.Cat(data, "\n", "seed=", effect.seed);
        Printer.Log(35, "Zone.Log EFFECT");

        data = Printer.Cat(data, "\n", "Cycler=", effect.Cycler);
        Printer.Log(35, "Zone.Log Cycler");
        data = Printer.Cat(data, "\n", "Repeater=", effect.Repeater);
        Printer.Log(35, "Zone.Log Repeater");


        data = Printer.Cat(data, "\n"); 
        data = Printer.Cat(data, "\n", "options=", effect.opt);
        Printer.Log(35, "Zone.Log options"); // firestorm:last ok

        // Printer.Print("Zone.Log OptionBlock=", effect.opt.OptionBlock);
        if (effect.opt.OptionBlock == null) {
            data = Printer.Cat(data, "\n", "Block", "OptionBlock is null !");
        } else {
            data = Printer.Cat(data, "\n", "Block", effect.opt.OptionBlock.block);
        }
        Printer.Log(35, "Zone.Log Block");
        data = Printer.Cat(data, "\n", "pace", effect.opt.OptionShape.pace, "shape", effect.opt.OptionShape.shape);
        data = Printer.Cat(data, "\n", " setter   (avB/avE,elastic):",
                        effect.opt.OptionBlock.avoidBlock, effect.opt.OptionBlock.avoidEntity, effect.opt.OptionBlock.elastic);
        Printer.Log(35, "Zone.Log Repeater");
    }
    public void Log(string prefix= "") {
        return;
        String file = String.Format("C:\\Users\\N4TH\\Desktop\\ZBgen\\zb{0}_{1}_{2}.txt", prefix, x, z);
        String data = "";

        Printer.Log(35, "Zone.Log start");
        data = Printer.Cat(data, "\n", "--- ZONE ---");
        data = Printer.Cat(data, "\n", "Pos=", x, z);
        data = Printer.Cat(data, "\n", "seed=", seed);
        data = Printer.Cat(data, "\n", "biome=", biomeDef, "from player=", biomeFromPlayer);
        data = Printer.Cat(data, "\n", "intensity=", intensity, "difficulty=", difficulty);
        Printer.Log(35, "Zone.Log Zone");

        data = Printer.Cat(data, "\n");
        LogEffect(effect, ref data);

        System.IO.File.WriteAllText(@file, data);
    }
}