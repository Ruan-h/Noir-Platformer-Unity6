using UnityEngine;
using System;

public class TargetedPatrol : MonoBehaviour
{
    [Header("Patrol Route")]
    [Tooltip("Pontos onde o inimigo vai andar.")]
    [SerializeField] private Transform[] waypoints;

    [Tooltip("Opcional: Para cada Waypoint, defina um alvo para onde olhar ENQUANTO ESPERA. Se deixar vazio, ele mantém a direção.")]
    [SerializeField] private Transform[] lookTargets; 

    [Header("Settings")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float waitTime = 2f;
    [SerializeField] private float minDistance = 0.1f;

    // Esta é a variável correta que devemos resetar
    private int currentIndex = 0; 
    private float waitTimer;
    private bool isWaiting;

    public void PatrolUpdate(Rigidbody2D rb, bool isFacingRight, Action flipAction)
    {
        if (waypoints.Length == 0) return;

        // Garante que os arrays tenham tamanhos compatíveis para evitar erro
        bool hasLookTarget = lookTargets != null && 
                             lookTargets.Length > currentIndex && 
                             lookTargets[currentIndex] != null;

        if (isWaiting)
        {
            rb.linearVelocity = Vector2.zero;
            waitTimer -= Time.deltaTime;

            // --- LÓGICA ESPECIAL: OLHAR PARA O ALVO DURANTE A ESPERA ---
            if (hasLookTarget)
            {
                Transform target = lookTargets[currentIndex];
                
                // 1. Onde o alvo está em relação a mim?
                bool targetIsToTheRight = target.position.x > transform.position.x;

                // 2. Verificação de Correção (Absolute Check)
                if (targetIsToTheRight && !isFacingRight)
                {
                    flipAction.Invoke();
                }
                else if (!targetIsToTheRight && isFacingRight)
                {
                    flipAction.Invoke();
                }
            }
            // -----------------------------------------------------------

            if (waitTimer <= 0)
            {
                isWaiting = false;
                currentIndex = (currentIndex + 1) % waypoints.Length;
            }
        }
        else
        {
            // Movimento Normal
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
    
    // --- A CORREÇÃO ESTÁ AQUI ---
    public void ResetPatrol()
    {
        // 1. Zera o índice usando o nome correto da variável
        currentIndex = 0; 

        // 2. Zera o timer
        waitTimer = 0f;

        // 3. Cancela o estado de espera para ele começar a andar imediatamente
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
