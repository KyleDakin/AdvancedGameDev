/*
 * Kyle Dakin
 * 1659221
 * NHE2422 Advanced Game Development
 * Innovative UI Development of a VR/AR Game
 */

using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public Text coinText;
    public float movespeed;
    public bool canUse; //public so the Button class can access as the player does not have a trigger

    private int _cointCount;
    private bool _canMove; //bool used to control when the player can move by being true when the screen is Green
    private float _yRot; //rotation of the player, value is changed by the direction of the Aruco mark

    private void Start()
    {
        coinText.text = "0";
    }

    private void Update()
    {
        ChangeDirection();
        if (_canMove)
        {
            transform.Translate(Vector3.forward * (movespeed * Time.deltaTime));
        }
    }

    // While on the elevator the player becomes a child of the elevator, this helps reduce jitteryness of the elevator
    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.CompareTag("Elevator"))
        {
            transform.parent = other.transform;
        }
        else
        {
            transform.parent = null;
        }
    }

    // If player collides with a coin, destroy the object and update CoinCount
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Coin"))
        {
            Destroy(other.gameObject);
            UpdateCoinCount();
        }
    }

    void UpdateCoinCount()
    { 
        _cointCount++;
        coinText.text = _cointCount.ToString();
    }

    void ChangeDirection()
    {
        var eulerAngles = transform.eulerAngles;
        eulerAngles = new Vector3(eulerAngles.x, _yRot, eulerAngles.z);
        gameObject.transform.eulerAngles = eulerAngles;
    }
    
    //Parses the input from the Python programmes and sets the correct action accordingly
    public void ParseInput(string action, string direction)
    {
        switch (action)
        {
            case "Move":
                _canMove = true;
                break;
            case "Use":
                canUse = true;
                break;
            case "Stop":
                canUse = false;
                _canMove = false;
                break;
        }
    
        switch (direction)
        {
            case "Forward":
                _yRot = 0;
                break;
            case "Right":
                _yRot = 90;
                break;
            case "Back":
                _yRot = 180;
                break;
            case "Left":
                _yRot = -90;
                break;
            case "_":
                break;
        }
    }
}
