using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelController : MonoBehaviour
{
    public static LevelController levelController;

    public string seed;

    public Vector2 generateStart;
    public Vector2 spacing;

    public GameObject blockPrefab;

    public List<BlockColumn> columns = new List<BlockColumn>();
    List<GameObject> blocks = new List<GameObject>();

    bool gameOver;

    [System.Serializable]
    public class BlockColumn
    {
        public List<Transform> blocks = new List<Transform>();
        public float baseY;
        public float xValue;
        public int columnLength;

        public BlockColumn(float x, float y, int length)
        {
            baseY = y;
            xValue = x;
            columnLength = length;
        }

        public void SetBlockList(List<Transform> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                Transform newBlock = Instantiate(list[i].gameObject, new Vector2(xValue, list[i].transform.position.y), Quaternion.identity).transform;
                newBlock.position = new Vector2(xValue, list[i].transform.position.y);
                if (list[i].GetComponent<BlockComponent>().maxHealth == 0)
                    newBlock.GetComponent<BlockComponent>().SetWall();
                newBlock.GetComponent<BlockComponent>().SetOpacity(1 - (i * 0.1f));
                blocks.Add(newBlock);
            }
        }

        public void GenerateColoumn(GameObject prefab, Vector2 spacing)
        {
            for (int i = 0; i < columnLength; i++)
            {
                Transform newBlock = Instantiate(prefab, new Vector2(xValue, baseY), Quaternion.identity).transform;
                newBlock.position = new Vector2(xValue, baseY) + new Vector2(0, spacing.y * -i);
                BlockTypeCalculation(i, newBlock.GetComponent<BlockComponent>());
                newBlock.GetComponent<BlockComponent>().SetOpacity(1 - (i * 0.1f));
                blocks.Add(newBlock);
            }
        }

        void BlockTypeCalculation(int order, BlockComponent block)
        {
            int roll = Random.Range(0, 100);

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
        if (levelController == null)
            levelController = this;
        else
            Destroy(gameObject);

        if (seed != "")
            Random.InitState(seed.GetHashCode());

        CreateColourSet();

        int heldIndex = 3;
        for (int i = 0; i < 8; i++)
        {
            int columnLength = Random.Range(2, 8);
            BlockColumn newColumn = new BlockColumn(generateStart.x + (spacing.x * i), generateStart.y, columnLength);
            if (i <= 3)
            {
                newColumn.GenerateColoumn(blockPrefab, spacing);
            }
            else
            {
                newColumn.SetBlockList(columns[heldIndex].blocks);
                heldIndex--;
            }
            columns.Add(newColumn);
        }

        SetBlockList();
    }

    private void Update()
    {
        if (blocks.Count == 0 && !gameOver)
        {
            gameOver = true;
            print("Yay nice");
        }
    }

    public void CreateColourSet()
    {
        Color baseColor = new Color(Random.Range(0f, 25500f) / 25500f, Random.Range(0f, 25500f) / 25500f, Random.Range(0f, 25500f) / 25500f);

        List<Color> colors = new List<Color>();
        colors.Add(baseColor);

        if(baseColor.r >= baseColor.b && baseColor.r >= baseColor.g)
        {
            colors.Add(new Color(baseColor.r, baseColor.b, baseColor.g));
            if (baseColor.b > baseColor.g)
            {
                colors.Add(new Color(baseColor.b, 1 - baseColor.g, baseColor.r));
                colors.Add(new Color(1 - baseColor.g, baseColor.b, baseColor.r));
            }
            else
            {
                colors.Add(new Color(baseColor.g, baseColor.r, 1 - baseColor.b));
                colors.Add(new Color(1 - baseColor.b, baseColor.r, baseColor.g));
            }
        }
        else if (baseColor.b >= baseColor.r && baseColor.b >= baseColor.g)
        {
            colors.Add(new Color(baseColor.g, baseColor.r, baseColor.b));
            if (baseColor.r > baseColor.g)
            {
                colors.Add(new Color(baseColor.g, 1 - baseColor.r, baseColor.b));
                colors.Add(new Color(1 - baseColor.r, baseColor.g, baseColor.b));
            }
            else
            {
                colors.Add(new Color(baseColor.r, 1 - baseColor.g,baseColor.b));
                colors.Add(new Color(1 - baseColor.g, baseColor.r, baseColor.b));
            }
        }
        else if (baseColor.g >= baseColor.r && baseColor.g >= baseColor.b)
        {
            colors.Add(new Color(baseColor.b, baseColor.r, baseColor.g));
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
            Color storedColor = colors[i];
            int randomRoll = Random.Range(0, colors.Count);
            colors[i] = colors[randomRoll];
            colors[randomRoll] = storedColor;
        }
        blockPrefab.GetComponent<BlockComponent>().SetColours(colors.ToArray());
        Camera.main.backgroundColor = colors[3];
    }

    public void SetBlockList()
    {
        GameObject[] b = GameObject.FindGameObjectsWithTag("Block");
        for (int i = 0; i < b.Length; i++)
        {
            blocks.Add(b[i]);
        }
    }

    public void RemoveBlock(GameObject g)
    {
        if (blocks.Contains(g))
            blocks.Remove(g);
    }
}
