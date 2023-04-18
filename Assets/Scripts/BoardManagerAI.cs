using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System;

public class BoardManagerAI : MonoBehaviour
{
    private const int BoardSize = 7;
    private const int WinningLength = 5;

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public Difficulty currentDifficulty = Difficulty.Medium;

    private int GetMaxDepth()
    {
        switch (currentDifficulty)
        {
            case Difficulty.Easy:
                return 2;
            case Difficulty.Medium:
                return 3;
            case Difficulty.Hard:
                return 5;
            default:
                return 3;
        }
    }


    Button[,] buttons = new Button[BoardSize, BoardSize];
    int[,] BoardState = new int[7,7]; // init = 0, Player =1, AI =2;
    [SerializeField] private Sprite xSprite,oSprite;
    // Start is called before the first frame update
    private void Awake() {
        Debug.Log("Awake Board AI");
        var cells = GetComponentsInChildren<Button>();
        int n=0;
        for(int i=0;i<7;i++){
            for (int j = 0; j< 7; j++)
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
            PrintBoard();

            (int r, int c) = FindBestMove();

            // Debug.Log("AI");
            buttons[r,c].GetComponent<Image>().sprite = oSprite;
            BoardState[r, c] = 2;
            buttons[r,c].interactable = false;

            // ChangSpriteClientRpc(r,c);
            if(CheckResult(r,c)){return;}
            GameManagerAI.TurnNow = GameManagerAI.currentTurn.PlayerTurn;
        }
    }


