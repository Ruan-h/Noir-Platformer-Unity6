using UnityEngine;
using System; // Necessário para usar Action

public class EnemyPatrol : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float minDistance = 0.1f;

    // Estado interno da patrulha
    private int currentWaypointIndex = 0;
    private float waitTimer;
    private bool isWaiting;

    // Função pública chamada pelo "Cérebro" (Anda.cs)
    // Recebe o Rigidbody para mover e uma função (Action) para flipar
    public void PatrolUpdate(Rigidbody2D rb, bool isFacingRight, Action flipCallback)
    {
        if (waypoints.Length == 0) return;

        // Lógica de Espera
        if (isWaiting)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y); // Garante que parou
            waitTimer -= Time.deltaTime;
            
            if (waitTimer <= 0)
            {
                isWaiting = false;
                currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
            return;
        }

        // Lógica de Movimento
        Transform targetPoint = waypoints[currentWaypointIndex];
        float distance = Vector2.Distance(transform.position, targetPoint.position);

        if (distance < minDistance)
        {
            // Chegou no ponto
            isWaiting = true;
            waitTimer = waitTime;
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        else
        {
            // Move em direção ao ponto
            float direction = Mathf.Sign(targetPoint.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

            // Verifica se precisa virar
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
    
    // Útil para debug visual
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
