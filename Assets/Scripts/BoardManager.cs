using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class BoardManager : NetworkBehaviour
{
    Button[,] buttons = new Button[7,7];
    public override void OnNetworkSpawn()
    {
        var cells = GetComponentsInChildren<Button>();
        int n = 0;
        for(int i=0; i < 7; i++)
        {
            for(int j = 0; j < 7; j++)
            {
                buttons[i, j] = cells[n];
                n++;

                int r = i;
                int c = j;

                buttons[i, j].onClick.AddListener(delegate
                {
                    OnClickCell(r, c);
                });
            }
        }

    }


    [SerializeField] private Sprite xSprite, oSprite;
    private void OnClickCell(int r, int c)
    {
        // If button clicked by host, then change button sprite as X

        if (NetworkManager.Singleton.IsHost && GameManager.Instance.currentTurn.Value==0)
        {
            buttons[r, c].GetComponent<Image>().sprite = xSprite;
            buttons[r, c].interactable = false;
            // Also change on Client side
            ChangeSpriteClientRpc(r, c);
            CheckResult(r, c);
            GameManager.Instance.currentTurn.Value = 1;
        }

        // If button is clicked by client, then change button sprite as O

        else if (!NetworkManager.Singleton.IsHost && GameManager.Instance.currentTurn.Value == 1)
        {
            buttons[r, c].GetComponent<Image>().sprite = oSprite;
            buttons[r, c].interactable = false;
            CheckResult(r, c);
            // Also change on host side
            ChangeSpriteServerRpc(r,c);
        }

        // Make the button non interactable after clicked once
            

    }


    [ClientRpc]
    private void ChangeSpriteClientRpc(int r, int c)
    {
        buttons[r, c].GetComponent<Image>().sprite = xSprite;
        buttons[r, c].interactable = false;

    }

    [ServerRpc(RequireOwnership = false)]
    private void ChangeSpriteServerRpc(int r, int c)
    {
        buttons[r, c].GetComponent<Image>().sprite = oSprite;
        buttons[r, c].interactable = false;
        GameManager.Instance.currentTurn.Value = 0;

    }


    
    private void CheckResult(int r, int c)
    {
        if (IsWon(r, c))
        {
            GameManager.Instance.ShowMsg("won");
        }
        else
        {
            if (IsGameDraw())
            {
                GameManager.Instance.ShowMsg("draw");
            }
        }
    }





    public bool IsWon(int r, int c)
    {
        Sprite clickedButtonSprite = buttons[r, c].GetComponent<Image>().sprite;
        // Check Column
        int RowCount = 0;
        int ColumnCount = 0;
        int LeftCount = 0;
        int RightCount = 0;
        for(int i = r+1;i<7;i++){
            if(buttons[i, c].GetComponentInChildren<Image>().sprite != clickedButtonSprite){
                break;
            }
            ColumnCount++;
        }
        for(int i = r-1;i>=0;i--){
            if(buttons[i, c].GetComponentInChildren<Image>().sprite != clickedButtonSprite){
                break;
            }
            ColumnCount++;
        }
        if(ColumnCount+1==5){
            return true;
        }
        // Check Row
        for(int i = c+1;i<7;i++){
            if(buttons[r, i].GetComponentInChildren<Image>().sprite != clickedButtonSprite){
                break;
            }
            RowCount++;
        }
        for(int i = c-1;i>=0;i--){
            if(buttons[r, i].GetComponentInChildren<Image>().sprite != clickedButtonSprite){
                break;
            }
            RowCount++;
        }
        if(RowCount+1==5){
            return true;
        }
        // Check Left Diagonal
        for(int i = c+1,j=r+1;i<7 && j < 7;i++,j++){
            if(buttons[j, i].GetComponentInChildren<Image>().sprite != clickedButtonSprite){
                break;
            }
            LeftCount++;
        }
        for(int i = c-1,j=r-1;i>=0 && j >=0;i--,j--){
            if(buttons[j, i].GetComponentInChildren<Image>().sprite != clickedButtonSprite){
                break;
            }
            LeftCount++;
        }
        if(LeftCount+1==5){
            return true;
        }
        for(int i = c+1,j=r-1;i<7 && j >=0;i++,j--){
            if(buttons[j, i].GetComponentInChildren<Image>().sprite != clickedButtonSprite){
                break;
            }
            RightCount++;
        }
        for(int i = c-1,j=r+1;i>=0 && j<7;i--,j++){
            if(buttons[j, i].GetComponentInChildren<Image>().sprite != clickedButtonSprite){
                break;
            }
            RightCount++;
        }
        if(RightCount+1==5){
            return true;
        }
        // // Checking Column
        // if (buttons[0, c].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        //     buttons[1, c].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        //     buttons[2, c].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        // {
        //     return true;
        // }

        // // Checking Row

        // else if (buttons[r, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        //     buttons[r, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        //     buttons[r, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        // {
        //     return true;
        // }

        // // Checking First Diagonal

        // else if (buttons[0, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        //     buttons[1, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        //     buttons[2, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        // {
        //     return true;
        // }

        // // Checking 2nd Diagonal
        // else if (buttons[0, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        // buttons[1, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        // buttons[2, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        // {
        //     return true;
        // }

        return false;
    }


    private bool IsGameDraw()
    {
        for (int i = 0; i < 7; i++)
        {
            for (int j = 0; j < 7; j++)
            {
                if (buttons[i, j].GetComponent<Image>().sprite != xSprite &&
                    buttons[i, j].GetComponent<Image>().sprite != oSprite)
                {
                    return false;
                }
            }
        }
        return true;
    }
}
