using UnityEngine;

public class Enemy : MonoBehaviour
{
    [HideInInspector] public bool isFacingRight = true;

    // Cache da posição inicial
    protected Vector3 startPosition;
    protected bool startFacingRight;

    protected virtual void Awake()
    {
        startPosition = transform.position;
        startFacingRight = isFacingRight;
    }

    public virtual bool IsFacingRight => isFacingRight;
    public virtual bool IsAlert => false; 

    public virtual void Die()
    {
        gameObject.SetActive(false);
    }

    // Chamado pelo RoomManager
    public void Revive()
    {
        // 1. Reset Físico (Posição/Rotação)
        transform.position = startPosition;
        isFacingRight = startFacingRight;
        
        Vector3 scaler = transform.localScale;
        scaler.x = isFacingRight ? Mathf.Abs(scaler.x) : -Mathf.Abs(scaler.x);
        transform.localScale = scaler;

        // 2. Reset Lógico (Cérebro)
        ResetState();

        // 3. Reativação
        gameObject.SetActive(true);
    }

    // Filhos sobrescrevem isso
    protected virtual void ResetState() { }
}
