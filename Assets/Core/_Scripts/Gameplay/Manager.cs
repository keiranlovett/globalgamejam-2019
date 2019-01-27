using UnityEngine;
using System.Collections;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public struct Unit
{
    public string name;
    public GameObject go;
    public Vector3 location;


    public Unit(string Name, GameObject Go, Vector3 Location)
    {
        name = Name;
        go = Go;
        location = Location;
    }

}


public class Manager : MonoBehaviour
{

    [Header("Mission")]
    public bool shouldDestroyBaseGoal;
    public int killEnemiesGoal;
    public int timeToGoalInSeconds;

    [Space(10)]

    //variables visible in the inspector
    public float fadespeed;

    //variables not visible in the inspector
    private GameObject GameOverMenu;
    private GameObject VictoryMenu;
    private GameObject GamePanel;
    private GameObject unitsButtons;

    private bool fading;
    private float alpha;

    private GameObject unitsLabel;

    private GameObject playerBaseStrengthText;
    private GameObject enemyBaseStrengthText;
    private GameObject playerBaseStrengthBar;
    private GameObject enemyBaseStrengthBar;

    private GameObject goalsPanel;

    private float playerBaseStrengthStart;
    private float enemyBaseStrengthStart;

    private float playerBaseStrength;
    private float enemyBaseStrength;

    public static int enemiesKilled;
    private float time;


    public Transform StartCameraPosition;

    public static bool gameOver;
    public static bool victory;
    public static GameObject StartMenu;






    void Start()
    {
        //find some objects
        unitsButtons = GameObject.Find("Character panel");
        unitsLabel = GameObject.Find("panel-Informatioon");

        playerBaseStrengthText = GameObject.Find("Player castle strength text");
        enemyBaseStrengthText = GameObject.Find("Enemy castle strength text");

        playerBaseStrengthBar = GameObject.Find("Player castle strength bar");
        enemyBaseStrengthBar = GameObject.Find("Enemy castle strength bar");

        //immediately freeze game
        Time.timeScale = 0;

        //find all UI panels
        StartMenu = GameObject.Find("Start panel");
        GameOverMenu = GameObject.Find("Game over panel");
        VictoryMenu = GameObject.Find("Victory panel");
        GamePanel = GameObject.Find("Game panel");

        //turn off all panels except the start menu panel
        StartMenu.SetActive(true);
        GameOverMenu.SetActive(false);
        VictoryMenu.SetActive(false);
        GamePanel.SetActive(false);

        //game over and victory are false
        gameOver = false;
        victory = false;

        //start menu alpha is alpa
        alpha = StartMenu.GetComponent<Image>().color.a;

        //get strength of all castles together
        GetBaseStrength();

        //set start castle strengths
        playerBaseStrengthStart = playerBaseStrength;
        enemyBaseStrengthStart = enemyBaseStrength;

        goalsPanel = GameObject.Find("Mission");
        string mission = "";

        if (shouldDestroyBaseGoal)
        {
            mission = "Destroy Base of the opponent";
        }
        else if (killEnemiesGoal > 0 && timeToGoalInSeconds > 0)
        {
            mission = "- Kill " + killEnemiesGoal + " enemies. \n- Battle at least " + timeToGoalInSeconds + " seconds.";
        }
        else if (killEnemiesGoal > 0 && timeToGoalInSeconds <= 0)
        {
            mission = "Kill " + killEnemiesGoal + " enemies.";
        }
        else if (killEnemiesGoal <= 0 && timeToGoalInSeconds > 0)
        {
            mission = "Battle at least " + timeToGoalInSeconds + " seconds.";
        }
        else
        {
            mission = "No mission";
        }

        goalsPanel.transform.Find("Mission text").gameObject.GetComponent<Text>().text = mission;
        goalsPanel.SetActive(false);
        time = 0;

        openMissionPanel();


    }

