using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float minMaxParams;
    public float squashAmount;

    float paddleX;
    int direction;

    Vector3 previousPostion;
    Transform graphic;

    void Start()
    {
        paddleX = 1;
        previousPostion = transform.position;
        graphic = transform.GetChild(0);
    }

    void Update()
    {
        PaddlePosition();
    }

    private void FixedUpdate()
    {
        float paddleY = 1 - (GetSpeed()/2);
        if (paddleY >= 0.5f)
            paddleY = 1;
        if (paddleY <= 0.2f)
            paddleY = 0.2f;
        Vector2 targetScale = new Vector2(paddleX, paddleY);
        graphic.localScale = Vector2.Lerp(graphic.localScale, targetScale, 50 * Time.deltaTime);
        CheckDirection();
        previousPostion = transform.position;
    }

    void PaddlePosition()
    {
        Vector3 mousePos = Input.mousePosition;
        mousePos.z = 10;
        Vector3 input = Camera.main.ScreenToWorldPoint(mousePos);
        input.x = Mathf.Clamp(input.x, -minMaxParams, minMaxParams);
        transform.position = new Vector3(input.x, transform.position.y);
    }

    void CheckDirection()
    {
        if (previousPostion.x > transform.position.x)
            direction = -1;
        else if (previousPostion.x < transform.position.x)
            direction = 1;
    }

    float GetSpeed()
    {
        float speed = Vector3.Distance(transform.position, previousPostion);
        return speed;
    }

}
