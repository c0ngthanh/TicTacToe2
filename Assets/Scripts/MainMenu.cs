using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void VsPeopleGame(){
        SceneManager.LoadScene("MultiGameScene");
    }
    public void QuitGame(){
        Application.Quit();
    }
    public void VsComGame(){
        SceneManager.LoadScene("VsComScene");
    }
}
