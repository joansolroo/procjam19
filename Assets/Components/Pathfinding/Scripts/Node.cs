using UnityEngine;
using System.Collections;

namespace Pathfinding
{
    public class Node : IHeapItem<Node>
    {

        public bool walkable;
        public Vector3 worldPosition;
        public int movementPenalty;

        public float gCost;
        public float hCost;
        public Node parent;

        public Node(bool _walkable, Vector3 _worldPos, int _penalty)
        {
            walkable = _walkable;
            worldPosition = _worldPos;
            movementPenalty = _penalty;
        }

        public float fCost
        {
            get
            {
                return gCost + hCost;
            }
        }

        public int HeapIndex { get; set; }

        public int CompareTo(Node nodeToCompare)
        {
            int compare = fCost.CompareTo(nodeToCompare.fCost);
            if (compare == 0)
            {
                compare = hCost.CompareTo(nodeToCompare.hCost);
            }
            return -compare;
        }
    }

}