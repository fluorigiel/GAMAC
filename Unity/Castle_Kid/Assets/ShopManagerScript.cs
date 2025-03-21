using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ShopManagerScript : MonoBehaviour
{
    public int[,] shopItems = new int[13,13];
    public float coins; //remplacer avec le systeme de configuration d'economie du jeu
    public Text CoinsTxt;
    
    
    void Start()
    {
        CoinsTxt.text = "Écus: " + coins.ToString();
        //ItemIDs
        shopItems[1, 1] = 1;
        shopItems[1, 2] = 2;
        shopItems[1, 3] = 3;
        shopItems[1, 4] = 4;
        shopItems[1, 5] = 5;
        shopItems[1, 6] = 6;
        shopItems[1, 7] = 7;
        shopItems[1, 8] = 8;
        shopItems[1, 9] = 9;
        shopItems[1, 10] = 10;
        shopItems[1, 11] = 11;
        shopItems[1, 12] = 12;
        
        //Price
        shopItems[2, 1] = 10;
        shopItems[2, 2] = 20;
        shopItems[2, 3] = 30;
        shopItems[2, 4] = 40;
        shopItems[2, 5] = 50;
        shopItems[2, 6] = 60;
        shopItems[2, 7] = 70;
        shopItems[2, 8] = 80;
        shopItems[2, 9] = 90;
        shopItems[2, 10] = 100;
        shopItems[2, 11] = 110;
        shopItems[2, 12] = 120;
        
        //Quantity
        shopItems[3, 1] = 10;
        shopItems[3, 2] = 0;
        shopItems[3, 3] = 0;
        shopItems[3, 4] = 0;
        shopItems[3, 5] = 0;
        shopItems[3, 6] = 0;
        shopItems[3, 7] = 0;
        shopItems[3, 8] = 0;
        shopItems[3, 9] = 0;
        shopItems[3, 10] = 0;
        shopItems[3, 11] = 0;
        shopItems[3, 12] = 0;
    }

    public void Buy()
    {
        GameObject ButtonRef = GameObject.FindGameObjectWithTag("Event").GetComponent<EventSystem>().currentSelectedGameObject;

        if (coins >= shopItems[2, ButtonRef.GetComponent<ButtonInfo>().ItemID])
        {

            coins -= shopItems[2, ButtonRef.GetComponent<ButtonInfo>().ItemID];
            shopItems[3, ButtonRef.GetComponent<ButtonInfo>().ItemID]++;
            CoinsTxt.text = "Écus: " + coins.ToString();
            ButtonRef.GetComponent<ButtonInfo>().QuantityTxt.text = shopItems[3, ButtonRef.GetComponent<ButtonInfo>().ItemID].ToString();
            
        }
    }
}
