using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public static LevelController levelController; //Level Controller singleton

    public string seed; //Seed to generate from
    [HideInInspector]public string randomSeed; //Stores the randomly generated seeds

    public Vector2 generateStart; //Position to start generating at (top left)
    public Vector2 spacing; //Spacing between blocks on x/y

    public GameObject blockPrefab; //Block prefab

    public List<BlockColumn> columns = new List<BlockColumn>(); //List of columns, used to mirror
    public List<GameObject> blocks = new List<GameObject>(); //List of destroyable blocks in game, goes down every time a block is destroyed, used to complete game

    bool gameStarted;
    bool gameOver;

    float timer;
    int gameTimer;

    [System.Serializable]
    public class BlockColumn //The column class
    {
        public List<Transform> blocks = new List<Transform>();
        public float baseY;
        public float xValue;
        public int columnLength;

        public BlockColumn(float x, float y, int length)
        {
            baseY = y; //Sets the starting heigh for generation
            xValue = x; //Sets the horizontal position of the column, this does not change
            columnLength = length; //Sets how many blocks the column will generate
        }

        public void GenerateColoumn(GameObject prefab, Vector2 spacing) //Generates a brand new column
        {
            for (int i = 0; i < columnLength; i++) //For how long the column is
            {
                Transform newBlock = Instantiate(prefab, new Vector2(xValue, baseY), Quaternion.identity).transform; //Creates a new block using the blockPrefab
                newBlock.position = new Vector2(xValue, baseY) + new Vector2(0, spacing.y * -i); //Sets the position of this new block to the xValue and adjusts the height dependant on which number in the list it is and how much spacing is being applied
                BlockTypeCalculation(i, newBlock.GetComponent<BlockComponent>()); //Rolls the block type, setting max health and whether or not the block is a wall
                newBlock.GetComponent<BlockComponent>().SetOpacity(1 - (i * 0.1f)); //Sets the block's opacity variable depending how high it is, higher = more opaque
                blocks.Add(newBlock); //Adds this block to the column's list of blocks
            }
        }

        public void SetBlockList(List<Transform> list) //Copies another column using the other column's block list
        {
            for (int i = 0; i < list.Count; i++)
            {
                Transform newBlock = Instantiate(list[i].gameObject, new Vector2(xValue, list[i].transform.position.y), Quaternion.identity).transform; //Creates a new block using the other list's block as a base
                newBlock.position = new Vector2(xValue, list[i].transform.position.y); //Sets the position, only changing the xValue to the current column's xValue
                if (list[i].GetComponent<BlockComponent>().maxHealth == 0) //Checks if the block being copied is a wall and sets this block to also be a wall
                    newBlock.GetComponent<BlockComponent>().SetWall();
                newBlock.GetComponent<BlockComponent>().SetOpacity(1 - (i * 0.1f)); //Sets the block's opacity variable depending how high it is, higher = more opaque
                blocks.Add(newBlock); //Adds this block to the column's list of blocks
            }
        }

        void BlockTypeCalculation(int order, BlockComponent block)
        {
            int roll = Random.Range(0, 100);

            //Calculations on block health/wall settings are done of one roll. The roll's chances change depending on how high up thhe screen the block is, if the higher the block, the more likely the max health will be higher
            //All rolls have a very chance to be walls

            if(order < 2)
            {
                if(roll < 60)
                {
                    block.SetMaxHealth(3);
                    return;
                }
                if(roll < 85)
                {
                    block.SetMaxHealth(2);
                    return;
                }
                if (roll < 95)
                {
                    block.SetMaxHealth(1);
                    return;
                }

                block.SetWall();
                return;
            }

            if (order < 4)
            {
                if (roll < 50)
                {
                    block.SetMaxHealth(2);
                    return;
                }
                if (roll < 75)
                {
                    block.SetMaxHealth(3);
                    return;
                }
                if (roll < 90)
                {
                    block.SetMaxHealth(1);
                    return;
                }

                block.SetWall();
                return;
            }

            if (order < 8)
            {
                if (roll < 75)
                {
                    block.SetMaxHealth(1);
                    return;
                }
                if (roll < 90)
                {
                    block.SetMaxHealth(2);
                    return;
                }
                if (roll < 95)
                {
                    block.SetMaxHealth(3);
                    return;
                }

                block.SetWall();
                return;
            }
        }
    }

    void Start()
    {
        if (levelController == null) //Sets the singleton if one doesn't exist
            levelController = this;
        else //If one does exists, destroys this instance
            Destroy(gameObject);
    }

    [ContextMenu("Generate New Level")]
    public void GenerateNewLevel() //Generates a new set of blocks
    {
        if (seed != "") //If there is a seed, generate a hashcode from it
            Random.InitState(seed.GetHashCode());
        else //If there is no seed, use the current game time as a seed
        {
            randomSeed = (Mathf.FloorToInt(Time.time + 1) * Random.Range(1, 4500)).ToString();
            Random.InitState(randomSeed.GetHashCode());
        }

        DestroyBlockList(); //Destroys all previous blocks if there are some

        CreateColourSet(); //Creates a new colour palette
        columns = new List<BlockColumn>(); //Resets the column list
        int heldIndex = 3; //This index is used to mirror the columns
        for (int i = 0; i < 8; i++)
        {
            int columnLength = Random.Range(2, 8); //Sets a random column length, maximum of 7, minimum of 2
            BlockColumn newColumn = new BlockColumn(generateStart.x + (spacing.x * i), generateStart.y, columnLength); //Create a new column
            if (i <= 3) //First half
            {
                newColumn.GenerateColoumn(blockPrefab, spacing); //Populate the new column with random blocks
            }
            else //Second half
            {
                newColumn.SetBlockList(columns[heldIndex].blocks); //Copy the selected column
                heldIndex--; //Work down the list to mirror the columns
            }
            columns.Add(newColumn); //Adds the new column to the list
        }

        SetBlockList(); //Generates a list of all destroyable blocks
    }

    private void Update()
    {
        if (gameStarted)
        {
            if(Time.time >= timer)
            {
                gameTimer++;
                timer = Time.time + 1;
                GameController.gameController.SetTimerText(gameTimer.ToString());
            }
        }

        if (blocks.Count == 0 && !gameOver && gameStarted) //If there are no more destroyable blocks (if the block list is empty) and the game isn't over
        {
            gameOver = true; //Prevent extra runs
            gameStarted = false;
            GameController.gameController.FinishGame(); //End the game
        }

        //if (Input.GetKeyDown(KeyCode.Space)) Generate a new level on space press, used for making my gif
        //    GenerateNewLevel();
    }

    public void CreateColourSet()
    {
        //This function generates a base colour and then attempts to mirror a tetradic colour theory e.g: Base Colour = R:255, G:40, B:10
        //From the base colour we keep the highest value and reverse the other two thus creating a second colour e.g: Base Colour = R:255, G:10, B:40
        //For the third colour we swap the highest and lowest values, then reverse the third value e.g: Base Colour = R:10, G:215, B:255
        //For the final fourth colour we repeat the second step with the third step e.g: Base Colour = R:215, G:10, B:255
        //This creates four colours in a list. The list positions are then shuffled and elements 0-2 are set as block colours, whilst the final element 3 is set as the camera colour

        Color baseColor = new Color(Random.Range(0f, 25500f) / 25500f, Random.Range(0f, 25500f) / 25500f, Random.Range(0f, 25500f) / 25500f); //Sets base

        List<Color> colors = new List<Color>(); //Creates a new list of colours
        colors.Add(baseColor); //Adds the base colour

        if(baseColor.r >= baseColor.b && baseColor.r >= baseColor.g) //Steps 2-4 are done here if red is highest
        {
            colors.Add(new Color(baseColor.r, baseColor.b, baseColor.g));
            if (baseColor.b >= baseColor.g)
            {
                colors.Add(new Color(baseColor.g, baseColor.r, 1 - baseColor.b));
                colors.Add(new Color(1 - baseColor.b, baseColor.r, baseColor.g));
            }
            else if (baseColor.b < baseColor.g)
            {
                colors.Add(new Color(baseColor.b, 1 - baseColor.g, baseColor.r));
                colors.Add(new Color(1 - baseColor.g, baseColor.b, baseColor.r));
            }
        }
        else if (baseColor.b >= baseColor.r && baseColor.b >= baseColor.g) //Steps 2-4 are done here if blue is highest
        {
            colors.Add(new Color(baseColor.g, baseColor.r, baseColor.b));
            if (baseColor.r >= baseColor.g)
            {
                colors.Add(new Color(baseColor.g, baseColor.b, 1 - baseColor.r));
                colors.Add(new Color(1 - baseColor.r, baseColor.b, baseColor.g));
            }
            else if(baseColor.r < baseColor.g)
            {
                colors.Add(new Color(baseColor.b, 1 - baseColor.g, baseColor.r));
                colors.Add(new Color(baseColor.b, baseColor.r,  1 - baseColor.g));
            }
        }
        else if (baseColor.g >= baseColor.r && baseColor.g >= baseColor.b) //Steps 2-4 are done here if green is highest
        {
            colors.Add(new Color(baseColor.b, baseColor.g, baseColor.r));

            if (baseColor.b > baseColor.r)
            {
                colors.Add(new Color(baseColor.g, 1 - baseColor.b, baseColor.r));
                colors.Add(new Color(baseColor.g, baseColor.r, 1 - baseColor.b));
            }
            else
            {
                colors.Add(new Color(baseColor.b, 1 -baseColor.r, baseColor.g));
                colors.Add(new Color(1 - baseColor.r, baseColor.b, baseColor.g));
            }
        }

        for (int i = 0; i < colors.Count; i++)
        {
            print(i + "--" + colors[i].r + ":" + colors[i].g + ":" + colors[i].b);
        }

        for (int i = 0; i < colors.Count; i++) //Randomly distributes the colours in the list
        {
            Color storedColor = colors[i];
            int randomRoll = Random.Range(0, colors.Count);
            colors[i] = colors[randomRoll];
            colors[randomRoll] = storedColor;
        }
        blockPrefab.GetComponent<BlockComponent>().SetColours(colors.ToArray()); //Sets the block colours
        Camera.main.backgroundColor = colors[3]; //Sets the background colour
    }

    [ContextMenu("Destroy all block")]
    public void DestroyBlockList()
    {
        if (blocks.Count > 0) //If there are blocks in the list, destroy all blocks
        {
            foreach (GameObject block in blocks)
            {
                Destroy(block);
            }
        }

        GameObject[] blockWalls = GameObject.FindGameObjectsWithTag("BlockWall"); //Find all block walls
        if (blockWalls.Length > 0) //If any block walls are found, destroy them
        {
            foreach (GameObject bW in blockWalls)
            {
                Destroy(bW);
            }
        }

        blocks = new List<GameObject>(); //Create a new block list
    }

    public void SetBlockList()
    {
        DestroyBlockList(); //Precautionary list destruction

        GameObject[] b = GameObject.FindGameObjectsWithTag("Block"); //Find all the blocks in the level
        for (int i = 0; i < b.Length; i++)
        {
            blocks.Add(b[i]); //Add them to the main list
        }
    }

    public void RemoveBlock(GameObject g) //Called when a block is destroyed, removes it from the list if it is in the list
    {
        if (blocks.Contains(g))
            blocks.Remove(g);
    }

    public void SetSeed(string s) //Sets seed to input string
    {
        seed = s;
    }

    public void StartGame()
    {
        gameStarted = true;
        gameOver = false;
        ResetTimer();
    }

    public void ResetTimer()
    {
        timer = 1;
        gameTimer = 0;
        GameController.gameController.SetTimerText("0");
    }
}
