using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class HexCell : MonoBehaviour {
	public HexCoordinates coordinates;

	public int ColumnIndex { get; set; }

	public RectTransform uiRect;

	public HexGridChunk chunk;

	public HexCellShaderData ShaderData { get; set; }
	public int Index { get; set; }

	public Vector3 Position
	{
		get { return transform.localPosition; }
	}

    public void Initialize(
        Vector3 localPosition,
        HexCoordinates coordinates, 
		int index,
		int columnIndex,
		HexCellShaderData ShaderData
    )
    {
        transform.localPosition = localPosition;
        this.coordinates = coordinates;
        this.Index = index;
        this.ColumnIndex = columnIndex;
        this.ShaderData = ShaderData;
        this.terrain = new Terrain(this.ShaderData, this);
    }

	// Terrain getters/settersHasRoads
	private Terrain terrain;

    public Terrain Terrain
    {
		get { return terrain; }
    }

    // Fog of war
	int visibility;
    bool explored;
    public bool IsExplored
    {
        get { return explored && Explorable; }
        private set { explored = value; }
    }

    public bool IsVisible
    {
        get
        {
            return visibility > 0 && Explorable;
        }
    }

    public bool Explorable { get; set; }

    public void IncreaseVisibility()
    {
        visibility += 1;
        if(visibility == 1)
        {
            IsExplored = true;
            ShaderData.RefreshVisibility(this);
        }
    }
    
    public void DecreaseVisibility()
    {
        visibility -= 1;
        if(visibility <= 0)
        {
            ShaderData.RefreshVisibility(this);
            visibility = 0;     // Should never have have fewer than 0 units providing visibility
        }
    }

    public int ViewElevation
    {
        get { return terrain.ViewElevation; }
    }


    // Pathfinding
    public HexCell PathFrom { get; set; }

    public int SearchHeuristic { get; set; }

    public int SearchPhase { get; set; }

    public int SearchPriority
    {
        get
        {
            return distance + SearchHeuristic;
        }
    }

    public HexCell NextWithSamePriority { get; set; }

    int distance;

    public HexUnit Unit { get; set; }

    public int Distance
    {
        get
        {
            return distance;
        }
        set
        {
            distance = value;
        }
    }

    public void SetLabel (string text)
    {
        Text label = uiRect.GetComponent<Text>();
        label.text = text;
    }

	public int UrbanLevel {
		get {
			return urbanLevel;
		}
		set {
			if (urbanLevel != value) {
				urbanLevel = value;
				RefreshSelfOnly();
			}
		}
	}

	public int FarmLevel {
		get {
			return farmLevel;
		}
		set {
			if (farmLevel != value) {
				farmLevel = value;
				RefreshSelfOnly();
			}
		}
	}

	public int PlantLevel {
		get {
			return plantLevel;
		}
		set {
			if (plantLevel != value) {
				plantLevel = value;
				RefreshSelfOnly();
			}
		}
	}

	public int SpecialIndex {
		get {
			return specialIndex;
		}
		set {
			if (specialIndex != value && !terrain.HasRiver) {
				specialIndex = value;
				RemoveRoads();
				RefreshSelfOnly();
			}
		}
	}

	public bool IsSpecial {
		get {
			return specialIndex > 0;
		}
	}

	public bool Walled {
		get {
			return walled;
		}
		set {
			if (walled != value) {
				walled = value;
				Refresh();
			}
		}
	}


	int urbanLevel, farmLevel, plantLevel;

	int specialIndex;

	bool walled;

	[SerializeField]
	HexCell[] neighbors;

	public HexCell GetNeighbor (HexDirection direction) {
		return neighbors[(int)direction];
	}

	public void SetNeighbor (HexDirection direction, HexCell cell) {
		neighbors[(int)direction] = cell;
		cell.neighbors[(int)direction.Opposite()] = this;
	}

	public HexEdgeType GetEdgeType (HexDirection direction) {
		return HexMetrics.GetEdgeType(
			terrain.Elevation, neighbors[(int)direction].terrain.Elevation
		);
	}

	public HexEdgeType GetEdgeType (HexCell otherCell) {
		return HexMetrics.GetEdgeType(
			terrain.Elevation, otherCell.terrain.Elevation
		);
	}

	public bool HasRiverThroughEdge (HexDirection direction) {
		return
			terrain.HasIncomingRiver && terrain.IncomingRiver == direction ||
			terrain.HasOutgoingRiver && terrain.OutgoingRiver == direction;
	}

	public void RemoveIncomingRiver () {
		if (!terrain.HasIncomingRiver) {
			return;
		}
		terrain.HasIncomingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(terrain.IncomingRiver);
		neighbor.RemoveOutgoingRiver();
		neighbor.RefreshSelfOnly();
	}

	public void RemoveOutgoingRiver () {
		if (!terrain.HasOutgoingRiver) {
			return;
		}
		terrain.HasOutgoingRiver = false;
		RefreshSelfOnly();

		HexCell neighbor = GetNeighbor(terrain.OutgoingRiver);
		neighbor.RemoveIncomingRiver();
		neighbor.RefreshSelfOnly();
	}

	public void RemoveRiver () {
		RemoveOutgoingRiver();
		RemoveIncomingRiver();
	}

	public void SetOutgoingRiver (HexDirection direction) {
		if (terrain.HasOutgoingRiver && terrain.OutgoingRiver == direction) {
			return;
		}

		HexCell neighbor = GetNeighbor(direction);
		if (!IsValidRiverDestination(neighbor)) {
			return;
		}

		RemoveOutgoingRiver();
		if (terrain.HasIncomingRiver && terrain.IncomingRiver == direction) {
			RemoveIncomingRiver();
		}
		terrain.HasOutgoingRiver = true;
		terrain.OutgoingRiver = direction;
		specialIndex = 0;

		neighbor.RemoveIncomingRiver();
		neighbor.terrain.HasIncomingRiver = true;
		neighbor.terrain.IncomingRiver = direction.Opposite();
		neighbor.specialIndex = 0;

		terrain.SetRoad((int)direction, false);
	}

	public bool HasRoadThroughEdge (HexDirection direction) {
		return terrain.HasRoadThroughEdge(direction);
	}

	public void AddRoad (HexDirection direction) {
		if (
			!terrain.HasRoadThroughEdge(direction) && !HasRiverThroughEdge(direction) &&
			!IsSpecial && !GetNeighbor(direction).IsSpecial &&
			GetElevationDifference(direction) <= 1
		) {
			terrain.SetRoad((int)direction, true);
		}
	}

	public void RemoveRoads () {
		for (int i = 0; i < neighbors.Length; i++) {
			if (terrain.HasRoadThroughEdge((HexDirection)i)) {
				terrain.SetRoad(i, false);
			}
		}
	}

	public int GetElevationDifference (HexDirection direction) {
		int difference = terrain.Elevation - GetNeighbor(direction).terrain.Elevation;
		return difference >= 0 ? difference : -difference;
	}

	public bool IsValidRiverDestination (HexCell neighbor) {
		return neighbor && (
			terrain.Elevation >= neighbor.terrain.Elevation || terrain.WaterLevel == neighbor.terrain.Elevation
		);
	}

    //Highlights
    public void DisableHighlight()
    {
        Image highlight = uiRect.GetChild(0).GetComponent<Image>();
        highlight.enabled = false;
    }

    public void EnableHighlight(Color color)
    {
        Image highlight = uiRect.GetChild(0).GetComponent<Image>();
        highlight.color = color;
        highlight.enabled = true;
    }

    //Cell Refreshing

    public void RefreshPosition () {
		Vector3 position = transform.localPosition;
		position.y = terrain.Elevation * HexMetrics.elevationStep;
		position.y +=
			(HexMetrics.SampleNoise(position).y * 2f - 1f) *
			HexMetrics.elevationPerturbStrength;
		transform.localPosition = position;

		Vector3 uiPosition = uiRect.localPosition;
		uiPosition.z = -position.y;
		uiRect.localPosition = uiPosition;
	}

	public void Refresh () {
		if (chunk) {
			chunk.Refresh();
			for (int i = 0; i < neighbors.Length; i++) {
				HexCell neighbor = neighbors[i];
				if (neighbor != null && neighbor.chunk != chunk) {
					neighbor.chunk.Refresh();
				}
			}
            if (Unit)
            {
                Unit.ValidateLocation();
            }
		}
	}

	public void RefreshSelfOnly () {
		chunk.Refresh();
        if (Unit)
        {
            Unit.ValidateLocation();
        }
	}

    public void ResetVisibility()
    {
        if (visibility > 0)
        {
            visibility = 0;
            ShaderData.RefreshVisibility(this);
        }
    }

    // SAVING AND LOADING
     
	public void Save (BinaryWriter writer) {
		writer.Write((byte)terrain.TerrainTypeIndex);
		writer.Write((byte)terrain.Elevation);
		writer.Write((byte)terrain.WaterLevel);
		writer.Write((byte)urbanLevel);
		writer.Write((byte)farmLevel);
		writer.Write((byte)plantLevel);
		writer.Write((byte)specialIndex);
		writer.Write(walled);

		if (terrain.HasIncomingRiver) {
			writer.Write((byte)(terrain.IncomingRiver + 128));
		}
		else {
			writer.Write((byte)0);
		}

		if (terrain.HasOutgoingRiver) {
			writer.Write((byte)(terrain.OutgoingRiver + 128));
		}
		else {
			writer.Write((byte)0);
		}

		int roadFlags = 0;
		for (int i = 0; i < terrain.RoadLength; i++) {
			if (terrain.HasRoadThroughEdge((HexDirection)i)) {
				roadFlags |= 1 << i;
			}
		}
		writer.Write((byte)roadFlags);
        writer.Write(IsExplored);
	}

	public void Load (BinaryReader reader, int header) {
		terrain.TerrainTypeIndex = reader.ReadByte();
        ShaderData.RefreshTerrain(this);
		terrain.Elevation = reader.ReadByte();
		RefreshPosition();
		terrain.WaterLevel = reader.ReadByte();
		urbanLevel = reader.ReadByte();
		farmLevel = reader.ReadByte();
		plantLevel = reader.ReadByte();
		specialIndex = reader.ReadByte();
		walled = reader.ReadBoolean();

		byte riverData = reader.ReadByte();
		if (riverData >= 128) {
			terrain.HasIncomingRiver = true;
			terrain.IncomingRiver = (HexDirection)(riverData - 128);
		}
		else {
			terrain.HasIncomingRiver = false;
		}

		riverData = reader.ReadByte();
		if (riverData >= 128) {
			terrain.HasOutgoingRiver = true;
			terrain.OutgoingRiver = (HexDirection)(riverData - 128);
		}
		else {
			terrain.HasOutgoingRiver = false;
		}

		int roadFlags = reader.ReadByte();
		for (int i = 0; i < terrain.RoadLength; i++) {
			bool currentFlag = (roadFlags & (1 << i)) != 0;
            terrain.SetRoad(i, currentFlag);
        }
        if (header >= 3)
        {
            IsExplored = reader.ReadBoolean();
        }
        ShaderData.RefreshVisibility(this);
	}

    public void SetMapData (float data)
    {
        ShaderData.SetMapData(this, data);
    }
}