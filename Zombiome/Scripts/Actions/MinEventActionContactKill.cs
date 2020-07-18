using System;
using System.Xml;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

public class MinEventActionContactKill : MinEventActionTargetedBaseCatcher {
    /** Kills self on contact with another Entity */
    public override void ExecuteUncatch(MinEventParams _params) {
        Printer.Log(39, "ContactKill", _params.Self, this.targets.Count);
        foreach (Entity target in this.targets) {
            if (target !=  _params.Self) {
                SdtdUtils.EntityCreation.Kill(_params.Self);
                Printer.Log(39, "ContactKill", _params.Self, "KILLED");
                return;
            }
        }
	}
}