using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class Mobile : MonoBehaviour {
	
	private GameObject buttons;
	private GameObject switchSelectionModeButton;
	
	public static bool camEnabled = true;
	public static bool deployMode;
	public static bool selectionModeMove;
	
	//visible in the inspector
	public Sprite moveUnits;
	public Sprite selectUnits;


   

    private RtsCameraTouch _rtsCameraTouch;

    void Start(){
		//find gameobjects
		switchSelectionModeButton = GameObject.Find("Switch selection mode button");
		buttons = GameObject.Find("Buttons");
        
    }

    void Update(){
    	
		//turn the buttons of when any of the menu's is active
		if(!Manager.StartMenu.activeSelf && !Manager.victory && !Manager.gameOver){
			buttons.SetActive(true);
		}
		else{
			buttons.SetActive(false);
		}
		
		//Set mobile selection mode button active/not active
		if(UnitManager.selectionMode){
			switchSelectionModeButton.SetActive(true);
		}
		else{
			switchSelectionModeButton.SetActive(false);
		}














    }


    //toggle deploymode on/off
    public void moveCameraButtonDown()
    {

    }

    //toggle deploymode on/off
    public void moveCameraButtonUp()
    {

    }

    //toggle deploymode on/off
    public void moveCameraButtonLeft()
    {

    }

    //toggle deploymode on/off
    public void moveCameraButtonRight()
    {

    }



    //toggle deploymode on/off
    public void toggleDeployMode(){
		deployMode = !deployMode;
		if(deployMode){
			//set button color to red
			GameObject.Find("Deploy units button").GetComponent<Image>().color = Color.red;
			if(UnitManager.selectionMode){
				//switch selection mode off
				GameObject.Find("Manager").GetComponent<UnitManager>().selectCharacters();
			}
			//camera off
			camEnabled = false;
		}
		else{
			//set button color to white
			GameObject.Find("Deploy units button").GetComponent<Image>().color = Color.white;
			//camera on
			camEnabled = true;
		}
	}
	
	//switch between selection modes (box select & move units mode)
	public void switchSelectionMode(){
		selectionModeMove = !selectionModeMove;
		
		//change the sprite displayed
		if(selectionModeMove){
			switchSelectionModeButton.GetComponent<Image>().sprite = moveUnits;
		}
		else{
			switchSelectionModeButton.GetComponent<Image>().sprite = selectUnits;
		}
	}
}