    void Update()
    {
        //keep updating castle strengths
        GetBaseStrength();

        //show the castle strengths
        playerBaseStrengthText.GetComponent<Text>().text = "" + (int)playerBaseStrength;
        enemyBaseStrengthText.GetComponent<Text>().text = "" + (int)enemyBaseStrength;

        //set the fillamount (round bar) to the percentage of castles left
        enemyBaseStrengthBar.GetComponent<Image>().fillAmount = enemyBaseStrength / enemyBaseStrengthStart;
        playerBaseStrengthBar.GetComponent<Image>().fillAmount = playerBaseStrength / playerBaseStrengthStart;

        time += Time.deltaTime;

        //set game over true when the castles are destroyed
        if (playerBaseStrength <= 0)
        {
            Time.timeScale = 0;
            gameOver = true;
        }
        //set victory based on mission
        if (enemyBaseStrength <= 0 && shouldDestroyBaseGoal)
        {
            Time.timeScale = 0;
            victory = true;
        }
        else if (killEnemiesGoal > 0 && enemiesKilled >= killEnemiesGoal && timeToGoalInSeconds > 0 && timeToGoalInSeconds < time)
        {
            Time.timeScale = 0;
            victory = true;
        }
        else if ((timeToGoalInSeconds > 0 && timeToGoalInSeconds < time) && !(killEnemiesGoal > 0))
        {
            Time.timeScale = 0;
            victory = true;
        }
        else if ((killEnemiesGoal > 0 && enemiesKilled == killEnemiesGoal) && !(timeToGoalInSeconds > 0))
        {
            Time.timeScale = 0;
            victory = true;
        }

        //control the visibility of the panels
        if (gameOver && !GameOverMenu.activeSelf)
        {
            GamePanel.SetActive(false);
            GameOverMenu.SetActive(true);
        }
        if (victory && !VictoryMenu.activeSelf)
        {
            GamePanel.SetActive(false);
            VictoryMenu.SetActive(true);
        }

        //fade the start panel out when fading is true
        if (fading && alpha > 0)
        {
            alpha -= Time.deltaTime * fadespeed;

            StartMenu.GetComponent<Image>().color = new Color(
            StartMenu.GetComponent<Image>().color.r,
            StartMenu.GetComponent<Image>().color.g,
            StartMenu.GetComponent<Image>().color.b,
            alpha);
        }
        else if (alpha <= 0)
        {
            //remove start panel when alpha is 0
            fading = false;
            StartMenu.SetActive(false);
            GamePanel.SetActive(true);
        }

        //get the amount of units and the amount of selected units
        int playerUnits = GameObject.FindGameObjectsWithTag("Knight").Length;
        int enemyUnits = GameObject.FindGameObjectsWithTag("Enemy").Length;
        int selectedUnits = 0;

        //add 1 to selected units for each selected unit
        foreach (GameObject unit in GameObject.FindGameObjectsWithTag("Knight"))
        {
            if (unit.GetComponent<UnitBase>().selected)
            {
                selectedUnits++;
            }
        }
        //show the amount of units
        unitsLabel.GetComponentInChildren<Text>().text = "Player Units: " + playerUnits + "  |  Enemy Units: " + enemyUnits + "  |  Selected Units: " + selectedUnits;
    }



    void GetBaseStrength()
    {
        //castle strength is 0
        playerBaseStrength = 0;
        enemyBaseStrength = 0;

        //add the strength of each enemy castle
        foreach (GameObject enemyBase in GameObject.FindGameObjectsWithTag("Enemy Castle"))
        {
            if (enemyBase.GetComponent<Base>().lives > 0)
            {
                enemyBaseStrength += enemyBase.GetComponent<Base>().lives;
            }
        }
        //add the strength of each player castle
        foreach (GameObject playerBase in GameObject.FindGameObjectsWithTag("Player Castle"))
        {
            if (playerBase.GetComponent<Base>().lives > 0)
            {
                playerBaseStrength += playerBase.GetComponent<Base>().lives;
            }
        }
    }

    //start the game
    public void startGame()
    {
        //set buttons false
        foreach (Transform button in StartMenu.transform)
        {
            button.gameObject.SetActive(false);
        }
        //set timescale to normal and start fading out
        Time.timeScale = 1;
        fading = true;
    }

    //open mission panel to start game
    public void openMissionPanel()
    {
        goalsPanel.SetActive(true);
    }

    public void endGame()
    {
        //end game
        Application.Quit();
    }

    public void surrender()
    {
        //Freeze game and set the game over panel visible
        Time.timeScale = 0;
        Manager.gameOver = true;
    }

}
