﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public class DemoPanel
{
    public enum Difficulty { Easy, Normal, Hard, Flis};

    GameObject panel;
    TextMeshProUGUI difficultyText;
    Button easyButton;
    Button mediumButton;
    Button hardButton;
    Button flisButton;

    UnityAction<Difficulty> function;

    public DemoPanel(Transform panel, bool isFlisActive) 
    {
        this.panel = panel.gameObject;
        difficultyText = panel.Find("Difficulty Text").GetComponent<TextMeshProUGUI>();
        easyButton = panel.Find("Easy Button").GetComponent<Button>();
        mediumButton = panel.Find("Medium Button").GetComponent<Button>();
        hardButton = panel.Find("Hard Button").GetComponent<Button>();
        flisButton = panel.Find("FLIS Button").GetComponent<Button>();
        flisButton.gameObject.SetActive(isFlisActive);
        if (!isFlisActive) 
        {
            var localPos = hardButton.transform.localPosition;
            localPos.x = 0;
            hardButton.transform.localPosition = localPos;
        }
        easyButton.onClick.AddListener(() => function(Difficulty.Easy));
        mediumButton.onClick.AddListener(() => function(Difficulty.Normal));
        hardButton.onClick.AddListener(() => function(Difficulty.Hard));
        flisButton.onClick.AddListener(() => function(Difficulty.Flis));
        this.panel.SetActive(false);

        var textsToShow = TextReader.TextsToSet("Components/DemoPanel");

        difficultyText.text = textsToShow[0];
        Debug.Log(textsToShow[1]);
        easyButton.GetComponentInChildren<TextMeshProUGUI>().text = textsToShow[1];
        mediumButton.GetComponentInChildren<TextMeshProUGUI>().text = textsToShow[2];
        hardButton.GetComponentInChildren<TextMeshProUGUI>().text = textsToShow[3];
        flisButton.GetComponentInChildren<TextMeshProUGUI>().text = textsToShow[4];
    }

    public void ShowDemoPanel(UnityAction<Difficulty> action) 
    {
        Debug.Log("Show me");
        function = action;
        panel.SetActive(true);
    }
}
