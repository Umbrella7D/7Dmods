//using System.Xml;
using System;
//using System.Collections;
using UnityEngine;
using System.Collections.Generic;

//using Harmony;


//using System.Linq;
//using System.Reflection.Emit;
using CSutils;

namespace SdtdUtils {
public class Emplacement {
    public static Vector3 Truncate(Vector3 arg, bool D2, bool normalize) {
        // arg = new Vector3(arg);
        arg = arg * 1f; // copy ?
        if (D2) arg.y = 0;
        if (normalize) arg = arg.normalized;
        return arg;
    }

    public override string ToString() {
        return String.Format("Emplacement[{0}/{1}]", position, direction);
    }
    public Vector3i ipos {
        get {
            // Printer.Print("ipos", position, World.worldToBlockPos(position));
            return World.worldToBlockPos(position);
        }
        //set {position = Vectors.ToFloat(value);}
        set {position = World.blockToTransformPos(value);}
    }
    public Vector3 position;
    public Vector3 direction = new Vector3(0.0f, 1.0f, 0.0f);
    public bool valid = true;
    public string valid_msg = "";
    public void Invalid(string msg) {
        valid = false;
        valid_msg = msg;
    }
    public static Emplacement At(Vector3 pos, Vector3 dir) {
        // Printer.Print("Emplacement At", pos, dir);
        Emplacement e = new Emplacement();
        e.position = pos;
        e.direction = dir;
        return e;
    }
    private Emplacement() {}
    public Emplacement(Entity target, OptionEffect options) {
        // Debug.Log("Emplacement 'at' = " + options.at);
        // position = Vectors.ToInt(target.position);
        position = target.position;
        EntityPlayer player = target as EntityPlayer;
        // case of Z : get their target (player or direction or focus)
        if (player==null) return;
        if (options.at == "ray") position = Geo3D.intersectLook(player);
        direction = Geo3D.directionLook(player);

        if (options.th == "true") {
            int h = (int) target.world.GetTerrainHeight(ipos.x, ipos.z);
            Debug.Log(string.Format("GetTerrainHeight {0} => {1}", position, h));
            position.y = h;
        }
    }
    /* 
    public Emplacement(Entity target, IDictionary<string, string> attr_xml) {
        Debug.Log("Emplacement 'at' = " + attr_xml["at"]);
        // position = Vectors.ToInt(target.position);
        position = World.worldToBlockPos(target.position);
        EntityPlayer player = target as EntityPlayer;
        // case of Z : get their target (player or direction or focus)
        if (player==null) return;
        if (attr_xml["at"] == "ray") position = Vectors.ToInt(BlockSpawnUtils.intersectLook(player));
        // if (attr_xml["at"] == "direction") direction = BlockSpawnUtils.directionLook(player);
        direction = BlockSpawnUtils.directionLook(player);

        if (attr_xml["th"] == "true") {
            int h = (int) target.world.GetTerrainHeight(position.x, position.z);
            Debug.Log(string.Format("GetTerrainHeight {0} => {1}", position, h));
            position.y = h;
        }
    } */
}


////////////////////
} // End Namespace
////////////////////