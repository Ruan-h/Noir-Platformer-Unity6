using UnityEngine;

public class Checkpoint : MonoBehaviour
{
    public enum CheckpointType
    {
        Bench,         
        ZoneTransition
    }

    [Header("Configuração")]
    public CheckpointType type = CheckpointType.Bench;
    
    [Header("Apenas para ZoneTransition")]
    public string nextSceneName; 

    [Header("Visual")]
    public bool changeColorOnActivate = false;
    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            switch (type)
            {
                case CheckpointType.Bench:
                    
                    GameManager.instance.RestAtBench(transform.position);
                    ActivateVisuals();
                    break;

                case CheckpointType.ZoneTransition:
                    Debug.Log("Mudando de Zona...");
                    GameManager.instance.LoadNextZone(nextSceneName);
                    break;
            }
        }
    }

    private void ActivateVisuals()
    {
        if (changeColorOnActivate && !isActivated)
        {
            isActivated = true;
            SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.green; 
            }
        }
    }
}
