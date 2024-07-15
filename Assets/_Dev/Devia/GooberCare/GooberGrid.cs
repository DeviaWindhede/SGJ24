using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

class GooberState
{
    public float Dirtiness = 0.0f;
    public bool ShouldIgnore = false;
    public bool IsDirty => Dirtiness < 0.9f;
}

public class GooberGrid : MonoBehaviour
{
    [Min(1)] public int width;
    [Min(1)] public int height;
    public float speed;
    List<List<GooberState>> grid = new();

    private float _cleanliness;
    private void CalculateCleanliness()
    {
        int total = 0;
        int clean = 0;
        for (int i = 0; i < grid.Count; i++)
        {
            for (int j = 0; j < grid[i].Count; j++)
            {
                if (grid[i][j].ShouldIgnore) { continue; }
                ++total;
                clean += grid[i][j].IsDirty ? 0 : 1;
            }
        }
        _cleanliness = (float)clean / (float)total;
    }

    //public Vector3 GetCellCenter(int x, int y)
    //{
    //    return new Vector3(x, y) * CellSize + Origin;
    //}

    // Start is called before the first frame update
    void Start()
    {
        for (int x = 0; x < width; x++)
        {
            grid.Add(new List<GooberState>());
            for (int y = 0; y < height; y++)
            {
                grid[x].Add(new GooberState());
                grid[x][y].ShouldIgnore = true;
                
                Vector3 viewport = CellToViewport(new(x, y));
                Ray ray = Camera.main.ViewportPointToRay(viewport);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit))
                {
                    grid[x][y].ShouldIgnore = false;
                }
            }
        }
    }

    List<GooberState> list = new();
    private void Update()
    {
        if (!Input.GetMouseButton(0)) { return; }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 cell = Camera.main.WorldToViewportPoint(hit.point);
            cell.x = cell.x * width;
            cell.y = cell.y * height;

            var c = grid[(int)cell.x][(int)cell.y];
            if (c.ShouldIgnore) { return; }

            c.Dirtiness += speed * Time.deltaTime;
            if (c.Dirtiness > 1.0f) { c.Dirtiness = 1.0f; }

            CalculateCleanliness();
        }
    }

    Vector3 CellToViewport(Vector2 cell)
    {
        return new Vector3(cell.x / width, cell.y / height, Camera.main.nearClipPlane);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        for (int x = 0; x < grid.Count; x++)
        {
            for (int y = 0; y < grid[x].Count; y++)
            {
                if (grid[x][y].Dirtiness >= 1.0f || grid[x][y].ShouldIgnore) { continue; }
                Color c = Color.Lerp(Color.red, Color.green, grid[x][y].Dirtiness);
                Gizmos.color = c;

                Vector3 cell = Camera.main.ViewportToWorldPoint(CellToViewport(new(x, y)));
                Gizmos.DrawWireCube(cell, new Vector3(1.0f / width, 1.0f / height, 0));
                //Gizmos.DrawSphere(cell, 0.1F);
            }
        }

        //for (int i = 0; i < Width; i++)
        //{
        //    for (int j = 0; j < Height; j++)
        //    {
        //        Vector3 cellCenter = GetCellCenter(i, j);

        //        Vector3 cell = new Vector3(CellSize, CellSize, 0);

        //        print(Camera.main.ScreenToViewportPoint(new Vector3(i, j, 0)));
        //        Camera.main.ViewportToWorldPoint
        //        //Gizmos.
        //        //Gizmos.DrawWireCube(temp, new Vector3(CellSize, CellSize, 0));
        //    }
        //}
    }
}
