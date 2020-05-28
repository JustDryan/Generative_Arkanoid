using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public float ballSpeed; //How fast the ball is currently moving
    public float maxBallSpeed; //The maximum speed the ball can go
    public float ballSpeedIncrease; //How much to increase the speed by
    float rotZ; //Ball's rotation
    float coolDownTime; //Tracks the ball's cooldown

    public AudioClip clip; //Holds collision clip

    float prevX; //Used to track the horizontal direction of the ball, updates every frame
    GameObject prevCollision; //Stores the last thing the ball collided with, used to prevent the ball from getting stuck on objects

    Vector3 spawnPos; //Stores the position the ball starts in/will return to
    bool ballPaused;

    bool active;

    Rigidbody2D rb; //The rigidbody2D of the ball

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); //Find the Rigidbody2D on the object
        spawnPos = transform.position; //Sets the point the ball will return to when respawned
        RespawnBall();
    }

    void Update()
    {
        if (ballPaused) //If the ball is not allowed to move, prevents the rest Update from running, subsequently preventing movement
            return;
        rb.velocity = transform.up * ballSpeed; //The ball constantly moves on its local up axis
        prevX = transform.position.x; //Used to determine the direction the ball is moving
    }

    public void RespawnBall() //Places the ball back at the spawn point, makes it face downwards and resets collisions
    {
        if (!active)
            active = true;
        else
            GameController.gameController.AddBallUses(1);
        transform.position = spawnPos;
        SetBallRotation(180);
        prevCollision = null;
        ballPaused = true;
        StartCoroutine(PauseBall());
    }

    public int GetDirection() //Checks the direction that the ball is moving, 1 for right, -1 for left, defaults to 1
    {
        if (transform.position.x > prevX)
            return 1;
        else if (transform.position.x < prevX)
            return -1;

        return 1;
    }

    public void SetBallRotation(float rotation) //Sets the rotation of the ball
    {
        transform.eulerAngles = new Vector3(0, 0, rotation);
    }

    public void ReflectBall(Vector3 normal, float angleIncrease, bool isPaddleHit) //Reflects the ball when it hits a surface, takes the normal from the object hit, and adds/subtracts degrees depending on where it lands on the paddle
    {
        StartBallCooldown(); //To prevent the ball from getting stuck
        if (ballSpeed < maxBallSpeed) //Checks how fast the ball is going
            ballSpeed += ballSpeedIncrease; //If the ball is going slower than the max, increment speed
        float f = 90 + (angleIncrease * GetDirection()); //The angle returned by Vector3.Reflect is 90 degrees off. Here we account for that and alter the angle depending on the direction the ball is moving, and where the ball landed on the paddle
        Vector3 reflectedPosition = Vector3.Reflect(transform.up, normal); //Gets the reflectecd position of thw ball
        Vector3 dir = (reflectedPosition).normalized; //Gets thenew direction the ball will move in

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; //Converts this direction to degrees
        transform.eulerAngles = new Vector3(0, 0, angle - f); //Sets the ball angle with the 90 degrees accounted for and angleIncrease added on

        if (isPaddleHit) //Checks if it is a paddle hit
        {
            //The lines below simply prevent the ball's angle from going through the paddle after being adjusted. If the angle of the ball would send it horizontal or underneath the paddle, the ball rotation is adjusted.

            if(transform.eulerAngles.z <= -80 && transform.eulerAngles.z >= -180)
            {
                SetBallRotation(-80);
            }
            else if(transform.eulerAngles.z >= -280 && transform.eulerAngles.z <= -180)
            {
                SetBallRotation(-280);
            }
            else if (transform.eulerAngles.z >= 80 && transform.eulerAngles.z <= 180)
            {
                SetBallRotation(80);
            }
            else if (transform.eulerAngles.z <= 280 && transform.eulerAngles.z >= 180)
            {
                SetBallRotation(280);
            }
        }
    }

    public void StartBallCooldown() //The ball has a small coooldown on interacting with an object to prevent it getting stuck on the same object
    {
        coolDownTime = Time.time + 0.05f;
    }

    bool BallOnCooldown() //Checks if the bool is currently on cooldown
    {
        if (Time.time >= coolDownTime)
            return false;

        return true;
    }

    void PlayHitSound() //Plays a clip 
    {
        AudioSource aud = new GameObject("BallHitClip", typeof(AudioSource)).GetComponent<AudioSource>(); //Creates an empty gameobject and attaches an AudioSource
        aud.volume = 0.5f; //Sets the volume of the new AudioSource
        aud.clip = clip; //Sets the clip to use
        aud.pitch = Mathf.Pow(1.05946f, Mathf.Floor(transform.position.y + 3)); //Changes the octave of the clip depending on how high the ball is

        if (aud.clip != null) //If the AudioSource has been properly assigned a clip
        {
            aud.Play(); //Play the clip
            Destroy(aud.gameObject, aud.clip.length); //Destroy the object after the clip has played
        }
        else //If it has not been assigned a clip, destroy it
            Destroy(aud.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (BallOnCooldown()) //Checks if the ball is on cooldown
            return;
        if (collision.gameObject == prevCollision) //Checks if this collision is the same as the last to prevent the ball getting stuck
            return;
        if (ballPaused) //Prevents angles from changing if the ball is paused
            return;

        Vector3 normal = collision.contacts[0].normal; //Sets the normal of the contact point to a variable to use later

        //Almost all collisions play a sound

        if (collision.gameObject.CompareTag("Paddle") && transform.position.y > collision.transform.position.y) //Checks for the Paddle, implements the angle adjustments to ReflectBall
        {
            ReflectBall(normal, (transform.position.x - collision.transform.position.x) * 20, true);
            PlayHitSound();
            prevCollision = collision.gameObject;
        }

        if (collision.gameObject.CompareTag("Block")) //Checks for a block, if it is a block, remove 1 health from it
        {
            ReflectBall(normal, 0, false);
            collision.gameObject.GetComponent<BlockComponent>().RemoveHealth();
            PlayHitSound();
            prevCollision = collision.gameObject;
        }

        if (collision.gameObject.CompareTag("Wall")) //Check if the ball has hit the bounds of the game
        {
            ReflectBall(normal, 0, false);
            PlayHitSound();
            prevCollision = collision.gameObject;
        }

        if (collision.gameObject.CompareTag("BlockWall")) //Check if the ball has hit a block that has been turned into a wall
        {
            ReflectBall(normal, 0, false);
            PlayHitSound();
            prevCollision = collision.gameObject;
        }

        if (collision.gameObject.CompareTag("Ball")) //For power-ups, checks if the ball hits another ball
        {
            ReflectBall(normal, 0, false);
            PlayHitSound();
            prevCollision = collision.gameObject;
        }

        if (collision.gameObject.CompareTag("Bounds")) //Checks if the ball hits the lower bounds of the game and resets the ball
            RespawnBall();
    }

    IEnumerator PauseBall() //The ball is paused whilst this runs
    {
        ballPaused = true; //Sets the ball to paused
        rb.velocity = Vector3.zero; //Halts the rigidbody2D
        rb.angularVelocity = 0; //Stops the rigidbody2D from spinning
        float timer = 0; //Used to count wait time
        float increment = 0.01f; //Used to match wait time to actual seconds
        float radius = 0.25f; //Radius the ball will spin in
        float angle = 0; //Tracks the angle the ball is at around the circle
        while(timer < 1.5f) //1 and a half second wait timer
        {
            transform.position = new Vector2(spawnPos.x + Mathf.Cos(angle) * radius, spawnPos.y + Mathf.Sin(angle) * radius); //Moves the ball around a circle
            yield return new WaitForSeconds(increment); //Pauses for increment seconds
            timer += increment; //Increases timer 
            angle += increment * 10; //Rate at which the ball moves around the circle
        }
        timer = 0; //Reset the timer to use again
        while (timer < 0.5f) //Half a second wait timer
        {
            transform.position = Vector2.MoveTowards(transform.position, spawnPos, 0.05f); //Move the ball toward the spawn position
            yield return new WaitForSeconds(increment);
            timer += increment;
        }

        transform.position = spawnPos; //Make sure the ball is in position
        ballPaused = false; //Unpause the ball
    }
}
