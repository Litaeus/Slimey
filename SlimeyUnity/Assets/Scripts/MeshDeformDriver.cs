using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SlimeMeshDeformer))]
public class MeshDeformDriver : MonoBehaviour
{
    public Vector3 cachedLastPosition;

    private float distToGround;
    public bool isOnGround;

    public float velocityDeformStrength = 50f;

    private SlimeMeshDeformer slimeMeshDeformer;
    
    // Start is called before the first frame update
    void Start()
    {
        slimeMeshDeformer = GetComponent<SlimeMeshDeformer>();
        cachedLastPosition = transform.position;

        distToGround = GetComponent<SkinnedMeshRenderer>().sharedMesh.bounds.extents.y;
    }

    public void UpdateVelocityDeform()
    {
        slimeMeshDeformer.AddDeformingForce(transform.position + ((cachedLastPosition - transform.position).normalized * distToGround * transform.lossyScale.magnitude /4f), velocityDeformStrength * Time.deltaTime * (Vector3.Distance(cachedLastPosition,transform.position)));

        cachedLastPosition = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        UpdateVelocityDeform();

        
    }
}
