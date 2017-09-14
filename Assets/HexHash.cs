
using UnityEngine;

public struct HexHash  {

    public float a, b, c, d, e;

    public static HexHash Create()
    {
        HexHash hash;
        hash.a = Random.value * .999f;
        hash.b = Random.value * .999f;
        hash.c = Random.value * .999f;
        hash.d = Random.value * .999f;
        hash.e = Random.value * .999f;
        return hash;
    }
}
