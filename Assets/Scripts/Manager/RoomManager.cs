using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    [Header("Configurações da Sala")]
    public Collider2D cameraBounds; 
    
    public bool isStartingRoom = false; 
    public Transform initialSpawnPoint; 

    private List<Enemy> enemiesInRoom = new List<Enemy>();
    private CinemachineConfiner2D confiner;

    private void Awake()
    {
        var vCam = FindFirstObjectByType<CinemachineCamera>();
        if (vCam != null) confiner = vCam.GetComponent<CinemachineConfiner2D>();

        Enemy[] foundEnemies = GetComponentsInChildren<Enemy>(true);
        enemiesInRoom.AddRange(foundEnemies);
    }

    private void Start()
    {

        if (isStartingRoom)
        {

            if (GameManager.instance != null && GameManager.instance.hasBenchSaved)
            {
                return; 
            }

            Vector2 startPos = Vector2.zero;

            if (initialSpawnPoint != null)
            {
                startPos = initialSpawnPoint.position;
            }
            else
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) startPos = player.transform.position;
            }

            OnPlayerEnterRoom(startPos);
        }
    }

    public void OnPlayerEnterRoom(Vector2 spawnPosition)
    {
        if (confiner != null && cameraBounds != null)
        {
            if (confiner.BoundingShape2D != cameraBounds)
            {
                confiner.BoundingShape2D = cameraBounds;
            }
        }

        if (GameManager.instance != null)
        {
            GameManager.instance.RegisterRoomEntry(spawnPosition, this);
        }
    }

    public void ResetEnemies()
    {
        foreach (Enemy enemy in enemiesInRoom)
        {
            if (enemy != null) enemy.Revive();
        }
    }
}
