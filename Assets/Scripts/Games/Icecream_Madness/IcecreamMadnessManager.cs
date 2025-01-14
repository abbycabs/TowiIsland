﻿using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using TMPro;

public class IcecreamMadnessManager : MonoBehaviour {

    public GameObject TableCenterManager;
    public GameObject Canvas;
    public GameObject uiCanvas;

    PauseTheGame pauseManager;

    GameObject ordersPanel;
    GameObject instructionPanel;

    IcecreamChef chef;

    readonly List<int> possibleToppings = new List<int> { 4, 5, 6, 7, 8 };
    List<int> kindOfMachines = new List<int>();
    List<int> kindOfCookedMeals = new List<int>();
    List<int> kindOfContainer = new List<int>();
    List<int> kindOfIngredients = new List<int>();

    List<int> availableToppings = new List<int>();
    List<int> recepiesToShow = new List<int>();
    List<IceCreamOrders> ordersList = new List<IceCreamOrders>();
    List<float> timesOfOrder = new List<float>();

    List<GameObject> fullTables = new List<GameObject>();
    List<string> tableNames = new List<string>();
    List<string> machineTableNames = new List<string>();

    List<int> playedLevels = new List<int>();

    Dictionary<string, int> tableMapPositions = new Dictionary<string, int>();

    readonly Dictionary<string, string> plusCornerPositions = new Dictionary<string, string> { { "C1", "U1" }, { "C2", "R1" }, { "C3", "D1" }, { "C4", "L1" } };
    readonly Dictionary<string, string> lessCornerPositions = new Dictionary<string, string> { { "C1", "L5" }, { "C2", "U7" }, { "C3", "R5" }, { "C4", "D7" } };
    readonly List<int> levelsTutorials = new List<int> { 0, 10, 20 };

    IcecreamUI icecreamUI;
    TutorialUI tutorialUI;

    ParticleSystem confettiSystem;
    ParticleSystem crossesSystem;

    AudioClip cashClip;
    AudioClip wrongClip;

    const string cashClipDir = "SFX/Cash";
    const string wrongClipDir = "SFX/Fail";

    AudioSource audioSource;

    int maxAmountOfOrdersAtTheSameTime = 5;

    int amountOfTopings = 5;
    int amoutOfChoppers = 1;

    const int amountOfTrashPlaces = 1;

    int currentOrders;

    int currentEssay;
    const int maxNumberOfEssay = 4;

    List<int> wellMade;
    List<int> badMade;
    List<int> trashOrders;
    List<int> ordersAsked;
    List<int> ordersDelivered;
    List<int> ordersMissed;
    List<int> ordersMade;
    List<int> tipsEarned;
    List<int> ordersWithTips;
    List<int> totalScores;

    List<float> latencies;

    int level = 0;
    const int maxLevelNumber = 33;

    int passLevels;
    int repeatedLevels;

    int targetCoins;

    const float essayTime = 180f;
    float currentEssayTime;
    float timeOfGame;

    float newOrderTiming = 20f;
    float currentNewOrderTiming;

    bool isGameTime = false;
    bool isAlreadyMove = false;
    bool isFLISTime = false;

    bool needTutorial;
    int kindOfTutorial;

    Slider levelSlider;
    Text valueText;

    #region Standart Region

    // Use this for initialization
    void Start()
    {
        GameParametersInitialization();
    }

    // Update is called once per frame
    void Update()
    {
        if (isGameTime)
        {
            timeOfGame += Time.deltaTime;
            NewOrderForTime();

            SetTimersOnTime();
        }
    }

    #endregion

    #region Initilizations
    /// <summary>
    /// This one start all the parameters that only has to be initialized one time per game session
    /// </summary>
    async void GameParametersInitialization()
    {
        pauseManager = FindObjectOfType<PauseTheGame>();

        TableInitilization();

        //Counters are initilized
        wellMade = new List<int>(new int[maxNumberOfEssay]);
        badMade = new List<int>(new int[maxNumberOfEssay]);
        trashOrders = new List<int>(new int[maxNumberOfEssay]);
        ordersAsked = new List<int>(new int[maxNumberOfEssay]);
        ordersDelivered = new List<int>(new int[maxNumberOfEssay]);
        ordersMissed = new List<int>(new int[maxNumberOfEssay]);
        ordersMade = new List<int>(new int[maxNumberOfEssay]);
        tipsEarned = new List<int>(new int[maxNumberOfEssay]);
        ordersWithTips = new List<int>(new int[maxNumberOfEssay]);
        totalScores = new List<int>(new int[maxNumberOfEssay]);
        latencies = new List<float>(new float[maxNumberOfEssay]);
        playedLevels = new List<int>(new int[maxNumberOfEssay]);

        //Especilieced Objects are initialized
        icecreamUI = new IcecreamUI(uiCanvas, this);

        if (FindObjectOfType<DemoKey>())
        {
            var demoKey = FindObjectOfType<DemoKey>();
            icecreamUI.ChooseChef();
            isFLISTime = false;
            switch (demoKey.GetDifficulty()) 
            {
                case DemoPanel.Difficulty.Easy:
                    level = 0;
                    break;
                case DemoPanel.Difficulty.Normal:
                    level = 10;
                    break;
                case DemoPanel.Difficulty.Hard:
                    level = 20;
                    break;
                case DemoPanel.Difficulty.Flis:
                    isFLISTime = true;
                    break;
            }
        }
        else
        {
            var manager = FindObjectOfType<SessionManager>();
            string character = "towi";
            switch (manager.activeKid.avatar)
            {
                case "jaguar":
                    character = "kali";
                    break;
                case "tortuga":
                    character = "tello";
                    break;
                case "mono":
                    character = "moki";
                    break;
                case "tucan":
                    character = "tuki";
                    break;
            }
            level = manager.activeKid.icecreamLevel;
            isFLISTime = manager.activeKid.firstsGames[6];
            SetChef(character);
            if (!isFLISTime)
            {
                AskForTutorial();
            }
        }

        if (isFLISTime)
        {
            level = 10;
        }

        ordersPanel = Canvas.transform.GetChild(0).gameObject;
        instructionPanel = Canvas.transform.GetChild(1).gameObject;

        //level = PlayerPrefs.GetInt("levIce");

        confettiSystem = GameObject.FindGameObjectWithTag("Arrow").GetComponent<ParticleSystem>();
        crossesSystem = GameObject.FindGameObjectWithTag("Ground").GetComponent<ParticleSystem>();

        audioSource = gameObject.AddComponent<AudioSource>();
        cashClip = await SetClip(cashClipDir);
        wrongClip = await SetClip(wrongClipDir);
    }

