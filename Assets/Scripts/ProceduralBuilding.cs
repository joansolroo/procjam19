using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProceduralBuilding : MonoBehaviour
{
    [SerializeField] bool generate = false;
    [SerializeField] bool flip = false;
    [SerializeField] Transform[] contour;
    [SerializeField] MeshFilter filter;
    [SerializeField] [Range(1, 10)] int height = 1;
    [SerializeField] Vector2 windowScale = Vector2.one;

    [SerializeField] bool useControlPoints = false;
    [SerializeField] Vector3[] contourPoints;

    [SerializeField] MeshCollider collider;
    // Start is called before the first frame update
    void Start()
    {
        OnValidate();
    }

    private void OnValidate()
    {
        if (contourPoints == null || contourPoints.Length != contour.Length)
        {
            contourPoints = new Vector3[contour.Length];
        }
        if (useControlPoints)
        {
            for (int i = 0; i < contour.Length; ++i)
            {
                contourPoints[i] = contour[i].localPosition;
            }
        }
        else
        {
            for (int i = 0; i < contour.Length; ++i)
            {
                contour[i].localPosition = contourPoints[i];
            }
        }
        if (generate)
        {
            Generate(contourPoints,height, windowScale);
        }
    }

    public void Generate(Vector3[] points, int height,Vector2 windowScale)
    {
        this.height = height;
        if (filter == null)
        {
            filter = GetComponent<MeshFilter>();
        }
        Mesh mesh = filter.mesh;
        if (mesh != null)
        {
            mesh.Clear();
        }
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector3> normals = new List<Vector3>();
        List<Vector2> uvs = new List<Vector2>();

        // CONTOUR
        int contourCount = points.Length;
        for (int h = 0; h <= height; ++h)
        {
            for (int i = 0; i <= contourCount; ++i)
            {
                vertices.Add((points[i % contourCount]) + Vector3.up * h);
                // TODO normalize using distance between vertices
                if (i % 2 == 0 && h % 2 == 0) uvs.Add(new Vector2(0, 0));
                else if (i % 2 != 0 && h % 2 == 0) uvs.Add(new Vector2(windowScale.x, 0));
                else if (i % 2 == 0 && h % 2 != 0) uvs.Add(new Vector2(0, windowScale.y));
                else if (i % 2 != 0 && h % 2 != 0) uvs.Add(new Vector2(windowScale.x, windowScale.y));

            }
        }
        int levelCount = contourCount + 1;
        for (int h = 0; h < height; ++h)
        {
            for (int i = 0; i < contourCount; ++i)
            {
                int idx = i + h * levelCount;
                
                triangles.Add(idx);
                if (flip)
                {
                    
                    triangles.Add(idx + 1);
                    triangles.Add(idx + levelCount);
                }
                else
                {
                    triangles.Add(idx + levelCount);
                    triangles.Add(idx + 1);
                   
                }

                triangles.Add(idx + 1);
                if (flip)
                {
                    triangles.Add(idx + levelCount + 1);
                    triangles.Add(idx + levelCount);
                    
                }
                else
                {
                    triangles.Add(idx + levelCount);
                    triangles.Add(idx + levelCount + 1);
                }
               
            }
        }

        // ROOF
        Vector3 centroid = Vector3.zero;
        for (int i = 0; i <= contourCount; ++i)
        {
            Vector3 vertex = (points[i % contourCount]) + Vector3.up * height;
            vertices.Add(vertex);
            uvs.Add(new Vector2(0, 0));
            if (i != 0)
            {
                centroid += vertex;
            }
        }
        centroid /= contourCount;
        int centroidIdx = vertices.Count;
        uvs.Add(new Vector2(0, 0));
        vertices.Add(centroid);
        for (int i = 0; i < contourCount; ++i)
        {
            int idx = i + (height + 1) * levelCount;
            triangles.Add(idx);
            triangles.Add(centroidIdx);
            triangles.Add(idx + 1);
        }

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.RecalculateNormals();

        collider = GetComponent<MeshCollider>();
        if (collider)
        {
            collider.sharedMesh = mesh;
        }
    }
    private void OnDrawGizmos()
    {
        for (int i = 0; i < contour.Length; ++i)
        {
            Gizmos.DrawSphere(contour[i].position, 0.25f);
            Gizmos.DrawLine(contour[i].position, contour[(i + 1) % contour.Length].position);
        }
    }
}
