using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Utils;

public class EnemyFollowPlayer : MonoBehaviour
{
    [SerializeField] private Transform[] targets;
    [SerializeField] private float secondsToUpdateTarget = 5f;
    private NavMeshAgent _agent;
    private Transform _currentTarget;

    // Start is called before the first frame update
    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        _agent.updateRotation = false;
        _agent.updateUpAxis = false;
        
        StartCoroutine(UpdatePath());
    }

    private IEnumerator UpdatePath()
    {
        while (true)
        {
            FindClosestTarget();
            yield return new WaitForSeconds(secondsToUpdateTarget);
        }
    }

    // Update is called once per frame
    void Update()
    {
        _agent.SetDestination(_currentTarget.position);
    }
    
    private void FindClosestTarget()
    {
        Transform closestTarget = null;
        float closestDistance = Mathf.Infinity;
        foreach (Transform target in targets)
        {
            float distance = Vector3.Distance(transform.position, target.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = target;
            }
        }
        _currentTarget = closestTarget;
    }
    
    public CharacterFacingDirection GetFacingDirection()
    {
        Vector2 direction = _currentTarget.position - transform.position;
        return direction.GetFacingDirection();
    }
}