    async Task<AudioClip> SetClip(string dir)
    {
        var aucl = Addressables.LoadAssetAsync<AudioClip>(dir);
        while (!aucl.IsDone)
        {
            await Task.Yield();
        }

        return aucl.Result;
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

    void AskForTutorial()
    {
        pauseManager.WantTutorial();
        pauseManager.howToPlayButton.onClick.AddListener(icecreamUI.PrintTheStory);
        pauseManager.playButton.onClick.AddListener(icecreamUI.PrintAreYouReady);
    }

    /// <summary>
    /// This parameters has to be set in every essay of the game session
    /// </summary>
    public void EssayParameterInitilization()
    {
        currentEssayTime = essayTime;

        icecreamUI.PrintTheCurrentEarnings(GetEarnings());

        currentNewOrderTiming = newOrderTiming;

        SetLevelData();

        ClearTables();
    }

    public void SetChef(string chefName)
    {
        string pathOfChef = $"{FoodDicctionary.prefabGameObjectDirection}{FoodDicctionary.chefDirection}{chefName}";
        chef = Instantiate(Resources.Load<GameObject>(pathOfChef)).GetComponent<IcecreamChef>();
        icecreamUI.HideAllManagers();
        if (isFLISTime)
        {
            icecreamUI.PrintTheStory();
        }
        else
        {
            AskForTutorial();
        }
    }

    /// <summary>
    /// Obatain all the data need for the tables
    /// </summary>
    void TableInitilization()
    {
        for (int i = 0; i < TableCenterManager.transform.childCount; i++)
        {
            for (int j = 0; j < TableCenterManager.transform.GetChild(i).childCount; j++)
            {
                var gameObjectToDeal = TableCenterManager.transform.GetChild(i).GetChild(j).gameObject;
                fullTables.Add(gameObjectToDeal);
                tableNames.Add(gameObjectToDeal.name);
                tableMapPositions.Add(gameObjectToDeal.name, tableNames.Count - 1);
                if (gameObjectToDeal.name.Contains("U") || gameObjectToDeal.name.Contains("D"))
                {
                    machineTableNames.Add(gameObjectToDeal.name);
                }
            }
        }
    }

    /// <summary>
    /// We destroy all the previous table information to reset a new game
    /// </summary>
    void ClearTables()
    {
        for (int i = 0; i < fullTables.Count; i++)
        {
            if (fullTables[i].GetComponent<Table>())
            {
                fullTables[i].GetComponent<Table>().RestoreToOriginal();
                Destroy(fullTables[i].GetComponent<Table>());
            }

        }
    }

    void SetLevelData()
    {
        var levelConfigurator = GameConfigurator.GetIcecreamConfiguration(level);

        kindOfMachines.Clear();
        kindOfContainer.Clear();
        kindOfCookedMeals.Clear();
        kindOfMachines.Clear();
        kindOfIngredients.Clear();
        availableToppings.Clear();
        recepiesToShow.Clear();

        NeedsTutorial();

        amoutOfChoppers = levelConfigurator.amountOfChoppers;
        amountOfTopings = levelConfigurator.amountOfToppings;

        targetCoins = levelConfigurator.amountOfCoinsToPassLevel;

        maxAmountOfOrdersAtTheSameTime = levelConfigurator.maxNumberOfOrders;

        newOrderTiming = levelConfigurator.newOrderTiming;

        for (int i = 0; i < levelConfigurator.kindOfMeals.Count; i++)
        {
            int kindOfMeal = levelConfigurator.kindOfMeals[i];
            kindOfCookedMeals.Add(kindOfMeal);
            kindOfContainer.Add(kindOfMeal);
            switch (kindOfMeal)
            {
                case 0:
                    kindOfMachines.Add(0);
                    break;
                case 1:
                    kindOfMachines.Add(1);
                    kindOfIngredients.Add(0);
                    break;
                case 2:
                    kindOfMachines.Add(2);
                    kindOfMachines.Add(3);
                    kindOfIngredients.Add(1);
                    kindOfIngredients.Add(2);
                    kindOfIngredients.Add(3);
                    break;
                default:
                    Debug.LogError("its an exception");
                    break;
            }
        }
        icecreamUI.SetNeedCoins(targetCoins);
    }

    public void SaveLevel() 
    {
        var sessionManager = FindObjectOfType<SessionManager>();
        sessionManager.activeKid.kiwis += (passLevels + 1);
        sessionManager.activeKid.playedGames[(int)GameConfigurator.KindOfGame.Icecream] = true;
        sessionManager.activeKid.needSync = true;

        var levelSaver = GetComponent<LevelSaver>();

        sessionManager.activeKid.firstsGames[(int)GameConfigurator.KindOfGame.Icecream] = false;

        float correctPercentage = 0;
        float errorsPercentage = 0;
        float missPercentage = 0;

        for (int i =0; i < maxNumberOfEssay; i++) 
        {
            var ordersDeliveredNow = ordersDelivered[i];
            if (ordersDeliveredNow < 1)
            {
                ordersDeliveredNow = 1;
            }
            correctPercentage += Mathf.CeilToInt((wellMade[i] * 100) / ordersDeliveredNow);
            errorsPercentage += Mathf.FloorToInt((badMade[i] * 100) / ordersDeliveredNow);
            missPercentage += (ordersMissed[i] * 100) / (ordersAsked[i] - wellMade[i]);
        }

        correctPercentage /= maxNumberOfEssay;
        errorsPercentage /= maxNumberOfEssay;
        missPercentage /= maxNumberOfEssay;

        levelSaver.CreateSaveBlock("Helados", timeOfGame, passLevels, repeatedLevels, playedLevels.Count);

        levelSaver.SetIcecreamData(ordersAsked, wellMade, ordersMissed, ordersDelivered, ordersMade, badMade, trashOrders, latencies,
            correctPercentage, errorsPercentage, missPercentage, timeOfGame, playedLevels, playedLevels[0]);

        /*levelSaver.AddLevelsToBlock();
        levelSaver.PostProgress();*/
    }
    #endregion

    public int GetNeededCoins()
    {
        return targetCoins;
    }

    public void SetLatencies()
    {
        var latencie = essayTime - currentEssayTime;
        latencies[currentEssay] = latencie;
    }

    void NeedsTutorial()
    {
        needTutorial = false;
        if (levelsTutorials.Contains(level))
        {
            switch (level)
            {
                case 0:
                    recepiesToShow.Add(0);
                    recepiesToShow.Add(1);
                    recepiesToShow.Add(2);
                    needTutorial = true;
                    kindOfTutorial = 0;
                    break;
                case 10:
                    recepiesToShow.Add(3);
                    recepiesToShow.Add(4);
                    StopAllCoroutines();
                    needTutorial = true;
                    kindOfTutorial = 1;
                    break;
                case 20:
                    recepiesToShow.Add(5);
                    recepiesToShow.Add(6);
                    recepiesToShow.Add(7);
                    recepiesToShow.Add(8);
                    recepiesToShow.Add(9);
                    needTutorial = true;
                    kindOfTutorial = 2;
                    break;
                default:
                    break;
            }
        }
    }

    void SetTimersOnTime()
    {
        foreach (IceCreamOrders order in ordersList)
        {
            order.ChangeTimeStatus();
        }
        currentEssayTime -= Time.deltaTime;
        icecreamUI.PrintTheStoreTime(currentEssayTime);

        if (currentEssayTime <= 0)
        {
            CloseRestaurant();
        }
    }

    int GetEarnings()
    {
        int salesTotal = wellMade[currentEssay] * 20;
        int lossesTotal = (badMade[currentEssay] + trashOrders[currentEssay]) * 5;
        int penalizationTotal = ordersMissed[currentEssay] * 10;
        int totals = salesTotal + tipsEarned[currentEssay] - lossesTotal - penalizationTotal;

        return totals;
    }

    void CloseRestaurant()
    {
        isGameTime = false;
        chef.PrepareTheChef();
        foreach (IceCreamOrders order in ordersList)
        {
            Destroy(order.order);
        }
        ordersList.Clear();
        ClearTables();
        int totalScore = GetEarnings();
        icecreamUI.PrintTheEarnings(wellMade[currentEssay], tipsEarned[currentEssay], (badMade[currentEssay] + trashOrders[currentEssay]), ordersMissed[currentEssay]);
        totalScores[currentEssay] = totalScore;
        icecreamUI.SetButtonToPrintResults(totalScore, targetCoins);

        if (needTutorial)
        {
            tutorialUI.HideAll();
            Destroy(tutorialUI);
            tutorialUI = null;
        }
    }

    public void HandleResult(bool passTheLevel)
    {
        Debug.Log($"Amount of played levels {playedLevels.Count} cuerrent essay is {currentEssay}");
        playedLevels[currentEssay] = level;
        currentEssay++;
        if (passTheLevel)
        {
            if (isFLISTime)
            {
                level += FLISChange();
            }
            else
            {
                level++;
            }

            if(level > maxLevelNumber)
            {
                level = maxLevelNumber;
            }

            Debug.Log($"New Level is {level}");
            passLevels++;
            if (level >= maxLevelNumber)
            {
                level = maxLevelNumber;
            }

            PlayerPrefs.SetInt("levIce", level);
        }
        else
        {
            if (isFLISTime)
            {
                level -= FLISChange();
            }
            else
            {
                level--;
            }
            repeatedLevels++;
        }

        if (currentEssay < maxNumberOfEssay)
        {
            icecreamUI.SetButtonToPlayAgain();
        }
        else
        {
            icecreamUI.SetButtonToFinish();
        }
    }

    public void ShowEarnings()
    {
        pauseManager.ShowKiwiEarnings(passLevels + 1);
        if (!FindObjectOfType<DemoKey>())
        {
            SaveLevel();
        }
    }

    void NewOrderForTime()
    {
        currentNewOrderTiming -= Time.deltaTime;
        if (currentNewOrderTiming <= 0)
        {
            currentNewOrderTiming = newOrderTiming;
            AskForAnOrder();
        }
    }

    public void FillRandomTables()
    {
        var tempTablenames = new List<string>();
        var tempMachineTableNames = new List<string>();

        foreach (string s in tableNames)
        {
            tempTablenames.Add(s);
        }

        foreach (string s in machineTableNames)
        {
            tempMachineTableNames.Add(s);
        }

        //Create the finsh tables
        int randomFinishPlace = Random.Range(0, TableCenterManager.transform.GetChild(0).childCount);
        fullTables[tableMapPositions[tempTablenames[randomFinishPlace]]].AddComponent<TableFinish>();
        if (Random.Range(0, 2) == 0)
        {
            fullTables[tableMapPositions[plusCornerPositions[tempTablenames[randomFinishPlace]]]].AddComponent<TableFinish>();
            if (plusCornerPositions[tempTablenames[randomFinishPlace]].Contains("U") || plusCornerPositions[tempTablenames[randomFinishPlace]].Contains("D"))
            {
                tempMachineTableNames.Remove(plusCornerPositions[tempTablenames[randomFinishPlace]]);
            }
            tempTablenames.Remove(plusCornerPositions[tempTablenames[randomFinishPlace]]);
        }
        else
        {
            fullTables[tableMapPositions[lessCornerPositions[tempTablenames[randomFinishPlace]]]].AddComponent<TableFinish>();
            if (lessCornerPositions[tempTablenames[randomFinishPlace]].Contains("U") || lessCornerPositions[tempTablenames[randomFinishPlace]].Contains("D"))
            {
                tempMachineTableNames.Remove(lessCornerPositions[tempTablenames[randomFinishPlace]]);
            }
            tempTablenames.Remove(lessCornerPositions[tempTablenames[randomFinishPlace]]);
        }
        tempTablenames.Remove(tempTablenames[randomFinishPlace]);

        //Create the trash Table
        int randomTrashPlace = Random.Range(0, TableCenterManager.transform.GetChild(0).childCount-1);
        fullTables[tableMapPositions[tempTablenames[randomTrashPlace]]].AddComponent<TableTrash>();
        tempTablenames.Remove(tempTablenames[randomTrashPlace]);

        fullTables[tableMapPositions[tempTablenames[0]]].AddComponent<TableSimple>();
        tempTablenames.RemoveAt(0);
        fullTables[tableMapPositions[tempTablenames[0]]].AddComponent<TableSimple>();
        tempTablenames.RemoveAt(0);

        for (int i = 0; i < kindOfMachines.Count; i++)
        {
            int numToInput = Random.Range(0, tempMachineTableNames.Count);
            switch (kindOfMachines[i])
            {
                case 0:
                    fullTables[tableMapPositions[tempMachineTableNames[numToInput]]].AddComponent<TableIcreamMachine>();
                    break;
                case 1:
                    fullTables[tableMapPositions[tempMachineTableNames[numToInput]]].AddComponent<TableBlender>();
                    break;
                case 2:
                    fullTables[tableMapPositions[tempMachineTableNames[numToInput]]].AddComponent<TableMixer>();
                    break;
                case 3:
                    fullTables[tableMapPositions[tempMachineTableNames[numToInput]]].AddComponent<TableWaffleMaker>();
                    break;
            }
            tempTablenames.Remove(tempMachineTableNames[numToInput]);
            tempMachineTableNames.Remove(tempMachineTableNames[numToInput]);
        }

        //Create all the toppings that will be avilable
        var tempToppings = new List<int>();
        foreach (int i in possibleToppings)
        {
            tempToppings.Add(i);
        }
        for (int i = 0; i < amountOfTopings; i++)
        {
            int numToInput = Random.Range(0, tempTablenames.Count);
            int toppingNum = Random.Range(0, tempToppings.Count);
            fullTables[tableMapPositions[tempTablenames[numToInput]]].AddComponent<TableIngredients>().ingridientNumber = tempToppings[toppingNum];
            availableToppings.Add(tempToppings[toppingNum]);
            tempTablenames.Remove(tempTablenames[numToInput]);
            tempToppings.Remove(tempToppings[toppingNum]);
        }

        for (int i = 0; i < kindOfIngredients.Count; i++)
        {
            int numToInput = Random.Range(0, tempTablenames.Count);
            fullTables[tableMapPositions[tempTablenames[numToInput]]].AddComponent<TableIngredients>().ingridientNumber = kindOfIngredients[i];
            tempTablenames.Remove(tempTablenames[numToInput]);
        }

        //Creation of chopper tables
        for (int i = 0; i < amoutOfChoppers; i++)
        {
            int numToInput = Random.Range(0, tempTablenames.Count);
            fullTables[tableMapPositions[tempTablenames[numToInput]]].AddComponent<TableChopper>();
            tempTablenames.Remove(tempTablenames[numToInput]);
        }

        //Crate the table containers that will be in the game
        for (int i = 0; i < kindOfContainer.Count; i++)
        {
            int numToInput = Random.Range(0, tempTablenames.Count);
            fullTables[tableMapPositions[tempTablenames[numToInput]]].AddComponent<TableContainers>().kindOfContainer = kindOfContainer[i];
            tempTablenames.Remove(tempTablenames[numToInput]);
        }

        for (int i = 0; i < tempTablenames.Count; i++)
        {
            fullTables[tableMapPositions[tempTablenames[i]]].AddComponent<TableSimple>();
        }
    }

    void AskForAnOrder()
    {
        if (ordersList.Count < maxAmountOfOrdersAtTheSameTime)
        {
            ordersAsked[currentEssay]++;
            IceCreamOrders tempOrder = new IceCreamOrders(Instantiate(Resources.Load<GameObject>("IcecreamMadness/Prefabs/Order"), Canvas.transform.GetChild(0)), this);
            int randomFood = Random.Range(0, kindOfContainer.Count);
            int randomTooping = Random.Range(0, availableToppings.Count);
            tempOrder.SetAnOrder(kindOfContainer[randomFood], kindOfCookedMeals[randomFood], availableToppings[randomTooping]);
            tempOrder.SetOrderPosistion(ordersList.Count);
            ordersList.Add(tempOrder);
            tempOrder.order.name = ("Order_" + ordersList.Count);
        }
    }

    public bool CompareTrays(int?[] trayDelivered)
    {
        bool hasAMatch = false;

        for (int i = 0; i < ordersList.Count; i++)
        {
            if (ordersList[i].IsTheOrderWellMade(trayDelivered))
            {
                int tip = ordersList[i].TipsForThisOrder();
                if (tip > 0)
                {
                    ordersWithTips[currentEssay]++;
                }
                tipsEarned[currentEssay] += tip;
                Destroy(ordersList[i].order);
                ordersList.Remove(ordersList[i]);
                hasAMatch = true;
                confettiSystem.GetComponentInChildren<TextMeshPro>().text = $"${tip + 20}";
                break;
            }
        }
        if (hasAMatch)
        {
            for (int i = 0; i < ordersList.Count; i++)
            {
                ordersList[i].SetOrderPosistion(i);
            }

            if (ordersList.Count <= 0)
            {
                AskForAnOrder();
            }
        }
        return hasAMatch;
    }

    public void GoodAnswer(Vector3 positionOfTable)
    {
        wellMade[currentEssay]++;
        ordersDelivered[currentEssay]++;
        icecreamUI.PrintTheCurrentEarnings(GetEarnings());
        StartCoroutine(AnswerRoutine(confettiSystem, positionOfTable, cashClip));
    }

    public void BadAnswer(Vector3 positionOfTable)
    {
        badMade[currentEssay]++;
        ordersDelivered[currentEssay]++;
        crossesSystem.transform.GetComponentInChildren<TextMeshPro>().text = "<color=#FF0000>-$5</color>";
        icecreamUI.PrintTheCurrentEarnings(GetEarnings());
        StartCoroutine(AnswerRoutine(crossesSystem, positionOfTable, wrongClip));

    }

    public void TrashAnswer(Vector3 positionOfTable)
    {
        trashOrders[currentEssay]++;
        crossesSystem.transform.GetComponentInChildren<TextMeshPro>().text = "<color=#FF0000>-$5</color>";
        icecreamUI.PrintTheCurrentEarnings(GetEarnings());
        StartCoroutine(AnswerRoutine(crossesSystem, positionOfTable, wrongClip));

    }

    IEnumerator AnswerRoutine(ParticleSystem particleSystem, Vector3 positionOfTable, AudioClip clip)
    {
        var initialPos = particleSystem.transform.position;
        particleSystem.transform.position = positionOfTable;
        particleSystem.Play();

        audioSource.clip = clip;
        audioSource.Play();
        while (particleSystem.isPlaying)
        {
            yield return null;
        }

        particleSystem.transform.position = initialPos;

    }

    public void MissOrder(IceCreamOrders orderToDelete)
    {

        ordersMissed[currentEssay]++;
        crossesSystem.transform.GetComponentInChildren<TextMeshPro>().text = "<color=#FF0000>-$10</color>";
        icecreamUI.PrintTheCurrentEarnings(GetEarnings());
        StartCoroutine(MissRoutine(crossesSystem, orderToDelete));
    }

    public void OrderIsFullMade() 
    {
        ordersMade[currentEssay]++;
    }

    IEnumerator MissRoutine(ParticleSystem particleSystem, IceCreamOrders orderToDelete)
    {

        var initialPos = particleSystem.transform.position;

        particleSystem.transform.position = orderToDelete.order.transform.position;

        ordersList.Remove(orderToDelete);
        Destroy(orderToDelete.order);

        particleSystem.Play();

        while (particleSystem.isPlaying)
        {
            yield return null;
        }

        particleSystem.transform.position = initialPos;
        for (int i = 0; i < ordersList.Count; i++)
        {
            ordersList[i].SetOrderPosistion(i);
        }

        if (ordersList.Count < 1)
        {
            AskForAnOrder();
        }
    }

    public bool IsGameOnAction()
    {
        return isGameTime;
    }

    public void StartGameNow()
    {
        isGameTime = true;
        AskForAnOrder();
        if (needTutorial) 
        {
            tutorialUI = gameObject.AddComponent<TutorialUI>();
            tutorialUI.SetTutorial(kindOfTutorial);
        }
    }

    public List<int> GetRecipes()
    {
        return recepiesToShow;
    }

    int FLISChange()
    {
        switch (currentEssay)
        {
            case 1:
                return 10;
            case 2:
                return 5;
            case 3:
                return 2;
            default:
                return 1;
        }
    }
}

public class IceCreamOrders
{
    public GameObject order;
    public IcecreamMadnessManager manager;
    public Image container;
    public Image cookedMeal;
    public Image topping;
    public Image baseBar;
    public Image dynamicBar;
    public Image helperImage;

