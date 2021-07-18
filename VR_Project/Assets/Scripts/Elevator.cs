/*
 * Kyle Dakin
 * 1659221
 * NHE2422 Advanced Game Development
 * Innovative UI Development of a VR/AR Game
 */

using UnityEngine;

public class Elevator : MonoBehaviour
{
    public float minY, maxY; //min and max Y height of the elevator to set where it moves between
    public int liftSpeed;
    public bool up; //marks whether the elevator starts by going up or down and sets _direction accordingly
    private int _direction = 1;
    private bool _canMove;

    void Start()
    {
        _direction = up ? 1 : -1;
    }
    
    // Update is called once per frame
    void FixedUpdate()
    {
        if (_canMove)
        {
            transform.Translate(0, _direction * Time.deltaTime * liftSpeed, 0);
            if (transform.position.y >= maxY || transform.position.y <= minY)
            {
                _direction *= -1;
            }
        }
    }

    //Makes the elevator able to move when the player is touching it, so there's no cycles for the player to wait for
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _canMove = true;
        }
    }

    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            _canMove = false;
        }
    }
}
