﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using DragonBones;

public class TableInstrument : Table
{

    public int retunerKindOfFood;
    protected List<int> ingredientsToInput = new List<int>();
    protected int numberOfIngridients;

    [System.NonSerialized]
    public bool thingsAreMade;
    public bool workingMachine = false;
    public bool thingGoodMade;

    protected Color colorToReturn;

    protected Tray trayToReturn;

    public UnityArmatureComponent armature;

    public AudioSource audioSource;

    // Use this for initialization
    void Start ()
    {
        Initializing();
	}

    public override void Initializing()
    {
        base.Initializing();
        ChangeTableSprite(FoodDicctionary.normalTable);
        ChangeTableColor();

        if (!GetComponent<AudioSource>())
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            audioSource = GetComponent<AudioSource>();
        }
    }

    public void SetAudioClip(string audioName)
    {
        Addressables.LoadAssetAsync<AudioClip>($"SFX/{audioName}").Completed += LoadAudioClip;
        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }


    void LoadAudioClip(UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<AudioClip> clip)
    {
        if (clip.Result == null)
        {
            Debug.LogError("This is Bad");
            return;
        }

        audioSource.clip = clip.Result;
        Debug.Log("We set the clip sucessfully");
    }

    // Update is called once per frame
    void Update ()
    {
		
	}
}
