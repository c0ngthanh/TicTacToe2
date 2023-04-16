using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System;

public class BoardManagerAI : MonoBehaviour
{
    Button[,] buttons = new Button[3,3];
    int[,] BoardState = new int[3,3]; // init = 0, Player =1, AI =2;
    [SerializeField] private Sprite xSprite,oSprite;
    // Start is called before the first frame update
    private void Awake() {
        var cells = GetComponentsInChildren<Button>();
        int n=0;
        for(int i=0;i<3;i++){
            for (int j = 0; j< 3; j++)
            {
                buttons[i,j] = cells[n];
                BoardState[i,j] = 0;
                n++;
                int r = i;
                int c = j;
                buttons[i,j].onClick.AddListener(delegate
                {
                    OnClickCell(r,c);
                });
            }
        }
    }

    // Update is called once per frame
    // void Update()
    // {
    //     for(int i=0;i<3;i++){
    //         for (int j = 0; j< 3; j++)
    //         {
    //             buttons[i,j].onClick.AddListener(delegate
    //             {
    //                 OnClickCell(i,j);
    //             });
    //         }
    //     }
    // }
    private void OnClickCell(int r, int c){
        // Debug.Log((r,c));
        if(GameManagerAI.TurnNow == GameManagerAI.currentTurn.PlayerTurn){
            // Debug.Log("Player");
            buttons[r,c].GetComponent<Image>().sprite = xSprite;
            BoardState[r,c] = 1;
            buttons[r,c].interactable = false;
            // ChangSpriteClientRpc(r,c);
            if(CheckResult(r,c)){return;}
            GameManagerAI.TurnNow = GameManagerAI.currentTurn.AiTurn;
            AiTurn();
        }
        // else if(GameManagerAI.TurnNow == GameManagerAI.currentTurn.AiTurn){
        //     // Debug.Log("AI");
        //     buttons[r,c].GetComponent<Image>().sprite = oSprite;
        //     buttons[r,c].interactable = false;
        //     // ChangSpriteClientRpc(r,c);
        //     CheckResult(r,c);
        //     GameManagerAI.TurnNow = GameManagerAI.currentTurn.PlayerTurn;
        // }
    }
    private void AiTurn(){
        // int r =0,c=0;
        if(GameManagerAI.TurnNow == GameManagerAI.currentTurn.AiTurn){
            (int r, int c) = FindBestMove();
            // Debug.Log("AI");
            buttons[r,c].GetComponent<Image>().sprite = oSprite;
            buttons[r,c].interactable = false;
            BoardState[r,c] = 2;
            // ChangSpriteClientRpc(r,c);
            if(CheckResult(r,c)){return;}
            GameManagerAI.TurnNow = GameManagerAI.currentTurn.PlayerTurn;
        }
    }
    private int EvaluateBoard()
    {
        for (int row = 0; row < 3; row++)
        {
            if (BoardState[row, 0] == BoardState[row, 1] && BoardState[row, 1] == BoardState[row, 2])
            {
                if (BoardState[row, 0] == 1)
                    return 10;
                else if (BoardState[row, 0] == 2)
                    return -10;
            }
        }

        for (int col = 0; col < 3; col++)
        {
            if (BoardState[0, col] == BoardState[1, col] && BoardState[1, col] == BoardState[2, col])
            {
                if (BoardState[0, col] == 1)
                    return 10;
                else if (BoardState[0, col] == 2)
                    return -10;
            }
        }

        if (BoardState[0, 0] == BoardState[1, 1] && BoardState[1, 1] == BoardState[2, 2])
        {
            if (BoardState[0, 0] == 1)
                return 10;
            else if (BoardState[0, 0] == 2)
                return -10;
        }

        if (BoardState[0, 2] == BoardState[1, 1] && BoardState[1, 1] == BoardState[2, 0])
        {
            if (BoardState[0, 2] == 1)
                return 10;
            else if (BoardState[0, 2] == 2)
                return -10;
        }

        return 0;
    }

    private bool IsMovesLeft()
    {
        for (int i = 0; i < 3; i++)
            for (int j = 0; j < 3; j++)
                if (BoardState[i, j] == 0)
                    return true;
        return false;
    }

