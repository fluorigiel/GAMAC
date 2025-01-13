using UnityEngine;
using Unity.Netcode;

public class PauseMenu : NetworkBehaviour
{
    //VARIABLES
    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    public GameObject pauseMenu;
    public static bool isPaused;


    //FUNCTIONS
    //-------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------

    private void Pause()
    {
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    private void Resume()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
    }



    //START AND UPDATE
    //-------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pauseMenu.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {

        Debug.Log(InputManager.PauseMenuWasPressed);

        if (InputManager.PauseMenuWasPressed)
        {
            Debug.Log("Game Paused");
            if (isPaused)
                Resume();

            else
                Pause();
        }



    }
}
