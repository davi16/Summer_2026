using UnityEngine;
using UnityEngine.UI;

public class CoinBehaviour : MonoBehaviour
{
    public GameObject Player;
    public GameObject AllCoins;
    public static int NumCoins = 0;
    public Text CoinsText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        // only player can collect coins
        if (other.gameObject == Player.gameObject)
        {
            NumCoins++;  // counts collected coins
            CoinsText.text = "Gold: "+NumCoins.ToString();

            gameObject.SetActive(false);
            AudioSource sound = AllCoins.GetComponent<AudioSource>();
            sound.Play();
     
        }
    }
}
