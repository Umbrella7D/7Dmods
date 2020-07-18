using System;
using System.Xml;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Reflection;

using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

using CSutils;
using SdtdUtils;


public abstract class MinEventActionTargetedBaseCatcher : MinEventActionTargetedBase {
    bool SwallowErrors = false;
    public sealed override void Execute(MinEventParams _params) {
        Execute(_params, this.ExecuteUncatch, GetType(), this.SwallowErrors);
    }
    public static void Execute(MinEventParams _params, Action<MinEventParams> action, Type mea, bool swallow=false) {
        try {action(_params);}
        catch(Exception ex) {
            if (GameManager.Instance == null) Printer.Print("Routines Catcher with null GameManager");
            if (GameManager.Instance.World == null) Printer.Print("Routines Catcher with null World");
            else {
                if (swallow) {
                    Printer.WriteError(mea, ex);
                }
                else {
                    Printer.WriteError(mea, ex);
                    Printer.Print("ERROR in Routine:", mea);
                    Printer.Print(ex); // prints the stack trace, even better than adding actual names ?
                    throw ex;
                }
            }
        }
    }

    public abstract void ExecuteUncatch(MinEventParams _params);


}