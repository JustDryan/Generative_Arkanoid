using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public float ballSpeed;
    public float maxBallSpeed;
    public float ballSpeedIncrease;
    float rotZ;
    float canChangeTime;

    public AudioClip[] clips;

    float prevX;
    GameObject prevCollision;

    Vector3 spawnPos;

    Rigidbody2D rb;
    int hitIndex;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); //Find the Rigidbody2D on the object
        SetBallRotation(180); //Sets the fall to face downwards
        spawnPos = transform.position; //Sets the point the ball will return to when respawned
    }

    void Update()
    {
        rb.velocity = transform.up * ballSpeed; //The ball constantly moves on its local up axis

        prevX = transform.position.x; //Used to determine the direction the ball is moving
    }

    public void RespawnBall() //Places the ball back at the spawn point, makes it face downwards and resets collisions
    {
        transform.position = spawnPos;
        SetBallRotation(180);
        prevCollision = null;
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
        if (!BallOnCooldown())
            transform.eulerAngles = new Vector3(0, 0, rotation);
    }

    public void ReflectBall(Vector3 normal, float angleIncrease) //Reflects the ball when it hits a surface, takes the normal from the object hit, and adds/subtracts degrees depending on where it lands on the paddle
    {
        StartBallCooldown(); //To prevent the ball from getting stuck
        if (ballSpeed < maxBallSpeed) //Checks how fast the ball is going
            ballSpeed += ballSpeedIncrease; //If the ball is going slower than the max, increment speed
        float f = 90 + (angleIncrease * GetDirection()); //The angle returned by Vector3.Reflect is 90 degrees off. Here we account for that and alter the angle depending on the direction the ball is moving, and where the ball landed on the paddle
        Vector3 reflectedPosition = Vector3.Reflect(transform.up, normal); //Gets the reflectecd position of thw ball
        Vector3 dir = (reflectedPosition).normalized; //Gets thenew direction the ball will move in

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg; //Converts this direction to degrees
        transform.eulerAngles = new Vector3(0, 0, angle - f); //Sets the ball angle with the 90 degrees accounted for and angleIncrease added on
    }

    public void StartBallCooldown() //The ball has a small coooldown on interacting with an object to prevent it getting stuck on the same object
    {
        canChangeTime = Time.time + 0.05f;
    }

    bool BallOnCooldown() //Checks if the bool is currently on cooldown
    {
        if (Time.time >= canChangeTime)
            return false;

        return true;
    }

    void PlayHitSound() //Plays a clip 
    {
        AudioSource aud = new GameObject("BallHitClip", typeof(AudioSource)).GetComponent<AudioSource>(); //Creates an empty gameobject and attaches an AudioSource
        aud.volume = 0.5f; //Sets the volume of the new AudioSource
        aud.clip = clips[hitIndex]; //Sets the clip to use
        aud.pitch = Mathf.Pow(1.05946f, Mathf.Floor(transform.position.y + 3)); //Changes the octave of the clip depending on how high the ball is
        hitIndex++; //Increases the index of the clip list
        if (hitIndex >= clips.Length) //Checks if the clip index exceeds the clip list
            hitIndex = 0; //Resets the index to zero if this is the case

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

        Vector3 normal = collision.contacts[0].normal; //Sets the normal of the contact point to a variable to use later

        //Almost all collisions play a sound

        if (collision.gameObject.CompareTag("Paddle") && transform.position.y > collision.transform.position.y) //Checks for the Paddle, implements the angle adjustments to ReflectBall
        {
            ReflectBall(normal, (transform.position.x - collision.transform.position.x) * 20);
            PlayHitSound();
            prevCollision = collision.gameObject;
        }

        if (collision.gameObject.CompareTag("Block")) //Checks for a block, if it is a block, remove 1 health from it
        {
            ReflectBall(normal, 0);
            collision.gameObject.GetComponent<BlockComponent>().RemoveHealth();
            PlayHitSound();
            prevCollision = collision.gameObject;
        }

        if (collision.gameObject.CompareTag("Wall")) //Check if the ball has hit the bounds of the game
        {
            ReflectBall(normal, 0);
            PlayHitSound();
            prevCollision = collision.gameObject;
        }

        if (collision.gameObject.CompareTag("BlockWall")) //Check if the ball has hit a block that has been turned into a wall
        {
            ReflectBall(normal, 0);
            PlayHitSound();
            prevCollision = collision.gameObject;
        }

        if (collision.gameObject.CompareTag("Ball")) //For power-ups, checks if the ball hits another ball
        {
            ReflectBall(normal, 0);
            PlayHitSound();
            prevCollision = collision.gameObject;
        }

        if (collision.gameObject.CompareTag("Bounds")) //Checks if the ball hits the lower bounds of the game and resets the ball
            RespawnBall();
    }
}
