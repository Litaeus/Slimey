using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SkinnedMeshRenderer))]
public class SlimeMeshDeformer : MonoBehaviour
{
    public float cullMovementThreshold = 0.1f;

    Mesh deformingMesh;
    Vector3[] originalVertices, displacedVertices, vertexVelocities;

    void Start()
    {
        deformingMesh = GetComponent<SkinnedMeshRenderer>().sharedMesh;
        originalVertices = deformingMesh.vertices;
        displacedVertices = new Vector3[originalVertices.Length];
        vertexVelocities = new Vector3[originalVertices.Length];

        for (int i = 0; i < originalVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i];
        }
    }

    public void AddDeformingForce(Vector3 point, float force)
    {
        if (shouldUpdate)
        {
            point = transform.InverseTransformPoint(point);

            for (int i = 0; i < displacedVertices.Length; i++)
            {
                AddForceToVertex(i, point, force);
            }
        }
    }

    void AddForceToVertex(int i, Vector3 point, float force)
    {
        Vector3 pointToVertex = displacedVertices[i] - point;
        float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
        float velocity = attenuatedForce * Time.deltaTime;
        vertexVelocities[i] += pointToVertex.normalized * velocity;
    }

    private bool shouldUpdate = true;

    private void OnBecameVisible()
    {
        shouldUpdate = true;
    }

    private void OnBecameInvisible()
    {
        shouldUpdate = false;
    }

    void FixedUpdate()
    {
        if (shouldUpdate)
        {
            for (int i = 0; i < displacedVertices.Length; i++)
            {
                UpdateVertex(i);
            }

            deformingMesh.vertices = displacedVertices;
        }
        //deformingMesh.RecalculateNormals();
    }

    public float springForce = 20f;

    public float damping = 5f;

    void UpdateVertex(int i)
    {
        Vector3 velocity = vertexVelocities[i];
        Vector3 displacement = displacedVertices[i] - originalVertices[i];
        velocity -= displacement * springForce * Time.deltaTime;
        velocity *= 1f - damping * Time.deltaTime;

        vertexVelocities[i] = velocity;
        displacedVertices[i] += velocity * Time.deltaTime;
    }

    private void OnDisable()
    {
        deformingMesh.vertices = originalVertices;
    }
}
