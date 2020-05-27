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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        SetBallRotation(180);
        spawnPos = transform.position;
    }

    void Update()
    {
        rb.velocity = transform.up * ballSpeed;

        prevX = transform.position.x;
    }

    public void RespawnBall()
    {
        transform.position = spawnPos;
        SetBallRotation(180);
        prevCollision = null;
    }

    public int GetDirection()
    {
        if (transform.position.x > prevX)
            return 1;
        else if (transform.position.x < prevX)
            return -1;

        return 1;
    }

    public void AddBallRotation(float increase)
    {
        if (!BallOnCooldown())
            rotZ += increase;
    }

    public void SetBallRotation(float rotation)
    {
        if (!BallOnCooldown())
            transform.eulerAngles = new Vector3(0, 0, rotation);
    }

    public void ReflectBall(Vector3 normal, float angleIncrease)
    {
        StartBallCooldown();
        if (ballSpeed < maxBallSpeed)
            ballSpeed += ballSpeedIncrease;
        float f = 90 + (angleIncrease * GetDirection());
        Vector3 reflectedPosition = Vector3.Reflect(transform.up, normal);
        Vector3 dir = (reflectedPosition).normalized;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.eulerAngles = new Vector3(0, 0, angle - f);
    }

    public void StartBallCooldown()
    {
        canChangeTime = Time.time + 0.05f;
    }

    bool BallOnCooldown()
    {
        if (Time.time >= canChangeTime)
            return false;

        return true;
    }

    void PlayHitSound(bool paddle)
    {
        AudioSource aud = new GameObject("BallHitClip", typeof(AudioSource)).GetComponent<AudioSource>();
        aud.volume = 0.5f;
        if (paddle)
            aud.clip = clips[0];
        else
        {
            if (transform.position.y >= 2)
                aud.clip = clips[3];
            else if (transform.position.y >= 1)
                aud.clip = clips[2];
            else
                aud.clip = clips[1];
        }
        if (aud.clip != null)
        {
            aud.Play();
            Destroy(aud.gameObject, aud.clip.length);
        }
        else
            Destroy(aud.gameObject);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (BallOnCooldown())
            return;
        if (collision.gameObject == prevCollision)
            return;

        Vector3 normal = collision.contacts[0].normal;

        if (collision.gameObject.CompareTag("Paddle") && transform.position.y > collision.transform.position.y)
        {
            ReflectBall(normal, (transform.position.x - collision.transform.position.x) * 20);
            PlayHitSound(true);
            prevCollision = collision.gameObject;
        }

        if (collision.gameObject.CompareTag("Block"))
        {
            ReflectBall(normal, 0);
            collision.gameObject.GetComponent<BlockComponent>().RemoveHealth();
            PlayHitSound(false);
            prevCollision = collision.gameObject;
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            ReflectBall(normal, 0);
            PlayHitSound(false);
            prevCollision = collision.gameObject;
        }

        if (collision.gameObject.CompareTag("Bounds"))
            RespawnBall();
    }
}
