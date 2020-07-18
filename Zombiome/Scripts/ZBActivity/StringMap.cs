using System.Xml;
using System;
using System.Collections;
using System.Collections.Generic;

using CSutils;


namespace SdtdUtils {
public class StringMap {
    /** Interpret float cvar as string */
    public static Dictionary<int,string> Map = new Dictionary<int,string>();
    public static Dictionary<string,int> RMap = new Dictionary<string,int>();
    static StringMap() {
        Map[0] = "";
        Map[1] = "ZBProj_poison";
        Map[2] = "ZBProj_boom";
        Map[3] = "ZBProj_spark";
        Map[4] = "ZBProj_impact";
        Map[5] = "ZBProj_boomr";
        Map[6] = "ZBProj_boomy";
        foreach(KeyValuePair<int,string> item in Map) RMap[item.Value] = item.Key;
    }
    public static float Encode(string key) {
        return (float) RMap[key];
    }
    public static string Get(EntityAlive entity, string key) {
        if (key.StartsWith("$")) key = key.Substring(1);
        int cvar = (int) Math.Round(entity.GetCVar(key));
        string value = (cvar == 0) ? "" : Map[cvar];
        Printer.Log(35, "StringMap Get()", entity, key, value);
        return value;
    }
}

}