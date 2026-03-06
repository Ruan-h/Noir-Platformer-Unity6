using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Alvo")]
    public Transform target; // O Transform do jogador que a câmera deve seguir

    [Header("Configurações")]
    public Vector3 offset = new Vector3(0f, 0f, -10f); // A distância da câmera para o jogador (Z=-10 é crucial)
    [Range(0.01f, 1.0f)]
    public float smoothSpeed = 0.125f; // A suavidade do movimento. Valores menores deixam a câmera mais rápida.

    // Variável interna para a função SmoothDamp
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        // Se não houver um alvo definido, não faz nada
        if (target == null)
        {
            Debug.LogWarning("A câmera não tem um alvo para seguir!");
            return;
        }

        // Posição que a câmera deseja alcançar (posição do jogador + offset)
        Vector3 desiredPosition = target.position + offset;

        // Interpola suavemente a posição atual da câmera para a posição desejada
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);
    }
}
