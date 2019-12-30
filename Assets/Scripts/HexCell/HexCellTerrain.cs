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
    private RiverTerrain riverTerrain;

    public Terrain(HexCellShaderData ShaderData,
                   HexCell cell)
    {
        this.ShaderData = ShaderData;
        this.cell = cell;
        this.roads = new bool[6];
        this.riverTerrain = new RiverTerrain(cell, this);
    }

	public RiverTerrain RiverTerrain { get { return riverTerrain; } }

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
			riverTerrain.ValidateRivers();

			for (int i = 0; i < roads.Length; i++)
			{
				if (roads[i] && GetElevationDifference((HexDirection)i) > 1)
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
			riverTerrain.ValidateRivers();
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

	public void AddRoad(HexDirection direction)
	{
		if (
			!HasRoadThroughEdge(direction) && !riverTerrain.HasRiverThroughEdge(direction) &&
			!cell.IsSpecial && !cell.GetNeighbor(direction).IsSpecial &&
			GetElevationDifference(direction) <= 1
		)
		{
			SetRoad((int)direction, true);
		}
	}

	public void RemoveRoads()
	{
		for (int i = 0; i < roads.Length; i++)
		{
			if (HasRoadThroughEdge((HexDirection)i))
			{
				SetRoad(i, false);
			}
		}
	}

	public int GetElevationDifference(HexDirection direction)
	{
		int difference = Elevation - cell.GetNeighbor(direction).Terrain.Elevation;
		return difference >= 0 ? difference : -difference;
	}
}