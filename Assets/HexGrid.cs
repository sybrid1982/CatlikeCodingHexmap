using UnityEngine;
using UnityEngine.UI;

public class HexGrid : MonoBehaviour {

    public int width = 6;
    public int height = 6;

    public HexCell cellPrefab;

    public Text cellLabelPrefab;

    public Color defaultColor = Color.white;

    Canvas gridCanvas;

    HexCell[] cells;

    HexMesh hexMesh;

    public Texture2D noiseSource;

    void Awake()
    {
        HexMetrics.noiseSource = noiseSource;

        gridCanvas = GetComponentInChildren<Canvas>();
        hexMesh = GetComponentInChildren<HexMesh>();

        cells = new HexCell[height * width];

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    private void Start()
    {
        hexMesh.Triangulate(cells);  
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
        int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
        HexCell cell = cells[index];
        return cell;
    }

    public void Refresh()
    {
        hexMesh.Triangulate(cells);
    }

    void CreateCell(int x, int z, int i)
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * HexMetrics.innerRadius * 2f;
        position.y = 0f;
        position.z = z * HexMetrics.outerRadius * 1.5f;

        HexCell cell = cells[i] = Instantiate<HexCell>(cellPrefab);
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);
        cell.color = defaultColor;

        CreateNeighborsForCell(x, z, i, cell);

        Text label = Instantiate<Text>(cellLabelPrefab);
        label.rectTransform.SetParent(gridCanvas.transform, false);
        label.rectTransform.anchoredPosition = new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeparateLines();
        cell.uiRect = label.rectTransform;

        cell.Elevation = 0;
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
                cell.SetNeighbor(HexDirection.SE, cells[i - width]);
                // since we've made a mess already, let's go ahead and grab the SW neighbor, which
                // every cell but the first has.
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - width - 1]);
                }
            } else
            {
                // odd rows follow the opposite logic
                cell.SetNeighbor(HexDirection.SW, cells[i - width]);
                if(x < width - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
                }
            }
        }
    }
}
