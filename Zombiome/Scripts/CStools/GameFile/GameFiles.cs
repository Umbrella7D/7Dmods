using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SdtdUtils {

public class GameFiles {
    /*
    NB: EnumGamePrefs.GameDifficulty etc

    */
    public static string Full(string name) {
        // return ""; // for testing
        return GamePrefs.GetString(EnumGamePrefs.SaveGameFolder) + "/" + name;
    }
    public void Clear(string path) {
        using (StreamWriter streamWriter = new StreamWriter(path)) {}
    }
    private Dictionary<string, StreamWriter> Writers = new Dictionary<string, StreamWriter>();
    public void Write(string name, string line) {
        Writers[name].WriteLine(line);
    }
    public void Close() {
        foreach(StreamWriter writer in Writers.Values) writer.Close();
    }

    public class OpenFile {
        /* Let's hope GC is similar to Python wrt yielding */
        private string pth;
        private StreamWriter streamWriter;
        private IEnumerator<object> ensure;
        private int status = 0;
        public override string ToString() {
            return String.Format("OpenFile({0}, {1})", status, pth); 
        }
        public OpenFile(string pth, bool flush=true) {
            this.pth = pth;
            if (flush) {
                using (StreamWriter streamWriterFlush = new StreamWriter(pth)) {}
            }
            this.ensure = EnsureRelease();
            this.ensure.MoveNext();
            Console.WriteLine(String.Format("OpenFile: {0} {1}", this.ensure.Current, this.streamWriter));
        }
        public void Write(string data) {
            // Console.WriteLine(String.Format("Writing: {0} {1}", this.ensure.Current, this.streamWriter));
            if (status != 1) throw new IOException(String.Format(
                "OpenFile invalid status {0} : {1} {2} {3}",
                status, pth, streamWriter, ensure
            )); 
            streamWriter.WriteLine(data);
        }
        private IEnumerator<object> EnsureRelease() {
            streamWriter = new StreamWriter(pth, true);
            streamWriter.AutoFlush = true;
            using (streamWriter) {
                status = 1;
                yield return null;
            }
            status = 2;
            // Console.WriteLine("Writing terminates");
            yield return null;
        }
    }

    static void Main(string[] args) {
        string pth = "C:\\Users\\N4TH\\Desktop\\testGF.txt";
        if (false) {
            using (StreamWriter streamWriter = new StreamWriter(pth, true)) {
                streamWriter.WriteLine("test WriteLine");
            }
        } else {
            OpenFile f = new OpenFile(pth);
            // Printer.Print(f);
            Console.WriteLine(string.Join(", ", "f", f.ToString()));
            f.Write("test data");
            f.Write("test again");
            throw new Exception();
            // f.ensure.MoveNext();
            // Console.WriteLine(f.ensure.Current);
        }


    }

}


} // END namespace