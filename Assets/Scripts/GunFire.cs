using System.Collections;
using UnityEngine;

public class GunFire : MonoBehaviour
{
    public GameObject bullet; // Prefab of the bullet to be fired
    public GameObject playerCamera; // Prefab of the muzzle flash effect
    LineRenderer tracer; // LineRenderer component for the tracer effect
    public GameObject muzzle;
    AudioSource gunFire;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        tracer = GetComponent<LineRenderer>();
        gunFire = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Check if the left mouse button is pressed
        {
            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit))
            {
                bullet.transform.position = hit.point;
                gunFire.Play();
                StartCoroutine(ShowFlash(hit.point));
            }
        }
    }
    IEnumerator ShowFlash(Vector3 hitPoint)
    {
        tracer.SetPosition(0, muzzle.transform.position);
        tracer.SetPosition(1, hitPoint);
        tracer.enabled = true;
        yield return new WaitForSeconds(0.05f); // Show the tracer for a short duration
        tracer.enabled = false;
    }
}