    int?[] orderType = new int?[3];
    float time;
    float totalTime;

    const float originalPositionInX = 65f;
    const float spaceBetweenCenters = 130f;

    public IceCreamOrders(GameObject prefabOrder, IcecreamMadnessManager managerScript)
    {
        manager = managerScript;
        order = prefabOrder;
        container = prefabOrder.transform.GetChild(0).GetComponent<Image>();
        cookedMeal = container.transform.GetChild(0).GetComponent<Image>();
        topping = cookedMeal.transform.GetChild(0).GetComponent<Image>();
        baseBar = prefabOrder.transform.GetChild(1).GetComponent<Image>();
        dynamicBar = baseBar.transform.GetChild(0).GetComponent<Image>();
        helperImage = order.transform.GetChild(2).GetComponent<Image>();
    }

    public void SetAnOrder(int? container, int? cookedMeal, int? topping)
    {
        orderType[0] = container;
        orderType[1] = cookedMeal;
        orderType[2] = topping;
        float timeToDeliver = 0;
        switch (orderType[0])
        {
            case 0:
                timeToDeliver = FoodDicctionary.icecremPreparationTime;
                break;
            case 1:
                timeToDeliver = FoodDicctionary.smoothiePreparationTime;
                break;
            case 2:
                timeToDeliver = FoodDicctionary.wafflePreparationTime;
                break;
            default:
                timeToDeliver = FoodDicctionary.icecremPreparationTime;
                break;
        }
        totalTime = timeToDeliver;
        SetImage();
    }

