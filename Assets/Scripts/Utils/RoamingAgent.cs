using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Utils
{
    public class RoamingAgent: Singleton<RoamingAgent>
    {
        private Vector3 RandomNavSphere(Vector3 origin, float dist, int layermask)
        {
            Vector3 randDirection = Random.insideUnitSphere * dist;
            randDirection += origin;
            NavMeshHit navHit;
            NavMesh.SamplePosition(randDirection, out navHit, dist, layermask);
            return navHit.position;
        }
        private IEnumerator RoamCoroutine(NavMeshAgent agent, float radius, float secondsToSwitchTgt)
        {
            while (true)
            {
                Vector3 newPos = RandomNavSphere(agent.transform.position, radius, -1);
                agent.SetDestination(newPos);
                yield return new WaitForSeconds(secondsToSwitchTgt);
            }
        }
        
        public Coroutine Roam(NavMeshAgent agent, float radius, float secondsToSwitchTgt)
        {
            return StartCoroutine(RoamCoroutine(agent, radius, secondsToSwitchTgt));
        }

    }
}