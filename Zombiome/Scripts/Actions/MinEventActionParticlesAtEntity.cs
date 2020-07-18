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




public class MinEventActionParticlesAtEntity : MinEventActionTargetedBaseCatcher { 
    /** Attach particle to self

    I can spawn particles from code, but then the only way to stop an "infinite" particle
    is attaching them to some entity
    */
    private string particle;
    private Color color = Color.white;
    private string sound = null;
    private bool ate = false;

    private static Color ParseColor(string str) {
        string[] split = str.Split(',');
        float[] def = new float[]{0,0,0,1};
        for( int k=0; k<split.Length; k++ ) def[k] = float.Parse(split[k]);
        return new Color(def[0], def[1], def[2], def[3]);
    }
    public override bool ParseXmlAttribute(XmlAttribute _attribute) {
        bool based = base.ParseXmlAttribute(_attribute);
        string name=  _attribute.Name;
        bool used = true;
        if (name == "particle") {
            this.particle = _attribute.Value;
            if (this.particle.StartsWith("p_")) this.particle = this.particle.Substring(2);
        }
        else if (name == "sound") this.sound =  _attribute.Value;
        else if (name == "color") this.color = ParseColor(_attribute.Value);
        else used = false;
        return used || based;
    }

    public override void ExecuteUncatch(MinEventParams _params) {
        foreach (EntityAlive Self in this.targets) {
            Execute(Self);
        }
    }
    private void Execute(EntityAlive ctrl) {
        Vector3 pos = ctrl.GetPosition();
        SdtdUtils.EffectsItem.SpawnParticle(pos, this.particle, this.color, this.sound);
    }

}