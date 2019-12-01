using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightTrail : MonoBehaviour
{
    [SerializeField] new Renderer renderer;
    [SerializeField] Transform parent;
    [SerializeField] MeshFilter filter;
    [SerializeField] Gradient color;
    [SerializeField] int size = 50;
    [SerializeField] int speedSize = 10;
    Vector3 prevPosition;

    List<Vector3> positions1 = new List<Vector3>();
    List<Vector3> positions2 = new List<Vector3>();
    // Start is called before the first frame update
    bool initialized = false;
    void Start()
    {
        if (!initialized)
        {
            initialized = true;
            prevPosition = parent.position;
            filter.mesh = new Mesh();
            renderer = GetComponent<Renderer>();
        }
    }

    private void OnEnable()
    {
        if (!initialized)
            Start();
        renderer.enabled = true;
        Clear();
    }
    private void OnDisable()
    {
        renderer.enabled = false;
    }

    void Clear()
    {
        prevPosition = parent.position;
        positions1.Clear();
        positions2.Clear();
    }
    float targetSize = 0;
    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 position = parent.position;
        Vector3 movement = position - prevPosition;
        prevPosition = position;

        if(movement.magnitude > 5f)
        {
            Clear();
        }
        
        positions1.Insert(0, this.transform.TransformPoint(-0.5f, 0, 0));
        positions2.Insert(0, this.transform.TransformPoint(0.5f, 0, 0));

        Vector3 movementHorizontal = movement;
        movementHorizontal.y = 0;
        targetSize = Mathf.MoveTowards(targetSize,(int)(Mathf.Min(size,size * speedSize * (movementHorizontal).magnitude)),60*Time.deltaTime);
        
        while (positions1.Count > targetSize || positions2.Count > targetSize)
        {
            positions1.RemoveAt(positions1.Count - 1);
            positions2.RemoveAt(positions2.Count - 1);
        }
        if (positions1.Count > 1)
        {
            Bake();
        }
        else
        {
            filter.mesh.Clear();
        }
    }
    void Bake()
    {
        Mesh mesh = filter.mesh;
        int pointCount = positions1.Count + positions2.Count;
        Vector3[] vertices = new Vector3[pointCount];
        Color[] colors = new Color[pointCount];
        for (int i = 0; i < positions1.Count; ++i)
        {
            // TODO improve, use spline smoothing for nicer curves
            vertices[i * 2] = this.transform.InverseTransformPoint(positions1[i]);
            vertices[i * 2 + 1] = this.transform.InverseTransformPoint(positions2[i]);
            Color c = color.Evaluate(i / (float)(positions1.Count - 1));
            colors[i * 2] = c;
            colors[i * 2 + 1] = c;
        }
        List<int> triangles = new List<int>();
        for (int i = 0; i < positions2.Count - 1; ++i)
        {
            triangles.Add(2 * i);
            triangles.Add(2 * i + 1);
            triangles.Add(2 * i + 2);

            triangles.Add(2 * i + 1);
            triangles.Add(2 * i + 2);
            triangles.Add(2 * i + 3);
        }
        try
        {
            mesh.Clear();
            mesh.vertices = vertices;
            mesh.triangles = triangles.ToArray();
            mesh.colors = colors;
        }catch(System.Exception)
        {
            Debug.LogWarning(mesh.vertices.Length + ";" + mesh.colors.Length + ";" + mesh.triangles.Length);
        }
    }
}
