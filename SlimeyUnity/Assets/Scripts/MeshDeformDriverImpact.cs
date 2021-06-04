using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDeformDriverImpact : MonoBehaviour
{
    public SlimeMeshDeformer meshDeformer;
    public float impactPowerModifier = 0.5f;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        foreach(ContactPoint contact in collision.contacts)
        {
            meshDeformer.AddDeformingForce(contact.point, collision.impulse.magnitude / Time.fixedDeltaTime * impactPowerModifier);
        }
    }
}
