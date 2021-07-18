using UnityEngine;
using UnityEngine.UI;

public class ChangeColour : MonoBehaviour
{
    //Sets the colour of the panel to be the selected colour.
    //This is needed for HSV colour picking to control the players actions
    
    public Image image;
    public Color colour;
    
    public void OnPress()
    {
        image.color = colour;
    }

    public void OnRelease()
    {
        image.color = Color.white;
    }
}
