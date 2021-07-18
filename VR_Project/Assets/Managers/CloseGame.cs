/*
 * Kyle Dakin
 * 1659221
 * NHE2422 Advanced Game Development
 * Innovative UI Development of a VR/AR Game
 */

using UnityEngine;

namespace Managers
{
    public class CloseGame : MonoBehaviour
    {
        // Update is called once per frame
        void Update()
        {
            //Closes the game if the escape key is pressed
            if (Input.GetKey("escape"))
            {
                Application.Quit();
            }
        }
    }
}
