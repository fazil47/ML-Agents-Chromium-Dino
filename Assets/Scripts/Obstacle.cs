using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    /// <summary>
    /// The speed with which the obstacle will be moving towards the dinosaur
    /// </summary>
    public float velocity = 10f;
    /// <summary>
    /// The vector from the spawn point to the dinosaur
    /// </summary>
    public Vector3 dinoPosition;

    // Update is called once per frame
    //void Update()
    //{
    //    this.transform.position = this.transform.position + (moveDirection * velocity * Time.deltaTime);
    //}

    private void FixedUpdate()
    {
        this.GetComponent<Rigidbody>().MovePosition(this.transform.position + (dinoPosition * velocity * Time.deltaTime));
    }
}
