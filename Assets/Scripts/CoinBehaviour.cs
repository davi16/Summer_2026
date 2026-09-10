using UnityEngine;
using UnityEngine.UI;

public class CoinBehaviour : MonoBehaviour
{
    public GameObject Player;
    public GameObject AllCoins;
    public static int NumCoins = 0;
    public Text CoinsText;
    
    void Start()
    {
        NumCoins = 0;
        // if the CoinsText is not null, update its text to show the current number of coins collected
        if (CoinsText != null)
        {
            CoinsText.gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // check if the player collides with the coin
        if (other.CompareTag("Player"))
        {
            NumCoins++;  // increment the coin count

            // activate the coin text and update the number
            if (CoinsText != null)
            {
                CoinsText.gameObject.SetActive(true);
                CoinsText.text = "Gold: " + NumCoins.ToString();
            }

            // play the coin collection sound
            if (AllCoins != null)
            {
                AudioSource sound = AllCoins.GetComponent<AudioSource>();
                if (sound != null) { sound.Play(); }
            }
     
            // deactivate the coin object
            gameObject.SetActive(false);
        }
    }
}