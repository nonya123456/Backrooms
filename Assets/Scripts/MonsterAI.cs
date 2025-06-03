using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ToggleRenderer toggleRenderer;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform playerCameraTarget;
    [SerializeField] private Transform eyePosition;
    [ReadOnly] [SerializeField] private List<Transform> waypoints;

    [Header("AI")]
    [SerializeField] private float idleTime;
    [ReadOnly] [SerializeField] private float idleTimer;
    [SerializeField] private float stalkingLevelThreshold;
    [SerializeField] private AnimationCurve stalkingLevelRateByDistance;
    [SerializeField] private float stalkingMaxDistance;
    [SerializeField] private float stalkingSpeed;
    [SerializeField] private float bonusStalkingLevelInPlayerView;
    [ReadOnly] [SerializeField] private float stalkingLevel;
    [SerializeField] private LayerMask obstacleLayerMask;
    [SerializeField] private float playerViewDistance;
    [SerializeField] private float playerViewAngle;
    [SerializeField] private float chasingTime;
    [SerializeField] private float chasingSpeed;
    [ReadOnly] [SerializeField] private float chasingTimer;
    private bool _skipStateUpdate;

    private enum State
    {
        Idle,
        Stalking,
        Chasing,
    }

    private State _state;

    private void Awake()
    {
        agent.updateRotation = false;
    }

    private void Start()
    {
        ChangeState(State.Idle);
    }

    private void Update()
    {
        UpdateRotation();

        if (_skipStateUpdate)
        {
            return;
        }

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
                if (IsInPlayerView())
                {
                    StartCoroutine(HandleInPlayerView());
                    return;
                }

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

    private void UpdateRotation()
    {
        var directionToPlayer = playerTransform.position - transform.position;
        directionToPlayer.y = 0;

        if (directionToPlayer != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(directionToPlayer);
        }
    }

    private void ChangeState(State nextState)
    {
        _skipStateUpdate = false;

        switch (nextState)
        {
            case State.Idle:
                idleTimer = idleTime;
                toggleRenderer.DisableRenderers();
                agent.ResetPath();
                break;
            case State.Stalking:
                toggleRenderer.EnableRenderers();
                agent.ResetPath();
                agent.speed = stalkingSpeed;
                WarpToFarthestWaypoint();
                break;
            case State.Chasing:
                toggleRenderer.EnableRenderers();
                chasingTimer = chasingTime;
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

    private bool IsInPlayerView()
    {
        var direction = (eyePosition.position - playerCameraTarget.position).normalized;
        var angle = Vector3.Angle(playerCameraTarget.forward, direction);
        if (angle > playerViewAngle)
        {
            return false;
        }

        var realDistance = Vector3.Distance(playerCameraTarget.position, eyePosition.position) - float.Epsilon;
        if (realDistance > playerViewDistance)
        {
            return false;
        }

        return !Physics.Raycast(playerCameraTarget.position, direction, realDistance, obstacleLayerMask);
    }

    private IEnumerator HandleInPlayerView()
    {
        _skipStateUpdate = true;
        stalkingLevel += bonusStalkingLevelInPlayerView;
        yield return new WaitForSeconds(0.5f);
        ChangeState(State.Idle);
    }
}
