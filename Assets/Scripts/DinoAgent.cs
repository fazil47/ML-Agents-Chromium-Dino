using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;

public class DinoAgent : Agent
{
    [Tooltip("Whether this is training mode or gameplay mode")]
    public bool trainingMode;

    [Tooltip("The force with which the dinosaur jumps")]
    public float jumpForce = 400;

    [Tooltip("The origin of the raycast to find the distance to the nearest obstacle")]
    public GameObject rayOriginObject;

    [SerializeField]
    private GameManager gameManager;

    /// <summary>
    /// Whether the agent is frozen
    /// </summary>
    public bool frozen = false;

    // The endless road the agent is in
    private EndlessRoad endlessRoad;

    // The dinosaur's animation component
    private Animation anim;

    // The dinosaur's animation component
    private Rigidbody rigidBody;

    // The dinosaur's possible states
    private enum DinoState
    {
        run,
        jump,
        duckRun
    }

    // The dinosaur's current state
    private DinoState dinoState;

    // Distance from dinosaur to the spawn point
    private float maxDistance;

    // The dinosaur's initial position
    private Vector3 initialDinoPosition;

    /// <summary>
    /// Initialize the agent
    /// </summary>
    public override void Initialize()
    {
        anim = this.GetComponent<Animation>();
        rigidBody = this.GetComponent<Rigidbody>();
        dinoState = DinoState.run;
        endlessRoad = GetComponentInParent<EndlessRoad>();
        maxDistance = Vector3.Distance(endlessRoad.spawnPoint.transform.position, this.transform.position);

        // If not in training mode, no max step so play forever
        if (!trainingMode) MaxStep = 0;
    }

    /// <summary>
    /// Reset the agent when an episode begins
    /// </summary>
    public override void OnEpisodeBegin()
    {
        // Destroys all obstacles
        // Spawns a new obstacle
        // Resets the score and obstacleVelocity
        ResetAgent();
        endlessRoad.ResetEndlessRoad();
    }

    /// <summary>
    /// Called when an action is received from either the player input or the neural network
    /// 
    /// vectorAction[] has one element, that element represents:
    /// Do nothing, if element = 0
    /// Jump, if element = 1
    /// Duck, if element = 2
    /// </summary>
    /// <param name="vectorAction">The actions to be taken</param>
    public override void OnActionReceived(float[] vectorAction)
    {
        //Debug.Log("Action taken: " + vectorAction[0]);
        // Don't take actions if frozen
        if (frozen || vectorAction[0] == 0)
        {
            if (dinoState == DinoState.duckRun)
            {
                anim.Play("Armature|Run");
                dinoState = DinoState.run;
            }
            return;
        }
        else if (vectorAction[0] == 1 && (dinoState != DinoState.jump))
        {
            dinoState = DinoState.jump;
            rigidBody.AddForce(Vector3.up * jumpForce);
            anim.Play("Armature|Run");
            anim.Stop();
        }
        else if (vectorAction[0] == 2)
        {
            anim.Play("Armature|DuckRun");
            if (dinoState == DinoState.jump)
            {
                rigidBody.AddForce(Vector3.down * jumpForce / 10f);
                anim.Stop();
            }
            else if (dinoState == DinoState.run)
            {
                dinoState = DinoState.duckRun;
            }
        }
    }

    /// <summary>
    /// Collect vector observations from the environment
    /// </summary>
    /// <param name="sensor">The vector sensor</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        // Make observations about the distance from the nearest obstacle to the dinosaur and it's distance from the ground
        RaycastHit hit;

        Vector3 rayToObstacleOrigin = rayOriginObject.transform.position;
        Ray rayToNearestObstacle = new Ray(rayToObstacleOrigin, Vector3.right);
        Debug.DrawRay(rayToObstacleOrigin, rayToNearestObstacle.direction * maxDistance, Color.red);
        if (Physics.Raycast(rayToNearestObstacle, out hit, maxDistance))
        {
            Debug.DrawRay(rayToObstacleOrigin, rayToNearestObstacle.direction * hit.distance, Color.green);
            //Debug.Log("Distance from nearest obstacle: " + hit.distance);
            sensor.AddObservation(hit.distance);
        }
        else
        {
            Debug.DrawRay(rayToObstacleOrigin, rayToNearestObstacle.direction * maxDistance, Color.red);
            sensor.AddObservation(maxDistance);
        }

        Ray rayToGround = new Ray(transform.position, Vector3.down);
        Physics.Raycast(rayToGround, out hit, maxDistance);
        Debug.DrawRay(transform.position, rayToGround.direction * hit.distance, Color.blue);
        sensor.AddObservation(hit.distance);
        // 2 observations
    }

    /// <summary>
    /// When Behavior type is set to "Heuristic Only" on the agent's Behavior Parameters
    /// this function will be called. Its return value will be fed into 
    /// <see cref="OnActionReceived(float[])"/> instead of using the neural network
    /// </summary>
    /// <param name="actionsOut">An Output action array</param>
    public override void Heuristic(float[] actionsOut)
    {
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            actionsOut[0] = 2;
        }
        else if ((Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)))
        {
            actionsOut[0] = 1;
        }
        else
        {
            actionsOut[0] = 0;
        }
        //else if (dinoState == DinoState.run)
        //{
        //    anim.Play("Armature|Run");
        //}
        //else if ((Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S)) && (dinoState == DinoState.duckRun))
        //{
        //    dinoState = DinoState.run;
        //}
    }

    /// <summary>
    /// Prevent the agent from taking actions and the obstacles from moving
    /// </summary>
    public void FreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported during training");
        frozen = true;
        rigidBody.Sleep();
        anim.Stop();
        endlessRoad.ResetEndlessRoad();
    }

    /// <summary>
    /// Resume agent actions and obstacle movement
    /// </summary>
    public void UnfreezeAgent()
    {
        Debug.Assert(trainingMode == false, "Freeze/Unfreeze not supported during training");
        frozen = false;
        rigidBody.WakeUp();
        //endlessRoad.UnfreezeAllObstacles();
        OnEpisodeBegin();
    }

    private void Awake()
    {
        if (gameManager == null)
        {
            gameManager = Instantiate(new GameManager());
        }
    }

    // Resets the agent
    private void ResetAgent()
    {
        rigidBody.velocity = Vector3.zero;
        rigidBody.angularVelocity = Vector3.zero;
        transform.position = initialDinoPosition;
        dinoState = DinoState.run;
        anim.Play("Armature|Run");
    }

    // Start is called before the first frame update
    private void Start()
    {
        initialDinoPosition = transform.position;
        dinoState = DinoState.run;
        anim.Play("Armature|Run");
    }

    // Update is called once per frame
    //private void Update()
    //{
    //    //AddReward(endlessRoad.scoreAdded);
    //}



    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.gameObject.tag == "Ground")
        {
            dinoState = DinoState.run;
            anim.Play("Armature|Run");
        }
        //else if (collision.collider.gameObject.tag == "Ceiling")
        //{
        //    AddReward(-0.1f);
        //    ResetAgent();
        //}
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Obstacle" && trainingMode)
        {
            AddReward(-1f);
            ResetAgent();
            endlessRoad.ResetEndlessRoad();
            //SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            gameManager.Lost(this.name);
        }
    }
}
