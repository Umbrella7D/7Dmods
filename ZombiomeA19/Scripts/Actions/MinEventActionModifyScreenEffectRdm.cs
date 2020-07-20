using System;
using System.Globalization;
using System.Xml;

// Token: 0x02000544 RID: 1348
public class MinEventActionModifyScreenEffectRdm : MinEventActionBase {

	public string effect_name = "";
	public float intensity = 1f;
	public float fade = 0f;
	public float rate0 = 0;
	
	public override bool ParseXmlAttribute(XmlAttribute _attribute) {
		bool flag = base.ParseXmlAttribute(_attribute);
		if (!flag) {
			string name = _attribute.Name;
			if (name == "effect_name") {
				this.effect_name = _attribute.Value;
				return true;
			}
			else if (name == "intensity") {
				this.intensity = StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
				return true;
			}
			else if (name == "rate0") {
				this.rate0 = float.Parse(_attribute.Value);
				return true;
			}
			else if (name == "fade") {
				this.fade = StringParsers.ParseFloat(_attribute.Value, 0, -1, NumberStyles.Any);
				return true;
			}
		}
		return flag;
	}

	public override void Execute(MinEventParams _params) {
        MinEventActionTargetedBaseCatcher.Execute(_params, _Execute, GetType());
    }
	public void _Execute(MinEventParams _params) {
		if (_params.Self as EntityPlayerLocal != null) {
			float intensity = 0;
			if (Zombiome.rand.RandomFloat > this.rate0) {
				intensity = Zombiome.rand.RandomFloat * this.intensity;
			}
			(_params.Self as EntityPlayerLocal).ScreenEffectManager.SetScreenEffect(this.effect_name, intensity, this.fade);
		}
	}


}
