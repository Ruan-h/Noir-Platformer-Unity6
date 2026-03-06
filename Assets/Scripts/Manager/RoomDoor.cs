using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    [Header("Referências")]
    // Quem é o chefe desta porta? (Arraste o objeto Pai Room_XX aqui)
    public RoomManager myRoomManager;

    // Onde o player nasce se entrar por ESSA porta específica?
    public Transform mySpawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (myRoomManager != null)
            {
                // Avisa o gerente: "Player entrou, e se morrer, volta para o meu SpawnPoint"
                Vector2 pos = mySpawnPoint != null ? mySpawnPoint.position : transform.position;
                myRoomManager.OnPlayerEnterRoom(pos);
            }
            else
            {
                Debug.LogError("ERRO: Porta sem RoomManager associado!");
            }
        }
    }
}
