using System;
using System.Xml;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Reflection;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;


using CSutils;
public class MinEventActionParticlesAtPos : MinEventActionTargetedBaseCatcher {
    /** For infinite particles, you should use a ghost (see MinEventActionParticlesAtEntity) */
    // TODO: offset (smoke is NE I need correct SW)
    private string particle;
    private Color color = Color.white;
    private Vector3 offset;
    private Vector3 rpos; // randomize position
    private Vector3 rcol; // randomize color
    private string sound = null;
    private bool ate = false;
    public int rep = 1;
    public int dt = 1000; // millis
    public GameRandom random;
    public MinEventActionParticlesAtPos() : base() {
        random = GameRandomManager.Instance.CreateGameRandom();
    }

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
        else if (name == "offset") {
            this.offset = StringParsers.ParseVector3(_attribute.Value);
        }
        else if (name == "rcol") {
            this.rcol = StringParsers.ParseVector3(_attribute.Value);
        }
        else if (name == "rpos") {
            this.rpos = StringParsers.ParseVector3(_attribute.Value);
        }
        else if (name == "sound") this.sound =  _attribute.Value;
        else if (name == "color") this.color = ParseColor(_attribute.Value); 
        else if (name == "rep") this.rep = int.Parse(_attribute.Value);
        else if (name == "dt") this.dt = int.Parse(_attribute.Value);
        else used = false;
        return used || based;
    }

    public override void ExecuteUncatch(MinEventParams _params) {
        foreach (EntityAlive Self in this.targets) { // Expect ghost only if Self
            Execute(Self, _params.Position); // Expects Position at ProjectileImpact
            break;
        }
    }
    public void Execute(EntityAlive ctrl, Vector3 pos) {
        Zombiome.Routines.Start(Particles(ctrl, pos), "MEAParticleAtPos.Particles({0})", ctrl);
    }

    private Color RCol() {
        if (this.rcol == Vectors.Float.Zero) return this.color;
        return new Color(
            Math.Min(1, Math.Max(0, this.color.r + this.rcol[0] * (-1 +2 *this.random.RandomFloat))),
            Math.Min(1, Math.Max(0, this.color.g + this.rcol[1] * (-1 +2 *this.random.RandomFloat))),
            Math.Min(1, Math.Max(0, this.color.b + this.rcol[2] * (-1 +2 *this.random.RandomFloat))),
            this.color.a
        );
    }
    public IEnumerator Particles(EntityAlive ctrl, Vector3 pos) {
        pos = pos + this.offset;
        Printer.Log(30, "MinEventActionParticleAtPos", ctrl, pos, ctrl.GetPosition());
        YieldInstruction dt = new WaitForSeconds((float) this.dt / 1000f);
        for (int replicate=0; replicate<rep; replicate++) {
            yield return dt;
            Vector3 rdmized = Vectors.Float.Randomize(random, pos, this.rpos);
            SdtdUtils.EffectsItem.SpawnParticle(rdmized, this.particle, RCol(), this.sound);
            // Vector3 motion = Vectors.Float.Randomize(random, 1f, Vectors.Float.UnitY);
            // SdtdUtils.EffectsItem.SpawnParticle(pos+offset, this.particle, this.color, this.sound);
        }
    }
}