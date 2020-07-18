//using System.Xml;
using System;
using System.Collections;
using System.Collections.Generic;

//using Harmony;


//using System.Linq;
//using System.Reflection.Emit;

//;

namespace CSutils {

public static partial class Geo3D {
    public class IntNeighbourhood {
        /** Traverse neighbooring hexes:
                5
            12 1 6
        11 4 0 2 7
            10 3 8
                9
        */

        public int x;
        public int y;
        public int xcenter;
        public int ycenter;
        public int ray=0; // distance to the center (int-distance <=> nb of hexes to cross)
        public int arc=0; // 4 arcs to explore: (0) N->E, (1) E->S (2) S->W (3) W-> N
        public int step=0; // step within the current arc, of length ray
        public int n; // total step from 0

        public static int[] dx = new int[]{1,-1,-1,1}; // SE SW NW NE
        public static int[] dy = new int[]{-1,-1,1,1};
        public static int[] dx0 = new int[]{0,1,0,-1}; // N E S W
        public static int[] dy0 = new int[]{1,0,-1,0};

        public virtual void Next() {
            // Console.WriteLine(String.Format("{0},{1},{2} -> {3},{4}", ray, arc, step, x ,y));
            n=n+1;
            if (ray==0) ray=1;
            else {
                if (step >= ray-1) {
                    arc = arc+1; step=0;
                    if (arc >=4) {arc=0; ray=ray+1;}
                } else step++;
            }
            x = xcenter + ray*dx0[arc] + dx[arc] * step;
            y = ycenter + ray*dy0[arc] + dy[arc] * step;
        }
        public virtual void Reset(int xcenter, int ycenter) {
            ray=0; // distance to the center (int-distance <=> nb of hexes to cross)
            arc=0; // 4 arcs to explore: (0) N->E, (1) E->S (2) S->W (3) W-> N
            step=0; 
            this.xcenter = xcenter;
            this.ycenter = ycenter;
        }

        public static void _Main(string[] args) {
            IntNeighbourhood x = new IntNeighbourhood();
            x.xcenter = 0; x.ycenter = 0;
            for ( int k=0; k<30; k++) {
                x.Next();
                // Console.WriteLine(String.Format("*** {0},{1},{2} -> {3},{4}", x.ray, x.arc, x.step, x.x, x.y));

            }
        }
    }
}

////////////////////
} // End Namespace
////////////////////