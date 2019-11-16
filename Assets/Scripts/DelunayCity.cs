using HullDelaunayVoronoi.Voronoi;
using HullDelaunayVoronoi.Delaunay;
using HullDelaunayVoronoi.Primitives;
using UnityEngine;
using System.Collections.Generic;

public class DelunayCity : MonoBehaviour
{

    public int NumberOfVertices = 1000;

    public float size = 10;

    public int seed = 0;

    private VoronoiMesh2 voronoi;

    private Material lineMaterial;

    public ProceduralBuilding buildingTemplate;
    public class Cell
    {
        public Vector3 center;
        public List<Vector3> contour;
        public List<Vector3> contourDeflated;

        public List<Vector3> localContourDeflated
        {
            get
            {
                List<Vector3> result = new List<Vector3>();
                foreach(Vector3 point in contourDeflated)
                {
                    result.Add(point - center);
                }
                return result;
            }
        }
    }
    List<Cell> cells;
    private void Start()
    {

        lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));

        List<Vertex2> vertices = new List<Vertex2>();

        Random.InitState(seed);
        for (int i = 0; i < NumberOfVertices; i++)
        {
            float x = size * Random.Range(-1.0f, 1.0f);
            float y = size * Random.Range(-1.0f, 1.0f);

            vertices.Add(new Vertex2(x, y));
        }

        voronoi = new VoronoiMesh2();
        voronoi.Generate(vertices);
        TranslateToUnity(voronoi);

        GenerateBuildings();
    }
    void GenerateBuildings()
    {
        foreach (Cell cell in cells)
        {
            ProceduralBuilding building = Instantiate(buildingTemplate);
            building.transform.parent = this.transform;
            building.transform.position = cell.center;
            int height = Random.Range(2, 20);

            building.Generate(cell.localContourDeflated.ToArray(), height, new Vector2(Random.Range(1, 5), Random.Range(1, 5)));
        }
    }
    void TranslateToUnity(VoronoiMesh2 voronoi)
    {
        cells = new List<Cell>();
        foreach (VoronoiRegion<Vertex2> region in voronoi.Regions)
        {
            float margin = Random.Range(0.1f, 0.5f);
            bool valid = true;

            foreach (DelaunayCell<Vertex2> ce in region.Cells)
            {
                if (!InBound(ce.CircumCenter))
                {
                    valid = false;
                    break;
                }
            }

            if (!valid) continue;

            Cell cell = new Cell();
            Vector3 centroid = Vector3.zero;
            Gizmos.color = Color.white;
            int c = 0;
            cell.contour = new List<Vector3>();
            var edges = region.Edges;
            int count = edges.Count;
            for (int repetitions = 0; repetitions < 5; ++repetitions)
                for (int i = 0; i < count - 1; ++i)
                {
                    for (int j = count - 1; j > i; --j)
                    {
                        // flip if needed
                        if (edges[i].From == edges[j].From || edges[i].To == edges[j].To)
                        {
                            if (edges[i].From == edges[j].From && edges[i].To == edges[j].To)
                            {
                                edges.RemoveAt(j);
                                count = edges.Count;
                            }
                            else
                            {
                                edges[j] = new VoronoiEdge<Vertex2>(edges[j].To, edges[j].From);
                            }
                        }
                    }
                }
            for (int i = 0; i < count - 1; ++i)
            {
                int prev = (i - 1 + count) % count;
                int next = (i + 1 + count) % count;
                for (int j = count - 1; j > i; --j)
                {
                    // flip if needed
                    if (edges[i].From == edges[j].From || edges[i].To == edges[j].To)
                    {
                        if (edges[i].From == edges[j].From && edges[i].To == edges[j].To)
                        {
                            Debug.Log("repeated");
                            edges.RemoveAt(j);
                            count = edges.Count;
                        }
                        else
                        {
                            Debug.Log("flipped");
                            edges[j] = new VoronoiEdge<Vertex2>(edges[j].To, edges[j].From);
                        }

                    }

                    if (edges[i].To.CircumCenter == edges[j].From.CircumCenter)
                    {
                        if (next != j)
                        {
                            var tmp = edges[next];
                            edges[next] = edges[j];
                            edges[j] = tmp;
                        }
                        //break;
                    }
                    if (edges[i].From.CircumCenter == edges[j].To.CircumCenter)
                    {
                        if (prev != j)
                        {
                            var tmp = edges[prev];
                            edges[prev] = edges[j];
                            edges[j] = tmp;
                        }
                        //break;
                    }
                }
            }
            foreach (VoronoiEdge<Vertex2> e in edges)
            {
                Vertex2 v0 = e.From.CircumCenter;
                Vector3 p0 = new Vector3(v0.X, 0, v0.Y);
                Vertex2 v1 = e.To.CircumCenter;
                Vector3 p1 = new Vector3(v1.X, 0, v1.Y);
                centroid += p0;

                cell.contour.Add(p0);
                ++c;
                /* if (c == region.Edges.Count)
                 {
                     cell.contour.Add(p1);
                 }*/
            }
            centroid /= region.Edges.Count;
            cell.center = centroid;

            cell.contourDeflated = new List<Vector3>();
            c = 0;
            foreach (VoronoiEdge<Vertex2> edge in edges)
            {
                Vertex2 v0 = edge.From.CircumCenter;
                Vector3 p0 = new Vector3(v0.X, 0, v0.Y);
                p0 = (p0 - centroid) * (1- margin) + centroid;
                Vertex2 v1 = edge.To.CircumCenter;
                Vector3 p1 = new Vector3(v1.X, 0, v1.Y);
                p1 = (p1 - centroid) * (1 - margin) + centroid;

                cell.contourDeflated.Add(p0);
                ++c;
                /*if(c== region.Edges.Count)
                {
                    cell.contourDeflated.Add(p1);
                }*/

            }
            cells.Add(cell);
        }

    }
    private void OnDrawGizmos()
    {
        if (cells == null)
        {
            return;
        }
        int count = 0;
        foreach (Cell cell in cells)
        {
            Gizmos.color = Color.white;
            Vector3 centroid = cell.center;
            for (int c = 0; c < cell.contour.Count; ++c)
            {
                Gizmos.DrawSphere(cell.center, 0.2f);
                Vector3 p0 = cell.contour[c];
                Vector3 p1 = cell.contour[(c + 1) % cell.contour.Count];
                Gizmos.DrawLine(p0, p1);
            }

            Gizmos.color = new Color((count / 5f) % 1, count % 5 / 5 % 5, 0);
            Gizmos.DrawSphere(cell.center, 0.2f);
            for (int c = 0; c < cell.contourDeflated.Count; ++c)
            {

                Gizmos.DrawSphere(cell.center, 0.2f);
                Vector3 p0 = cell.contourDeflated[c];
                Vector3 p1 = cell.contourDeflated[(c + 1) % cell.contourDeflated.Count];
                Gizmos.DrawLine(p0, p1);

            }
            ++count;
        }
    }

    private bool InBound(Vertex2 v)
    {
        if (v.X < -size || v.X > size) return false;
        if (v.Y < -size || v.Y > size) return false;

        return true;
    }

}








