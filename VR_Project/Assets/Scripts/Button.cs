/*
 * Kyle Dakin
 * 1659221
 * NHE2422 Advanced Game Development
 * Innovative UI Development of a VR/AR Game
 */

using UnityEngine;

public class Button : MonoBehaviour
{
    public GameObject[] attachedObjects; //List of objects that this button can control
    
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && other.gameObject.GetComponent<PlayerMovement>().canUse)
        {
            foreach (GameObject attached in attachedObjects)
            {
                attached.SetActive(false);
            }
            Destroy(this.gameObject); //Destroys the button so it can't be pressed multiple times
        }
    }
}
