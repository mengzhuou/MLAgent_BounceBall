using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

public class MoveAgent : Agent
{

    [SerializeField] private Transform target;

    private bool hitWall = false;
    private Vector2 lastBounceWallPosition;


    public override void OnEpisodeBegin()
    {
        transform.position = new Vector2 (0, -4.65f);
        //target position based on agent and the agle between them to bounce
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((Vector2)transform.localPosition);
        sensor.AddObservation((Vector2)target.localPosition);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {

        float moveX = actions.ContinuousActions[0];

        float movementSpeed = 5f;

        // If the agent has hit a BounceWall, ignore movement command to where wall is located.
        if (hitWall && ((lastBounceWallPosition.x > transform.position.x && moveX > 0) || (lastBounceWallPosition.x < transform.position.x && moveX < 0))) 
        {
            return;
        }
        transform.localPosition += new Vector3(moveX, 0) * Time.deltaTime * movementSpeed;
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.TryGetComponent(out Target target))
        {
            AddReward(10f);
            EndEpisode(); 
        }
        else if(collision.TryGetComponent(out Wall wall)){
            AddReward(-2f);
            EndEpisode();
        }else if(collision.TryGetComponent(out BounceWall bounceWall))
        {
            //bounce
            hitWall = true;
            lastBounceWallPosition = bounceWall.transform.position;
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");

        // If the agent is moving away from the BounceWall, allow it to move freely again
        if ((lastBounceWallPosition.x > transform.position.x && continuousActions[0] < 0) || (lastBounceWallPosition.x < transform.position.x && continuousActions[0] > 0))
        {
            hitWall = false;
        }
    }
}
