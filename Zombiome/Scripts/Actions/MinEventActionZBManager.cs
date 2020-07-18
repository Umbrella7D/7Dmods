using System;
using System.Xml;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Reflection;

using Harmony;
using System;
using System.Collections.Generic;

using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

public class MinEventActionZBManager : MinEventActionRemoveBuff {
    /** Start / Stop Zombiome processes */

    private static long LastCall = 0;
    public static bool started = false;
    private string arg = "__unset__";
    public override bool ParseXmlAttribute(XmlAttribute _attribute) {
        string name=  _attribute.Name;
        if (name == "arg") this.arg =  _attribute.Value;
        base.ParseXmlAttribute(_attribute);
        return true;
    }

    private long Tick() {
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        long delta = now - LastCall;
        LastCall = now;
        return delta;
    }
    public override void Execute(MinEventParams _params) {
        // FromItem(_params, options.attr_xml);
        Debug.Log(String.Format(" MEA ZBManager Execute: {0} {1} {2} {3} ",
                                 _params, _params.Self, this.arg, started));

        long delta = Tick();

        if (delta > 10) {
            Zombiome.ExitGame();  // force re-init. should happen in between games. make sure dt_ref >> frequency
        }

        if (this.arg == "start" || this.arg == "enter") {
            Zombiome.Init(_params.Self.entityId);
            started = true;
            // also act on reborn !
        }
        if (this.arg == "stop") {
            Zombiome.OnDisconnect();
            started = false;
            // also act on reborn !
        }
    }
}