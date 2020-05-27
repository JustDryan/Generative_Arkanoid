using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockComponent : MonoBehaviour
{
    public int maxHealth;
    int health;
    public SpriteRenderer shading;
    public Color[] healthColours;
    float opacity;
    bool isWall;

    void Start()
    {
        health = maxHealth;
        SetColour(healthColours[health]);

        if (isWall)
        {
            SetColour(healthColours[0]);
            gameObject.tag = "BlockWall";
            if (LevelController.levelController != null)
                LevelController.levelController.RemoveBlock(gameObject);
            Destroy(this);
        }
    }

    public void RemoveHealth()
    {
        health--;
        SetColour(healthColours[health]);

        if (health <= 0)
        {
            if (LevelController.levelController != null)
                LevelController.levelController.RemoveBlock(gameObject);
            Destroy(gameObject);
        }
    }

    public void SetMaxHealth(int h)
    {
        maxHealth = h;
    }

    public void SetWall()
    {
        isWall = true;
        maxHealth = 0;
    }

    void SetColour(Color c)
    {
        shading.color = c;
        shading.color = new Color(shading.color.r, shading.color.g, shading.color.b, opacity);
    }

    public void SetOpacity(float o)
    {
        opacity = o;
    }

    public void SetColours(Color[] newColours)
    {
        for (int i = 0; i < 3; i++)
        {
            healthColours[i + 1] = newColours[i];
        }
    }
}
