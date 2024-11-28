using UnityEngine;
using UnityEngine.UI;


public class UI : MonoBehaviour
{
    //VARIABLES
    //----------------------------------------------------------------------------------
    //Variables for Hearts  
    public int pv; //Health of the player 1 heart = 2 pv
    public int nbOfHearts; //Number of hearts that the player has

    public Image[] hearts;    //Array to store all the different hearts of the player (3 hearts -> hearts.Length == 3)
    public Sprite fullHeart;  //Sprite for a full heart
    public Sprite halfHeart;  //Sprite for half a heart
    public Sprite emptyHeart; //Sprite for empty heart

    //Variables for Coins
    //public Image coin; //To get the image of the coin


    //FUNCTIONS
    //-------------------------------------------------------------------------------

    //To write the current health of the player in the console (For debug purposes)
    private void PrintPV() 
    {
        Debug.Log(pv);
    }


    //To change the health of the player by taking a certain amount of damage
    private void LoseHealth(int damage)
    {
        if (damage > pv) pv = 0;

        else if (damage < 0) damage = 0;

        else pv -= damage;
    }

    //To constantly update the sprites in the UI for the hearts 
    private void HeartBar()
    {
        if (pv > nbOfHearts * 2)
        {
            pv = nbOfHearts * 2;
        }

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



    //START AND UPDATE
    //-------------------------------------------------------------------------------------

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        PrintPV();
        //coin.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        HeartBar();
    }
}
