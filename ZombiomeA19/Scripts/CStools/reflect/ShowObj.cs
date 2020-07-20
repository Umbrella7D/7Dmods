using System;
using System.Runtime;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;

namespace CSutils.Reflect {
class ShowObj
{
    // TODO separate attribute from parent class, rec ...
    public class _DemoBase {
        protected int pro = 1;
    }
    public class _DemoBase1 : _DemoBase {
        public _DemoBase1() : base() {}
    }

    public static void Main(string[] args)
    {

        // Action<String> printer = (x) => Console.WriteLine(x);
        string pth = @"C:\Users\N4TH\Desktop\wtf.txt";
        Action<List<String>> printer = (lines) => System.IO.File.WriteAllLines(@pth, lines);

        //printer = Console.WriteLine;
        // PrintMethods(new ShowObj(), printer);
        //PrintClass(typeof(String), printer);
        //PrintClass("".GetType());

        // PrintClass(new ShowObj());
        // PrintClass(typeof(String));
        // PrintClass(typeof(object));

        var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;
        // var flags = BindingFlags.NonPublic | BindingFlags.Instance;;
        FieldInfo[] fields = new _DemoBase1().GetType().BaseType.GetFields(flags);
        foreach(FieldInfo field in fields) {
            string name  = field.Name;
            object value = field.GetValue(new _DemoBase1());
            Console.WriteLine(String.Format("{0} -> {1}", name, value));
        }

        //PrintAttributes(new ShowObj());
        // PrintAttributes(typeof(String));
        // PrintAttributes(typeof(object));
        //PrintAttributes(new _DemoBase1());
    }
    public void foo(int a, float b, ShowObj c) {}
    public int n = 18;
    public string some_property { get; set; }

    private static int npa = 0;

    public static void PrintAttributes(Object obj) {PrintAttributes(obj, null); }
    public static void PrintAttributes(Object obj, String file) {
        if (obj == null) return;
        npa = npa +1;
        string s = ShowAttributes(obj);
        Console.WriteLine(s);
        if (file==null) file = String.Format("C:\\Users\\N4TH\\Desktop\\CObjects\\{0}_{1}.txt", obj.GetType().Name, npa);
        System.IO.File.WriteAllText(@file, s);
    }

    public static String ShowAttributes(Object obj) {
        string s = obj.ToString() + ". ";
        FieldInfo[] fields = obj.GetType().GetFields();
            foreach(FieldInfo field in fields) {
                string name  = field.Name;
                object value = field.GetValue(obj);
                s = String.Format("{0}{1}={2}, ", s, name, value);
            }
            if (false) {
                foreach(PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj)) {
                    string name=descriptor.Name;
                    object value=descriptor.GetValue(obj);
                    s = String.Format("{0}{1}={2}, ", s, name, value);
                }
            }
            return s;
    }


    public static void PrintClass(Object obj) {Hierarchy(obj.GetType(), null);}
    public static void PrintClass(Type tp) {Hierarchy(tp, null);}

    public static void PrintClass(Object obj, Action<List<String>> printer) {
        PrintClass(obj.GetType(), printer);
    }
    public static void PrintClass(Type tp, Action<List<String>> printer) {
        List<String> all = new List<String>();
        Type baseType = tp.BaseType;
        String bases = "";
        if (baseType != null) {
            bases =  baseType.Name;
        }
        all.Add(String.Format("{0} ({1}) : {2}", tp.Name, tp.Namespace, bases));
        all.Add(""); 
        if (printer==null) {
            string pth = String.Format("C:\\Users\\N4TH\\Desktop\\CSclasses\\{0}_{1}.txt", tp.Name, tp.Namespace);
            printer = (lines) => System.IO.File.WriteAllLines(@pth, lines);
        }
        FieldInfo[] fields = tp.GetFields();
        if (fields.Length > 0) {
            //all.Add("--- fields");
            foreach(FieldInfo field in fields) all.Add(field.ToString()); //printer(field.ToString());
        }
        MethodInfo[] methods = tp.GetMethods();
        if (methods.Length > 0) {
            //all.Add("--- methods");
            foreach(MethodInfo method in methods) all.Add(MethodInfoExtensions.GetSignature(method)); //all.Add(method.ToString()); // printer(method.ToString());
        }
        printer(all);
    }


