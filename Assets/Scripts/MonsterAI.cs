using UnityEngine;
using UnityEngine.AI;

public class MonsterAI : MonoBehaviour
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Transform playerTransform;

    private void Update()
    {
        agent.SetDestination(playerTransform.position);
    }
}
