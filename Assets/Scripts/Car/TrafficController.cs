using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrafficController : MonoBehaviour
{
    public class Street
    {
    }
    public class Path
    {
        public Vector3[] checkpoints;
    }
    public Path GetPath(Vector3 origin, Vector3 target)
    {
        Vector3[] p = new Vector3[2];
        p[0] = origin;
        p[1] = target;
        Path path = new Path();
        path.checkpoints = p;

        return path;
    }
}