    public static void Hierarchy(Type tp, Action<List<String>> printer) {
        // Dictionary<Type,Tuple<List<FieldInfo>,List<MethodInfo>>> levels = new Dictionary<Type, Tuple<List<FieldInfo>, List<MethodInfo>>>();
        Boolean disag = true;

        List<String> all = new List<String>();
        Type next = tp;
        while (next != null) {
            if (disag) all = new List<String>();
            Type baseType = next.BaseType;
            String bases = "";
            if (baseType != null) bases =  baseType.Name;
            all.Add(""); 
            all.Add(String.Format("=== {0} ({1}) : {2}", next.Name, next.Namespace, bases));
            // var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var flags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

            foreach(FieldInfo field in next.GetFields(flags)) all.Add(field.ToString());
            all.Add(""); 
            foreach(MethodInfo method in next.GetMethods(flags)) all.Add(MethodInfoExtensions.GetSignature(method));
            all.Add(""); 

            if (disag) {
                if (printer==null) {
                    string pth = String.Format("C:\\Users\\N4TH\\Desktop\\CSclasses\\{0}_{1}.txt", next.Name, next.Namespace);
                    // printer = (lines) => System.IO.File.WriteAllLines(@pth, lines);
                    System.IO.File.WriteAllLines(@pth, all);
                } else printer(all);
            }

            next = next.BaseType;
        }
        if (! disag) {
            if (printer==null) {
                string pth = String.Format("C:\\Users\\N4TH\\Desktop\\CSclasses\\{0}_{1}.txt", tp.Name, tp.Namespace);
                printer = (lines) => System.IO.File.WriteAllLines(@pth, lines);
            }
            printer(all);
        }
    }

    public static IEnumerable<Type> GetAllSubclassOf(Type parent)
    {
        foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            foreach (var t in a.GetTypes())
                if (t.IsSubclassOf(parent)) yield return t;
    }

    // Func<int, int, bool> testForEquality = (x, y) => x == y;
    // Action line = () => Console.WriteLine();
    // Func<int, string, bool> isTooLong = (int x, string s) => s.Length > x;
    static void PrintAdvanced(object o, Action<List<String>> printer) {
        // I can already see the get_/set_ methods
        var tp = o.GetType();
        PrintClass(tp, printer);

        // printer("--- PropertyDescriptor");

        Console.WriteLine("--- PropertyDescriptor");
        foreach(PropertyDescriptor descriptor in TypeDescriptor.GetProperties(o))
        {
            string name=descriptor.Name;
            object value=descriptor.GetValue(o);
            Console.WriteLine("{0}={1}",name,value);
        }

        Console.WriteLine("--- properties");
        PropertyInfo[] properties = tp.GetProperties(); 
        Console.WriteLine( properties );
        foreach(PropertyInfo property in properties) {
            Console.WriteLine( property );
        }
    }

    public static void Show() {
        // These examples assume a "C:\Users\Public\TestFolder" folder on your machine.
        // You can modify the path if necessary.

        // Example #1: Write an array of strings to a file.
        // Create a string array that consists of three lines.
        string[] lines = { "First line", "Second line", "Third line" };
        // WriteAllLines creates a file, writes a collection of strings to the file,
        // and then closes the file.  You do NOT need to call Flush() or Close().
        string pth = @"C:\Users\N4TH\Desktop\wtf.txt";
        System.IO.File.WriteAllLines(@pth, lines);
        // System.IO.File.WriteAllText(@pth, lines[0]);
    }

}

}