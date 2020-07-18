using System;
using System.Xml;
using UnityEngine;

// Token: 0x0200052F RID: 1327
public class OnGroundRequirement : RequirementBase {
	string block = ""; // TODO: check standing on XXX ?

	public override bool IsValid(MinEventParams _params) {
		Printer.Log(82, "OnGroundRequirement", _params.Self); // FIXME self ou target ?? target probably !
		if (!this.ParamsValid(_params)) return false;
		if(_params.Self == null) return false;
		return _params.Self.onGround;
	}
	// public override void GetInfoStrings(ref List<string> list)

	// public override bool ParseXmlAttribute(XmlAttribute _attribute)

}
