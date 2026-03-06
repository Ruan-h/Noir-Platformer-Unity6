using UnityEngine;
using System;

public class TargetedPatrol : MonoBehaviour
{
    [Header("Patrol Route")]
    [SerializeField] private Transform[] waypoints;

    [SerializeField] private Transform[] lookTargets; 

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float minDistance = 0.1f;

    private int currentIndex = 0; 
    private float waitTimer;
    private bool isWaiting;

    public void PatrolUpdate(Rigidbody2D rb, bool isFacingRight, Action flipAction)
    {
        if (waypoints.Length == 0) return;

        bool hasLookTarget = lookTargets != null && 
                             lookTargets.Length > currentIndex && 
                             lookTargets[currentIndex] != null;

        if (isWaiting)
        {
            rb.linearVelocity = Vector2.zero;
            waitTimer -= Time.deltaTime;

            if (hasLookTarget)
            {
                Transform target = lookTargets[currentIndex];
                
                bool targetIsToTheRight = target.position.x > transform.position.x;

                if (targetIsToTheRight && !isFacingRight)
                {
                    flipAction.Invoke();
                }
                else if (!targetIsToTheRight && isFacingRight)
                {
                    flipAction.Invoke();
                }
            }

            if (waitTimer <= 0)
            {
                isWaiting = false;
                currentIndex = (currentIndex + 1) % waypoints.Length;
            }
        }
        else
        {
            Transform destination = waypoints[currentIndex];
            float distance = Vector2.Distance(transform.position, destination.position);

            if (distance < minDistance)
            {
                isWaiting = true;
                waitTimer = waitTime;
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                float direction = Mathf.Sign(destination.position.x - transform.position.x);
                rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

                if (destination.position.x > transform.position.x && !isFacingRight) flipAction.Invoke();
                else if (destination.position.x < transform.position.x && isFacingRight) flipAction.Invoke();
            }
        }
    }
    
    public void ResetPatrol()
    {
        currentIndex = 0; 
        waitTimer = 0f;
        isWaiting = false;
    }
    
    public Transform GetCurrentLookTarget()
    {
        if (isWaiting && lookTargets != null && currentIndex < lookTargets.Length)
        {
            return lookTargets[currentIndex];
        }
        return null; 
    }
    
    private void OnDrawGizmos()
    {
        if (waypoints == null) return;
        
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(waypoints[i].position, 0.3f);

            if (lookTargets != null && lookTargets.Length > i && lookTargets[i] != null)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(waypoints[i].position, lookTargets[i].position);
                Gizmos.DrawWireSphere(lookTargets[i].position, 0.2f);
            }
        }
    }
}
