using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficController : MonoBehaviour
{
    public City city;
    public int RoadSize = 3;
    [SerializeField] int cars = 100;
    [SerializeField] float maxCarDistanceInCells = 5;

    [SerializeField] ParticlePool carPool;
    [SerializeField] List<CarAI> activeCars;

    Transform cameraTransform;
    Vector3 trafficCenter;

    public class Path
    {
        public GraphSparse<Vector3>.Node[] path;
        public List<Vector3> checkpoints;
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }
    bool created = false;
    private void Update()
    {
        trafficCenter = cameraTransform.position + cameraTransform.forward * 100;
        if (!created && city.carRoads != null)
        {
            for (int c = 0; c < cars; ++c)
            {
                AddCar();
            }
            created = true;
        }
        CheckCars();
    }

    void AddCar()
    {
        CarAI car = carPool.Take<CarAI>();
        activeCars.Add(car);
        car.gameObject.SetActive(true);
    }
    private void CheckCars()
    {
        for(int c = activeCars.Count-1;c>=0;--c)
        {
            CarAI car = activeCars[c];
            if (Vector3.Distance(car.transform.position, cameraTransform.position)>city.transform.lossyScale.x* maxCarDistanceInCells)
            {
                car.Destroy();
                activeCars.RemoveAt(c);
                AddCar();
            }
        }
    }
    /*public Path GetPath(GraphSparse<Vector3>.Node origin, GraphSparse<Vector3>.Node target)
    {
        Vector3[] p = new Vector3[2];
        p[0] = origin.data;
        p[1] = target.data;
        Path path = new Path();
        path.checkpoints = p;
        return path;
    }*/
    public GraphSparse<Vector3>.Node GetStartingPoint()
    {
        

        GraphSparse<Vector3>.Node node;
        int tries = 0;
        do
        {
            Vector3Int targetCell = city.WorldToCell(trafficCenter);
            targetCell.x = (int)Mathf.Clamp(targetCell.x + Random.Range(-maxCarDistanceInCells / 2, maxCarDistanceInCells / 2), 0, city.size.x);
            targetCell.y = (int)Random.Range(0, 5);
            targetCell.z = (int)Mathf.Clamp(targetCell.z + Random.Range(-maxCarDistanceInCells / 2, maxCarDistanceInCells / 2), 0, city.size.z);
            node = city.carNodes[targetCell.x, targetCell.y, targetCell.z];
        } while (node == null && tries < 10);
        if (node == null)
        {
            Debug.LogWarning("No available road");
        }
        return node;
    }

    public GraphSparse<Vector3>.Node GetRandomNode()
    {
        GraphSparse<Vector3>.Node node;
        int tries = 0;
        do
        {
            node = city.carRoads.nodes[Random.Range(0, city.carRoads.nodes.Count)];
        } while (node == null && tries<10);
        if(node == null)
        {
            Debug.LogWarning("No available road");
        }
        return node;
    }
    /*
     * Gives a random next node
     */
    public GraphSparse<Vector3>.Node GetRandomWalk(GraphSparse<Vector3>.Node current)
    {
        if (current == null)
        {
            Debug.LogError("Current node is null");
        }
        return city.carRoads.nodes[current.links[Random.Range(0, current.links.Count)].to];
    }
    /*
     * Gives a random next node, preventing going back if possible
     */
    public GraphSparse<Vector3>.Node GetRandomWalk(GraphSparse<Vector3>.Node previous, GraphSparse<Vector3>.Node current)
    {
        int previousId = previous.id;
        int nextId = previousId;
        do
        {
            nextId = Random.Range(0, current.links.Count);
        } while (current.links.Count > 1 && nextId == previousId);

        // Debug.Log("From:" + current.id + ", goto:" + nextId + "/" + current.links.Count);
        return city.carRoads.nodes[current.links[nextId].to];
    }
    public Vector3 GetRoadPoint(GraphSparse<Vector3>.Node node, Vector3 direction)
    {
        return node.data+GetOffset(direction);
    }
    public Vector3 GetOffset(Vector3 direction)
    {
        Vector3 offset = Vector3.zero;
        if (direction.x > 0)
        {
            offset = new Vector3(0, 1, -1);
        }
        else if (direction.x < 0)
        {
            offset = new Vector3(0, 1, 1);
        }
        else if (direction.z > 0)
        {
            offset = new Vector3(-1, -1, 0);
        }
        else if (direction.z < 0)
        {
            offset = new Vector3(1, -1, 0);
        }
        else if (direction.y > 0)
        {
            offset = new Vector3(-1, 0, -1) * 2;
        }
        else if (direction.y < 0)
        {
            offset = new Vector3(1, 0, 1) * 2;
        }
        return offset;
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(Camera.main.transform.position, city.transform.lossyScale.x * maxCarDistanceInCells);
        Gizmos.DrawWireSphere(trafficCenter, city.transform.lossyScale.x * maxCarDistanceInCells);
    }
    private void OnDrawGizmosSelected()
    {
        if (city!=null && city.carRoads != null)
        {

            GraphSparse<Vector3> roads = city.carRoads;
            foreach (GraphSparse<Vector3>.Node node1 in roads.nodes)
            {
                if (node1 != null)
                {
                    Gizmos.color = Color.white;
                    Gizmos.DrawCube(node1.data, Vector3.one * 3 * 2);
                    foreach (GraphSparse<Vector3>.Link link in node1.links)
                    {
                        GraphSparse<Vector3>.Node node2 = roads.nodes[link.to];
                        if (node1 != null)
                        {
                            Gizmos.color = Color.gray;
                            Gizmos.DrawLine(node1.data, node2.data);

                            Vector3 direction = node2.data - node1.data;
                            Vector3 offset = GetOffset(direction);
                            Color color = Color.black;
                            if (direction.x != 0)
                            {
                                color = Color.green;
                            }
                            else if (direction.z != 0)
                            {
                                color = Color.red;
                            }

                            else if (direction.y != 0)
                            {
                                color = Color.blue;
                            }

                            Gizmos.color = color;
                            Gizmos.DrawLine(node1.data + offset * RoadSize, node2.data + offset * RoadSize);
                        }
                    }

                }
            }
        }
    }
}
