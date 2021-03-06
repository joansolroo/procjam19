﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OcclusionCulling : MonoBehaviour
{
    new static Camera camera;
    static Plane[] cameraPlanes;
    static OcclusionCulling instance;
    [SerializeField] bool cullAABBvsFustrum = true;
    [SerializeField] bool cullAABBvsSingleOccluder = true;
    [SerializeField] bool cullCollider = true;
    [SerializeField] bool cullTerrainElements = true;
    [SerializeField] LayerMask mask;

    // Start is called before the first frame update
    private void OnValidate()
    {
        instance = this;
        camera = Camera.main;
    }
    private void Awake()
    {
        OnValidate();
    }

    private void FixedUpdate()
    {
        cameraPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
    }
    public static Collider TestPoint(Vector3 point)
    {
        RaycastHit hitInfo;
        float distance = Vector3.Distance(point, camera.transform.position);
        bool hit = Physics.Raycast(camera.transform.position, point - camera.transform.position, out hitInfo, distance, instance.mask);
        if (hit && hitInfo.distance < distance)
        {
            return hitInfo.collider;
        }
        else
        {
            return null;
        }
    }
    public static bool IsVisibleDiagonal(Vector3 from, Vector3 to)
    {
        Collider c1 = TestPoint(from);
        if (c1 != null)
        {
            Collider c2 = TestPoint(to);
            if (c2 != null && c1 == c2)
            {
                return false;
            }
        }
        return true;
    }

    public static bool ContainsCamera(Bounds bounds)
    {
        return bounds.Contains(camera.transform.position);
    }
    public static bool IsVisibleAABB(Bounds bounds)
    {
        if (!instance.enabled) return true;
        if (!instance.cullAABBvsFustrum && !instance.cullAABBvsSingleOccluder) return true;

        if (ContainsCamera(bounds))
        {
            return true;
        }
        Vector3 distance = bounds.center - camera.transform.position;
        Vector3 extents = bounds.extents;
        /*if(Mathf.Abs(distance.x)<extents.x*2|| Mathf.Abs(distance.y) < extents.y * 2 || Mathf.Abs(distance.z) < extents.z * 2)
        {
            return true;
        }*/
        bool inFOV = !instance.cullAABBvsFustrum || GeometryUtility.TestPlanesAABB(cameraPlanes, bounds);
        if (inFOV)
        {
            if (!instance.cullAABBvsSingleOccluder)
            {
                return true;
            }
            else
            {
                Vector3 boundPoint1 = bounds.min;
                Vector3 boundPoint2 = bounds.max;
                if (IsVisibleDiagonal(boundPoint1, boundPoint2)) return true;

                Vector3 boundPoint3 = new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z);
                Vector3 boundPoint4 = new Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z);
                if (IsVisibleDiagonal(boundPoint3, boundPoint4)) return true;

                Vector3 boundPoint5 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z);
                Vector3 boundPoint6 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z);
                if (IsVisibleDiagonal(boundPoint5, boundPoint6)) return true;

                Vector3 boundPoint7 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z);
                Vector3 boundPoint8 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z);
                if (IsVisibleDiagonal(boundPoint3, boundPoint4)) return true;

                return false;
            }
        }
        return false;
    }
    public static bool IsVisibleAABB(TerrainElement element)
    {
        Bounds bounds = element.Bounds;

        bool visible = IsVisibleAABB(bounds);
       /* if (visible)
        {
            RaycastHit hitInfo;
            bool hit = Physics.Raycast(new Ray(camera.transform.position, bounds.center - camera.transform.position), out hitInfo, 1000);
            if (hit)
            {
                visible = hitInfo.collider.transform.parent.gameObject == group.b;
            }
        }*/
        return visible;
    }
    public static bool IsVisiblePoint(Collider owner, Vector3 point)
    {
        RaycastHit hitInfo;
        bool hit = Physics.Raycast(camera.transform.position, point - camera.transform.position, out hitInfo);
        if (hit &&(owner != hitInfo.collider || camera.transform.InverseTransformPoint(hitInfo.point).z < camera.transform.InverseTransformPoint(point).z))
        {
            return false;
        }
        else
        {
            return true;
        }
    }
    public static bool IsVisibleCollider(Collider collider)
    {
        return IsVisibleAABB(collider.bounds);
        /*
        if (!instance.cullCollider) return true;

        if (IsVisiblePoint(collider, collider.bounds.center)) return true;

        Vector3 boundPoint1 = collider.bounds.min;
        if (IsVisiblePoint(collider, boundPoint1)) return true;

        Vector3 boundPoint2 = collider.bounds.max;
        if (IsVisiblePoint(collider, boundPoint2)) return true;

        Vector3 boundPoint3 = new Vector3(boundPoint1.x, boundPoint1.y, boundPoint2.z);
        if (IsVisiblePoint(collider, boundPoint3)) return true;

        Vector3 boundPoint4 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint1.z);
        if (IsVisiblePoint(collider, boundPoint4)) return true;

        Vector3 boundPoint5 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint1.z);
        if (IsVisiblePoint(collider, boundPoint5)) return true;

        Vector3 boundPoint6 = new Vector3(boundPoint1.x, boundPoint2.y, boundPoint2.z);
        if (IsVisiblePoint(collider, boundPoint6)) return true;

        Vector3 boundPoint7 = new Vector3(boundPoint2.x, boundPoint1.y, boundPoint2.z);
        if (IsVisiblePoint(collider, boundPoint7)) return true;

        Vector3 boundPoint8 = new Vector3(boundPoint2.x, boundPoint2.y, boundPoint1.z);
        if (IsVisiblePoint(collider, boundPoint8)) return true;

        return false;*/
    }
    public static bool IsVisibleTerrainElement(TerrainElement element)
    {
        if (IsVisibleAABB(element))
        {
            /*if (!instance.cullTerrainElements)
            {
                return true;
            }
            if (element.colliders != null && element.colliders.Count > 0)
            {
                foreach (Collider collider in element.colliders)
                {
                    bool result = IsVisibleCollider(collider);
                    if(result)
                    {
                        return true;
                    }
                }
                return false;
            }
            else*/
            {
                return true;
            }
        }
        return false;
    }
}
