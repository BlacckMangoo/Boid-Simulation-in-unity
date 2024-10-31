using UnityEngine;
using System.Collections.Generic;

public class Boid : MonoBehaviour
{
    private static List<Boid> allBoids = new List<Boid>();

    [SerializeField] private float speed = 5f;
    [SerializeField] private float randomness = 1f;
    [SerializeField] private float separationRadius = 5f;
    [SerializeField] private float separationWeight = 10f;
    [SerializeField] private float cohesionWeight = 1f;
    [SerializeField] private float cohesionRadius = 5f;
    [SerializeField] private float alignmentWeight = 1f;

    private Rigidbody cachedRigidbody;
    private static readonly Vector3[] randomDirectionCache = new Vector3[1000];
    private static int randomDirectionIndex = 0;

    // Spatial partitioning optimization
    private static float gridSize = 10f; // Adjust based on your scene
    private static Dictionary<Vector3Int, List<Boid>> spatialGrid = new Dictionary<Vector3Int, List<Boid>>();

    void Awake()
    {
        cachedRigidbody = GetComponent<Rigidbody>();
        allBoids.Add(this);
        UpdateSpatialGridPosition();
    }

    void OnDestroy()
    {
        allBoids.Remove(this);
        RemoveFromSpatialGrid();
    }

    void Start()
    {
        // Pre-cache random directions to reduce allocation
        if (randomDirectionIndex == 0)
        {
            for (int i = 0; i < randomDirectionCache.Length; i++)
            {
                randomDirectionCache[i] = Random.insideUnitSphere.normalized;
            }
        }

        Vector3 randomDirection = GetNextCachedRandomDirection();
        cachedRigidbody.velocity = randomDirection * speed;
    }

    void Update()
    {
        transform.forward = cachedRigidbody.velocity.normalized;
        CalculateBoidBehaviors();
        UpdateSpatialGridPosition();
    }

    private void UpdateSpatialGridPosition()
    {
        RemoveFromSpatialGrid();
        Vector3Int gridPosition = Vector3Int.FloorToInt(new Vector3(
            Mathf.Floor(transform.position.x / gridSize),
            Mathf.Floor(transform.position.y / gridSize),
            Mathf.Floor(transform.position.z / gridSize)
        ));

        if (!spatialGrid.ContainsKey(gridPosition))
        {
            spatialGrid[gridPosition] = new List<Boid>();
        }
        spatialGrid[gridPosition].Add(this);
    }

    private void RemoveFromSpatialGrid()
    {
        Vector3Int previousGridPosition = Vector3Int.FloorToInt(new Vector3(
            Mathf.Floor(transform.position.x / gridSize),
            Mathf.Floor(transform.position.y / gridSize),
            Mathf.Floor(transform.position.z / gridSize)
        ));

        if (spatialGrid.ContainsKey(previousGridPosition))
        {
            spatialGrid[previousGridPosition].Remove(this);
        }
    }

    private Vector3 GetNextCachedRandomDirection()
    {
        randomDirectionIndex = (randomDirectionIndex + 1) % randomDirectionCache.Length;
        return randomDirectionCache[randomDirectionIndex];
    }

    private void CalculateBoidBehaviors()
    {
        Vector3 separationForce = Vector3.zero;
        Vector3 sumOfVelocity = Vector3.zero;
        Vector3 sumOfPosition = Vector3.zero;
        Vector3 alignmentForce = Vector3.zero;
        int noOfNeighbours = 0;

        // Use spatial grid to reduce neighbor checks
        Vector3Int currentGridPosition = Vector3Int.FloorToInt(transform.position / gridSize);

        // Check neighboring grid cells
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3Int checkPosition = currentGridPosition + new Vector3Int(x, y, z);
                    if (!spatialGrid.TryGetValue(checkPosition, out List<Boid> neighbourBoids))
                        continue;

                    foreach (Boid boid in neighbourBoids)
                    {
                        if (boid == this) continue;

                        float distance = Vector3.Distance(transform.position, boid.transform.position);
                        if (distance < separationRadius)
                        {
                            Vector3 directionAway = transform.position - boid.transform.position;
                            float forceMultiplier = Mathf.Clamp(1 / (distance * distance), 0, 10);
                            separationForce += directionAway.normalized * forceMultiplier;
                        }

                        if (distance < cohesionRadius)
                        {
                            noOfNeighbours++;
                            sumOfVelocity += boid.cachedRigidbody.velocity;
                            sumOfPosition += boid.transform.position;
                            alignmentForce += boid.cachedRigidbody.velocity;
                        }
                    }
                }
            }
        }

        if (noOfNeighbours == 0) return;

        // Cohesion
        Vector3 averagePosition = sumOfPosition / noOfNeighbours;
        Vector3 cohesionForceDirection = averagePosition - transform.position;
        cachedRigidbody.velocity += cohesionForceDirection * cohesionWeight * Time.deltaTime;

        // Separation
        cachedRigidbody.velocity += separationForce * separationWeight * Time.deltaTime;

        // Alignment
        Vector3 averageVelocity = alignmentForce / noOfNeighbours;
        Vector3 alignmentForceDirection = averageVelocity.normalized * speed - cachedRigidbody.velocity;
        cachedRigidbody.velocity += alignmentForceDirection * alignmentWeight * Time.deltaTime;

        // Randomness
        Vector3 randomForce = GetNextCachedRandomDirection() * randomness;
        cachedRigidbody.AddForce(randomForce);

        // Clamp velocity
        cachedRigidbody.velocity = Vector3.ClampMagnitude(cachedRigidbody.velocity, speed);
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, separationRadius);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, cohesionRadius);
    }
}