using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EndlessRoad : MonoBehaviour
{
    /// <summary>
    /// Minimum gap between obstacles
    /// </summary>
    public int minGap = 5;
    /// <summary>
    /// Maximum gap between obstables
    /// </summary>
    public int maxGap = 20;
    /// <summary>
    /// List of different obstacle prefabs
    /// </summary>
    public GameObject[] obstacles;
    /// <summary>
    /// Velocity of obstacles when the scene loads
    /// </summary>
    public float initialObstacleVelocity = 10;
    /// <summary>
    /// Accelaration of obstacles
    /// </summary>
    public float obstacleAcceleration = 0.001f;
    /// <summary>
    /// The dinosaur gameobject
    /// </summary>
    public DinoAgent dino;
    /// <summary>
    /// The spawn point for the obstacles
    /// </summary>
    public GameObject spawnPoint;
    /// <summary>
    /// The score displayed in the UI
    /// </summary>
    public TextMeshProUGUI scoreText;
    /// <summary>
    /// Current score
    /// </summary>
    public float score;
    /// <summary>
    /// Score added in current frame
    /// </summary>
    public float scoreAdded;
    /// <summary>
    /// Velocity of obstacles in real time
    /// </summary>
    public float obstacleVelocity;

    // The last spawned obstacle
    private GameObject lastSpawnedObstacle;
    // Distance from the spawnPoint to the last spawned obstacle
    private int lastSpawnedObstacleDistance;
    // Distance from the last spawned obstacle at which to spawn a new obstacle
    private int obstacleGap;

    /// <summary>
    /// Called when the endless road wakes up
    /// </summary>
    //private void Awake()
    //{

    //}

    /// <summary>
    /// Resets the Endless road
    /// </summary>
    public void ResetEndlessRoad()
    {
        scoreAdded = 0;
        score = 0;
        obstacleGap = maxGap;
        obstacleVelocity = initialObstacleVelocity;
        DestroyAllObstacles();
        SpawnObstacle();
    }

    ///// <summary>
    ///// Freezes all obstacles
    ///// </summary>
    //public void FreezeAllObstacles()
    //{
    //    for (int i = 0; i < spawnPoint.transform.childCount; i++)
    //    {
    //        spawnPoint.transform.GetChild(i).gameObject.GetComponent<Rigidbody>().Sleep();
    //    }
    //}

    ///// <summary>
    ///// Unfreezes all obstacles
    ///// </summary>
    //public void UnfreezeAllObstacles()
    //{
    //    for (int i = 0; i < spawnPoint.transform.childCount; i++)
    //    {
    //        spawnPoint.transform.GetChild(i).gameObject.GetComponent<Rigidbody>().WakeUp();
    //    }
    //}

    // Destroys all obstacles
    private void DestroyAllObstacles()
    {
        for (int i = 0; i < spawnPoint.transform.childCount; i++)
        {
            Destroy(spawnPoint.transform.GetChild(i).gameObject);
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!dino.frozen)
        {
            if (lastSpawnedObstacle == null)
            {
                SpawnObstacle();
            }
            obstacleVelocity += obstacleAcceleration * Time.deltaTime;
            if (dino.trainingMode)
            {
                scoreAdded += (1 + obstacleAcceleration) * Time.deltaTime;
                score += scoreAdded * Time.deltaTime;
                scoreText.text = "Score: " + UnityEngine.Mathf.FloorToInt(score);
            }
            lastSpawnedObstacleDistance = UnityEngine.Mathf.FloorToInt(Vector3.Distance(spawnPoint.transform.position, lastSpawnedObstacle.transform.position));
            if (lastSpawnedObstacleDistance == minGap)
            {
                obstacleGap = UnityEngine.Random.Range(minGap, maxGap);
            }
            else if (lastSpawnedObstacleDistance == obstacleGap || lastSpawnedObstacleDistance > maxGap)
            {
                // lastSpawnedObstacleDistance might go past values minGap and obstacleGap and the last spawn obstacle could get destroyed
                SpawnObstacle();
            }
        }
    }

    /// <summary>
    /// Spawns a new obstacle at <see cref="spawnPoint"/> and assigns <see cref="lastSpawnedObstacle"/>
    /// </summary>
    private void SpawnObstacle()
    {
        if (dino.frozen == false)
        {
            int sample = UnityEngine.Random.Range(0, obstacles.Length);
            GameObject obstacle = obstacles[sample];
            obstacle = Instantiate(obstacle, spawnPoint.transform.position, Quaternion.identity, spawnPoint.transform);
            Obstacle obstacleObject = obstacle.GetComponent<Obstacle>();
            obstacleObject.velocity = obstacleVelocity;
            //Vector3 spawnPointToPlayer = dino.transform.position - spawnPoint.transform.position;
            obstacleObject.dinoPosition = dino.transform.position;
            lastSpawnedObstacle = obstacle;
        }
        else
        {
            DestroyAllObstacles();
        }
    }
}
