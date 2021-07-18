using UnityEngine;
using UnityEngine.UI;

public class SetDirection : MonoBehaviour
{
    //Controls the angle rotation of the Aruco marker on screen
    //This is needed to control the direction the player faces
    
    public int angle;
    public Image image;

    public void OnPress()
    {
        image.transform.rotation = Quaternion.Euler(0,0,angle);
    }
}
