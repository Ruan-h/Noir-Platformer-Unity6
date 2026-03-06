using UnityEngine;
using Unity.Cinemachine;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    [Header("Configurações da Sala")]
    public Collider2D cameraBounds; 
    
    [Tooltip("Marque isso APENAS na sala onde o player começa o jogo.")]
    public bool isStartingRoom = false; 
    [Tooltip("Se for a sala inicial, onde o player deve renascer se morrer nela?")]
    public Transform initialSpawnPoint; 

    private List<Enemy> enemiesInRoom = new List<Enemy>();
    private CinemachineConfiner2D confiner;

    // --- MUDANÇA: AWAKE PARA INICIALIZAÇÃO ---
    private void Awake()
    {
        // 1. Garante referências pesadas IMEDIATAMENTE
        var vCam = FindFirstObjectByType<CinemachineCamera>();
        if (vCam != null) confiner = vCam.GetComponent<CinemachineConfiner2D>();

        // 2. Indexa os inimigos antes de qualquer lógica de jogo
        Enemy[] foundEnemies = GetComponentsInChildren<Enemy>(true);
        enemiesInRoom.AddRange(foundEnemies);
    }

    private void Start()
    {
        // --- LÓGICA DE SALA INICIAL COM TRAVA ---
        if (isStartingRoom)
        {
            // O BLOQUEIO: Se o GameManager já carregou um Save (Banco),
            // esta sala "cala a boca" e não puxa a câmera.
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

            // Aqui é seguro chamar, pois o 'confiner' foi preenchido no Awake
            OnPlayerEnterRoom(startPos);
        }
    }

    public void OnPlayerEnterRoom(Vector2 spawnPosition)
    {
        // Atualiza a Câmera (Seguro: confiner já existe desde o Awake)
        if (confiner != null && cameraBounds != null)
        {
            if (confiner.BoundingShape2D != cameraBounds)
            {
                confiner.BoundingShape2D = cameraBounds;
            }
        }

        // Avisa o GameManager
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
        // Debug.Log($"Inimigos da sala {gameObject.name} resetados.");
    }
}
