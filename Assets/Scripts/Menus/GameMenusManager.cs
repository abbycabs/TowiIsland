﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameMenusManager : MonoBehaviour {

    //These are the UI objects needed for this part
    public Button gamesButton;
    public Button avatarButton;
    public Button shopButton;
    public Button dificultyButton;
    public Button goBackButton;

    public AudioClip[] Clips;

    public GameObject normalCanvas;
    public GameObject miniShopGame;

    AudioPlayerForMenus player;
    GameCenterCamera centerCamera;
    SessionManager sessionManager;

	// Use this for initialization
	void Start ()
    {
        sessionManager = FindObjectOfType<SessionManager>();
        gamesButton.transform.parent.gameObject.SetActive(false);
        player = FindObjectOfType<AudioPlayerForMenus>();
        player.GetComponent<AudioSource>().clip = Clips[0];
        player.GetComponent<AudioSource>().Play();
        centerCamera = FindObjectOfType<GameCenterCamera>();
        gamesButton.onClick.AddListener(GoGamingZone);
        goBackButton.onClick.AddListener(GoBackToLogin);
        avatarButton.onClick.AddListener(GoAvatars);
        shopButton.onClick.AddListener(GoShoping);
        if (sessionManager.activeKid.needSync)
        {
            sessionManager.UpdateProfile();
        }
        centerCamera.StartTheThingNow();
        ReturnToNormality();
    }

    public void FinishAnim()
    {
        gamesButton.transform.parent.gameObject.SetActive(true);
        player.GetComponent<AudioSource>().clip = Clips[1];
        player.GetComponent<AudioSource>().Play();
    }

	// Update is called once per frame
	void Update () {
		
	}

    void GoGamingZone()
    {
        DontDestroyOnLoad(player);
        PrefsKeys.SetNextScene("GameCenter");
        SceneManager.LoadScene("Loader_Scene");
    }
    
    void GoBackToLogin()
    {
        Destroy(player.gameObject);
        PrefsKeys.SetNextScene("NewLogin");
        SceneManager.LoadScene("Loader_Scene");
    }

    void GoAvatars()
    {
        PrefsKeys.SetNextScene("Avatar_Selection");
        SceneManager.LoadScene("Loader_Scene");
    }

    //This is used top start the shoping minigame
    void GoShoping()
    {
        normalCanvas.SetActive(false);
        centerCamera.gameObject.SetActive(false);
        miniShopGame.SetActive(true);
    }

    //This is used to return to normality
    public void ReturnToNormality()
    {
        normalCanvas.SetActive(true);
        centerCamera.gameObject.SetActive(true);
        miniShopGame.SetActive(false);
    }
}