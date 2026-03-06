using UnityEngine;

public class Enemy : MonoBehaviour
{
    [HideInInspector] public bool isFacingRight = true;

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

    public void Revive()
    {
        transform.position = startPosition;
        isFacingRight = startFacingRight;
        
        Vector3 scaler = transform.localScale;
        scaler.x = isFacingRight ? Mathf.Abs(scaler.x) : -Mathf.Abs(scaler.x);
        transform.localScale = scaler;

        ResetState();

        gameObject.SetActive(true);
    }

    protected virtual void ResetState() { }
}
