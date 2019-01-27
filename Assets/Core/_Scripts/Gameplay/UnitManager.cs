using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine.UI;

//troop/unit settings
[System.Serializable]
public class Troop
{
    public GameObject deployableTroops;
    public int troopCosts;
    public Sprite buttonImage;
    [HideInInspector]
    public Button button;
}

public class UnitManager : MonoBehaviour
{

    //variables visible in the inspector    
    public int initialResources;
    public GUIStyle rectangleStyle;
    public ParticleSystem newUnitEffect;
    public Texture2D cursorTexture;
    public Texture2D cursorTexture1;
    public bool highlightSelectedButton;
    public Color buttonHighlight;
    public Button button;
    public float abilityLoadingSpeed;
    public float abilityRange;
    public GameObject abilityExplosion;
    [Space(10)]

    public UnitTypeTransport dropShip;


    public List<Troop> playerUnits;

    //variables not visible in the inspector
    public static Vector3 clickedPos;
    public static int gold;
    public static GameObject target;

    private Vector2 mouseDownPos;
    private Vector2 mouseLastPos;
    private bool visible;
    private bool isDown;
    public GameObject[] knights;
    private int selectedUnit;
    private GameObject goldText;
    private GameObject promptResourcesWarning;
    private GameObject promptResourcesAdded;
    private GameObject characterList;
    private GameObject characterParent;
    private GameObject selectButton;

    private GameObject abilityLoadingBar;
    private GameObject abilityButton;
    private float abilityProgress;
    private bool isPlacingability;
    private GameObject abilityRangeGO;

    public static bool selectionMode;

    void Awake()
    {
        //find some objects
        characterParent = new GameObject("Characters");
        selectButton = GameObject.Find("button-unitSelection");
        target = GameObject.Find("VFX_Target");
        target.SetActive(false);
    }

    void Start()
    {
        //set cursor and add the character buttons
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        characterList = GameObject.Find("layout-unitSelectorUI");
        addUnitsUIButtons();
        //set gold amount to start gold amount
        gold = initialResources;
        //find text that displays your amount of gold
        goldText = GameObject.Find("text-promptResourcesCount");

        //find text that appears when you get extra gold and set it not active
        promptResourcesAdded = GameObject.Find("text-promptResourcesAdded");
        promptResourcesAdded.SetActive(false);
        //find warning that appears when you don't have enough gold to deploy playerUnits and set it not active
        promptResourcesWarning = GameObject.Find("text-promptResourcesWarning");
        promptResourcesWarning.SetActive(false);

        //play function addGold every five seconds
        InvokeRepeating("AddGold", 1.0f, 5.0f);

        //find ability gameobjects
        abilityLoadingBar = GameObject.Find("indicator-abilityProgress");
        abilityButton = GameObject.Find("btn-ability");
        //  abilityRangeGO = GameObject.Find("AbilityUI");
        // abilityRangeGO.SetActive(false);
        isPlacingability = false;
    }

