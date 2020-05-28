using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController gameController;

    [Header("Game Info")]
    public GameObject ballPrefab;
    public Transform ballSpawn;
    GameObject blockPrefab;
    public List<GameObject> balls = new List<GameObject>();

    [Header("Menu Screen")]
    public GameObject mainMenu;
    public Transform playButton, seedButton;
    public InputField seedInput;
    public Text currentSeed;

    [Header("Game Screen")]
    public GameObject gameScreen;
    public Text scoreText, timerText, ballText;
    public int score, ballCount;

    void Start()
    {
        if (gameController == null) //Sets the singleton if one doesn't exist
            gameController = this;
        else //If one does exists, destroys this instance
            Destroy(gameObject);

        SceneStart();
    }

    void SceneStart()
    {
        blockPrefab = LevelController.levelController.blockPrefab;
        LevelController.levelController.GenerateNewLevel();
        SetButtonColours(playButton);
        SetButtonColours(seedButton);
        DisplayNewSeed();
        SetMenuActive(true);
        SetGameScreenActive(false);
    }

    public void FinishGame()
    {
        if(balls.Count > 0)
        {
            foreach (GameObject b in balls)
                Destroy(b);
        }
        GameObject[] trails = GameObject.FindGameObjectsWithTag("Trail");
        if(trails.Length > 0)
        {
            foreach (GameObject t in trails)
                Destroy(t);
        }
        balls = new List<GameObject>();
        CycleLevel();
        FindObjectOfType<PlayerController>().SetPaddleActive(false);
        FindObjectOfType<PlayerController>().transform.position = new Vector2(0, FindObjectOfType<PlayerController>().transform.position.y);
        LevelController.levelController.ResetTimer();
        ResetBallUses();
        SetMenuActive(true);
        Cursor.visible = true;
        ResetScore();

    }

    public void CycleLevel()
    {
        blockPrefab = LevelController.levelController.blockPrefab;
        LevelController.levelController.GenerateNewLevel(); //Generates a new set of blocks
        SetButtonColours(playButton);
        SetButtonColours(seedButton);
        DisplayNewSeed();
    }

    void SetButtonColours(Transform buttonParent)
    {
        buttonParent.transform.GetChild(0).GetComponent<Text>().color = blockPrefab.GetComponent<BlockComponent>().healthColours[1];
        buttonParent.transform.GetChild(1).GetComponent<Text>().color = blockPrefab.GetComponent<BlockComponent>().healthColours[2];
    }

    public void StartGame()
    {
        Cursor.visible = false; //Removes the cursor
        SpawnNewBall();
        LevelController.levelController.StartGame();
        FindObjectOfType<PlayerController>().SetPaddleActive(true);
        SetMenuActive(false);
        SetGameScreenActive(true);
    }

    public void ChangeSeed()
    {
        string s = seedInput.text;
        LevelController.levelController.SetSeed(s);
        seedInput.text = "";
        CycleLevel();
    }

    public void DisplayNewSeed()
    {
        if (LevelController.levelController.seed != "")
            currentSeed.text = LevelController.levelController.seed;
        else
            currentSeed.text = LevelController.levelController.randomSeed;
    }

    public void SetTimerText(string t)
    {
        timerText.text = t;
    }

    public void SpawnNewBall()
    {
        Transform b = Instantiate(ballPrefab, ballSpawn).transform; //Spawn the ball
        b.SetParent(null);
        balls.Add(b.gameObject);
    }

    public void AddPoints(int i)
    {
        score += i;
        scoreText.text = score.ToString();
    }

    public void ResetScore()
    {
        score = 0;
        scoreText.text = score.ToString();
    }

    public void AddBallUses(int i)
    {
        ballCount += i;
        ballText.text = ballCount.ToString();
    }

    public void ResetBallUses()
    {
        ballCount = 0;
        ballText.text = ballCount.ToString();
    }

    void SetMenuActive(bool b)
    {
        mainMenu.SetActive(b);
    }

    void SetGameScreenActive(bool b)
    {
        gameScreen.SetActive(b);
    }

}
