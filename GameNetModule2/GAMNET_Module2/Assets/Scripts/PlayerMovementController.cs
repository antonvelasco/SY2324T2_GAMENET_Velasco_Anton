using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
public class PlayerMovementController : MonoBehaviour
{
    public Joystick joystick;

    private RigidbodyFirstPersonController rigidbodyFirstPersonController;


    // Start is called before the first frame update
    void Start()
    {
        rigidbodyFirstPersonController = this.GetComponent<RigidbodyFirstPersonController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        rigidbodyFirstPersonController.joystickImputAxis.x = joystick.Horizontal;
        rigidbodyFirstPersonController.joystickImputAxis.y = joystick.Vertical;
    }
}
