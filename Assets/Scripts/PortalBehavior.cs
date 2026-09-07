using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalBehavior : MonoBehaviour
{
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
        // update gold coins in Persistent Object
        PersistentObjectManager.NumGoldCoins = CoinBehaviour.NumCoins;

        // save Spawn point
        if (SceneManager.GetActiveScene().buildIndex == 0)
        {
            Vector3 position = transform.position;
            position.x += 2;
            position.z += 2;
            PersistentObjectManager.Instance.UpdateSpawnPointScene0(position);
        }


            // go to another scene
            if (SceneManager.GetActiveScene().buildIndex == 0)
            SceneManager.LoadScene(1);
        else SceneManager.LoadScene(0);

    }
}
