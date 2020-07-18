using System;
using System.Linq; // .Select()
using UnityEngine; // for Debug.Log !

/* static class Debug {
    public static void Log(string x) {
        Console.WriteLine(x);
    }
} */

using SdtdUtils;

public static class Printer {

    private static string get(int n) {
        switch (n) { //uggly but faster than strcat of loop ? dict ?
            case 0: return ""; // break;
            case 1: return "{0}";
            case 2: return "{0} {1}";
            case 3: return "{0} {1} {2}";
            case 4: return "{0} {1} {2} {3}";
            case 5: return "{0} {1} {2} {3} {4}";
            case 6: return "{0} {1} {2} {3} {4} {5}";
            case 7: return "{0} {1} {2} {3} {4} {5} {6}";
            case 8: return "{0} {1} {2} {3} {4} {5} {6} {7}";
            case 9: return "{0} {1} {2} {3} {4} {5} {6} {7} {8}";
            case 10: return "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9}";
            case 11: return "{0} {1} {2} {3} {4} {5} {6} {7} {8} {9} {10}";
            default: return "";
      }
    }

    // todo: Write and test in Zbtest
    static Printer() {
        // string pth = GameFiles.Full("zb_log.txt");
        // NB GameFiles.Full= >NPE
        // string pth = "C:\\Users\\N4TH\\Desktop\\zb_log.txt";
        string pth = (Application.platform == RuntimePlatform.OSXPlayer) ? (Application.dataPath + "/../../Mods/zb_log.txt")
                                                                         : (Application.dataPath + "/../Mods/zb_log.txt");
        Debug.Log(String.Format("Printer at {0}", pth));
        _LogFile = new GameFiles.OpenFile(pth);
        Debug.Log(String.Format("Printer opened {0}", _LogFile));

        pth = (Application.platform == RuntimePlatform.OSXPlayer) ? (Application.dataPath + "/../../Mods/zb_err.txt")
                                                                : (Application.dataPath + "/../Mods/zb_err.txt");
        Debug.Log(String.Format("Printer at {0}", pth));
        _ErrFile = new GameFiles.OpenFile(pth);
        Debug.Log(String.Format("Printer opened {0}", _ErrFile));

        Print("Test");
        FPrint("Zombiome started at: ", DateTime.Now.ToString());
    }
    private static GameFiles.OpenFile _LogFile;
    public static GameFiles.OpenFile _ErrFile;
    // private static void _Print(string data) {_LogFile.Write(data);}
    public static void Write(params object[] args) {
        Print("Write _LogFile:", ((_LogFile == null) ? "null" : _LogFile.ToString()));
        args = args.Select(x => ((x==null) ? "Null" : x)).ToArray();
        _LogFile.Write(String.Format(get(args.Length), args));
    }

    public static void WriteError(params object[] args) {
        Print("Write _LogFile:", ((_ErrFile == null) ? "null" : _ErrFile.ToString()));
        args = args.Select(x => ((x==null) ? "Null" : x)).ToArray();
        _ErrFile.Write(String.Format(get(args.Length), args));
    }

    private static void _Print(string data) {Debug.Log(data);}

    public static void Print(params object[] args) {
        args = args.Select(x => ((x==null) ? "Null" : x)).ToArray();
        _Print(String.Format(get(args.Length), args));
    }
    public static void FPrint(string fmt, params object[] args) {
        _Print(String.Format(fmt, args));
    }
    public static int level = 0; //60;
    /*
    DEBUG: < 100
    10_19: Internals (forgetting dict, Routines)
    20_29: Block function
    26: EntityPool internals
    30_39: buffs internals
    35: Zone internal
    40_49: ZBactivity loop
    50_54: ZBiomeInfo
    55_59: ZChunk
    60_69: ZBactivity creation
    71: Transition block
    81: action fragment
    82: on ground requir
    85-89: decoration
    91-99: ghost
    */
    private static void _Log(string data) {_LogFile.Write(data);}

    public static void Log(int level, params object[] args) {
        if (level < Printer.level) return;
        args = args.Select(x => ((x==null) ? "Null" : x)).ToArray();
        _Log(String.Format(get(args.Length), args));
    }
    public static void FLog(int level, string fmt, params object[] args) {
        if (level < Printer.level) return;
        _Log(String.Format(fmt, args));
    }

    public static string Cat(params object[] args) {
        return String.Format(get(args.Length), args);
    }

/*     static void _Main(string[] args) {
        Printer.Print("This is", "a test", 0);
        Printer.Print("This is", "a test", null);
        Printer.Print(null, null);
    } */


}