using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;

using CSutils;

/*
TODO use an instance  : Bound, center etc depends on size, I want a different size for chunk and zone
*/
public class ZChunk {
    public static Vector3i[] adjacents = new Vector3i[]{
        new Vector3i(0,0,0),
        new Vector3i(1,0,0),
        new Vector3i(0,0,1),
        new Vector3i(1,0,1)
    };
    public static int size = 32; // 9182, 4096, 6144 (navezgane) -> let size divide this
    public static Vector3i TL1(int x, int z) {
        int i = (int) Math.Floor(((float) x / size));
        int j = (int) Math.Floor(((float) z / size));
        return new Vector3i(i, 0, j);
    }
    public static Vector3i TL4(int x, int z, int size = -1) {
        /** Return topleft of the 4 blocks. Position is not necessary in */
        int wsize = Zombiome.worldSize;
        if (size==-1) size = ZChunk.size;

        int bound = - wsize / (2 * size); 
        int i = (int) Math.Floor( (float) x / size);
        if (x - i*size < size/2) {
            i = i-1;
            if (i<bound) i = bound;
        }
        int j = (int) Math.Floor( (float) z / size);
        if (z - j*size < size/2) {
            j = j-1;
            if (j<bound) j = bound;
        }
        Printer.Log(55, "ZChunk.TL4", x, z, "=>", i, j, "sizes", size, wsize, bound);
        return new Vector3i(i, 0, j);
    }
    public static Vector3i TL4(Vector3i pos, int size = -1) {return TL4(pos.x, pos.z, size);}
    public static Vector3i TL4(Vector3 pos, int size = -1) {return TL4(Vectors.ToInt(pos), size);}

    public static Bounds Bounds(Vector3i chunk, int iniguess, int size=-1) {
        // I could use 125. Do I need center at surface ?
        if (size==-1) size = ZChunk.size;
        Vector3i center = Center(chunk);
        int y0 = Geo3D.Surface(center, iniguess).y;
        center.y = y0;
        return new Bounds(Vectors.ToFloat(center), new Vector3(size, 100, size));
    }
    public static Bounds Bounds4(Vector3i chunk, int iniguess, int size=-1) {
        // Union of 4 adjacent <=> Zone twice larger, centered at intersect
        if (size==-1) size = ZChunk.size;
        Vector3i center = new Vector3i( (1+chunk.x) * size, 0 , (1+chunk.z * size));
        int y0 = Geo3D.Surface(center, iniguess).y;
        center.y = y0;
        return new Bounds(Vectors.ToFloat(center), new Vector3(2*size, 100, 2*size));
    }
    public static Vector3i Center(Vector3i chunk, int size=-1) {
        if (size==-1) size = ZChunk.size;
        return new Vector3i(chunk.x * size + size/2, 0 , chunk.z * size + size/2);
    }

    public static int Size(float rate, int size=-1) {
        if (size==-1) size = ZChunk.size;
        return (int) Math.Floor(rate * ZChunk.size * ZChunk.size);
    }

     public static int Len(Vector3i zchunk, int maxsize) {
        return Hashes.Rand(1, maxsize, String.Format("{0}{1}", zchunk.x, zchunk.z));
    }

    public static Vector3 Position(string worldseed, Vector3i chunk, int index) {
        /* Random 2D position in chunk, reproducible
        use chunk pos in seed to avoid periodic positions
        */
        String seed = String.Format("{0}{1}{2}", worldseed, chunk.x, chunk.z);
        Vector3 gen = new Vector3(
            size * chunk.x + Hashes.Rand(0, size, seed, "x", index.ToString()),
            0,
            size * chunk.z + Hashes.Rand(0, size, seed, "z", index.ToString())
        );
        // Printer.Print("ZChunk.Position", chunk, "->", gen, "offset", size * chunk.x, size * chunk.z);
        return gen;
    }
    public static Vector3[] PositionsN(string worldseed, Vector3i chunk, int size) {
        Vector3[] pos = new Vector3[size];
        for (int index=0; index<size; index++) pos[index] = Position(worldseed, chunk, index);
        return pos;
    }

    public static Vector3[] Positions(string worldseed, Vector3i chunk, int maxsize) {
        // To avoid constant density / block, size should be x,y seeded
        // so size is a size max, and we guarantee at least one
        int len = Len(chunk, maxsize);
        Vector3[] pos = new Vector3[len];
        for (int index=0; index<len; index++) pos[index] = Position(worldseed, chunk, index);
        return pos;
    }

    public static Vector3[] Positions4(string worldseed, Vector3i chunk, int maxsize) {
        Printer.Log(55, "ZChunk.Positions4 start", worldseed, chunk, maxsize);
        int len00 = Hashes.Rand(1, maxsize, String.Format("{0}{1}", chunk.x, chunk.z));
        int len10 = Hashes.Rand(1, maxsize, String.Format("{0}{1}", chunk.x+1, chunk.z));
        int len01 = Hashes.Rand(1, maxsize, String.Format("{0}{1}", chunk.x, chunk.z+1));
        int len11 = Hashes.Rand(1, maxsize, String.Format("{0}{1}", chunk.x+1, chunk.z+1));
        List<Vector3> pos = new List<Vector3>();
        for (int index=0; index<maxsize; index++) { // todo : 4 boolean to optimize test and break
            if (index< len00) pos.Append(ZChunk.Position(Zombiome.worldSeed, chunk, index));
            if (index< len10) pos.Append(ZChunk.Position(Zombiome.worldSeed, chunk + new Vector3i(1,0,0), index));
            if (index< len01) pos.Append(ZChunk.Position(Zombiome.worldSeed, chunk + new Vector3i(0,0,1), index));
            if (index< len11) pos.Append(ZChunk.Position(Zombiome.worldSeed, chunk + new Vector3i(1,0,1), index));
        }
        Printer.Log(55, "ZChunk.Positions4 OK", pos.Count(), pos);
        return pos.ToArray();
    }


}