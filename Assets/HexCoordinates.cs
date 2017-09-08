using UnityEngine;

[System.Serializable]
public struct HexCoordinates  {
    [SerializeField]
    private int x, z;

    public int X { get { return x; } }
    public int Z { get { return z; } }
    public int Y { get { return -X - Z; } }

    public HexCoordinates (int x, int z)
    {
        this.x = x;
        this.z = z;
    }

    public static HexCoordinates FromOffsetCoordinates (int x, int z)
    {
        return new HexCoordinates(x - z /2, z);
    }

    public override string ToString()
    {
        return "(" + X.ToString() + ", " + Y.ToString() + ", " + Z.ToString() + ")";
    }

    public string ToStringOnSeparateLines ()
    {
        return X.ToString() + "\n" + Y.ToString() + "\n" + Z.ToString();
    }

    /* This function will take world positions and translate them into hex coordinates so that
     * when a hex is clicked on, no matter where, which hex it is can be calculated */
    public static HexCoordinates FromPosition (Vector3 position)
    {
        float x = position.x / (HexMetrics.innerRadius * 2f);
        float y = -x;

        float offset = position.z / (HexMetrics.outerRadius * 3f);
        x -= offset;
        y -= offset;

        // If we are near the center of the hex, then this should give the correct result
        int iX = Mathf.RoundToInt(x);
        int iY = Mathf.RoundToInt(y);
        int iZ = Mathf.RoundToInt(-x - y);

        // By definition, all three coordinates add to zero, 
        // so if they don't, then we know there's a rounding error
        if (iX + iY + iZ != 0)
        {
            // so find out where the most rounding is occurring...
            float dX = Mathf.Abs(x - iX);
            float dY = Mathf.Abs(y - iY);
            float dZ = Mathf.Abs(-x - y - iZ);

            // ...Then recalculate the most rounded value from the least rounded values
            if(dX > dY && dX > dZ)
            {
                iX = -iY - iZ;
            } else if (dZ > dY)
            {
                iZ = -iX - iY;
            }
        }

        return new HexCoordinates(iX, iZ);
    }
}
