using UnityEngine;

public class CoinSpin : MonoBehaviour
{
    public float spinSpeed = 100f; // speed of rotation in degrees per second

    void Update()
    {
        // rotates the coin around the Z-axis smoothly without being dependent on frame rate
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }
}