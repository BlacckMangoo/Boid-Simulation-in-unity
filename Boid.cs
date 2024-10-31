using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Boid : MonoBehaviour
{
    Camera camera;
    Rigidbody rb;
  public float speed;
 public float randomness = 1f;
    public float separationRadius = 5f;
    public float separationWeight = 10f;
    public float cohesionWeight = 1f;
    public float cohesionRadius = 5f; // Using this for both cohesion and alignment
    public   float alignmentWeight = 1f; // Alignment weight
   
    
    void Start()
    {
        camera = FindAnyObjectByType();
        rb = GetComponent();
        Vector3 randomDirection = UnityEngine.Random.insideUnitSphere;
        rb.velocity = randomDirection * speed;
    }
    private void Update()
    {
  
        
        transform.forward = rb.velocity.normalized;
        CalculateBoidBehaviors();
    }
    private void CalculateBoidBehaviors()
    {
        Boid[] boids = FindObjectsOfType();
        Vector3 separationForce = Vector3.zero;
        Vector3 sumOfVelocity = Vector3.zero; // For cohesion and alignment
        Vector3 sumOfPosition = Vector3.zero; // For cohesion
        Vector3 alignmentForce = Vector3.zero; // For alignment
        int noOfNeighbours = 0;
        foreach (Boid boid in boids)
        {
            if (boid != this)
            {
                float distance = Vector3.Distance(transform.position, boid.transform.position);
                if (distance < separationRadius)
                {
                    Vector3 directionAway = transform.position - boid.transform.position;
                    float forceMultiplier = Mathf.Clamp(1 / distance*distance, 0, 10);
                    separationForce += directionAway.normalized * forceMultiplier;
                }
                if (distance < cohesionRadius)
                {
                    noOfNeighbours++;
                    sumOfVelocity += boid.rb.velocity;
                    sumOfPosition += boid.transform.position; // For cohesion
                    // Add to alignment force
                    alignmentForce += boid.rb.velocity; // Sum velocities for alignment
                }
            }
        }
        if (noOfNeighbours == 0)
        {
            return;
        }
        // Calculate cohesion force direction
        Vector3 averagePosition = sumOfPosition / noOfNeighbours;
        Vector3 cohesionForceDirection = averagePosition - transform.position;
        rb.velocity += cohesionForceDirection * cohesionWeight * Time.deltaTime;
        // Apply separation force
        rb.velocity += separationForce * separationWeight * Time.deltaTime;
        // Calculate average velocity for alignment
        Vector3 averageVelocity = alignmentForce / noOfNeighbours;
        Vector3 alignmentForceDirection = averageVelocity.normalized * speed - rb.velocity; // Steering force for alignment
        rb.velocity += alignmentForceDirection * alignmentWeight * Time.deltaTime;
        // Add random perturbation
        Vector3 randomForce = new Vector3(
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f),
            UnityEngine.Random.Range(-1f, 1f)
        ).normalized * randomness;
        rb.AddForce(randomForce); // Add randomness
        // Clamp velocity to max speed
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, speed);
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, cohesionRadius);
        // If you want to visualize alignment, you can do so here too
    }
}


optmise it to the max so i can run a lot particles , without changing any calculation methods or results
