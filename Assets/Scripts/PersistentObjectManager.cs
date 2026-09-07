using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

// is based on Singleton pattern
public class PersistentObjectManager : MonoBehaviour
{
    public static PersistentObjectManager Instance = null;
    public static int NumGoldCoins = 0;
    public Text CoinsText;
    public static Vector3 SpawnPointPositionScene0 ;
    public GameObject Player;
    private void Awake()
    {
        if(Instance == null) // for the first time
        {
            Instance = this;
        }
        else // not for the first time
        {
            Destroy(gameObject);
            // place the player at spawn point position
            if(SceneManager.GetActiveScene().buildIndex==0)// initial scene
            {
                Player.transform.position = SpawnPointPositionScene0;
            }
        }

        CoinsText.text = "Gold: " + NumGoldCoins;

        DontDestroyOnLoad(gameObject);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateSpawnPointScene0(Vector3 position)
    {
        SpawnPointPositionScene0 = position;
    }
}
