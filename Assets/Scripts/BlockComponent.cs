using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlockComponent : MonoBehaviour
{
    public int maxHealth; //Maximum health of the block, used on start
    int health; //Actual/current health of the block
    public SpriteRenderer shading; //Sprite that overlays a white sprite, using to colour in the blocks. I use two sprites tomake the opacity scale more obvious and (hopefully) conflict less with the background.
    public Color[] healthColours; //Stores the colours that will represent what health a block is on.
    float opacity; //Stores how opaque the shading sprite whill be
    bool isWall; //Checks if the block is a wall, changes it as such in Start

    void Start()
    {
        health = maxHealth; //Sets the actual health of the block to the max health
        SetColour(healthColours[health]); //Sets the colour of the block to the appropriate colour, opacity is determined by hiegh on screen

        if (isWall) //Checks if the block has been change to a wall
        {
            SetColour(healthColours[0]); //Sets the colour to black, opacity still determined by screen height
            gameObject.tag = "BlockWall"; //Changes the tag so that it doesn't interfere with actual blocks
            if (LevelController.levelController != null)
                LevelController.levelController.RemoveBlock(gameObject); //Removes the block from the list of blocks the player has to destroy
            Destroy(this); //Destroys the script as there is no more need for it
        }
    }

    public void RemoveHealth() 
    {
        health--; //Removes health from the block
        SetColour(healthColours[health]); //Changes the colour to suit the damage, opactiy is the same
        GameController.gameController.AddPoints(100);

        if (health <= 0) //If the block has run out of health
        {
            if (LevelController.levelController != null)
                LevelController.levelController.RemoveBlock(gameObject); //Removes the block from the list of blocks the player needs to destroy
            GameController.gameController.AddPoints(100 * maxHealth);
            Destroy(gameObject); //Destroys the object
        }
    }

    public void SetMaxHealth(int h) //Used when generating a new level to set how many hits the block can take, 1 - 3
    {
        maxHealth = h;
    }

    public void SetWall() // Sets the block to be a wall which is then used in the Start function
    {
        isWall = true;
        maxHealth = 0;
    }

    void SetColour(Color c) //Sets the colour of the block, but uses the varaible opacity which is set elsewhere and does not change
    {
        shading.color = c;
        shading.color = new Color(shading.color.r, shading.color.g, shading.color.b, opacity);
    }

    public void SetOpacity(float o) //Called when the block is created and sets the opacity the block will use
    {
        opacity = o;
    }

    public void SetColours(Color[] newColours) //Called on the prefab, changes the selection of colours the blocks will use depending on the generated colour scheme
    {
        for (int i = 0; i < 3; i++)
        {
            healthColours[i + 1] = newColours[i];
        }
    }
}
