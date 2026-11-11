using UnityEngine;

public class GetawayVehicleTrigger : MonoBehaviour
{
    public float escapeTriggerDistance = 4.5f;
    private Transform playerTransform;

    void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) playerTransform = playerObj.transform;
    }

    void Update()
    {
        if (!MenuAndGameManager.GameHasStarted || HealthSystem.MatchEnded || playerTransform == null) return;

        // רלוונטי לשודד שחזר לרחוב אחרי ששדד את הכספת
        if (MenuAndGameManager.SelectedRole == MenuAndGameManager.PlayerRole.Robber &&
            HeistAlarm.IsHeistActive && PersistentObjectManager.NumGoldCoins > 0)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            if (dist <= escapeTriggerDistance)
            {
                MenuAndGameManager menu = FindAnyObjectByType<MenuAndGameManager>();
                if (menu != null)
                {
                    menu.TriggerEndOfMatch(true, "HEIST COMPLETED! You reached the Getaway Vehicle with " + PersistentObjectManager.NumGoldCoins + " Gold!");
                }
            }
        }
    }
}