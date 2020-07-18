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



public class MinEventActionZBManagerOption : MinEventActionRemoveBuff {
    /* Should not be used as action.
    Only purpose it to parse Zombiome configuration from xml
    */
    public override bool ParseXmlAttribute(XmlAttribute _attribute) {
        // Maybe base.ParseXmlAttribute populates a list of attributes ? then just call it
        string name=  _attribute.Name;
        if (name == "log") {
            // Printer.Print("ZBManagerOption Parsed Log", _attribute.Value);
            Zombiome.Log = int.Parse(_attribute.Value);
            Printer.level = Zombiome.Log;
            // int.Parse(_attribute.Value);
            // Printer.Print("ZBManagerOption Parsed Log", Zombiome.Log);
        } 
        else if (name == "autostart") {
            Zombiome.AutoStart = bool.Parse(_attribute.Value);
        }
        else if (name == "nz4") {
            Zombiome.nz4 = bool.Parse(_attribute.Value);
        }
        else if (name == "frequency") {
            Zombiome.FrequencyManager = float.Parse(_attribute.Value);
        }
        else if (name == "swallow") {
            Zombiome.SwallowError = bool.Parse(_attribute.Value);
        }



         else {
            return base.ParseXmlAttribute(_attribute);
        }
        return true;
    }
    // public override bool CanExecute(MinEventTypes _eventType, MinEventParams _params) {return false;}
    public override void Execute(MinEventParams _params) {}
}