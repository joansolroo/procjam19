using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Person : Particle
{
    public float speed = 1;
    public List<Vector3> path;
    public float pathIndex;
    private CharacterController controller;
    private Vector3 smoothDirection;
    public float smoothAimming = 0.1f;
    public float positionDispertion = 0.8f;
    private Animator animator;
    Vector3 direction;

   public  new Renderer renderer;

    public void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        renderer = GetComponentInChildren<SkinnedMeshRenderer>();
    }
    public void ResetPerson(float size)
    {
        //animator.playbackTime = Random.Range(0f, 0.3f);
        transform.localScale = new Vector3(size, size, size);
        speed = 0.7f * size / 0.002f;
        if (Random.Range(0f, 1f) > 0.5f)
            path.Reverse();
        Vector3 pathOffset = new Vector3(Random.Range(-positionDispertion, positionDispertion), 0, Random.Range(-positionDispertion, positionDispertion));
        for (int i = 0; i < path.Count; i++)
            path[i] += pathOffset;
        
        pathIndex = Random.Range(0f, path.Count);
        transform.position = Vector3.Lerp(path[(int)pathIndex], path[(int)(pathIndex+1) % path.Count], pathIndex - (int)pathIndex);
        smoothDirection = (path[(int)(pathIndex + 1) % path.Count] - path[(int)pathIndex]).normalized;
    }



    protected override void DoCreate() { }
    protected override void DoTick() {
        direction = path[(int)(pathIndex + 1) % path.Count] - path[(int)pathIndex];
        pathIndex = (int)pathIndex + Mathf.Clamp(Vector3.Dot((transform.position - path[(int)pathIndex]) / direction.magnitude, direction.normalized), 0f, 1f);
        if (pathIndex >= path.Count)
            pathIndex -= path.Count;
        pathIndex = Mathf.Clamp(pathIndex, 0f, path.Count);
        Vector3 gravity = controller.isGrounded ? Vector3.zero : -Vector3.up;
        controller.Move(direction.normalized * speed * Time.deltaTime + gravity);

        bool visible = OcclusionCulling.IsVisibleAABB(controller.bounds) && OcclusionCulling.IsVisibleCollider(controller);
        renderer.enabled = visible;
        animator.enabled = visible;
    }
    protected override void DoDestroy() { }

    protected override void UpdateVisuals()
    {
        if (renderer.enabled)
        {
            smoothDirection = (1f - smoothAimming) * smoothDirection + smoothAimming * direction.normalized;
            transform.LookAt(transform.position + smoothDirection);
        }
    }
}