    void SetImage()
    {
        if (orderType[0] != null) {
            container.sprite = LoadSprite.GetSpriteFromSpriteSheet($"{FoodDicctionary.prefabSpriteDirection}{FoodDicctionary.containersDirection}", FoodDicctionary.Containers.ShapeOfConatiner((int)orderType[0]));
        }
        if (orderType[1] != null)
        {
            cookedMeal.sprite = LoadSprite.GetSpriteFromSpriteSheet($"{FoodDicctionary.prefabSpriteDirection}{FoodDicctionary.cookedMealDirection}", FoodDicctionary.CookedMeal.ShapeOfCookedMeal((int)orderType[1]));
            if (orderType[1] == 1)
            {
                cookedMeal.color = FoodDicctionary.Toppings.ColorOfSmoothie((int)orderType[2]);
            }
        }
        if (orderType[2] != null)
        {
            topping.sprite = LoadSprite.GetSpriteFromSpriteSheet($"{FoodDicctionary.prefabSpriteDirection}{FoodDicctionary.toppingServedDirection}{FoodDicctionary.CookedMeal.ShapeOfCookedMeal((int)orderType[1])}", FoodDicctionary.Toppings.ShapeOfTopping((int)orderType[2]));
            helperImage.sprite = LoadSprite.GetSpriteFromSpriteSheet($"{FoodDicctionary.prefabSpriteDirection}{FoodDicctionary.helperToppingDirection}", FoodDicctionary.Toppings.ShapeOfTopping((int)orderType[2]));
        }
    }

