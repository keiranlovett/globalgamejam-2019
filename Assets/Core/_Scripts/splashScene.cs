using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class splashScene : MonoBehaviour
{

    public string scene;


    // Update is called once per frame
    void Update()
    {
        
    }

    // Update is called once per frame
    public void StartGame(string scene)
    {
        SceneManager.LoadScene(scene);
    }
}
