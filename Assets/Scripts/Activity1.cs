using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;

public class Activity1 : Agent
{
    [SerializeField] Transform buttonTransform;
    [SerializeField] Transform rewardTransform;
    [SerializeField] Material winMaterial;
    [SerializeField] Material loseMaterial;
    [SerializeField] MeshRenderer floorMeshRenderer;
    [SerializeField] bool isButtonPressed;
    [SerializeField] float moveSpeed = 5.0f;
    private bool isbuttonInRange=false;

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-3f,3f),0.5f,Random.Range(-3f,3f));
        buttonTransform.localPosition = new Vector3(Random.Range(-3f,3f),0f,Random.Range(-3f,3f));
        rewardTransform.localPosition = Vector3.zero;
        rewardTransform.GetComponent<MeshRenderer>().enabled = false;
        rewardTransform.GetComponent<Collider>().enabled = false;
        isbuttonInRange = false;
        ResetButton();

        Debug.Log("Episode Begin: Agent Reset");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(buttonTransform.localPosition);
        sensor.AddObservation(rewardTransform.localPosition);
        
        sensor.AddObservation(isButtonPressed);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        int pressButton = actions.DiscreteActions[0];

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;

        if (pressButton==1 && isbuttonInRange && !isButtonPressed)
            PressButton();

        SetReward(-0.01f);
    }
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;

        continuousActions[0] = Input.GetAxis("Horizontal");
        continuousActions[1] = Input.GetAxis("Vertical");

        discreteActions[0] = Input.GetKey(KeyCode.Z) ? 1:0;
    }

    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("button")){
            isbuttonInRange = true;
        }
        if(other.CompareTag("goal")){
            SetReward(2f);
            EndEpisode();
        }
        if(other.CompareTag("wall")){
            SetReward(-0.2f);
        }
    }
    private void OnTriggerExit(Collider other) {
        if(other.CompareTag("button")){
            isbuttonInRange = false;
        }
    }
    private void ResetButton(){
        isButtonPressed = false;
        buttonTransform.GetComponent<Renderer>().material.color = Color.red;
    }
    private void PressButton(){
        SetReward(1f);
        isButtonPressed = true;
        buttonTransform.GetComponent<Renderer>().material.color = Color.green;
        buttonTransform.localPosition = new Vector3(buttonTransform.localPosition.x, -0.2f, buttonTransform.localPosition.z);
        StartCoroutine(GenerateReward());
    }
    private IEnumerator GenerateReward(){
        rewardTransform.localPosition = new Vector3(Random.Range(-3f,3f),0.5f,Random.Range(-3f,3f));
        yield return new WaitForSeconds(0.05f);
        rewardTransform.GetComponent<MeshRenderer>().enabled = true;
        rewardTransform.GetComponent<Collider>().enabled = true;
    }
}
