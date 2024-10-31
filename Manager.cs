using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    [SerializeField] Boid boidPrefab;

    [SerializeField] int numberOfBoids;
    [SerializeField] float spawnsphere;


    private Boid[] boids;
    private void Start()
    {
        for (int i = 0; i < numberOfBoids; i++)
        {
           Boid boid =   Instantiate(boidPrefab);
            boid.transform.position = UnityEngine.Random.insideUnitSphere * spawnsphere;
            
        }


    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Vector3.zero, spawnsphere);
    }
   
}
