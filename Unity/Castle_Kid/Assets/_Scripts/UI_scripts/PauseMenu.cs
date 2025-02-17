using Unity.Netcode;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    //VARIABLES
    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;

    public static bool isPaused;


    //FUNCTIONS
    //-------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------

    void Pause()
    {
        //Debug.Log("Pause Pressed");
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }
    

    public void Resume()
    {
        //Debug.Log("Resume Pressed");
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }

    public void Host()
    {
        Debug.Log("Host Pressed");
        NetworkManager.Singleton.StartHost();
    }

    public void Join()
    {
        Debug.Log("Join Pressed");
        NetworkManager.Singleton.StartClient();
    }

    public void Options()
    {
        Debug.Log("Options Pressed");
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void Back()
    {
        Debug.Log("Back Pressed");
        pauseMenuUI.SetActive(true);
        optionsMenuUI.SetActive(false);
    }

    public void Quit()
    {
        Debug.Log("Quit Pressed");
        Application.Quit(); //Doesn't do anything in the Unity editor
    }

    //START AND UPDATE
    //-------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (InputManager.PauseMenuWasPressed)
        {
            if (isPaused)
                Resume();

            else
                Pause();
        }

    }
}