    Sprite ChangeImage(string shape)
    {
        return Resources.Load<Sprite>($"IcecreamMadness/Sprites/{shape}");
    }

    public void SetOrderPosistion(int number)
    {
        float xPosition = originalPositionInX + (number * spaceBetweenCenters);
        Vector3 localPos = order.GetComponent<RectTransform>().anchoredPosition;
        localPos.x = xPosition;
        order.GetComponent<RectTransform>().anchoredPosition = localPos;
    }

    public bool IsTheOrderWellMade(int?[] dishToCompare)
    {
        for (int i = 0; i < orderType.Length; i++)
        {
            if (orderType[i] != dishToCompare[i])
            {
                return false;
            }
        }

        return true;
    }

    public int TipsForThisOrder()
    {
        float percentageOfTimeSpend = time / totalTime;
        if (percentageOfTimeSpend <= 0.25)
        {
            return 8;
        }
        else if (percentageOfTimeSpend <= 0.50)
        {
            return 4;
        }
        else if (percentageOfTimeSpend <= 0.75)
        {
            return 2;
        }
        else
        {
            return 0;
        }
    }

    public void MissedOrder()
    {
        manager.MissOrder(this);
    }

    public void ChangeTimeStatus()
    {
        time += Time.deltaTime;
        float percentageTimeLeft = time / totalTime;
        dynamicBar.fillAmount = percentageTimeLeft;
        if (time > totalTime)
        {
            MissedOrder();
        }
    }
}

class IcecreamUI
{
    GameObject mainCanvas;

