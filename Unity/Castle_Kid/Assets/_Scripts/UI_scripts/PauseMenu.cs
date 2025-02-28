using Unity.Netcode;
using UnityEngine;
using TMPro;
public class PauseMenu : MonoBehaviour
{
    //VARIABLES
    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------

    //Variables for the different panels
    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;
    public GameObject multiplayerMenuUI;

    public TMP_InputField HostInputField;
    public TMP_InputField JoinInputField;

    public static bool isPaused; //To check if the game is paused or not


    //FUNCTIONS
    //-------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------

    void Pause()
    {
        //Debug.Log("Pause Pressed");
        pauseMenuUI.SetActive(true);
        isPaused = true;
    }
    

    public void Resume()
    {
        //Debug.Log("Resume Pressed");
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        multiplayerMenuUI.SetActive(false);
        isPaused = false;
    }

    public void Multiplayer()
    {
        Debug.Log("Multiplayer Pressed");
        SetPanel(multiplayerMenuUI);
    }

    public void Host()
    {
        Debug.Log("Host Pressed");
        Debug.Log($"Input: {HostInputField.text}");
        HostInputField.text = "";
        
    }

    public void Join()
    {
        Debug.Log("Join Pressed");
        Debug.Log($"Input: {JoinInputField.text}");
        JoinInputField.text = "";
    }

    public void Options()
    {
        Debug.Log("Options Pressed");
        SetPanel(optionsMenuUI);
    }

    public void Back()
    {
        Debug.Log("Back Pressed");
        SetPanel(pauseMenuUI);
    }

    public void Quit()
    {
        Debug.Log("Quit Pressed");
        Application.Quit(); //Doesn't do anything in the Unity editor
    }



    private void SetPanel(GameObject MenuUI)
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        multiplayerMenuUI.SetActive(false);

        MenuUI.SetActive(true);
    }
    
    //START AND UPDATE
    //-------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        multiplayerMenuUI.SetActive(false);
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
