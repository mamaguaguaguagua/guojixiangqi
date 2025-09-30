using System;
using System.Collections;
using System.Collections.Generic;
using ChessModel;
using UnityEngine;
using UnityEngine.UI;

public class EndGameUI : MonoBehaviour {
    
    public GameObject EndGameMenu;
    public MainMenuScpirt MenuScpirt;
    public BoardManager BoardManager;
    private Text _WinText;

    private void Start()
    {
        _WinText = GetComponentInChildren<Text>();
        EndGameMenu.SetActive(false);
    }

    public void EndGameNull()
    {
        EndGameMenu.SetActive(true);
        _WinText.text = "Æ½¾Ö £¡";
    }
    
    public void EndGameWin(ChessColor color)
    {
        EndGameMenu.SetActive(true);
        _WinText.text = color == ChessColor.White ? "°×Æå Ê¤!" : "ºÚÆå Ê¤!";
    }
    
    public void RestartGame()
    {
        BoardManager.RestartGame();
        EndGameMenu.SetActive(false);
    }
    
    public void BackToMenu()
    {
        EndGameMenu.SetActive(false);
        MenuScpirt.BackToMenu();
    }
}
