using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleDestroyer : MonoBehaviour
{
    [Tooltip("The dinosaur agent")]
    public DinoAgent dinoAgent;
    [Tooltip("The Endless Road GameObject")]
    public EndlessRoad endlessRoad;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Obstacle")
        {
            dinoAgent.AddReward(1f);
            Destroy(other.gameObject);
        }
    }
}
