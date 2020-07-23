//using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

//using System.Linq;
//using System.Reflection.Emit;
using CSutils;

// using ZBActivity;

/* TODO: how to limit per biome ? 


*/


namespace ZBActivity {
public class ZombiomeActivitySelector {

    private static bool V2=true;

    public static string RestrictChoice = ""; // Command "zb select n"
    public static void SelectDebug(string single) {
        RestrictChoice = single;
        if (single.Length > 0) Printer.Print("ZBEffect debug select", single, Activities[single]);
    }

    private static SortedList<string,Func<Zone,ZBEffect>> Activities;
    /* NB SortedList guaranteed i) less memory ii) efficient index access */

    private static Hashes.Weighted<int> ActivitiesWIndex;
    public static void Initialize() {
        SortedList<string,Func<Zone,ZBEffect>> WActivities = new SortedList<string,Func<Zone,ZBEffect>>();
        // I can now associate World in init

        // Ground motion
        WActivities["Peak:5"] = zone => new ZBActivity.Ground.Peak(zone).ApplyConfigure();
        WActivities["Rift:3"] = zone => new ZBActivity.Ground.Rift(zone).ApplyConfigure();
        WActivities["Wave:2"] = zone => new ZBActivity.Ground.Wave(zone).ApplyConfigure();
        WActivities["Geyser:1"] = zone => new ZBActivity.Ground.Geyser(zone).ApplyConfigure();
        WActivities["Slime:1"] = zone => new ZBActivity.Projectile.SlimeBlocks(zone).ApplyConfigure();

        WActivities["PeakAt:1"] = zone => new ZBActivity.Ground.PeakAt(zone).ApplyConfigure();
        WActivities["PeakProjecting:1"] = zone => new ZBActivity.Ground.PeakProjecting(zone).ApplyConfigure();
        
        // Deco
        WActivities["TrapLine:3"] = zone => new ZBActivity.Ground.TrapLine(zone).ApplyConfigure();
        WActivities["FloatingDeco:2"] = zone => new ZBActivity.Ground.FloatingDeco(zone).ApplyConfigure(); // FIXME
        WActivities["MovingDeco:3"] = zone => new ZBActivity.Deco.MovingDeco(zone).ApplyConfigure();
        WActivities["Wind:3"] = zone => new ZBActivity.Deco.FlyDeco(zone).ApplyConfigure();

        // Collapse
        WActivities["Flood:2"] = zone => new ZBActivity.Collapse.Flood(zone).ApplyConfigure();
        WActivities["RiftCollapse:3"] = zone => new ZBActivity.Collapse.RiftCollapse(zone).ApplyConfigure();
        WActivities["Puit:1"] = zone => new ZBActivity.Collapse.Puit(zone).ApplyConfigure();
        WActivities["Cave:2"] = zone => new ZBActivity.Collapse.Cave(zone).ApplyConfigure();

        // Projectile - Air
        // WActivities["Wind:3"] = zone => new ZBActivity.Projectile.Wind(zone).ApplyConfigure();        

        WActivities["Meteorite:3"] = zone => new ZBActivity.Projectile.Meteorite(zone).ApplyConfigure();
        WActivities["BlockRain:3"] = zone => new ZBActivity.Projectile.BlockRain(zone).ApplyConfigure();

        // Projectile - Env
        /* NB: actual projectiles depend on biome, so it is not always fire (cold, poison ...) */
        WActivities["FireStorm:12"] = zone => new ZBActivity.Projectile.FireStorm(zone).ApplyConfigure();
        WActivities["Fire:14"] = zone => new ZBActivity.Projectile.Fire(zone).ApplyConfigure();

        // Inventory
        WActivities["FillBag:2"] = zone => new ZBActivity.Entities.FillBag(zone).ApplyConfigure();
        WActivities["Slippery:2"] = zone => new ZBActivity.Entities.Slippery(zone).ApplyConfigure();
        WActivities["NoSack:1"] = zone => new ZBActivity.Entities.NoSack(zone).ApplyConfigure();
        
        // fixme: hand animation not updated when pistol taken back from ground after fall
      
        // Motion / Size
        WActivities["AttractivePlayer:2"] = zone => new ZBActivity.Environment.AttractivePlayer(zone).ApplyConfigure();
        WActivities["Gravity:3"] = zone => new ZBActivity.Environment.Gravity(zone).ApplyConfigure();
        WActivities["Jumping:1"] = zone => new ZBActivity.Environment.Jumping(zone).ApplyConfigure();
        WActivities["DwarfPlayer:2"] = zone => new ZBActivity.Environment.DwarfPlayer(zone).ApplyConfigure();
        WActivities["RandomSize:2"] = zone => new ZBActivity.Environment.RandomSize(zone).ApplyConfigure();
        WActivities["Slip:0"] = zone => new ZBActivity.Environment.Slip(zone).ApplyConfigure();
        WActivities["MovingSands:0"] = zone => new ZBActivity.Environment.MovingSands(zone).ApplyConfigure();     // FIXME

        // Summon
        WActivities["Ghost:15"] = zone => new ZBActivity.Entities.Ghost(zone).ApplyConfigure();
        WActivities["Giant:5"] = zone => new ZBActivity.Entities.Giant(zone).ApplyConfigure();
        WActivities["MovingGhost:0"] = zone => new ZBActivity.Entities.MovingGhost(zone).ApplyConfigure();

        Printer.Write("ZombiomeActivitySelector..Initialize WActivities", WActivities.Count);

        // Weighted
        Activities = new SortedList<string,Func<Zone,ZBEffect>>();
        float[] wgts = new float[WActivities.Count];
        Func<Zone,ZBEffect>[] values = new Func<Zone,ZBEffect>[WActivities.Count];
        int index = -1;
        foreach(string key in WActivities.Keys) {
            index++;
            string[] parts = key.Split(':');
            // Printer.Write(" - Initialize Activities", index, key, parts.Length);
            wgts[index] = float.Parse(parts[1]);
            // Printer.Write(" - Initialize Activities", index, wgts[index], parts[0]);
            Activities[parts[0]] = WActivities[key]; // remove rate for debug access to name
            // Printer.Write(" - Initialize Activities", WActivities[key], Activities[parts[0]]);
            values[index] = WActivities.Values[index];
            // Printer.Write(" - Initialize Activities", WActivities[key], WActivities.Values[index], values[index]); 
        }
        Printer.Write("ZombiomeActivitySelector..Index:", Activities.Count, WActivities.Count);
        ActivitiesWIndex = new Hashes.Weighted<int>.Index(1000, wgts);
        Printer.Write("ZombiomeActivitySelector..Weighting:", Activities.Count, ActivitiesWIndex);


        /* A19 
        changement de meshfile dans ZombieMoe
        */

        /*
        
        -- weights
        G: 9 + 3 trap line
        D: 3
        C: 5
        W: 3
        F : 20 +12 - includes eject/slip and biome projectiles
        I: 5
        SG: 10
        __



        20% ground (+peak line, at entity)
        5% collapse (+flood)
        20% proj air (1, per biome)
        10% proj env : wind meteorite blockrain
        5% inventory
        5% gravity (grav, jumping)
        5% size (RS, dwarf)
        5% giant
        15% ghosts (2)


        */

        /* TODO
        ensevelir (peak avoid ent at entities pos, needs pos)
        true animal ghosts attack disappear
        AirSupport: insert 1 block below surface !        
        CactusGrowth
        big peaks avec echo
        buff suffocate outside
        */


    }
    public static ZBEffect Random(Zone zone, string restrict="") {
        Func<Zone,ZBEffect> selected;
        string name ="?";
        if (restrict.Length == 0) restrict = RestrictChoice;
        if (restrict.Length == 0) {
            int index = (V2) ? ActivitiesWIndex[zone.seed] : Hashes.Rand(0, Activities.Count - 1, zone.seed);
            Printer.Log(65, "Random ZBEffect", index, Activities.Keys[index], Activities.Values[index]);
            selected = Activities.Values[index];
            name = Activities.Keys[index];
        } else {
            Printer.Log(65, "Random ZBEffect restricted to", restrict, Activities.ContainsKey(restrict));
            selected = Activities[restrict];
            name = restrict;
        }
        Printer.Log(65, "Random ZBEffect selected", selected);
        ZBEffect effect = selected(zone);
        // effect.name = name;
        if (effect.name == "?") effect.name = name;
        else effect.name = String.Format("{0}{1}", name, effect.name.Replace("?", ""));
        Printer.Log(65, "Random ZBEffect instance", effect);
        return effect;
    }

}

}


/* 
private class ReflectionConstructor : Func<string, BiomeDefinition, ZBEffect> {

    public 

    Func<int,ZBEffect> GetFactoryFunction(Type t) {
        if(t == null) throw new ArgumentNullException("t");
        if(!typeof(ZBEffect).IsAssignableFrom(t)) throw new ArgumentException();
        return i => (ZBEffect) Activator.CreateInstance(t,i);
    }

    Func<int,TDerived> GetFactoryFunction<TDerived>() where TDerived : ZBEffect {
        return i => (TDerived)Activator.CreateInstance(typeof(TDerived),i);
    }
}
*/ 
