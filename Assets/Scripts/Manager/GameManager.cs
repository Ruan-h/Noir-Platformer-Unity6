using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    [Header("Sistema de Vínculos")]
    public int maxVinculos = 3;
    public int currentVinculos;

    [Header("Dados de Respawn")]
    public Vector2 lastBenchPos;
    public Vector2 lastRoomEntryPos;
    public bool hasBenchSaved = false;

    private RoomManager currentRoom; 
    private Controller playerController;

    private void Awake()
    {
        if (instance == null) { instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    private void Start()
    {
        currentVinculos = maxVinculos;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    public void RegisterRoomEntry(Vector2 pos, RoomManager roomScript)
    {
        if (currentVinculos > 0)
        {
            lastRoomEntryPos = pos;
            currentRoom = roomScript; 
        }
    }

    public void RestAtBench(Vector2 pos)
    {
        lastBenchPos = pos;
        lastRoomEntryPos = pos;
        hasBenchSaved = true;
        currentVinculos = maxVinculos;
    }

    public void HandlePlayerDeath()
    {
        if (playerController == null) playerController = FindAnyObjectByType<Controller>();
			
		  playerController.ResetCharges();
        if (currentVinculos > 0)
        {
            currentVinculos--;
            
            playerController.transform.position = lastRoomEntryPos;
            
            playerController.Revive();

            if (currentRoom != null)
            {
                currentRoom.ResetEnemies();
            }
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            currentVinculos = maxVinculos;
        }
    }

    public void LoadNextZone(string sceneName)
    {
        currentVinculos = maxVinculos;
        hasBenchSaved = false; 
        SceneManager.LoadScene(sceneName);
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {

        Controller p = FindAnyObjectByType<Controller>();
        var vCam = FindFirstObjectByType<Unity.Cinemachine.CinemachineCamera>();
        
        if (vCam != null && p != null)
        {
            vCam.Follow = p.transform;
        }


        if (p != null && hasBenchSaved) 
        {
            p.transform.position = lastBenchPos;


            RoomManager[] allRooms = FindObjectsByType<RoomManager>(FindObjectsSortMode.None);
            
            foreach (RoomManager room in allRooms)
            {
    
                if (room.cameraBounds != null && room.cameraBounds.OverlapPoint(lastBenchPos))
                {
                    room.OnPlayerEnterRoom(lastBenchPos);
                    currentRoom = room;
                    
                    Debug.Log($"Hard Respawn: Player restaurado na sala {room.name}");
                    break;
                }
            }
        }
    }
}
