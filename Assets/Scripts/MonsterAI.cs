using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private List<Transform> waypoints;

    [Header("AI")]
    [SerializeField] private float idleTime;
    [ReadOnly] [SerializeField] private float idleTimer;
    [SerializeField] private float stalkingLevelThreshold;
    [SerializeField] private float stalkingLevelRate;
    [SerializeField] private float stalkingTime;
    [ReadOnly] [SerializeField] private float stalkingLevel;
    [ReadOnly] [SerializeField] private float stalkingTimer;
    [SerializeField] private float chasingTime;
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
        idleTimer = idleTime;
        _state = State.Idle;
        Debug.Log("Change to idle state");
    }

    private void Update()
    {
        switch (_state)
        {
            case State.Idle:
                meshRenderer.enabled = false;
                idleTimer -= Time.deltaTime;
                if (idleTimer <= 0f)
                {
                    stalkingTimer = stalkingTime;
                    _state = State.Stalking;
                    Debug.Log("Change to stalking state");
                }

                break;
            case State.Stalking:
                meshRenderer.enabled = true;
                stalkingTimer -= Time.deltaTime;
                stalkingLevel += stalkingLevelRate * Time.deltaTime;
                if (stalkingLevel >= stalkingLevelThreshold)
                {
                    stalkingLevel = 0;
                    chasingTimer = chasingTime;
                    _state = State.Chasing;
                    Debug.Log("Change to chasing state");
                }
                else if (stalkingTimer <= 0f)
                {
                    idleTimer = idleTime;
                    _state = State.Idle;
                    Debug.Log("Change to idle state");
                }

                break;
            case State.Chasing:
                meshRenderer.enabled = true;
                agent.SetDestination(playerTransform.position);
                chasingTimer -= Time.deltaTime;
                if (chasingTimer <= 0f)
                {
                    agent.SetDestination(transform.position);
                    idleTimer = idleTime;
                    _state = State.Idle;
                    Debug.Log("Change to idle state");
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