    Image timerReadyPanel;
    TextMeshProUGUI timerReadyText;
    Button OkButton;

    GameObject ropeManager;

    Image timerPanel;
    TextMeshProUGUI timerText;

    Image coinsPanel;
    TextMeshProUGUI coinsAmountText;

    GameObject storyManager;
    List<GameObject> storyImages = new List<GameObject>();

    Image storyPanel;
    TextMeshProUGUI storyText;
    Button nextButton;

    string[] storyStrings;
    string[] instructionsStrings;

    int storyIndex = 0;
    int recipesIndex = 0;

    GameObject recipesPanel;
    List<GameObject> recipesObjects = new List<GameObject>();
    Button recipesButton;

    GameObject characterSelectionPanel;
    GameObject selectCharacterPanel;
    TextMeshProUGUI selectCharacterText;

    IcecreamMadnessManager manager;

    public IcecreamUI(GameObject canvas, IcecreamMadnessManager managerToRefer)
    {
        manager = managerToRefer;
        mainCanvas = canvas;

        timerReadyPanel = mainCanvas.transform.Find("Timer Ready Panel").GetComponent<Image>();
        timerReadyText = timerReadyPanel.transform.GetComponentInChildren<TextMeshProUGUI>();
        OkButton = timerReadyPanel.transform.GetComponentInChildren<Button>();

        ropeManager = mainCanvas.transform.Find("Rope Manager").gameObject;

        timerPanel = mainCanvas.transform.Find("Timer Panel").GetComponent<Image>();
        timerText = timerPanel.GetComponentInChildren<TextMeshProUGUI>();

        coinsPanel = mainCanvas.transform.Find("Coins Panel").GetComponent<Image>();
        coinsAmountText = coinsPanel.GetComponentInChildren<TextMeshProUGUI>();

        storyManager = mainCanvas.transform.Find("Story Manager").gameObject;
        for (int i = 0; i < storyManager.transform.childCount; i++)
        {
            storyImages.Add(storyManager.transform.GetChild(i).gameObject);
        }

        storyPanel = mainCanvas.transform.Find("Story Panel").GetComponent<Image>();
        storyText = storyPanel.GetComponentInChildren<TextMeshProUGUI>();

        nextButton = storyPanel.GetComponentInChildren<Button>();
        nextButton.onClick.AddListener(PrintTheStory);

        var storyAsset = Resources.Load<TextAsset>($"{LanguagePicker.BasicTextRoute()}Games/Icecream/Story");
        storyStrings = TextReader.TextsToShow(storyAsset);

        var instructionAsset = Resources.Load<TextAsset>($"{LanguagePicker.BasicTextRoute()}Games/Icecream/Instructions");
        instructionsStrings = TextReader.TextsToShow(instructionAsset);

        recipesPanel = mainCanvas.transform.Find("Recepies Panel").gameObject;
        for (int i = 0; i < recipesPanel.transform.childCount - 1; i++)
        {
            recipesObjects.Add(recipesPanel.transform.GetChild(i).gameObject);
            recipesObjects[i].SetActive(false);
        }
        recipesButton = recipesPanel.transform.GetChild(recipesPanel.transform.childCount - 1).GetComponent<Button>();

        characterSelectionPanel = mainCanvas.transform.Find("Character Selection Panel").gameObject;
        selectCharacterPanel = characterSelectionPanel.transform.GetChild(0).gameObject;
        selectCharacterText = selectCharacterPanel.transform.GetComponentInChildren<TextMeshProUGUI>();
        var characterSelectionManager = characterSelectionPanel.transform.GetChild(1);
        for (int i = 0; i < characterSelectionManager.childCount; i++)
        {
            var buttonToSet = characterSelectionManager.GetChild(i).GetComponent<Button>();
            buttonToSet.onClick.AddListener(() => manager.SetChef(buttonToSet.transform.GetChild(0).name));
        }
    }

