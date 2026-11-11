using UnityEngine;
using UnityEngine.UI;

public class PersistentObjectManager : MonoBehaviour
{
    public static PersistentObjectManager Instance = null;
    public static int NumGoldCoins = 0;
    public static bool HasGun = false;
    public Text CoinsText;
    public static Vector3 SpawnPointPositionScene0;
    public GameObject Player;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (CoinsText != null)
        {
            CoinsText.text = "Gold: " + NumGoldCoins;
        }
    }

    public void UpdateSpawnPointScene0(Vector3 position)
    {
        SpawnPointPositionScene0 = position;
    }
}