using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManagerAI : MonoBehaviour
{
    [SerializeField] private GameObject boardPrefab;
    private GameObject newBoard;
    public enum currentTurn {PlayerTurn,AiTurn};
    public static currentTurn TurnNow = currentTurn.PlayerTurn;
    [SerializeField] private GameObject gameEndPanel;
    [SerializeField] private TextMeshProUGUI msgText;
    public static GameManagerAI Instance;
    private void Awake()
    {
        if(Instance!=null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }
    public void ShowMsg(string msg)
    {
        if(msg.Equals("lose")){
            Debug.Log("Lose");
            msgText.text = "You Lose";
            gameEndPanel.SetActive(true);
        }
        else if (msg.Equals("won"))
        {
            Debug.Log("Won");
            msgText.text = "You Won";
            gameEndPanel.SetActive(true);
            // Show Panel with text that Opponent Won
            // ShowOpponentMsg("You Lose");
        }
        else if (msg.Equals("draw"))
        {
            msgText.text = "Game Draw";
            gameEndPanel.SetActive(true);
            // ShowOpponentMsg("Game Draw");
        }
    }

    public void Restart()
    {
            Destroy(newBoard);
            // SpwanBoard();
            // RestartClientRpc();
            gameEndPanel.SetActive(false);
    }
}