    public void ChooseChef()
    {
        HideAllManagers();
        characterSelectionPanel.SetActive(true);
    }

    public void HideAllManagers()
    {
        timerReadyPanel.gameObject.SetActive(false);
        timerPanel.gameObject.SetActive(false);
        coinsPanel.gameObject.SetActive(false);
        ropeManager.SetActive(false);
        storyManager.SetActive(false);
        storyPanel.gameObject.SetActive(false);
        recipesPanel.SetActive(false);
        characterSelectionPanel.SetActive(false);
    }

    public void PrintTheStory()
    {
        HideAllManagers();
        storyManager.SetActive(true);
        storyPanel.gameObject.SetActive(true);

        foreach (GameObject o in storyImages)
        {
            o.SetActive(false);
        }

        storyImages[storyIndex].SetActive(true);
        storyText.text = storyStrings[storyIndex];

        storyIndex++;
        if (storyIndex >= storyImages.Count)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(PrintAreYouReady);
        }
    }

    public void PrintRecipes()
    {
        HideAllManagers();

        recipesPanel.SetActive(true);
        var recipes = manager.GetRecipes();

        foreach (GameObject o in recipesObjects)
        {
            o.SetActive(false);
        }

        recipesObjects[recipes[recipesIndex]].SetActive(true);

        recipesIndex++;

        recipesButton.onClick.RemoveAllListeners();
        if (recipesIndex < recipes.Count)
        {
            recipesButton.onClick.AddListener(PrintRecipes);
        }
        else
        {
            recipesIndex = 0;
            recipesButton.onClick.AddListener(() => PrintYouNeed(manager.GetNeededCoins()));
        }
    }

    public void PrintAreYouReady()
    {
        HideAllManagers();
        timerReadyPanel.gameObject.SetActive(true);
        timerReadyText.text = instructionsStrings[0];

        OkButton.onClick.AddListener(manager.EssayParameterInitilization);
    }

    public void SetNeedCoins(int needs)
    {
        OkButton.onClick.RemoveAllListeners();
        if (manager.GetRecipes().Count > 0)
        {
            //OkButton.onClick.AddListener(PrintRecipes);
            PrintRecipes();
        }
        else
        {
            //OkButton.onClick.AddListener(() => PrintYouNeed(needs));
            PrintYouNeed(needs);
        }
    }

    public void PrintYouNeed(int neededCoins)
    {
        HideAllManagers();
        timerReadyPanel.gameObject.SetActive(true);
        timerReadyText.text = $"{instructionsStrings[1]} ${neededCoins} {instructionsStrings[2]}";

        OkButton.onClick.RemoveAllListeners();
        OkButton.onClick.AddListener(() => { manager.StartCoroutine(StartCountDown()); });
    }

    public IEnumerator StartCountDown()
    {
        HideAllManagers();
        timerReadyPanel.gameObject.SetActive(true);
        OkButton.gameObject.SetActive(false);
        manager.FillRandomTables();
        timerReadyText.text = $"{instructionsStrings[3]}\n 3";
        yield return new WaitForSeconds(1f);
        timerReadyText.text = $"{instructionsStrings[3]}\n 2";
        yield return new WaitForSeconds(1f);
        timerReadyText.text = $"{instructionsStrings[3]}\n 1";
        yield return new WaitForSeconds(1f);
        timerReadyText.text = instructionsStrings[4];
        yield return new WaitForSeconds(1f);
        timerReadyPanel.gameObject.SetActive(false);
        timerPanel.gameObject.SetActive(true);
        coinsPanel.gameObject.SetActive(true);
        ropeManager.SetActive(true);
        manager.StartGameNow();

    }

