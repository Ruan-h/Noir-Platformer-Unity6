using UnityEngine;

public class RoomDoor : MonoBehaviour
{
    [Header("Referências")]
    public RoomManager myRoomManager;


    public Transform mySpawnPoint;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (myRoomManager != null)
            {
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
