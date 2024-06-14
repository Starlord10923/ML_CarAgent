using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;

public class CarAgent : Agent
{
    private Rigidbody rb;
    private CarController carController;
    public List<Transform> checkpoints=new();
    public int checkpointNum = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carController = GetComponent<CarController>();
        checkpoints = GameObject.Find("Track1/Checkpoints").GetComponentsInChildren<Transform>().ToList();
        checkpoints.RemoveAt(0);
    }

    public override void OnEpisodeBegin()
    {
        rb.velocity = Vector3.zero;
        transform.position = new Vector3(Random.Range(-7f,7f),0f,Random.Range(-7f,0f));
        transform.rotation = Quaternion.identity;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // foreach (Transform raycastOrigin in raycastOrigins){
        //     if (Physics.Raycast(raycastOrigin.position, raycastOrigin.forward, out RaycastHit hit, raycastDistance, raycastMask))
        //         sensor.AddObservation(hit.distance);
        //     else
        //         sensor.AddObservation(raycastDistance);
        // }

        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.velocity);
        sensor.AddObservation(rb.angularVelocity);
        sensor.AddObservation(checkpoints[checkpointNum].position - transform.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float horizontalInput = actions.ContinuousActions[0];
        float verticalInput = actions.ContinuousActions[1];

        // int space = actions.DiscreteActions[0];

        carController.horizontalInput = horizontalInput;
        carController.verticalInput = verticalInput;
        // carController.space = space == 1;

        SetReward(-(checkpoints[checkpointNum].position - transform.localPosition).magnitude/500000);
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
        
        // ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        // discreteActions[0] = Input.GetKey(KeyCode.Space)?1:0;
    }
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("goal") && checkpoints.Contains(other.transform)){
            if (checkpointNum < checkpoints.IndexOf(other.transform))
            {
                checkpointNum += 1;
                SetReward(4f);
            }
            if(checkpointNum>=checkpoints.Count){
                checkpointNum = 0;
            }
        }
        // Debug.Log("Triggered : " + other.tag);
    }
    private void OnCollisionEnter(Collision other) {
        if(other.transform.CompareTag("wall")){
            SetReward(-1f);
        }
        // Debug.Log("Collided : " + other.transform.tag);
    }
}
