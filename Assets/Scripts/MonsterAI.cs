using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform playerTransform;
    [ReadOnly] [SerializeField] private List<Transform> waypoints;

    [Header("AI")]
    [SerializeField] private float idleTime;
    [ReadOnly] [SerializeField] private float idleTimer;
    [SerializeField] private float stalkingLevelThreshold;
    [SerializeField] private AnimationCurve stalkingLevelRateByDistance;
    [SerializeField] private float stalkingMaxDistance;
    [SerializeField] private float stalkingSpeed;
    [ReadOnly] [SerializeField] private float stalkingLevel;
    [SerializeField] private float chasingTime;
    [SerializeField] private float chasingSpeed;
    [ReadOnly] [SerializeField] private float chasingTimer;

    private enum State
    {
        Idle,
        Stalking,
        Chasing,
    }

    private State _state;

    private void Start()
    {
        ChangeState(State.Idle);
    }

    private void Update()
    {
        switch (_state)
        {
            case State.Idle:
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0f)
                {
                    ChangeState(State.Stalking);
                }

                break;
            case State.Stalking:
                agent.SetDestination(playerTransform.position);
                stalkingLevel += GetStalkingLevelRate() * Time.deltaTime;
                if (stalkingLevel >= stalkingLevelThreshold)
                {
                    stalkingLevel = 0;
                    ChangeState(State.Chasing);
                }

                break;
            case State.Chasing:
                agent.SetDestination(playerTransform.position);
                chasingTimer -= Time.deltaTime;
                if (chasingTimer <= 0f)
                {
                    ChangeState(State.Idle);
                }

                break;
        }
    }

    private void ChangeState(State nextState)
    {
        switch (nextState)
        {
            case State.Idle:
                idleTimer = idleTime;
                meshRenderer.enabled = false;
                agent.ResetPath();
                break;
            case State.Stalking:
                meshRenderer.enabled = true;
                agent.ResetPath();
                agent.speed = stalkingSpeed;
                WarpToFarthestWaypoint();
                break;
            case State.Chasing:
                chasingTimer = chasingTime;
                meshRenderer.enabled = true;
                agent.ResetPath();
                agent.speed = chasingSpeed;
                WarpToFarthestWaypoint();
                break;
        }

        _state = nextState;
        Debug.Log($"Current state: {_state}");
    }

    public void SetWaypoints(Transform waypointsTransform)
    {
        waypoints = new List<Transform>();
        foreach (Transform child in waypointsTransform)
        {
            waypoints.Add(child);
        }

        transform.position = waypoints[0].position;
    }

    private void WarpToFarthestWaypoint()
    {
        var selected = 0;
        var maxDistance = Vector3.Distance(playerTransform.position, waypoints[0].position);
        for (var i = 1; i < waypoints.Count; i++)
        {
            var distance = Vector3.Distance(playerTransform.position, waypoints[i].position);
            if (distance > maxDistance)
            {
                maxDistance = distance;
                selected = i;
            }
        }

        agent.Warp(waypoints[selected].position);
    }

    private float GetStalkingLevelRate()
    {
        var distance = Vector3.Distance(playerTransform.position, transform.position);
        var distanceNormalized = Mathf.Clamp01(distance / stalkingMaxDistance);
        return stalkingLevelRateByDistance.Evaluate(distanceNormalized);
    }
}
