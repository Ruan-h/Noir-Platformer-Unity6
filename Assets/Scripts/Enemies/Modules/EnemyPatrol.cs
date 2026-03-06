using UnityEngine;
using System;

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float minDistance = 0.1f;

    private int currentWaypointIndex = 0;
    private float waitTimer;
    private bool isWaiting;


    public void PatrolUpdate(Rigidbody2D rb, bool isFacingRight, Action flipCallback)
    {
        if (waypoints.Length == 0) return;

        if (isWaiting)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            waitTimer -= Time.deltaTime;
            
            if (waitTimer <= 0)
            {
                isWaiting = false;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            return;
        }

        Transform targetPoint = waypoints[currentWaypointIndex];
        float distance = Vector2.Distance(transform.position, targetPoint.position);

        if (distance < minDistance)
        {
            isWaiting = true;
            waitTimer = waitTime;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            float direction = Mathf.Sign(targetPoint.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

            CheckFlip(targetPoint.position, isFacingRight, flipCallback);
        }
    }

    private void CheckFlip(Vector3 targetPos, bool isFacingRight, Action flipAction)
    {
        if (targetPos.x > transform.position.x && !isFacingRight)
        {
            flipAction.Invoke();
        }
        else if (targetPos.x < transform.position.x && isFacingRight)
        {
            flipAction.Invoke();
        }
    }
    
    private void OnDrawGizmos()
    {
        if (waypoints == null) return;
        Gizmos.color = Color.cyan;
        foreach (Transform t in waypoints)
        {
            if(t != null) Gizmos.DrawWireSphere(t.position, 0.3f);
        }
    }
}
