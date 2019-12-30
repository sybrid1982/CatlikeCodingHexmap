public class RiverTerrain
{
    bool hasIncomingRiver, hasOutgoingRiver;
    HexDirection incomingRiver, outgoingRiver;

    private Terrain terrain;
    private HexCell cell;

    public RiverTerrain(HexCell cell, Terrain terrain)
    {
        this.cell = cell;
        this.terrain = terrain;
    }

    public bool HasIncomingRiver
    {
        get
        {
            return hasIncomingRiver;
        }
        set
        {
            hasIncomingRiver = value;
        }

    }

    public bool HasOutgoingRiver
    {
        get
        {
            return hasOutgoingRiver;
        }
        set
        {
            hasOutgoingRiver = value;
        }
    }

    public bool HasRiver
    {
        get
        {
            return hasIncomingRiver || hasOutgoingRiver;
        }
    }

    public bool HasRiverBeginOrEnd
    {
        get
        {
            return hasIncomingRiver != hasOutgoingRiver;
        }
    }

    public HexDirection RiverBeginOrEndDirection
    {
        get
        {
            return hasIncomingRiver ? incomingRiver : outgoingRiver;
        }
    }
    public HexDirection IncomingRiver
    {
        get
        {
            return incomingRiver;
        }
        set
        {
            incomingRiver = value;
        }
    }

    public HexDirection OutgoingRiver
    {
        get
        {
            return outgoingRiver;
        }
        set
        {
            outgoingRiver = value;
        }

    }

    public float StreamBedY
    {
        get
        {
            return
                (terrain.Elevation + HexMetrics.streamBedElevationOffset) *
                HexMetrics.elevationStep;
        }
    }

    public float RiverSurfaceY
    {
        get
        {
            return
                (terrain.Elevation + HexMetrics.waterElevationOffset) *
                HexMetrics.elevationStep;
        }
    }
    public bool HasRiverThroughEdge(HexDirection direction)
    {
        return
            HasIncomingRiver && IncomingRiver == direction ||
            HasOutgoingRiver && OutgoingRiver == direction;
    }

    public void RemoveIncomingRiver()
    {
        if (!HasIncomingRiver)
        {
            return;
        }
        HasIncomingRiver = false;
        cell.RefreshSelfOnly();

        HexCell neighbor = cell.GetNeighbor(IncomingRiver);
        neighbor.Terrain.RiverTerrain.RemoveOutgoingRiver();
        neighbor.RefreshSelfOnly();
    }

    public void RemoveOutgoingRiver()
    {
        if (!HasOutgoingRiver)
        {
            return;
        }
        HasOutgoingRiver = false;
        cell.RefreshSelfOnly();

        HexCell neighbor = cell.GetNeighbor(OutgoingRiver);
        neighbor.Terrain.RiverTerrain.RemoveIncomingRiver();
        neighbor.RefreshSelfOnly();
    }

    public void RemoveRiver()
    {
        RemoveOutgoingRiver();
        RemoveIncomingRiver();
    }

    public void SetOutgoingRiver(HexDirection direction)
    {
        if (HasOutgoingRiver && OutgoingRiver == direction)
        {
            return;
        }

        HexCell neighbor = cell.GetNeighbor(direction);
        if (!IsValidRiverDestination(neighbor))
        {
            return;
        }

        RemoveOutgoingRiver();
        if (HasIncomingRiver && IncomingRiver == direction)
        {
            RemoveIncomingRiver();
        }
        HasOutgoingRiver = true;
        OutgoingRiver = direction;
        cell.SpecialIndex = 0;

        neighbor.Terrain.RiverTerrain.RemoveIncomingRiver();
        neighbor.Terrain.RiverTerrain.HasIncomingRiver = true;
        neighbor.Terrain.RiverTerrain.IncomingRiver = direction.Opposite();
        neighbor.SpecialIndex = 0;

        terrain.SetRoad((int)direction, false);
    }

    public bool IsValidRiverDestination(HexCell neighbor)
    {
        return neighbor && (
                   terrain.Elevation >= neighbor.Terrain.Elevation || terrain.WaterLevel == neighbor.Terrain.Elevation
               );
    }

    public void ValidateRivers()
    {
        if (
            HasOutgoingRiver &&
            !IsValidRiverDestination(cell.GetNeighbor(OutgoingRiver))
        )
        {
            RemoveOutgoingRiver();
        }

        if (
            HasIncomingRiver &&
            !cell.GetNeighbor(IncomingRiver).Terrain.RiverTerrain.IsValidRiverDestination(cell)
        )
        {
            RemoveIncomingRiver();
        }
    }
}