    private int Minimax(int depth, bool isMax, int alpha, int beta)
{
     int score = EvaluateHeuristic();

    if (Math.Abs(score) == 2)
        return score;

    if (!IsMovesLeft())
        return 0;

    // Rest of the Minimax function remains the same

    if (isMax)
    {
        int best = -1000;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (BoardState[i, j] == 0)
                {
                    BoardState[i, j] = 2;
                    best = Math.Max(best, Minimax(depth + 1, !isMax, alpha, beta));
                    BoardState[i, j] = 0;

                    alpha = Math.Max(alpha, best);
                    if (beta <= alpha)
                        break;
                }
            }
        }
        return best;
    }
    else
    {
        int best = 1000;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                if (BoardState[i, j] == 0)
                {
                    BoardState[i, j] = 1;
                    best = Math.Min(best, Minimax(depth + 1, !isMax, alpha, beta));
                    BoardState[i, j] = 0;

                    beta = Math.Min(beta, best);
                    if (beta <= alpha)
                        break;
                }
            }
        }
        return best;
    }
}

    private (int, int) FindBestMove()
{
    int bestVal = -1000;
    int bestRow = -1;
    int bestCol = -1;
    int alpha = -1000;
    int beta = 1000;

    for (int i = 0; i < 3; i++)
    {
        for (int j = 0; j < 3; j++)
        {
            if (BoardState[i, j] == 0)
            {
                BoardState[i, j] = 2;
                int moveVal = Minimax(0, false, alpha, beta);
                BoardState[i, j] = 0;

                if (moveVal > bestVal)
                {
                    bestRow = i;
                    bestCol = j;
                    bestVal = moveVal;
                }
            }
        }
    }

    return (bestRow, bestCol);
}
    public bool IsWon(int r, int c)
    {
        Sprite clickedButtonSprite = buttons[r, c].GetComponent<Image>().sprite;
        // Checking Column
        if (buttons[0, c].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[1, c].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[2, c].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        // Checking Row

        else if (buttons[r, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[r, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[r, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        // Checking First Diagonal

        else if (buttons[0, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[1, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
            buttons[2, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        // Checking 2nd Diagonal
        else if (buttons[0, 2].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        buttons[1, 1].GetComponentInChildren<Image>().sprite == clickedButtonSprite &&
        buttons[2, 0].GetComponentInChildren<Image>().sprite == clickedButtonSprite)
        {
            return true;
        }

        return false;
    }
    private bool IsGameDraw()
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
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
    private void TurnOffBoard(){
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                buttons[i,j].interactable = false;
            }
        }
    }
    private int EvaluateHeuristic()
{
    int score = 0;

    // Evaluate rows
    for (int row = 0; row < 3; row++)
    {
        int playerCount = 0;
        int aiCount = 0;

        for (int col = 0; col < 3; col++)
        {
            if (BoardState[row, col] == 1)
                playerCount++;
            else if (BoardState[row, col] == 2)
                aiCount++;
        }

        if (playerCount == 0)
            score += aiCount;
        else if (aiCount == 0)
            score -= playerCount;
    }

    // Evaluate columns
    for (int col = 0; col < 3; col++)
    {
        int playerCount = 0;
        int aiCount = 0;

        for (int row = 0; row < 3; row++)
        {
            if (BoardState[row, col] == 1)
                playerCount++;
            else if (BoardState[row, col] == 2)
                aiCount++;
        }

        if (playerCount == 0)
            score += aiCount;
        else if (aiCount == 0)
            score -= playerCount;
    }

    // Evaluate diagonals
    int[] playerDiagonalCounts = new int[2] { 0, 0 };
    int[] aiDiagonalCounts = new int[2] { 0, 0 };

    for (int i = 0; i < 3; i++)
    {
        if (BoardState[i, i] == 1)
            playerDiagonalCounts[0]++;
        else if (BoardState[i, i] == 2)
            aiDiagonalCounts[0]++;

        if (BoardState[i, 2 - i] == 1)
            playerDiagonalCounts[1]++;
        else if (BoardState[i, 2 - i] == 2)
            aiDiagonalCounts[1]++;
    }

    for (int i = 0; i < 2; i++)
    {
        if (playerDiagonalCounts[i] == 0)
            score += aiDiagonalCounts[i];
        else if (aiDiagonalCounts[i] == 0)
            score -= playerDiagonalCounts[i];
    }

    return score;
}
    private bool CheckResult(int r,int c){
        if(IsWon(r,c) && GameManagerAI.TurnNow == GameManagerAI.currentTurn.AiTurn){
            // Debug.Log("AI won");
            TurnOffBoard();
            GameManagerAI.Instance.ShowMsg("lose");
            return true;
        }else if(IsWon(r,c) && GameManagerAI.TurnNow == GameManagerAI.currentTurn.PlayerTurn){
            TurnOffBoard();
            GameManagerAI.Instance.ShowMsg("won");
            // Debug.Log("Player won");
            return true;
        }else{
            if(IsGameDraw()){
                TurnOffBoard();
                GameManagerAI.Instance.ShowMsg("draw");
                // Debug.Log("draw");
            return true;
            }
        }
        return false;
    }
}
