using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

namespace CSutils {

public static class Objects {
    public static void Swap<T>(ref T x, ref T y) {
        T temp = x;
        x = y;
        y = x;
    }
}

} // END namespace