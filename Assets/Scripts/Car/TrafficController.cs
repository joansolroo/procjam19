using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficController : MonoBehaviour
{
    public City city;

    public int RoadSize = 3;

    public class Path
    {
        public GraphSparse<Vector3>.Node[] path;
        public List<Vector3> checkpoints;
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
    public GraphSparse<Vector3>.Node GetRandomNode()
    {
        return city.carRoads.nodes[Random.Range(0, city.carRoads.nodes.Count)];
    }

    /*
     * Gives a random next node
     */
    public GraphSparse<Vector3>.Node GetRandomWalk(GraphSparse<Vector3>.Node current)
    {
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
            offset = new Vector3(0, 1, 1);
        }
        else if (direction.x < 0)
        {
            offset = new Vector3(0, 1, -1);
        }
        else if (direction.z > 0)
        {
            offset = new Vector3(1, -1, 0);
        }
        else if (direction.z < 0)
        {
            offset = new Vector3(-1, -1, 0);
        }
        else if (direction.y > 0)
        {
            offset = new Vector3(1, 0, 1) * 2;
        }
        else if (direction.y < 0)
        {
            offset = new Vector3(-1, 0, -1) * 2;
        }
        return offset;
    }

    private void OnDrawGizmos()
    {
        if (city!=null && city.carRoads != null)
        {

            GraphSparse<Vector3> roads = city.carRoads;
            foreach (GraphSparse<Vector3>.Node node1 in roads.nodes)
            {
                Gizmos.color = Color.white;
                Gizmos.DrawCube(node1.data, Vector3.one * 3 * 2);
                foreach (GraphSparse<Vector3>.Link link in node1.links)
                {
                    GraphSparse<Vector3>.Node node2 = roads.nodes[link.to];
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
