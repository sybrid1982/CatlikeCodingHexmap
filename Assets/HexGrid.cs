using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

    public int chunkCountX = 4, chunkCountZ = 3;

    public Color defaultColor = Color.white;
    
    public HexCell cellPrefab;

    public Text cellLabelPrefab;

    public HexGridChunk chunkPrefab;

    int cellCountX, cellCountZ;

    HexCell[] cells;
    HexGridChunk[] chunks;

    public Texture2D noiseSource;

    void Awake()
    {
        HexMetrics.noiseSource = noiseSource;

        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;

        CreateChunks();
        CreateCells();
    }

    private void OnEnable()
    {
        HexMetrics.noiseSource = noiseSource;
    } 

    public HexCell GetCell(Vector3 position)
    {
        // get coordinates for clicked cell
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);
        // translate those coordinates into the index in the array of cells
        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;
        HexCell cell = cells[index];
        return cell;
    }

    public HexCell GetCell(HexCoordinates coordinates)
    {
        int z = coordinates.Z;
        if(z < 0 || z >= cellCountZ)
        {
            return null;
        }
        int x = coordinates.X + z / 2;
        if( x < 0 || x >= cellCountX)
        {
            return null;
        }
        return cells[x + z * cellCountX];
    }

    private void CreateChunks()
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];
        for(int z = 0, i = 0; z < chunkCountZ; z++)
        {
            for(int x = 0; x < chunkCountX; x++)
            {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab);
                chunk.transform.SetParent(transform);
            }
        }
    }

    private void CreateCells()
    {
        cells = new HexCell[cellCountZ * cellCountX];

        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    void CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * HexMetrics.innerRadius * 2f;
        position.y = 0f;
        position.z = z * HexMetrics.outerRadius * 1.5f;

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        //cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.Color = defaultColor;

        CreateNeighborsForCell(x, z, i, cell);

        Text label = Instantiate<Text>(cellLabelPrefab);
        //label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeparateLines();
        cell.uiRect = label.rectTransform;

        cell.Elevation = 0;

        AddCellToChunk(x, z, cell);
    }

    void AddCellToChunk(int x, int z, HexCell cell)
    {
        int chunkX = x / HexMetrics.chunkSizeX;
        int chunkZ = z / HexMetrics.chunkSizeZ;
        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];

        int localX = x - chunkX * HexMetrics.chunkSizeX;
        int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
    }

    private void CreateNeighborsForCell(int x, int z, int i, HexCell cell)
    {
        // Connect all cells E-W wise,
        // skipping the first cell in each row which has no westward cell
        // If you wanted the map to wrap E-W, you'd want to connect the cell at the end
        // of a row with one at the beginning
        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }
        // Connect all cells NW-SE wise
        // The first row of cells has no SE neighbors, so skip them
        if (z > 0)
        {
            // if the index bitwise ends in 1, then it's an 'even' row because we count up from 0
            // even rows (other than the first) always have a SE neighbor
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
                // since we've made a mess already, let's go ahead and grab the SW neighbor, which
                // every cell but the first has.
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                }
            } else
            {
                // odd rows follow the opposite logic
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if(x < cellCountX - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                }
            }
        }
    }

    public void ShowUI (bool visible)
    {
        for(int i = 0; i < chunks.Length; i++)
        {
            chunks[i].ShowUI(visible);
        }
    }
}
