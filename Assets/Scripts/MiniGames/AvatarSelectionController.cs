﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class AvatarSelectionController : MonoBehaviour {

    [Header("UI Elements")]
    public GameObject instructionPanel;
    public TextMeshProUGUI instructionText;
    public Button noButton;
    public Button yesButton;
	public Button returnButton;

    [Header("Text Elements")]
    public TextAsset textAsset;
    string[] stringToShow;

    public GameObject spotLight;

    bool isSomeOneSelected;
    bool isSomeOneMoving;

    AvatarToSelect selectedChacracter;
    SessionManager sessionManager;

	// Use this for initialization
	void Start ()
    {
        sessionManager = FindObjectOfType<SessionManager>();
        textAsset = Resources.Load<TextAsset>($"{LanguagePicker.BasicTextRoute()}MiniGames/AvatarSelection");
        stringToShow = TextReader.TextsToShow(textAsset);
        instructionText.text = stringToShow[0];
        noButton.onClick.AddListener(RegretSelection);
        yesButton.onClick.AddListener(SetTheCorrectCharacter);
        noButton.gameObject.SetActive(false);
        yesButton.gameObject.SetActive(false);
		returnButton.onClick.AddListener(GoBackMenu);
	}
	
	// Update is called once per frame
	void Update ()
    {
		
	}

    public void MoveSpotLight(Vector3 positionOfSpotLight)
    {
        positionOfSpotLight.y = spotLight.transform.position.y;
        spotLight.transform.position = positionOfSpotLight;
    }

    public void ClearText()
    {
        instructionPanel.SetActive(false);
    }

    public bool CanSelect()
    {
        if (isSomeOneSelected || isSomeOneMoving)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void SomeOneIsSelected(AvatarToSelect selected)
    {
        selectedChacracter = selected;
        isSomeOneSelected = true;
    }

    void RegretSelection()
    {
        isSomeOneSelected = false;
        selectedChacracter.GoBack();
        selectedChacracter = null;
        noButton.gameObject.SetActive(false);
        yesButton.gameObject.SetActive(false);
    }

    public void SomeOneIsMoving()
    {
        isSomeOneMoving = true;
    }

    public void StopMoving()
    {
        isSomeOneMoving = false;
    }

    public void AskIfItsAllRight()
    {
        instructionText.text = stringToShow[1];
        noButton.gameObject.SetActive(true);
        yesButton.gameObject.SetActive(true);
        instructionPanel.SetActive(true);
    }

    public void AskForTheCharacter()
    {
        instructionText.text = stringToShow[0];
        instructionPanel.SetActive(true);
    }

    void SetTheCorrectCharacter()
    {
        sessionManager.activeKid.avatar = selectedChacracter.name.ToLower();
        sessionManager.activeKid.needSync = true;
        PrefsKeys.SetNextScene("GameMenus");
        SceneManager.LoadScene("Loader_Scene");
    }

	void GoBackMenu()
	{
		PrefsKeys.SetNextScene("GameMenus");
        SceneManager.LoadScene("Loader_Scene");
	}
}
