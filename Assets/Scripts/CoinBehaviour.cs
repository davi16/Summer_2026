using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CoinBehaviour : MonoBehaviour
{
    public GameObject Player;
    public GameObject AllCoins;
    public static int NumCoins = 0;
    public Text CoinsText;
    public float autoPickupRadius = 2.35f;

    private Transform playerTransform;
    private bool isCollected = false;

    void Start()
    {
        NumCoins = PersistentObjectManager.NumGoldCoins;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;

        if (CoinsText == null && PersistentObjectManager.Instance != null)
        {
            CoinsText = PersistentObjectManager.Instance.CoinsText;
        }

        if (CoinsText != null)
        {
            CoinsText.gameObject.SetActive(NumCoins > 0);
            if (NumCoins > 0) CoinsText.text = "Gold: " + NumCoins.ToString();
        }
    }

    void Update()
    {
        if (isCollected || !MenuAndGameManager.GameHasStarted || Time.timeScale == 0f) return;

        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTransform = playerObj.transform;
        }

        // מאפשר לשחקן לאסוף את השטרות והמטבעות שעל שולחנות הכספת ברגע שהוא מתקרב אליהם!
        if (playerTransform != null)
        {
            Vector2 playerXZ = new Vector2(playerTransform.position.x, playerTransform.position.z);
            Vector2 coinXZ = new Vector2(transform.position.x, transform.position.z);
            float horizontalDist = Vector2.Distance(playerXZ, coinXZ);
            float verticalDiff = Mathf.Abs(playerTransform.position.y - transform.position.y);

            if (horizontalDist <= autoPickupRadius && verticalDiff <= 2.5f)
            {
                CollectMoneyItem();
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isCollected && other.CompareTag("Player"))
        {
            CollectMoneyItem();
        }
    }

    private void CollectMoneyItem()
    {
        if (isCollected) return;
        isCollected = true;

        NumCoins++;
        PersistentObjectManager.NumGoldCoins = NumCoins;

        if (CoinsText == null && PersistentObjectManager.Instance != null)
        {
            CoinsText = PersistentObjectManager.Instance.CoinsText;
        }

        if (CoinsText != null)
        {
            CoinsText.gameObject.SetActive(true);
            CoinsText.text = "Gold: " + NumCoins.ToString();
        }

        MenuAndGameManager menu = FindAnyObjectByType<MenuAndGameManager>();
        if (menu != null) menu.UpdateHUD();

        if (AllCoins != null)
        {
            AudioSource sound = AllCoins.GetComponent<AudioSource>();
            if (sound != null) sound.Play();
        }

        gameObject.SetActive(false);
        CheckIfVaultIsCompletelyEmptied();
    }

    private void CheckIfVaultIsCompletelyEmptied()
    {
        if (!HeistAlarm.IsVaultScene(SceneManager.GetActiveScene().name)) return;

        CoinBehaviour[] remainingCoins = FindObjectsByType<CoinBehaviour>();
        int activeCount = 0;
        foreach (CoinBehaviour c in remainingCoins)
        {
            if (c != this && c.gameObject.activeInHierarchy) activeCount++;
        }

        if (activeCount == 0)
        {
            HeistAlarm.AllVaultCoinsLooted = true;
            if (MenuAndGameManager.SelectedRole == MenuAndGameManager.PlayerRole.Robber && !HeistAlarm.FirefightStarted)
            {
                if (StoryAndDialogueManager.Instance != null)
                {
                    StoryAndDialogueManager.Instance.ShowSingleDialogue(
                        "Max (Best Friend)",
                        "THE VAULT IS COMPLETELY EMPTY! We beat the clock! Run out of the Bank to the Getaway Vehicle before SWAT arrives!",
                        6f
                    );
                }
            }
        }
    }
}