using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using Unity.VisualScripting;

public class CarAgent1 : Agent
{
    private Rigidbody rb;
    private CarController1 carController;
    public List<Transform> checkpoints=new();
    public int checkpointNum = 0;
    public float rewards=0;
    public Coroutine wallTouchPenalty;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        carController = GetComponent<CarController1>();
        checkpoints = GameObject.Find("Track1/Checkpoints").GetComponentsInChildren<Transform>().ToList();
        checkpoints.RemoveAt(0);
    }

    public override void OnEpisodeBegin()
    {
        carController.StopCar();

        // checkpointNum = Random.Range(0,checkpoints.Count);
        // transform.position = checkpoints[checkpointNum].position-checkpoints[checkpointNum].forward*3f;
        // transform.rotation = checkpoints[checkpointNum].rotation;
        checkpointNum = 0;
        transform.position = new Vector3(Random.Range(-7f,7f),0f,Random.Range(-5f,4f));
        transform.rotation = Quaternion.identity;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(rb.velocity);
        sensor.AddObservation(rb.angularVelocity);
        sensor.AddObservation(checkpoints[checkpointNum%checkpoints.Count].position - transform.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {
        float horizontalInput = actions.ContinuousActions[0];
        float verticalInput = actions.ContinuousActions[1];
        
        int space = actions.DiscreteActions[0];

        carController.horizontalInput = horizontalInput;
        carController.verticalInput = verticalInput;
        carController.isBraking = space == 1;

        SetReward(-(checkpoints[checkpointNum].position - transform.localPosition).magnitude/500000);
        rewards = GetCumulativeReward();
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");
        
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = Input.GetKey(KeyCode.Space)?1:0;
    }
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("goal") && checkpoints.Contains(other.transform)){
            if (checkpointNum == checkpoints.IndexOf(other.transform))
            {
                checkpointNum += 1;
                SetReward(4f);
            }
            if(checkpointNum>=checkpoints.Count){
                checkpointNum = 0;
            }
        }
    }
    private void OnCollisionEnter(Collision other) {
        if(other.transform.CompareTag("wall")){
            SetReward(-1f);
            wallTouchPenalty=StartCoroutine(WallTouchPenalty());
        }
    }
    private void OnCollisionExit(Collision other) {
        if(other.transform.CompareTag("wall")){
            StopCoroutine(wallTouchPenalty);
        }
    }
    private IEnumerator WallTouchPenalty(){
        while (true)
        {
            SetReward(-0.001f);
            yield return null;
        }
    }
}
