using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteTrail : MonoBehaviour
{
    public float fadeSpeed;

    public int scaleDirection;
    public float scaleSpeed;

    public float timeBetweenSprites;
    float spriteTimer;

    public bool runTrailOnStart;
    bool trailRunning;
    List<GameObject> sprites = new List<GameObject>();
    SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (runTrailOnStart)
            trailRunning = true;
    }

    void Update()
    {
        if(Time.time >= spriteTimer)
        {
            NewTrailUnit();
            spriteTimer = Time.time + timeBetweenSprites;
        }

        RunTrail();   
    }

    void RunTrail()
    {
        if(sprites.Count > 0)
        {
            List <GameObject> objectsToRemove = new List<GameObject>();

            for (int i = 0; i < sprites.Count; i++)
            {
                sprites[i].transform.localScale += new Vector3(1, 1, 1) * (scaleSpeed * scaleDirection);
                sprites[i].GetComponent<SpriteRenderer>().color -= new Color(0, 0, 0, fadeSpeed);
                if (sprites[i].transform.localScale.x <= scaleDirection || sprites[i].transform.localScale.y <= scaleSpeed)
                {
                    Destroy(sprites[i].gameObject);
                    objectsToRemove.Add(sprites[i]);
                }
            }

            if(objectsToRemove.Count > 0)
            {
                for (int i = 0; i < objectsToRemove.Count; i++)
                {
                    sprites.Remove(objectsToRemove[i]);
                }
            }

        }
    }

    void NewTrailUnit()
    {
        if (!trailRunning)
            return;
        Transform newSprite = new GameObject("Trail", typeof(SpriteRenderer)).transform;
        newSprite.GetComponent<SpriteRenderer>().sprite = spriteRenderer.sprite;
        newSprite.GetComponent<SpriteRenderer>().color = spriteRenderer.color;
        newSprite.position = transform.position;
        newSprite.rotation = transform.rotation;
        newSprite.localScale = transform.localScale/1.5f;
        newSprite.SetParent(null);
        sprites.Add(newSprite.gameObject);
    }

    public void SetTrailActive(bool b)
    {
        trailRunning = false;
    }
}