    private int Minimax(int[,] board, int depth, int maxDepth, bool isMax, int alpha, int beta)
    {
        if (depth == maxDepth)
        {
            return EvaluateHeuristic(board);
        }

        int score = EvaluateHeuristic(board);
        //  PrintHeuristicBoard(board);

        if (Math.Abs(score) >= 10000)
            return score;

        if (!IsMovesLeft(board))
            return 0;


        if (isMax)
        {
            int best = -1000000;

            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    if (board[i, j] == 0)
                    {
                        board[i, j] = 2;
                        best = Math.Max(best, Minimax(board, depth + 1, maxDepth, !isMax, alpha, beta));
                        board[i, j] = 0;

                        alpha = Math.Max(alpha, best);
                        if (beta <= alpha)
                        {
                            //PrintHeuristicBoard(board);
                            break;
                        }
                            
                    }
                }
            }
            return best;
        }
        else
        {
            int best = 1000000;

            for (int i = 0; i < BoardSize; i++)
            {
                for (int j = 0; j < BoardSize; j++)
                {
                    if (board[i, j] == 0)
                    {
                        board[i, j] = 1;
                        best = Math.Min(best, Minimax(board, depth + 1, maxDepth, !isMax, alpha, beta));
                        board[i, j] = 0;

                        beta = Math.Min(beta, best);
                        if (beta <= alpha)
                        {
                            //PrintHeuristicBoard(board);
                            break;
                        }
                    }
                }
            }
            return best;
        }
    }

    private int[,] CopyBoard(int[,] originalBoard)
    {
        int rows = originalBoard.GetLength(0);
        int cols = originalBoard.GetLength(1);
        int[,] copiedBoard = new int[rows, cols];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                copiedBoard[i, j] = originalBoard[i, j];
            }
        }
        return copiedBoard;
    }


    private (int, int) FindBestMove()
    {
        int maxDepth = GetMaxDepth();
        
        int[,] boardCopy = CopyBoard(BoardState);
        int bestVal = -1000000;
        int bestRow = -1;
        int bestCol = -1;
        int alpha = -1000000;
        int beta = 1000000;

        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                if (boardCopy[i, j] == 0)
                {
                    boardCopy[i, j] = 2;
                    //PrintHeuristicBoard(boardCopy);
                    int moveVal = Minimax(boardCopy, 0, maxDepth, false, alpha, beta);
                    //Debug.Log(moveVal);
                    //PrintHeuristicBoard(boardCopy);
                    boardCopy[i, j] = 0;

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

    private int EvaluateHeuristic(int[,] board)
    {
        int score = 0;

        // Evaluate rows
        for (int row = 0; row < BoardSize; row++)
        {
            for (int col = 0; col <= BoardSize - 5; col++)
            {
                int playerCount = 0;
                int aiCount = 0;

                for (int i = 0; i < 5; i++)
                {
                    if (board[row, col + i] == 1)
                        playerCount++;
                    else if (board[row, col + i] == 2)
                        aiCount++;
                }

                score += EvaluateLine(playerCount, aiCount);
            }
        }

        // Evaluate columns
        for (int col = 0; col < BoardSize; col++)
        {
            for (int row = 0; row <= BoardSize - 5; row++)
            {
                int playerCount = 0;
                int aiCount = 0;

                for (int i = 0; i < 5; i++)
                {
                    if (board[row + i, col] == 1)
                        playerCount++;
                    else if (board[row + i, col] == 2)
                        aiCount++;
                }

                score += EvaluateLine(playerCount, aiCount);
            }
        }

        // Evaluate diagonals
        for (int row = 0; row <= BoardSize - 5; row++)
        {
            for (int col = 4; col < BoardSize; col++)
            {
                int playerCount1 = 0;
                int aiCount1 = 0;
                int playerCount2 = 0;
                int aiCount2 = 0;

                for (int i = 0; i < 5; i++)
                {
                    // Main diagonal
                    if (board[row + i, col - i] == 1)
                        playerCount1++;
                    else if (board[row + i, col - i] == 2)
                        aiCount1++;

                    // Anti-diagonal
                    if (row + i < BoardSize && col + i < BoardSize)
                    {
                        if (board[row + i, col + i] == 1)
                            playerCount2++;
                        else if (board[row + i, col + i] == 2)
                            aiCount2++;
                    }
                }

                score += EvaluateLine(playerCount1, aiCount1);
                score += EvaluateLine(playerCount2, aiCount2);
            }
        }


        return score;
    }

    private int EvaluateLine(int playerCount, int aiCount)
    {
        int score = 0;

        if (playerCount == 0)
        {
            switch (aiCount)
            {
                case 1:
                    score += 10;
                    break;
                case 2:
                    score += 100;
                    break;
                case 3:
                    score += 1000;
                    break;
                case 4:
                    score += 50000;
                    break;
                case 5:
                    score += 100000;
                    break;
            }
        }
        else if (aiCount == 0)
        {
            switch (playerCount)
            {
                case 1:
                    score -= 20;
                    break;
                case 2:
                    score -= 150;
                    break;
                case 3:
                    score -= 15000;
                    break;
                case 4:
                    score -= 75000;
                    break;
                case 5:
                    score -= 100000;
                    break;
            }
        }
        else
        {
            // Mixed line situations
            if (playerCount == 1)
            {
                switch (aiCount)
                {
                    case 1:
                        score += 0;
                        break;
                    case 2:
                        score += 80;
                        break;
                    case 3:
                        score += 300;
                        break;
                    case 4:
                        score += 2000;
                        break;
                }
            }
            else if (playerCount == 2)
            {
                switch (aiCount)
                {
                    case 1:
                        score -= 100;
                        break;
                    case 2:
                        score -= 50;
                        break;
                    case 3:
                        score += 500;
                        break;
                }
            }
            else if (playerCount == 3)
            {
                switch (aiCount)
                {
                    case 1:
                        score -= 5000;
                        break;
                    case 2:
                        score -= 10000;
                        break;
                }
            }
            else if (playerCount == 4)
            {
                switch (aiCount)
                {
                    case 1:
                        score -= 20000;
                        break;
                }
            }
        }

        return score;
    }



    private bool IsMovesLeft(int[,] board)
    {
        for (int i = 0; i < BoardSize; i++)
            for (int j = 0; j < BoardSize; j++)
                if (board[i, j] == 0)
                    return true;
        return false;
    }

    public void PrintBoard()
    {
        string output = "Board State:\n";
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                output += BoardState[i, j].ToString() + " ";
            }
            output += "\n";
        }
        Debug.Log(output);
    }

    public void PrintHeuristicBoard(int[,] board)
    {
        int boardSize = board.GetLength(0);
        string output = "Board Heuristic State:\n";
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                output += board[i, j].ToString() + " ";
            }
            output += "\n";
        }
        Debug.Log(output);
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
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
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
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                buttons[i,j].interactable = false;
            }
        }
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
