using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class UI : MonoBehaviour
{
    //VARIABLES
    //----------------------------------------------------------------------------------
    //----------------------------------------------------------------------------------
    //Variables for Hearts  
    public int pv; //Health of the player 1 heart = 2 pv
    public int nbOfHearts; //Number of hearts that the player has

    public Image[] hearts;    //Array to store all the different hearts of the player (3 hearts -> hearts.Length == 3)
    public Sprite fullHeart;  //Sprite for a full heart
    public Sprite halfHeart;  //Sprite for half a heart
    public Sprite emptyHeart; //Sprite for empty heart

    //Variables for Coins
    public int coinCounter;           //Keeps track of the number of coins the player has
    public TextMeshProUGUI coinText;  //Text showing the number of coins currently held

    //Variables for PowerFrames
    public Image[] powerImages;  //To store the two power frames at the bottom right of the UI

    public Sprite noPower;  //Sprite for an empty frame
    public Sprite bat;      //Sprite for the frame with the bat power
    public Sprite shield;   //Sprite for the frame with the shield power

    //For the two active powers
    private Sprite power0;  //Sprite shown on the bottom right corner and shows the first active power (the left one)
    private Sprite power1;  //Sprite shown on the bottom right corner and shows the second active power (the right one)

    //FUNCTIONS
    //-------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------


    //For UI of hearts
    //-------------------------------------------------------------------------------

    //To constantly update the sprites in the UI for the hearts 
    private void HeartBar()
    {
        if (pv > nbOfHearts * 2) pv = nbOfHearts * 2;

        else if (pv < 0) pv = 0;

        for (int i = 0; i < hearts.Length; i++)
        {
            if (i * 2 + 1 < pv)
                hearts[i].sprite = fullHeart;
            else if (i * 2 < pv)
                hearts[i].sprite = halfHeart;
            else
                hearts[i].sprite = emptyHeart;


            if (i < nbOfHearts)
                hearts[i].enabled = true;
            else
                hearts[i].enabled = false;
        }
    }


    //To write the current health of the player in the console (For debug purposes)
    private void PrintPV() 
    {
        Debug.Log(pv);
    }

    private void GainHealth(int amount)
    {
        pv += amount;
    }

    //To change the health of the player by taking a certain amount of damage
    private void LoseHealth(int amount)
    {
        pv -= amount;
    }


    //For UI of Coin
    //-------------------------------------------------------------------------------
    //To constantly update the sprites in the UI for the coin counter 
    private void CoinUI()
    {
        if (coinCounter < 0) coinCounter = 0;
        else if (coinCounter > 999) coinCounter = 999;

        coinText.text = coinCounter.ToString();
    }


    //Functions for gaining and losing coins
    private void GainCoin(int amount)
    {
        coinCounter += amount;
    }

    private void LoseCoin(int amount)
    {
        coinCounter -= amount;
    }

    //For UI of Power Frames
    //-------------------------------------------------------------------------------
    //To constantly update the sprites in the UI for the coin counter 
    private void PowerFrames()
    {
        powerImages[0].sprite = power0;
        powerImages[1].sprite = power1;
    }


    //To initialize both power frames to empty frames
    private void SetToNoPower()
    {
        power0 = noPower;
        power1 = noPower;
    }
       


    private void SetNewPower(string power)
    {
        switch (power)
        {
            case "bat":
                if (power0 == noPower)
                    power0 = bat;
                else
                    power1 = bat;
                break;

            case "shield":
                if (power0 == noPower)
                    power0 = shield;
                else
                    power1 = shield;
                break;

            default:
                power0 = noPower;
                power1 = noPower;
                break;
        }
    }


    //START AND UPDATE
    //-------------------------------------------------------------------------------------
    //-------------------------------------------------------------------------------------

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        SetToNoPower();
        //PrintPV();
        SetNewPower("bat");
        SetNewPower("shield");
    }

    // Update is called once per frame
    void Update()
    {
        HeartBar();
        CoinUI();
        PowerFrames();
    }

}