using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class Terrain
{
	int terrainTypeIndex;

	int elevation = int.MinValue;
	int waterLevel;

	bool hasIncomingRiver, hasOutgoingRiver;
	HexDirection incomingRiver, outgoingRiver;

	bool[] roads;

    private HexCellShaderData ShaderData;
    private HexCell cell;

    public Terrain(HexCellShaderData ShaderData,
                   HexCell cell)
    {
        this.ShaderData = ShaderData;
        this.cell = cell;
        this.roads = new bool[6];
    }

	public int TerrainTypeIndex
	{
		get
		{
			return terrainTypeIndex;
		}
		set
		{
			if (terrainTypeIndex != value)
			{
				terrainTypeIndex = value;
                ShaderData.RefreshTerrain(cell);
            }
		}
	}

	public int Elevation
	{
		get
		{
			return elevation;
		}
		set
		{
			if (elevation == value)
			{
				return;
			}
			int originalViewElevation = ViewElevation;
			elevation = value;
			if (ViewElevation != originalViewElevation)
			{
				ShaderData.ViewElevationChanged();
			}
			cell.RefreshPosition();
			ValidateRivers();

			for (int i = 0; i < roads.Length; i++)
			{
				if (roads[i] && cell.GetElevationDifference((HexDirection)i) > 1)
				{
					SetRoad(i, false);
				}
			}

			cell.Refresh();
		}
	}

    public int ViewElevation
    {
        get
        {
            return Elevation >= WaterLevel ? Elevation : WaterLevel;
        }
    }


	public int WaterLevel
	{
		get
		{
			return waterLevel;
		}
		set
		{
			if (waterLevel == value)
			{
				return;
			}
			int originalViewElevation = ViewElevation;
			waterLevel = value;
			if (ViewElevation != originalViewElevation)
			{
				ShaderData.ViewElevationChanged();
			}
			ValidateRivers();
			cell.Refresh();
		}
	}

	public bool IsUnderwater
	{
		get
		{
			return waterLevel > elevation;
		}
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

	public bool HasRoads
	{
		get
		{
			for (int i = 0; i < roads.Length; i++)
			{
				if (roads[i])
				{
					return true;
				}
			}
			return false;
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
				(elevation + HexMetrics.streamBedElevationOffset) *
				HexMetrics.elevationStep;
		}
	}

	public float RiverSurfaceY
	{
		get
		{
			return
				(elevation + HexMetrics.waterElevationOffset) *
				HexMetrics.elevationStep;
		}
	}

	public float WaterSurfaceY
	{
		get
		{
			return
				(waterLevel + HexMetrics.waterElevationOffset) *
				HexMetrics.elevationStep;
		}
	}

	public bool HasRoadThroughEdge(HexDirection direction)
	{
		return roads[(int)direction];
	}

    public void SetRoad(int index, bool state)
    {
        roads[index] = state;
        cell.GetNeighbor((HexDirection)index).Terrain.roads[(int)((HexDirection)index).Opposite()] = state;
        cell.GetNeighbor((HexDirection)index).RefreshSelfOnly();
        cell.RefreshSelfOnly();
    }

    public int RoadLength
    {
        get { return roads.Length; }
    }

    void ValidateRivers()
    {
        if (
            HasOutgoingRiver &&
            !cell.IsValidRiverDestination(cell.GetNeighbor(OutgoingRiver))
        )
        {
            cell.RemoveOutgoingRiver();
        }

        if (
            HasIncomingRiver &&
            !cell.GetNeighbor(IncomingRiver).IsValidRiverDestination(cell)
        )
        {
            cell.RemoveIncomingRiver();
        }
    }
}