    public void PrintTheStoreTime(float timeRemaining)
    {
        int minutes = Mathf.FloorToInt(timeRemaining / 60f);
        int seconds = Mathf.FloorToInt(timeRemaining % 60f);
        timerText.text = $"<color=#{ColorToPrint(minutes)}>{minutes}:{seconds.ToString("00")}</color>";
    }

    public void PrintTheCurrentEarnings(int earnings)
    {
        coinsAmountText.text = $"x {earnings}";
    }

    string ColorToPrint(int minutesRemaining)
    {
        var returner = minutesRemaining > 0 ? "42210B" : "B32006";
        return returner;
    }

    string ColorToPrintTotal(int totalAmount)
    {
        var returner = totalAmount >= 0 ? "42210B" : "B32006";
        return returner;
    }

    string SimbolToPrint(int totalAmount)
    {
        var returner = totalAmount < 0 ? " - " : "   ";
        return returner;
    }

    public void PrintTheEarnings(int objectsWellMade, int tipScore, int objectsBadMade, int objectsMissed)
    {
        HideAllManagers();
        timerReadyPanel.gameObject.SetActive(true);
        OkButton.gameObject.SetActive(true);

        int salesTotal = objectsWellMade * 20;
        int lossesTotal = objectsBadMade * 5;
        int penalizationTotal = objectsMissed * 10;
        int totals = salesTotal + tipScore - lossesTotal - penalizationTotal;

        int salesToPrint = 0;
        if (salesTotal > 0)
        {
            salesToPrint = salesTotal;
        }

        Debug.Log($"sales to print are {salesToPrint}");

        string sales = $"<color=#42210B>{instructionsStrings[5]}   ${salesToPrint.ToString("000")}</color>";
        string tips = $"<color=#42210B>{instructionsStrings[6]}   ${tipScore.ToString("000")}</color>";
        string looses = $"<color=#B32006>{instructionsStrings[7]} - ${lossesTotal.ToString("000")}</color>";
        string penalization = $"<color=#B32006>{instructionsStrings[8]} - ${penalizationTotal.ToString("000")}</color>";
        string total = $"<color=#{ColorToPrintTotal(totals)}>{instructionsStrings[9]}{SimbolToPrint(totals)}${Mathf.Abs(totals).ToString("000")}</color>";

        timerReadyText.text = $"{sales}\n{tips}\n{looses}\n{penalization}\n{total}";
    }

    public void PrintTheResults(int totalCoins, int needCoins)
    {
        string total = $"<color=#42210B>{instructionsStrings[10]}   ${(totalCoins >= 0 ? totalCoins.ToString("000") : "000")}</color>";
        string needs = $"<color=#42210B>{instructionsStrings[11]}   ${needCoins.ToString("000")}</color>";
        timerReadyText.text = $"{total}\n{needs}";

        bool isCorrect = totalCoins >= needCoins;

        if (isCorrect)
        {
            timerReadyText.text += $"\n{instructionsStrings[12]}";
        }
        else
        {
            timerReadyText.text += $"\n{instructionsStrings[13]}";
        }

        manager.HandleResult(isCorrect);
    }

    public void SetButtonToPrintResults(int totals, int needCoins)
    {
        OkButton.onClick.RemoveAllListeners();
        OkButton.onClick.AddListener(() => PrintTheResults(totals, needCoins));
    }

    public void SetButtonToPlayAgain()
    {
        OkButton.onClick.RemoveAllListeners();
        OkButton.onClick.AddListener(PrintAreYouReady);
    }

    public void SetButtonToFinish()
    {
        OkButton.onClick.RemoveAllListeners();
        OkButton.onClick.AddListener(()=>
        {
            HideAllManagers();
            manager.ShowEarnings();
        });

        HideAllManagers();
        timerReadyPanel.gameObject.SetActive(true);
        timerReadyText.text = instructionsStrings[14];
    } 
}

public class TutorialUI : MonoBehaviour 
{
    string[] tutorialStrings;

    int index;
    int firstMessage;
    int lastMessage;

    const float timeBetweenInstructions = 4f;
    float timePassLastInstruction;

    GameObject tutorialPanel;
    TextMeshProUGUI instructionsText;

    private void Update()
    {
        timePassLastInstruction -= Time.deltaTime;
        if (timePassLastInstruction <= 0)
        {
            if (index <= lastMessage)
            {
                instructionsText.text = tutorialStrings[index];
                index += 1;
            }
            else
            {
                instructionsText.text = tutorialStrings[tutorialStrings.Length - 1];
                index = firstMessage;
            }
            timePassLastInstruction = timeBetweenInstructions;
        }
    }

    void Initialize() 
    {
        var tutorialAsset = Resources.Load<TextAsset>($"{LanguagePicker.BasicTextRoute()}Games/Icecream/Tutorial");
        tutorialStrings = TextReader.TextsToShow(tutorialAsset);

        timePassLastInstruction = timeBetweenInstructions;
        tutorialPanel = GetComponent<IcecreamMadnessManager>().Canvas.transform.GetChild(1).gameObject;
        instructionsText = tutorialPanel.GetComponentInChildren<TextMeshProUGUI>();

        tutorialPanel.SetActive(true);
    }

    public void SetTutorial(int kindOfTutorial)
    {
        Initialize();

        switch (kindOfTutorial) 
        {
            case 0:
                firstMessage = 0;
                lastMessage = 3;
                break;
            case 1:
                firstMessage = 4;
                lastMessage = 6;
                break;
            case 2:
                firstMessage = 7;
                lastMessage = 10;
                break;
        }

        index = firstMessage;
        instructionsText.text = tutorialStrings[index];
        index += 1;
    }

    public void HideAll() 
    {
        tutorialPanel.gameObject.SetActive(false);
    }
}
