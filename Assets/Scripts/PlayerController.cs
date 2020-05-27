using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float minMaxParams; //Bounds of the stage

    float paddleX; //Paddle X scale, will be used for powerups

    Vector3 previousPostion; //Previous position of Paddle, updates at the end of every physics frame
    Transform graphic; //Parent graphic for paddle, used to squash paddle without altering the collider

    void Start()
    {
        paddleX = 1; //Sets the length of the Paddle
        previousPostion = transform.position; //Sets the starting position to measure speed
        graphic = transform.GetChild(0); //Sets the paddle sprite parent
    }

    void Update()
    {
        PaddlePosition(); //Updates the Paddle's position
    }

    private void FixedUpdate()
    {
        //Below is the code used to squash the paddle if it starts moving quickly, a lot of it is adjusted for visual clarity

        float paddleY = 1 - (GetSpeed()/2); //Gets a y value for the Paddle's y scale dependant on the Paddle's speed
        if (paddleY >= 0.5f) //If the value doesn't reach the threshold, leave the paddle thickness alone
            paddleY = 1;
        if (paddleY <= 0.2f) //If the value is too small, clamp to the lowest value I've determined
            paddleY = 0.2f;
        Vector2 targetScale = new Vector2(paddleX, paddleY); //Creates a Vector2 to Lerp to
        graphic.localScale = Vector2.Lerp(graphic.localScale, targetScale, 50 * Time.deltaTime); //Lerps the localScale to that Vector2
        previousPostion = transform.position; //Updates previous position to be used to calculate speed
    }

    void PaddlePosition()
    {
        Vector3 mousePos = Input.mousePosition; //Gets mouse position
        mousePos.z = 10; //Adds z as mousePosition defaults to camera position
        Vector3 input = Camera.main.ScreenToWorldPoint(mousePos); //Generates a useable Vector3 in the world space
        input.x = Mathf.Clamp(input.x, -minMaxParams, minMaxParams); //Prevents the Paddle from leaving the bounds of the game
        transform.position = new Vector3(input.x, transform.position.y); //Sets the paddle position
    }

    float GetSpeed() //Checks how quickly the paddle is moving by comparing its current position with its last physics frame position
    {
        float speed = Vector3.Distance(transform.position, previousPostion);
        return speed;
    }

}
