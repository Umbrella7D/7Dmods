using System;
using System.Linq;
using System.Collections.Generic;


using Unity;
using UnityEngine;

namespace CSutils {
// Token: 0x02000126 RID: 294
public class IntLine {
	/// Line points with integer coordinates.
	/* I need 3D in case of diagonal digging. On surface, expect direction.y=0 or SI breaks */
	private Vector3 Center;
	private Vector3 Direction;
	private int IdxMin = -1;
	private int IdxOther1 = -1;
	private float Other1 = -1.0f;
	private int IdxOther2 = -1;
	private float Other2 = -1.0f;

	public IntLine(Vector3i center, Vector3 dir) : this(Vectors.ToFloat(center), dir) {}
	public IntLine(Vector3 center, Vector3 dir) {
		// FIXME: allow negative direction
		// TODO: guard error 0 dir
		if (dir.Equals(Vectors.Zero)) Printer.Print("Error: Zero direction in IntLine !");
		this.Center = center;
		this.Direction = dir;
		// Console.WriteLine(String.Format("Line: {0} -> {1}", center, dir));
		IdxMin = 2;
		float a0 = Math.Abs(dir[0]); float a1 = Math.Abs(dir[1]); float a2 = Math.Abs(dir[2]);
		if (a0 >= a1 && a0 >= a2) IdxMin = 0;
		else if (a1 >= a0 && a1 >= a2) IdxMin = 1;
		IdxOther1 = (IdxMin + 1) % 3; // careful priorities : + < %
		IdxOther2 = (IdxMin + 2) % 3;
		// Console.WriteLine(String.Format("Idx: {0} {1} {2}", IdxMin, IdxOther1, IdxOther2));
		Other1 = dir[IdxOther1] / dir[IdxMin];
		Other2 = dir[IdxOther2] / dir[IdxMin];
		// Console.WriteLine(String.Format("Mult: {0} {1}", Other1, Other2));
	}
	public Vector3i Get(int k) {
		Vector3 result = new Vector3();
		result[IdxMin] = (int) Center[IdxMin] + k;
		result[IdxOther1] = (int) Center[IdxOther1] + Other1 * k;
		result[IdxOther2] = (int) Center[IdxOther2] + Other2 * k;
		return Vectors.ToInt(result);
	}
	public int IndexOf(Vector3 pos) {
		// TODO: project pos on this line first  ?
		float ratio = (pos[this.IdxOther1] - (float) Center[this.IdxOther1]) / Other1;
		Printer.Log(10, "IndexOf", pos, IdxMin, Center, Direction);
		return (int) Math.Floor(ratio);
	}

	public static List<Vector3i> SegmentList(Vector3 center, Vector3 dir, int start, int len) {
		IntLine l = new IntLine(center, dir);
		List<Vector3i> segment = new List<Vector3i>();
		for (int k=0; k<len; k++) segment.Add(l.Get(k));
		return segment;
	}
	public static IEnumerable<Vector3i> Segment(Vector3 center, Vector3 dir, int start, int len) {
		IEnumerable<int> range = Enumerable.Range(0, Math.Abs(len));
		if (len < 0) range = range.Select(x => -x);
		IntLine l = new IntLine(center, dir);
		return range.Select(x => l.Get(start + x));
	}

	public static IEnumerable<Vector3i> Segment(Vector3 from, Vector3 to) {
		IntLine l = new IntLine(from, (from - to));
		int start = 0;
		int len = l.IndexOf(to);
		Printer.Log(10, "Segment", start, len);
		IEnumerable<int> range = Enumerable.Range(0, Math.Abs(len));
		if (len < 0) range = range.Select(x => -x);
		return range.Select(x => l.Get(start + x));
	}

	public Vector3 Interp(Vector3 from, Vector3 to, float x) {
		return (1f -x) * from + x * to;
	}

	public static void _Main(string[] args)
    {	
		Vector3 c = new Vector3();
		c[1] = 1.0f;
		Vector3 d = new Vector3();
		d[0] = 2.0f;
		d[2] = 0.5f;
        IntLine l;
		if (true) {
			l = new IntLine(c, d);
			Console.WriteLine("-");
			foreach(int k in Enumerable.Range(-5,10)) {
				Console.WriteLine(String.Format("{0} -> {1}", k, l.Get(k)));
			}
			l = new IntLine(c, d * 0.01f); // Does not depend on dir scaling
			Console.WriteLine("-");
			foreach(int k in Enumerable.Range(-5,10)) {
				Console.WriteLine(String.Format("{0} -> {1}", k, l.Get(k)));
			}
		}

		Console.WriteLine("-");
		foreach(Vector3i v in IntLine.Segment(c, d, 2, 5)) {
			Console.WriteLine(String.Format("{0}", v));
		}
		Console.WriteLine("-");
		foreach(Vector3i v in IntLine.Segment(c, d, 2, -5)) {
			Console.WriteLine(String.Format("{0}", v));
		}
		Console.WriteLine("-");
		foreach(Vector3i v in IntLine.Segment(c * 10f, d *10f)) {
			Console.WriteLine(String.Format("{0}", v));
		}
    }

}

} 

// BELOW IS DEBUG ONLY
// in order to compile stand=alone
/*
public class Vector3 : List<float> {
	public Vector3() : base() {
		this.Add(0.0f); this.Add(0.0f); this.Add(0.0f);
	}
	public override String ToString() {
		return String.Format("({0},{1},{2})", base[0], base[1], base[2]);
	}
	public static Vector3 operator - (Vector3 x, Vector3 y) { 
        Vector3 diff = new Vector3();
		for (int d=0; d<3; d++) diff[d] = x[d] - y[d];
        return diff; 
    } 
	public static Vector3 operator * (Vector3 x, float y) { 
        Vector3 diff = new Vector3();
		for (int d=0; d<3; d++) diff[d] = x[d] * y;
        return diff; 
    } 
}
public class Vector3i : List<int> {
	public Vector3i() : base() {
		this.Add(0); this.Add(0); this.Add(0);
	}
	public override String ToString() {
		return String.Format("({0},{1},{2})", base[0], base[1], base[2]);
	}
}

public class Vectors {
	public static Vector3i Zero = new Vector3i();
	public static Vector3i ToInt(Vector3 v) {
		Vector3i w = new Vector3i();
		for (int d=0; d<3; d++) w[d] = (int) Math.Floor(v[d]);
		return w;
	}	
}

public class Printer {
	public static void Print(params object[] args) {
		string[] str = args.Select(x => x.ToString()).ToArray();
		Console.WriteLine(String.Join(' ', str));
	}	
}
*/