    void Update()
    {
        if (abilityProgress < 1)
        {
            //when ability is loading, set red color and disable button
            abilityProgress += Time.deltaTime * abilityLoadingSpeed;
            abilityLoadingBar.GetComponent<Image>().color = Color.red;
            abilityButton.GetComponent<Button>().enabled = false;
        }
        else
        {
            //if ability is done, set a blue color and enable the ability button
            abilityProgress = 1;
            abilityLoadingBar.GetComponent<Image>().color = new Color(0, 1, 1, 1);
            abilityButton.GetComponent<Button>().enabled = true;
        }

        //set fillamount to the ability progress
        abilityLoadingBar.GetComponent<Image>().fillAmount = abilityProgress;

        //set gold text to gold amount
        goldText.GetComponent<UnityEngine.UI.Text>().text = "" + gold;

        //ray from main camera
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit))

            //check if left mouse button gets pressed
            if (Input.GetMouseButtonDown(0))
            {

                //if you didn't click on UI and you have not enought gold, display a warning
                if (gold < playerUnits[selectedUnit].troopCosts && !EventSystem.current.IsPointerOverGameObject())
                {
                    StartCoroutine(GoldWarning());
                }
                //check if you hit any collider when clicking (just to prevent errors)
                if (hit.collider != null)
                {
                    //if you click battle ground, if click doesn't hit any UI, if space is not down and if you have enough gold, deploy the selected playerUnits and decrease gold amount
                    if (hit.collider.gameObject.CompareTag("Battle ground") && !selectionMode && !isPlacingability && !EventSystem.current.IsPointerOverGameObject()
                    && gold >= playerUnits[selectedUnit].troopCosts && (!GameObject.Find("Mobile") || (GameObject.Find("Mobile") && Mobile.deployMode)))
                    {

                        //GameObject newTroop = Instantiate(playerUnits[selectedUnit].deployableTroops, hit.point, playerUnits[selectedUnit].deployableTroops.transform.rotation) as GameObject;

                        Unit testUnit = new Unit("hello", playerUnits[selectedUnit].deployableTroops, hit.point);
                        dropShip.CreateUnit(testUnit);

                        Instantiate(newUnitEffect, hit.point, Quaternion.FromToRotation(Vector3.forward, hit.normal));
                        gold -= playerUnits[selectedUnit].troopCosts;

                    }

                    //if you are placing a ability and click...
                    if (isPlacingability && !EventSystem.current.IsPointerOverGameObject())
                    {
                        //instantiate explosion
                        Instantiate(abilityExplosion, hit.point, Quaternion.identity);

                        //reset ability progress
                        abilityProgress = 0;
                        isPlacingability = false;
                        abilityRangeGO.SetActive(false);

                        //find enemies
                        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

                        foreach (GameObject enemy in enemies)
                        {
                            if (enemy != null && Vector3.Distance(enemy.transform.position, hit.point) <= abilityRange / 2)
                            {
                                //kill enemy if its within the abilityrange
                                enemy.GetComponent<UnitBase>().healthFloat = 0;
                            }
                        }
                    }
                    else if (hit.collider.gameObject.CompareTag("Battle ground") && isPlacingability && EventSystem.current.IsPointerOverGameObject())
                    {
                        //if you place a ability and click any UI element, continue but don't reset ability progress
                        isPlacingability = false;
                        abilityRangeGO.SetActive(false);
                    }
                }
            }

        if (hit.collider != null && isPlacingability && !EventSystem.current.IsPointerOverGameObject())
        {
            //show the ability range at mouse position using a spot light
            abilityRangeGO.transform.position = new Vector3(hit.point.x, 75, hit.point.z);
            //adjust spotangle to correspond to ability range
            abilityRangeGO.GetComponent<Light>().spotAngle = abilityRange;
            abilityRangeGO.SetActive(true);
        }

        //if space is down too set the position where you first clicked
        if (Input.GetMouseButtonDown(0) && selectionMode && !isPlacingability
        && (!GameObject.Find("Mobile") || (GameObject.Find("Mobile") && !Mobile.selectionModeMove)))
        {
            mouseDownPos = Input.mousePosition;
            isDown = true;
            visible = true;
        }

        // Continue tracking mouse position until mouse button is up
        if (isDown)
        {
            mouseLastPos = Input.mousePosition;
            //if you release mouse button, remove rectangle and stop tracking
            if (Input.GetMouseButtonUp(0))
            {
                isDown = false;
                visible = false;
            }
        }

        //find and store all selectable objects (objects in scene with knight tag)
        knights = GameObject.FindGameObjectsWithTag("Player");

        //if player presses d, deselect all characters
        if (Input.GetKey("x"))
        {
            foreach (GameObject Knight in knights)
            {
                if (Knight != null)
                {
                    Knight.GetComponent<UnitBase>().selected = false;
                }
            }
        }

        //start selection mode when player presses spacebar
        if (Input.GetKeyDown("space"))
        {
            selectCharacters();
        }

        //for each button, check if we have enough gold to deploy the unit and color the button grey if it can not be deployed yet
        for (int i = 0; i < playerUnits.Count; i++)
        {
            if (playerUnits[i].troopCosts <= gold)
            {
                playerUnits[i].button.gameObject.GetComponent<Image>().color = Color.white;
            }
            else
            {
                playerUnits[i].button.gameObject.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f, 1);
            }
        }
    }

    void OnGUI()
    {
        //check if rectangle should be visible
        if (visible)
        {
            // Find the corner of the box
            Vector2 origin;
            origin.x = Mathf.Min(mouseDownPos.x, mouseLastPos.x);

            // GUI and mouse coordinates are the opposite way around.
            origin.y = Mathf.Max(mouseDownPos.y, mouseLastPos.y);
            origin.y = Screen.height - origin.y;

            //Compute size of box
            Vector2 size = mouseDownPos - mouseLastPos;
            size.x = Mathf.Abs(size.x);
            size.y = Mathf.Abs(size.y);

            // Draw the GUI box
            Rect rect = new Rect(origin.x, origin.y, size.x, size.y);
            GUI.Box(rect, "", rectangleStyle);

            foreach (GameObject Knight in knights)
            {
                if (Knight != null)
                {
                    Vector3 pos = Camera.main.WorldToScreenPoint(Knight.transform.position);
                    pos.y = Screen.height - pos.y;
                    //foreach selectable character check its position and if it is within GUI rectangle, set selected to true
                    if (rect.Contains(pos))
                    {
                        Knight.GetComponent<UnitBase>().selected = true;
                    }
                }
            }
        }
    }

    //function to select another unit
    public void selectUnit(int unit)
    {
        if (highlightSelectedButton)
        {
            //remove all outlines and set the current button outline visible
            for (int i = 0; i < playerUnits.Count; i++)
            {
                playerUnits[i].button.GetComponent<Outline>().enabled = false;
            }
            EventSystem.current.currentSelectedGameObject.GetComponent<Outline>().enabled = true;
        }
        //selected unit is the pressed button
        selectedUnit = unit;
    }

    public void selectCharacters()
    {
        //turn selection mode on/off
        selectionMode = !selectionMode;
        if (selectionMode)
        {
            //set cursor and button color to show the player selection mode is active
            // selectButton.GetComponent<Image>().color = Color.red;
            Cursor.SetCursor(cursorTexture1, Vector2.zero, CursorMode.Auto);
            if (GameObject.Find("Mobile"))
            {
                if (Mobile.deployMode)
                {
                    GameObject.Find("Mobile").GetComponent<Mobile>().toggleDeployMode();
                }
                Mobile.camEnabled = false;
            }
        }
        else
        {
            //show the player selection mode is not active
            // selectButton.GetComponent<Image>().color = Color.white;
            Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);

            //set target object false and deselect all units
            foreach (GameObject Knight in knights)
            {
                if (Knight != null)
                {
                    Knight.GetComponent<UnitBase>().selected = false;
                }
            }
            target.SetActive(false);
            if (GameObject.Find("Mobile"))
            {
                Mobile.camEnabled = true;
            }
        }
    }

    //warning if you need more gold
    IEnumerator GoldWarning()
    {
        if (!promptResourcesWarning.activeSelf)
        {
            promptResourcesWarning.SetActive(true);

            //wait for 2 seconds
            yield return new WaitForSeconds(2);
            promptResourcesWarning.SetActive(false);
        }
    }

    void addUnitsUIButtons()
    {
        //for all potential units...
        for (int i = 0; i < playerUnits.Count; i++)
        {

            //add a button to the list of buttons
            Button newButton = Instantiate(button);
            button.transform.SetAsLastSibling();

            RectTransform rectTransform = newButton.GetComponent<RectTransform>();
            rectTransform.SetParent(characterList.transform, false);

            //set button outline
            newButton.GetComponent<Outline>().effectColor = buttonHighlight;

            //set the correct button sprite
            newButton.gameObject.GetComponent<Image>().sprite = playerUnits[i].buttonImage;

            //only enable outline for the first button
            if (i == 0 && highlightSelectedButton)
            {
                newButton.GetComponent<Outline>().enabled = true;
            }
            else
            {
                newButton.GetComponent<Outline>().enabled = false;
            }

            //set button name to its position in the list(important for the button to work later on)
            newButton.transform.name = "" + i;

            //add a onclick function to the button with the name to select the proper unit
            newButton.GetComponent<Button>().onClick.AddListener(
            () =>
            {
                selectUnit(int.Parse(newButton.transform.name));
            }
            );

            //set the button stats
            newButton.GetComponentInChildren<Text>().text = "Unit Cost: " + playerUnits[i].troopCosts +
            "\n Damage: " + playerUnits[i].deployableTroops.GetComponentInChildren<UnitBase>().damage +
            "\n Health: " + playerUnits[i].deployableTroops.GetComponentInChildren<UnitBase>().healthFloat;

            //this is the new button
            playerUnits[i].button = newButton;
        }
    }

    public void placeability()
    {
        //start placing a ability
        isPlacingability = true;
    }

    //functions which adds 100 to your gold amount and shows text to let player know
    void AddGold()
    {
        gold += 100;
        StartCoroutine(AddedpromptResourcesCount());
    }

    IEnumerator AddedpromptResourcesCount()
    {
        promptResourcesAdded.SetActive(true);
        yield return new WaitForSeconds(0.7f);
        promptResourcesAdded.SetActive(false);
    }
}