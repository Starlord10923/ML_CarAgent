using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveToTargetAgent : Agent
{
    [SerializeField] Transform targetTransform;
    [SerializeField] float moveSpeed = 5.0f;
    [SerializeField] Material winMaterial;
    [SerializeField] Material loseMaterial;
    [SerializeField] MeshRenderer floorMeshRenderer;
    

    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(0, 0.5f, -2f);
        // targetTransform.localPosition = new Vector3(Random.Range(-3f,3f),0.5f,Random.Range(-3f,3f));
        Debug.Log("Episode Begin: Agent Reset");
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Collect agent's position and target's position
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // Manual control for testing
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Horizontal"); // Move X
        continuousActions[1] = Input.GetAxis("Vertical");   // Move Z
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Move the agent based on actions received
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        
        if (other.transform.CompareTag("wall"))
        {
            SetReward(-1f);
            floorMeshRenderer.material = loseMaterial;
        }
        if (other.CompareTag("goal"))
        {
            SetReward(1f);
            floorMeshRenderer.material = winMaterial;
            EndEpisode();
        }
    }
    private void OnCollisionEnter(Collision other) {
        
        if (other.transform.CompareTag("wall"))
        {
            SetReward(-1f);
            floorMeshRenderer.material = loseMaterial;
        }
    }
}
