using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class crystalMazeScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;

    //home
    public KMSelectable[] zoneAccess;
    public Renderer surface;
    public Material[] surfaceOptions;
    public Renderer[] crystals;
    private int collectedCrystals = 0;
    public GameObject[] zones;
    public GameObject complete;
    public Renderer[] zoneCrosses;
    private bool[] endOfZone = new bool[4];

    //dome
    public RuntimeAnimatorController[] animationOptions;
    public Animator[] tokenAnimators;
    public tokenScript[] tokenObjects;
    private List<int> goldTokens = new List<int>();
    public Material[] tokenMaterials;
    private int[] tokenCountValues = new int[3];
    public TextMesh[] countDisplays;
    public TextMesh domeTimer;
    private int domeTime = 0;
    private bool domeTimerOn;
    private bool tokensSet;
    public AudioClip[] domeThemes;
    private bool inDome = false;

    //aztec
    public KMSelectable[] aztecButtons;
    public aztecButton[] aztecButtonsScript;
    public KMSelectable aztecAdd;
    public Animator seesaw;
    private int aztecReset = 0;
    private int aztecTargetWeight = 0;
    public TextMesh aztecTargetText;
    private int aztecWeightAdded = 0;
    private int aztecClicks = 0;
    public TextMesh aztecGuessText;
    public TextMesh aztecTimer;
    public GameObject aztecOffsetBoard;
    public TextMesh aztecOffset;
    private int aztecTime = 59;
    private string initialOffsetDisp;
    private bool aztecSolved;
    private bool inAztec = false;

    //industrial
    public TextMesh industrialTimer;
    private int industrialTime = 59;
    private bool industrialSolved;
    private int displayedDigit = 0;
    public TextMesh displayedDigitText;
    public KMSelectable digitUpButton;
    public KMSelectable digitDownButton;
    public KMSelectable reverseButton;
    public char[] industrialLetterOptions;
    private char[] chosenSerialLetters = new char[5];
    private int[] chosenSerialIndices = new int[5];
    private string serialNumber = "";
    public TextMesh serialText;
    private int industrialX = 0;
    private int industrialY = 0;
    private int industrialZ = 0;
    public Animator platform;
    public RuntimeAnimatorController[] cogAnimationOptions;
    public Animator[] cogAnimators;
    public Animator crystalAnimator;
    private bool industrialCorrect;
    public GameObject crystalObject;
    private bool inIndustrial = false;

    //futuristic
    public KMSelectable[] screenButtons;
    public string[] wordOptions;
    public Color[] colorOptions;
    public TextMesh[] screenText;
    private string[] screen1Words = new string[3];
    private Color[] screen1Colors = new Color[3];
    private string[] screen1ColorNames = new string[3];
    private string[] screen2Words = new string[3];
    private Color[] screen2Colors = new Color[3];
    private string[] screen2ColorNames = new string[3];
    private string[] screen3Words = new string[3];
    private Color[] screen3Colors = new Color[3];
    private string[] screen3ColorNames = new string[3];
    private string[] correctWords = new string[3];
    private Color[] correctColors = new Color[3];
    private string[] correctColorsLog = new string[3];
    private bool[] wordSolved = new bool[3];
    private bool[] wordCorrect = new bool[3];
    public Renderer[] indicatorLightLeft;
    public Renderer[] indicatorLightRight;
    public Material[] indicatorOptions;
    public TextMesh tpDigit1;
    public TextMesh tpDigit2;
    public TextMesh tpDigit3;
    public TextMesh futuristicTimer;
    private List<int> pressedScreens = new List<int>();
    private int futuristicTime = 59;
    private bool futuristicSolved;
    private bool inFuturistic = false;

    //medieval
    public KMSelectable medievalCircle;
    public Material[] circleColourOptions;
    private string[] circleColourNameOptions = new string[6] { "blue", "brown", "green", "purple", "red", "yellow" };
    private string[] circleColourName = new string[4];
    public Renderer[] circles;
    public Animator[] circleAnimators;
    public RuntimeAnimatorController[] circleAnimationOptions;
    private bool[] clockwise = new bool[4];
    private string[] clockwiseLog = new string[4];
    private int pressTime = 0;
    private int[] primeOptions = new int[12] { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37 };
    public TextMesh medievalTimer;
    public Renderer[] arrow;
    private int medievalTime = 59;
    private bool medievalSolved;
    private bool inMedieval = false;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        foreach (KMSelectable zone in zoneAccess)
        {
            KMSelectable pressedZone = zone;
            zone.OnInteract += delegate () { ZonePress(pressedZone); return false; };
        }
        foreach (KMSelectable button in aztecButtons)
        {
            KMSelectable pressedButton = button;
            button.OnInteract += delegate () { AztecButton(pressedButton); return false; };
        }
        aztecAdd.OnInteract += delegate () { AztecAddPress(); return false; };
        foreach (KMSelectable screen in screenButtons)
        {
            KMSelectable pressedScreen = screen;
            screen.OnInteract += delegate () { FutureScreenPress(pressedScreen); return false; };
        }
        medievalCircle.OnInteract += delegate () { MedievalCirclePress(); return false; };
        digitUpButton.OnInteract += delegate () { DigitUpPress(); return false; };
        digitDownButton.OnInteract += delegate () { DigitDownPress(); return false; };
        reverseButton.OnInteract += delegate () { ReversePress(); return false; };
        StartCoroutine(delayDetection());
    }

    void Update()
    {
        if (endOfZone[0] && endOfZone[1] && endOfZone[2] && endOfZone[3] && collectedCrystals == 0)
        {
            if (TPActive)
            {
                Debug.LogFormat("[The Crystal Maze #{0}] You have attempted every game and not secured any crystals. Game reset.", moduleId);
                Reset();
            }
            else
            {
                Debug.LogFormat("[The Crystal Maze #{0}] Strike! You have attempted every game and not secured any crystals. Game reset.", moduleId);
                GetComponent<KMBombModule>().HandleStrike();
                Reset();
            }
        }
    }

    void Start()
    {
        Debug.LogFormat("[The Crystal Maze #{0}] Welcome to The Crystal Maze! Reckless Rick at your service!", moduleId);
        for (int i = 0; i <= 3; i++)
        {
            crystals[i].enabled = false;
            zoneCrosses[i].enabled = false;
            endOfZone[i] = false;
        }
        complete.SetActive(false);
        foreach (tokenScript token in tokenObjects)
        {
            token.parentObject.SetActive(true);
        }
        if (!tokensSet)
        {
            foreach (tokenScript token in tokenObjects)
            {
                tokenScript pressedToken = token;
                token.selectable.OnInteract += delegate () { TokenPress(pressedToken); return false; };
            }
            SelectTokenAnimations();
        }
        AztecSetUp();
        MedievalCircleSetUp();
        FuturisticSetUp();
        IndustrialSetUp();
        CalculateMedievalPressTime();
        for (int i = 0; i <= 5; i++)
        {
            zones[i].SetActive(false);
        }
        zones[0].SetActive(true);
        surface.material = surfaceOptions[0];
    }

    void SelectTokenAnimations()
    {
        for (int i = 0; i < tokenAnimators.Count(); i++)
        {
            int index = UnityEngine.Random.Range(0, 7);
            tokenAnimators[i].runtimeAnimatorController = animationOptions[index];
        }
        for (int i = 0; i <= 34; i++)
        {
            int index = UnityEngine.Random.Range(0, tokenAnimators.Count());
            while (goldTokens.Contains(index))
            {
                index = UnityEngine.Random.Range(0, tokenAnimators.Count());
            }
            goldTokens.Add(index);
            tokenObjects[goldTokens[i]].GetComponent<Renderer>().material = tokenMaterials[0];
            tokenObjects[goldTokens[i]].gold = true;
        }
        for (int i = 0; i < tokenObjects.Count(); i++)
        {
            if (!tokenObjects[i].gold)
            {
                tokenObjects[i].GetComponent<Renderer>().material = tokenMaterials[1];
            }

        }
        goldTokens.Clear();
        tokensSet = true;
    }

    void AztecSetUp()
    {
        aztecReset = UnityEngine.Random.Range(15, 21);
        aztecTargetWeight = UnityEngine.Random.Range(125, 176);
        aztecTargetText.text = aztecTargetWeight.ToString() + " KG";
        int aztecStartIndex = UnityEngine.Random.Range(0, 2);
        if (aztecStartIndex == 0)
        {
            aztecButtonsScript[0].buttonValue = 1;
            aztecButtonsScript[1].buttonValue = 2;
            foreach (aztecButton button in aztecButtonsScript)
            {
                button.buttonText.text = button.buttonValue.ToString() + " KG";
            }
        }
        else
        {
            aztecButtonsScript[0].buttonValue = 2;
            aztecButtonsScript[1].buttonValue = 1;
            foreach (aztecButton button in aztecButtonsScript)
            {
                button.buttonText.text = button.buttonValue.ToString() + " KG";
            }
        }
    }

    void MedievalCircleSetUp()
    {
        for (int i = 0; i <= 3; i++)
        {
            int colourIndex = UnityEngine.Random.Range(0, 6);
            circles[i].material = circleColourOptions[colourIndex];
            circleColourName[i] = circleColourNameOptions[colourIndex];

            int rotationIndex = UnityEngine.Random.Range(0, 2);
            circleAnimators[i].runtimeAnimatorController = circleAnimationOptions[rotationIndex];
            if (rotationIndex == 0)
            {
                clockwise[i] = true;
                clockwiseLog[i] = "clockwise";
            }
            else
            {
                clockwise[i] = false;
                clockwiseLog[i] = "counter-clockwise";
            }
        }
    }

    void FuturisticSetUp()
    {
        int option = UnityEngine.Random.Range(0, 2);
        if (option == 0)
        {
            int screen1Word1 = UnityEngine.Random.Range(0, 6);
            screen1Words[0] = wordOptions[screen1Word1];
            screen1Colors[0] = colorOptions[screen1Word1];
            screen1ColorNames[0] = wordOptions[screen1Word1];

            int screen1Word2 = UnityEngine.Random.Range(0, 6);
            while (screen1Word2 == screen1Word1)
            {
                screen1Word2 = UnityEngine.Random.Range(0, 6);
            }
            screen1Words[1] = wordOptions[screen1Word2];
            screen1Colors[1] = colorOptions[screen1Word2];
            screen1ColorNames[1] = wordOptions[screen1Word2];

            int screen1Word3 = UnityEngine.Random.Range(0, 6);
            while (screen1Word3 == screen1Word1 || screen1Word3 == screen1Word2)
            {
                screen1Word3 = UnityEngine.Random.Range(0, 6);
            }
            screen1Words[2] = wordOptions[screen1Word3];
            int screen1Color3 = UnityEngine.Random.Range(0, 6);
            while (screen1Color3 == screen1Word3)
            {
                screen1Color3 = UnityEngine.Random.Range(0, 6);
            }
            screen1Colors[2] = colorOptions[screen1Color3];
            screen1ColorNames[2] = wordOptions[screen1Color3];
            correctWords[0] = wordOptions[screen1Word3];
            correctColors[0] = colorOptions[screen1Color3];
            correctColorsLog[0] = wordOptions[screen1Color3];
        }
        else
        {
            int screen1Word1 = UnityEngine.Random.Range(0, 6);
            screen1Words[0] = wordOptions[screen1Word1];
            screen1Colors[0] = colorOptions[screen1Word1];
            screen1ColorNames[0] = wordOptions[screen1Word1];

            int screen1Word2 = UnityEngine.Random.Range(0, 6);
            while (screen1Word2 == screen1Word1)
            {
                screen1Word2 = UnityEngine.Random.Range(0, 6);
            }
            screen1Words[1] = wordOptions[screen1Word2];
            int screen1Color2 = UnityEngine.Random.Range(0, 6);
            while (screen1Color2 == screen1Word2)
            {
                screen1Color2 = UnityEngine.Random.Range(0, 6);
            }
            screen1Colors[1] = colorOptions[screen1Color2];
            screen1ColorNames[1] = wordOptions[screen1Color2];

            int screen1Word3 = UnityEngine.Random.Range(0, 6);
            while (screen1Word3 == screen1Word1 || screen1Word3 == screen1Word2)
            {
                screen1Word3 = UnityEngine.Random.Range(0, 6);
            }
            screen1Words[2] = wordOptions[screen1Word3];
            int screen1Color3 = UnityEngine.Random.Range(0, 6);
            while (screen1Color3 == screen1Word3)
            {
                screen1Color3 = UnityEngine.Random.Range(0, 6);
            }
            screen1Colors[2] = colorOptions[screen1Color3];
            screen1ColorNames[2] = wordOptions[screen1Color3];
            correctWords[0] = wordOptions[screen1Word1];
            correctColors[0] = colorOptions[screen1Word1];
            correctColorsLog[0] = wordOptions[screen1Word1];
        }

        int option2 = UnityEngine.Random.Range(0, 2);
        if (option2 == 0)
        {
            int screen2Word1 = UnityEngine.Random.Range(0, 6);
            screen2Words[0] = wordOptions[screen2Word1];
            screen2Colors[0] = colorOptions[screen2Word1];
            screen2ColorNames[0] = wordOptions[screen2Word1];

            int screen2Word2 = UnityEngine.Random.Range(0, 6);
            while (screen2Word2 == screen2Word1)
            {
                screen2Word2 = UnityEngine.Random.Range(0, 6);
            }
            screen2Words[1] = wordOptions[screen2Word2];
            screen2Colors[1] = colorOptions[screen2Word2];
            screen2ColorNames[1] = wordOptions[screen2Word2];

            int screen2Word3 = UnityEngine.Random.Range(0, 6);
            while (screen2Word3 == screen2Word1 || screen2Word3 == screen2Word2)
            {
                screen2Word3 = UnityEngine.Random.Range(0, 6);
            }
            screen2Words[2] = wordOptions[screen2Word3];
            int screen2Color3 = UnityEngine.Random.Range(0, 6);
            while (screen2Color3 == screen2Word3)
            {
                screen2Color3 = UnityEngine.Random.Range(0, 6);
            }
            screen2Colors[2] = colorOptions[screen2Color3];
            screen2ColorNames[2] = wordOptions[screen2Color3];
            correctWords[1] = wordOptions[screen2Word3];
            correctColors[1] = colorOptions[screen2Color3];
            correctColorsLog[1] = wordOptions[screen2Color3];
        }
        else
        {
            int screen2Word1 = UnityEngine.Random.Range(0, 6);
            screen2Words[0] = wordOptions[screen2Word1];
            screen2Colors[0] = colorOptions[screen2Word1];
            screen2ColorNames[0] = wordOptions[screen2Word1];

            int screen2Word2 = UnityEngine.Random.Range(0, 6);
            while (screen2Word2 == screen2Word1)
            {
                screen2Word2 = UnityEngine.Random.Range(0, 6);
            }
            screen2Words[1] = wordOptions[screen2Word2];
            int screen2Color2 = UnityEngine.Random.Range(0, 6);
            while (screen2Color2 == screen2Word2)
            {
                screen2Color2 = UnityEngine.Random.Range(0, 6);
            }
            screen2Colors[1] = colorOptions[screen2Color2];
            screen2ColorNames[1] = wordOptions[screen2Color2];

            int screen2Word3 = UnityEngine.Random.Range(0, 6);
            while (screen2Word3 == screen2Word1 || screen2Word3 == screen2Word2)
            {
                screen2Word3 = UnityEngine.Random.Range(0, 6);
            }
            screen2Words[2] = wordOptions[screen2Word3];
            int screen2Color3 = UnityEngine.Random.Range(0, 6);
            while (screen2Color3 == screen2Word3)
            {
                screen2Color3 = UnityEngine.Random.Range(0, 6);
            }
            screen2Colors[2] = colorOptions[screen2Color3];
            screen2ColorNames[2] = wordOptions[screen2Color3];
            correctWords[1] = wordOptions[screen2Word1];
            correctColors[1] = colorOptions[screen2Word1];
            correctColorsLog[1] = wordOptions[screen2Word1];
        }

        int option3 = UnityEngine.Random.Range(0, 2);
        if (option3 == 0)
        {
            int screen3Word1 = UnityEngine.Random.Range(0, 6);
            screen3Words[0] = wordOptions[screen3Word1];
            screen3Colors[0] = colorOptions[screen3Word1];
            screen3ColorNames[0] = wordOptions[screen3Word1];

            int screen3Word2 = UnityEngine.Random.Range(0, 6);
            while (screen3Word2 == screen3Word1)
            {
                screen3Word2 = UnityEngine.Random.Range(0, 6);
            }
            screen3Words[1] = wordOptions[screen3Word2];
            screen3Colors[1] = colorOptions[screen3Word2];
            screen3ColorNames[1] = wordOptions[screen3Word2];

            int screen3Word3 = UnityEngine.Random.Range(0, 6);
            while (screen3Word3 == screen3Word1 || screen3Word3 == screen3Word2)
            {
                screen3Word3 = UnityEngine.Random.Range(0, 6);
            }
            screen3Words[2] = wordOptions[screen3Word3];
            int screen3Color3 = UnityEngine.Random.Range(0, 6);
            while (screen3Color3 == screen3Word3)
            {
                screen3Color3 = UnityEngine.Random.Range(0, 6);
            }
            screen3Colors[2] = colorOptions[screen3Color3];
            screen3ColorNames[2] = wordOptions[screen3Color3];
            correctWords[2] = wordOptions[screen3Word3];
            correctColors[2] = colorOptions[screen3Color3];
            correctColorsLog[2] = wordOptions[screen3Color3];
        }
        else
        {
            int screen3Word1 = UnityEngine.Random.Range(0, 6);
            screen3Words[0] = wordOptions[screen3Word1];
            screen3Colors[0] = colorOptions[screen3Word1];
            screen3ColorNames[0] = wordOptions[screen3Word1];

            int screen3Word2 = UnityEngine.Random.Range(0, 6);
            while (screen3Word2 == screen3Word1)
            {
                screen3Word2 = UnityEngine.Random.Range(0, 6);
            }
            screen3Words[1] = wordOptions[screen3Word2];
            int screen3Color2 = UnityEngine.Random.Range(0, 6);
            while (screen3Color2 == screen3Word2)
            {
                screen3Color2 = UnityEngine.Random.Range(0, 6);
            }
            screen3Colors[1] = colorOptions[screen3Color2];
            screen3ColorNames[1] = wordOptions[screen3Color2];

            int screen3Word3 = UnityEngine.Random.Range(0, 6);
            while (screen3Word3 == screen3Word1 || screen3Word3 == screen3Word2)
            {
                screen3Word3 = UnityEngine.Random.Range(0, 6);
            }
            screen3Words[2] = wordOptions[screen3Word3];
            int screen3Color3 = UnityEngine.Random.Range(0, 6);
            while (screen3Color3 == screen3Word3)
            {
                screen3Color3 = UnityEngine.Random.Range(0, 6);
            }
            screen3Colors[2] = colorOptions[screen3Color3];
            screen3ColorNames[2] = wordOptions[screen3Color3];
            correctWords[2] = wordOptions[screen3Word1];
            correctColors[2] = colorOptions[screen3Word1];
            correctColorsLog[2] = wordOptions[screen3Word1];
        }
    }

    void IndustrialSetUp()
    {
        for (int i = 0; i <= 4; i++)
        {
            chosenSerialIndices[i] = UnityEngine.Random.Range(1, 27);
        }
        chosenSerialIndices[2] = UnityEngine.Random.Range(6, 11);
        for (int i = 0; i <= 4; i++)
        {
            chosenSerialLetters[i] = industrialLetterOptions[chosenSerialIndices[i]];
            serialNumber += chosenSerialLetters[i];
        }
        serialText.text = serialNumber;
        industrialX = chosenSerialIndices[0] + chosenSerialIndices[4];
        industrialY = chosenSerialIndices[1] + chosenSerialIndices[3];
        industrialZ = (industrialX * industrialY) % chosenSerialIndices[2];
    }

    void CalculateMedievalPressTime()
    {
        for (int i = 0; i <= 3; i++)
        {
            if (circleColourName[i] == "blue" && clockwise[i])
            {
                pressTime += primeOptions[0];
            }
            else if (circleColourName[i] == "brown" && !clockwise[i])
            {
                pressTime += primeOptions[1];
            }
            else if (circleColourName[i] == "green" && clockwise[i])
            {
                pressTime += primeOptions[2];
            }
            else if (circleColourName[i] == "purple" && !clockwise[i])
            {
                pressTime += primeOptions[3];
            }
            else if (circleColourName[i] == "red" && clockwise[i])
            {
                pressTime += primeOptions[4];
            }
            else if (circleColourName[i] == "yellow" && !clockwise[i])
            {
                pressTime += primeOptions[5];
            }
            else if (circleColourName[i] == "yellow" && clockwise[i])
            {
                pressTime += primeOptions[6];
            }
            else if (circleColourName[i] == "red" && !clockwise[i])
            {
                pressTime += primeOptions[7];
            }
            else if (circleColourName[i] == "purple" && clockwise[i])
            {
                pressTime += primeOptions[8];
            }
            else if (circleColourName[i] == "green" && !clockwise[i])
            {
                pressTime += primeOptions[9];
            }
            else if (circleColourName[i] == "brown" && clockwise[i])
            {
                pressTime += primeOptions[10];
            }
            else if (circleColourName[i] == "blue" && !clockwise[i])
            {
                pressTime += primeOptions[11];
            }
            Debug.Log(pressTime);
        }
        pressTime = (pressTime % 10);
    }

    void ZonePress(KMSelectable zone)
    {
        if (moduleSolved)
        {
            return;
        }
        if (zone.name == "AztecSelectable" && !aztecSolved)
        {
            zone.AddInteractionPunch();
            Audio.PlaySoundAtTransform("sting", transform);
            string begin = "";
            if (aztecTime >= 60)
            {
                begin = "1:";
            }
            else
            {
                begin = "0:";
            }
            aztecTimer.text = begin + (aztecTime % 60).ToString("00");
            aztecGuessText.text = "?";
            surface.material = surfaceOptions[1];
            if (TPActive)
            {
                aztecOffsetBoard.SetActive(true);
                initialOffsetDisp = aztecOffset.text;
                aztecOffset.text += "" + aztecReset;
            }
            else
            {
                aztecOffsetBoard.SetActive(false);
            }
            zones[0].SetActive(false);
            zones[1].SetActive(true);
            inAztec = true;
            StartCoroutine(AztecTimer());
            Debug.LogFormat("[The Crystal Maze #{0}] Welcome to Aztec world! Your target weight is {1}kg. Your buttons will switch every {2} presses.", moduleId, aztecTargetWeight, aztecReset);
        }
        else if (zone.name == "IndustrialSelectable" && !industrialSolved)
        {
            zone.AddInteractionPunch();
            Audio.PlaySoundAtTransform("sting", transform);
            string begin = "";
            if (industrialTime >= 60)
            {
                begin = "1:";
            }
            else
            {
                begin = "0:";
            }
            industrialTimer.text = begin + (industrialTime % 60).ToString("00");
            displayedDigit = UnityEngine.Random.Range(0, 10);
            displayedDigitText.text = displayedDigit.ToString();
            surface.material = surfaceOptions[2];
            zones[0].SetActive(false);
            zones[2].SetActive(true);
            inIndustrial = true;
            StartCoroutine(IndustrialTimer());
            Debug.LogFormat("[The Crystal Maze #{0}] Welcome to Industrial world! Your serial number is {1}. The reverse digit is {2}.", moduleId, serialNumber, industrialZ);
        }
        else if (zone.name == "FuturisticSelectable" && !futuristicSolved)
        {
            zone.AddInteractionPunch();
            Audio.PlaySoundAtTransform("sting", transform);
            futuristicTimer.text = "0:" + futuristicTime.ToString("00");
            for (int i = 0; i <= 2; i++)
            {
                indicatorLightLeft[i].material = indicatorOptions[0];
                indicatorLightRight[i].material = indicatorOptions[0];
            }
            surface.material = surfaceOptions[3];
            zones[0].SetActive(false);
            zones[3].SetActive(true);
            StartCoroutine(FutureScreen1());
            StartCoroutine(FutureScreen2());
            StartCoroutine(FutureScreen3());
            inFuturistic = true;
            StartCoroutine(FuturisticTimer());
            Debug.LogFormat("[The Crystal Maze #{0}] Welcome to Futuristic world!", moduleId);
            Debug.LogFormat("[The Crystal Maze #{0}] The screen 1 words are {1}, {2} & {3}. The screen 1 colours are {4}, {5} & {6}.", moduleId, screen1Words[0], screen1Words[1], screen1Words[2], screen1ColorNames[0], screen1ColorNames[1], screen1ColorNames[2]);
            Debug.LogFormat("[The Crystal Maze #{0}] The screen 2 words are {1}, {2} & {3}. The screen 2 colours are {4}, {5} & {6}.", moduleId, screen2Words[0], screen2Words[1], screen2Words[2], screen2ColorNames[0], screen2ColorNames[1], screen2ColorNames[2]);
            Debug.LogFormat("[The Crystal Maze #{0}] The screen 3 words are {1}, {2} & {3}. The screen 3 colours are {4}, {5} & {6}.", moduleId, screen3Words[0], screen3Words[1], screen3Words[2], screen3ColorNames[0], screen3ColorNames[1], screen3ColorNames[2]);
            Debug.LogFormat("[The Crystal Maze #{0}] The screen 1 anomaly is {1} colour, {2} word.", moduleId, correctColorsLog[0], correctWords[0]);
            Debug.LogFormat("[The Crystal Maze #{0}] The screen 2 anomaly is {1} colour, {2} word.", moduleId, correctColorsLog[1], correctWords[1]);
            Debug.LogFormat("[The Crystal Maze #{0}] The screen 3 anomaly is {1} colour, {2} word.", moduleId, correctColorsLog[2], correctWords[2]);
        }
        else if (zone.name == "MedievalSelectable" && !medievalSolved)
        {
            zone.AddInteractionPunch();
            Audio.PlaySoundAtTransform("sting", transform);
            arrow[0].enabled = false;
            arrow[1].enabled = false;
            string begin = "";
            if (medievalTime >= 60)
            {
                begin = "1:";
            }
            else
            {
                begin = "0:";
            }
            medievalTimer.text = begin + (medievalTime % 60).ToString("00");
            surface.material = surfaceOptions[4];
            zones[0].SetActive(false);
            zones[4].SetActive(true);
            inMedieval = true;
            StartCoroutine(MedievalTimer());
            Debug.LogFormat("[The Crystal Maze #{0}] Welcome to Medieval world! Your chosen circles are {1} {2}; {3} {4}; {5} {6} & {7} {8}.", moduleId, circleColourName[0], clockwiseLog[0], circleColourName[1], clockwiseLog[1], circleColourName[2], clockwiseLog[2], circleColourName[3], clockwiseLog[3]);
            Debug.LogFormat("[The Crystal Maze #{0}] Press the target when the last digit of the game timer is {1}.", moduleId, pressTime);
        }
        else if (zone.name == "DomeSelectable" && collectedCrystals > 0)
        {
            zone.AddInteractionPunch();
            for (int i = 0; i <= 3; i++)
            {
                endOfZone[i] = false;
            }
            domeTime = collectedCrystals * 5;
            Debug.LogFormat("[The Crystal Maze #{0}] You have {1} seconds of time inside the Crystal Dome. Good luck.", moduleId, domeTime);
            surface.material = surfaceOptions[5];
            zones[0].SetActive(false);
            zones[5].SetActive(true);
            inDome = true;
            StartCoroutine(DomeTimer());
        }
    }

    public void AztecButton(KMSelectable button)
    {
        if (aztecSolved)
        {
            return;
        }
        button.AddInteractionPunch(0.5f);
        Audio.PlaySoundAtTransform("sand", transform);
        aztecClicks++;
        aztecWeightAdded += button.GetComponent<aztecButton>().buttonValue;
        if (aztecClicks % aztecReset == 0)
        {
            if (aztecButtonsScript[0].buttonValue == 1)
            {
                aztecButtonsScript[0].buttonValue = 2;
                aztecButtonsScript[1].buttonValue = 1;
                foreach (aztecButton azButton in aztecButtonsScript)
                {
                    azButton.buttonText.text = azButton.buttonValue.ToString() + " KG";
                }
            }
            else
            {
                aztecButtonsScript[0].buttonValue = 1;
                aztecButtonsScript[1].buttonValue = 2;
                foreach (aztecButton azButton in aztecButtonsScript)
                {
                    azButton.buttonText.text = azButton.buttonValue.ToString() + " KG";
                }
            }
        }
    }

    public void AztecAddPress()
    {
        if (aztecSolved)
        {
            return;
        }
        aztecAdd.AddInteractionPunch();
        Audio.PlaySoundAtTransform("drop", transform);
        aztecGuessText.text = aztecWeightAdded.ToString() + " KG";
        aztecSolved = true;
        if (TPActive)
        {
            aztecOffset.text = initialOffsetDisp;
        }
        inAztec = false;
    }

    public void DigitUpPress()
    {
        if (industrialSolved)
        {
            return;
        }
        digitUpButton.AddInteractionPunch(0.5f);
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        displayedDigit = (displayedDigit + 1) % 10;
        displayedDigitText.text = displayedDigit.ToString();
    }

    public void DigitDownPress()
    {
        if (industrialSolved)
        {
            return;
        }
        digitDownButton.AddInteractionPunch(0.5f);
        GetComponent<KMAudio>().PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
        displayedDigit = (displayedDigit + 9) % 10;
        displayedDigitText.text = displayedDigit.ToString();
    }

    public void ReversePress()
    {
        if (industrialSolved)
        {
            return;
        }
        reverseButton.AddInteractionPunch();
        if (displayedDigit == industrialZ)
        {
            Audio.PlaySoundAtTransform("gears", transform);
            cogAnimators[0].runtimeAnimatorController = cogAnimationOptions[1];
            cogAnimators[1].runtimeAnimatorController = cogAnimationOptions[0];
            cogAnimators[2].runtimeAnimatorController = cogAnimationOptions[1];
            cogAnimators[3].runtimeAnimatorController = cogAnimationOptions[0];
            cogAnimators[4].runtimeAnimatorController = cogAnimationOptions[1];
            platform.SetBool("solved", true);
            crystalAnimator.SetBool("solved", true);
            industrialCorrect = true;
        }
        else
        {
            Audio.PlaySoundAtTransform("crunch", transform);
            foreach (Animator anim in cogAnimators)
            {
                anim.enabled = false;
            }
        }
        industrialSolved = true;
        inIndustrial = false;
    }

    public void MedievalCirclePress()
    {
        if (medievalSolved)
        {
            return;
        }
        medievalCircle.AddInteractionPunch();
        Debug.LogFormat("[The Crystal Maze #{0}] You stopped the target when the last digit of the game timer was {1}.", moduleId, medievalTime % 10);
        if (medievalTime % 10 == pressTime)
        {
            arrow[0].enabled = true;
            Audio.PlaySoundAtTransform("arrow", transform);
            crystals[collectedCrystals].enabled = true;
            collectedCrystals++;
        }
        else
        {
            arrow[1].enabled = true;
            Audio.PlaySoundAtTransform("drop", transform);
        }
        medievalSolved = true;
        inMedieval = false;
    }

    public void FutureScreenPress(KMSelectable screen)
    {
        if (futuristicSolved || pressedScreens.Contains(screen.GetComponent<ScreenLabel>().screenLabel))
        {
            return;
        }
        screen.AddInteractionPunch();
        Audio.PlaySoundAtTransform("laser", transform);
        int selectedScreen = screen.GetComponent<ScreenLabel>().screenLabel;
        pressedScreens.Add(selectedScreen);
        wordSolved[selectedScreen - 1] = true;
        //Debug.LogFormat("[The Crystal Maze #{0}] Screen {1} was set as {2} colour, {3} word.", moduleId, selectedScreen, screen.GetComponentInChildren<TextMesh>().color, screen.GetComponentInChildren<TextMesh>().text);
        if (screen.GetComponentInChildren<TextMesh>().text == correctWords[selectedScreen - 1] && screen.GetComponentInChildren<TextMesh>().color == correctColors[selectedScreen - 1])
        {
            wordCorrect[selectedScreen - 1] = true;
        }
        if (wordSolved[0] && wordSolved[1] && wordSolved[2])
        {
            StartCoroutine(CheckScreens());
        }
    }

    IEnumerator CheckScreens()
    {
        futuristicSolved = true;
        yield return new WaitForSeconds(2f);
        if (wordCorrect[0] && wordCorrect[1] && wordCorrect[2])
        {
            crystals[collectedCrystals].enabled = true;
            collectedCrystals++;
            Debug.LogFormat("[The Crystal Maze #{0}] You selected the three anomalous words.", moduleId);
        }
        else
        {
            for (int i = 0; i <= 2; i++)
                if (wordCorrect[i])
                {
                    Debug.LogFormat("[The Crystal Maze #{0}] Word {1} was correct.", moduleId, i + 1);
                }
                else
                {
                    Debug.LogFormat("[The Crystal Maze #{0}] Word {1} was incorrect.", moduleId, i + 1);
                }
        }
        int flash = 0;
        while (flash < 15)
        {
            indicatorLightLeft[0].material = indicatorOptions[0];
            indicatorLightRight[0].material = indicatorOptions[0];
            indicatorLightLeft[1].material = indicatorOptions[0];
            indicatorLightRight[1].material = indicatorOptions[0];
            indicatorLightLeft[2].material = indicatorOptions[0];
            indicatorLightRight[2].material = indicatorOptions[0];
            yield return new WaitForSeconds(0.05f);
            for (int i = 0; i <= 2; i++)
            {
                if (wordCorrect[i])
                {
                    indicatorLightLeft[i].material = indicatorOptions[1];
                    indicatorLightRight[i].material = indicatorOptions[1];
                }
                else
                {
                    indicatorLightLeft[i].material = indicatorOptions[2];
                    indicatorLightRight[i].material = indicatorOptions[2];
                }
            }
            yield return new WaitForSeconds(0.05f);
            flash++;
        }
        inFuturistic = false;
    }

    IEnumerator AztecTimer()
    {
        string begin = "";
        while (aztecTime > 0 || !aztecSolved)
        {
            yield return new WaitForSeconds(1f);
            aztecTime -= 1;
            if (aztecTime >= 60)
            {
                begin = "1:";
            }
            else
            {
                begin = "0:";
            }
            aztecTimer.text = begin + (aztecTime % 60).ToString("00");
            if (aztecSolved || aztecTime == 0)
            {
                break;
            }
        }
        aztecSolved = true;
        if (TPActive)
        {
            aztecOffset.text = initialOffsetDisp;
        }
        inAztec = false;
        if (aztecWeightAdded == aztecTargetWeight)
        {
            seesaw.SetBool("right", true);
            crystals[collectedCrystals].enabled = true;
            collectedCrystals++;
        }
        else if (aztecWeightAdded < aztecTargetWeight || aztecGuessText.text == "?")
        {
            seesaw.SetBool("light", true);
        }
        else if (aztecWeightAdded > aztecTargetWeight)
        {
            seesaw.SetBool("heavy", true);
        }
        Debug.LogFormat("[The Crystal Maze #{0}] You added {1}kg to your sandbag.", moduleId, aztecWeightAdded);
        yield return new WaitForSeconds(5f);
        surface.material = surfaceOptions[0];
        zoneCrosses[0].enabled = true;
        zones[0].SetActive(true);
        zones[1].SetActive(false);
        endOfZone[0] = true;
        Debug.LogFormat("[The Crystal Maze #{0}] You have {1} crystals in total.", moduleId, collectedCrystals);
    }

    IEnumerator IndustrialTimer()
    {
        string begin = "";
        while (industrialTime > 0 || !industrialSolved)
        {
            yield return new WaitForSeconds(1f);
            if (industrialSolved || industrialTime == 0)
            {
                break;
            }
            industrialTime -= 1;
            if (industrialTime >= 60)
            {
                begin = "1:";
            }
            else
            {
                begin = "0:";
            }
            industrialTimer.text = begin + (industrialTime % 60).ToString("00");
        }
        yield return new WaitForSeconds(2f);
        if (industrialCorrect)
        {
            crystalObject.SetActive(false);
            crystals[collectedCrystals].enabled = true;
            collectedCrystals++;
            yield return new WaitForSeconds(1f);
        }
        Debug.LogFormat("[The Crystal Maze #{0}] You entered {1} as the reversal digit.", moduleId, displayedDigit);
        yield return new WaitForSeconds(1f);
        inIndustrial = false;
        surface.material = surfaceOptions[0];
        zoneCrosses[1].enabled = true;
        zones[0].SetActive(true);
        zones[2].SetActive(false);
        endOfZone[1] = true;
        Debug.LogFormat("[The Crystal Maze #{0}] You have {1} crystals in total.", moduleId, collectedCrystals);
    }

    IEnumerator FutureScreen1()
    {
        int startPosition = UnityEngine.Random.Range(0, 3);
        if (!TPActive)
        {
            tpDigit1.text = "";
        }
        while (!wordSolved[0])
        {
            startPosition++;
            startPosition = startPosition % 3;
            if (TPActive)
            {
                tpDigit1.text = "" + (startPosition + 1);
            }
            screenText[0].text = screen1Words[startPosition];
            screenText[0].color = screen1Colors[startPosition];
            yield return new WaitForSeconds(0.9f);
        }
        int flash = 0;
        while (flash < 12)
        {
            screenText[0].color = colorOptions[6];
            yield return new WaitForSeconds(0.05f);
            screenText[0].color = screen1Colors[startPosition];
            yield return new WaitForSeconds(0.05f);
            flash++;
        }
    }

    IEnumerator FutureScreen2()
    {
        int startPosition = UnityEngine.Random.Range(0, 3);
        if (!TPActive)
        {
            tpDigit2.text = "";
        }
        while (!wordSolved[1])
        {
            startPosition++;
            startPosition = startPosition % 3;
            if (TPActive)
            {
                tpDigit2.text = "" + (startPosition + 1);
            }
            screenText[1].text = screen2Words[startPosition];
            screenText[1].color = screen2Colors[startPosition];
            yield return new WaitForSeconds(0.6f);
        }
        int flash = 0;
        while (flash < 12)
        {
            screenText[1].color = colorOptions[6];
            yield return new WaitForSeconds(0.05f);
            screenText[1].color = screen2Colors[startPosition];
            yield return new WaitForSeconds(0.05f);
            flash++;
        }
    }

    IEnumerator FutureScreen3()
    {
        int startPosition = UnityEngine.Random.Range(0, 3);
        if (!TPActive)
        {
            tpDigit3.text = "";
        }
        while (!wordSolved[2])
        {
            startPosition++;
            startPosition = startPosition % 3;
            if (TPActive)
            {
                tpDigit3.text = "" + (startPosition + 1);
            }
            screenText[2].text = screen3Words[startPosition];
            screenText[2].color = screen3Colors[startPosition];
            yield return new WaitForSeconds(0.75f);
        }
        int flash = 0;
        while (flash < 12)
        {
            screenText[2].color = colorOptions[6];
            yield return new WaitForSeconds(0.05f);
            screenText[2].color = screen3Colors[startPosition];
            yield return new WaitForSeconds(0.05f);
            flash++;
        }
    }

    IEnumerator FuturisticTimer()
    {
        while (futuristicTime > 0 || !futuristicSolved)
        {
            yield return new WaitForSeconds(1f);
            if (futuristicSolved || futuristicTime == 0)
            {
                break;
            }
            futuristicTime -= 1;
            futuristicTimer.text = "0:" + futuristicTime.ToString("00");
        }
        futuristicSolved = true;
        inFuturistic = false;
        yield return new WaitForSeconds(5f);
        surface.material = surfaceOptions[0];
        zoneCrosses[2].enabled = true;
        zones[0].SetActive(true);
        zones[3].SetActive(false);
        endOfZone[2] = true;
        Debug.LogFormat("[The Crystal Maze #{0}] You have {1} crystals in total.", moduleId, collectedCrystals);
    }

    IEnumerator MedievalTimer()
    {
        string begin = "";
        while (medievalTime > 0 || !medievalSolved)
        {
            yield return new WaitForSeconds(1f);
            if (medievalSolved || medievalTime == 0)
            {
                break;
            }
            medievalTime -= 1;
            if (medievalTime >= 60)
            {
                begin = "1:";
            }
            else
            {
                begin = "0:";
            }
            medievalTimer.text = begin + (medievalTime % 60).ToString("00");
        }
        yield return new WaitForSeconds(3f);
        inMedieval = false;
        surface.material = surfaceOptions[0];
        zoneCrosses[3].enabled = true;
        zones[0].SetActive(true);
        zones[4].SetActive(false);
        endOfZone[3] = true;
        Debug.LogFormat("[The Crystal Maze #{0}] You have {1} crystals in total.", moduleId, collectedCrystals);
    }

    IEnumerator DomeTimer()
    {
        for (int i = 0; i < tokenAnimators.Count(); i++)
        {
            tokenObjects[i].parentObject.SetActive(false);
        }
        domeTimer.text = "0:" + domeTime.ToString("00");
        Debug.LogFormat("[The Crystal Maze #{0}] WILL YOU START THE FANS, PLEASE!", moduleId);
        Audio.PlaySoundAtTransform(domeThemes[collectedCrystals - 1].name, transform);
        yield return new WaitForSeconds(3f);
        for (int i = 0; i < tokenAnimators.Count(); i++)
        {
            tokenAnimators[i].enabled = true;
            tokenObjects[i].parentObject.SetActive(true);
        }
        yield return new WaitForSeconds(6f);
        domeTimerOn = true;
        StartCoroutine(ticketStage());
        while (domeTime > 0)
        {
            yield return new WaitForSeconds(1f);
            domeTime -= 1;
            domeTimer.text = "0:" + domeTime.ToString("00");
            if (domeTime % 5 == 0)
            {
                crystals[collectedCrystals - 1].enabled = false;
                collectedCrystals--;
                Audio.PlaySoundAtTransform("bong", transform);
            }
        }
        foreach (tokenScript token in tokenObjects)
        {
            token.parentObject.SetActive(false);
        }
        domeTimerOn = false;
        inDome = false;
        Debug.LogFormat("[The Crystal Maze #{0}] You collected {1} gold tokens and {2} silver tokens, making a total after deduction of {3} tokens.", moduleId, tokenCountValues[0], tokenCountValues[1], tokenCountValues[2]);
        yield return new WaitForSeconds(3f);
        if (tokenCountValues[2] >= 15)
        {
            Debug.LogFormat("[The Crystal Maze #{0}] Congratulations! You cracked the Crystal Maze. Module disarmed.", moduleId);
            GetComponent<KMBombModule>().HandlePass();
            zones[5].SetActive(false);
            surface.material = surfaceOptions[0];
            complete.SetActive(true);
            moduleSolved = true;
        }
        else
        {
            if (TPActive)
            {
                Debug.LogFormat("[The Crystal Maze #{0}] Bad luck old chums. Better luck next time. Game reset.", moduleId);
                Reset();
            }
            else
            {
                Debug.LogFormat("[The Crystal Maze #{0}] Strike! Bad luck old chums. Better luck next time. Game reset.", moduleId);
                GetComponent<KMBombModule>().HandleStrike();
                Reset();
            }
        }
    }

    void TokenPress(tokenScript token)
    {
        if (moduleSolved || !domeTimerOn)
        {
            return;
        }
        if (token.gold)
        {
            tokenCountValues[0]++;
        }
        else
        {
            tokenCountValues[1]++;
        }
        tokenCountValues[2] = tokenCountValues[0] - tokenCountValues[1];
        for (int i = 0; i <= 2; i++)
        {
            countDisplays[i].text = tokenCountValues[i].ToString();
        }
        token.parentObject.SetActive(false);
    }

    private IEnumerator ticketStage()
    {
        if (collectedCrystals == 1)
        {
            while (domeTime > 0)
            {
                int rando = UnityEngine.Random.Range(1, 101);
                if (rando < 25)
                {
                    tokenCountValues[0]++;
                    tokenCountValues[2]++;
                    countDisplays[0].text = "" + tokenCountValues[0];
                    countDisplays[2].text = "" + tokenCountValues[2];
                }
                else
                {
                    tokenCountValues[1]++;
                    tokenCountValues[2]--;
                    countDisplays[1].text = "" + tokenCountValues[1];
                    countDisplays[2].text = "" + tokenCountValues[2];
                }
                yield return new WaitForSeconds(0.25f);
            }
        }
        else if (collectedCrystals == 2)
        {
            while (domeTime > 0)
            {
                int rando = UnityEngine.Random.Range(1, 101);
                if (rando < 50)
                {
                    tokenCountValues[0]++;
                    tokenCountValues[2]++;
                    countDisplays[0].text = "" + tokenCountValues[0];
                    countDisplays[2].text = "" + tokenCountValues[2];
                }
                else
                {
                    tokenCountValues[1]++;
                    tokenCountValues[2]--;
                    countDisplays[1].text = "" + tokenCountValues[1];
                    countDisplays[2].text = "" + tokenCountValues[2];
                }
                yield return new WaitForSeconds(0.25f);
            }
        }
        else if (collectedCrystals == 3)
        {
            while (domeTime > 0)
            {
                int rando = UnityEngine.Random.Range(1, 101);
                if (rando < 75)
                {
                    tokenCountValues[0]++;
                    tokenCountValues[2]++;
                    countDisplays[0].text = "" + tokenCountValues[0];
                    countDisplays[2].text = "" + tokenCountValues[2];
                }
                else
                {
                    tokenCountValues[1]++;
                    tokenCountValues[2]--;
                    countDisplays[1].text = "" + tokenCountValues[1];
                    countDisplays[2].text = "" + tokenCountValues[2];
                }
                yield return new WaitForSeconds(0.25f);
            }
        }
        else if (collectedCrystals == 4)
        {
            while (domeTime > 0)
            {
                tokenCountValues[0]++;
                tokenCountValues[2]++;
                countDisplays[0].text = "" + tokenCountValues[0];
                countDisplays[2].text = "" + tokenCountValues[2];
                yield return new WaitForSeconds(0.25f);
            }
        }
    }

    void Reset()
    {
        for (int i = 0; i <= 2; i++)
        {
            tokenCountValues[i] = 0;
            countDisplays[i].text = tokenCountValues[i].ToString();
        }
        collectedCrystals = 0;
        aztecSolved = false;
        aztecTime = 59;
        if (TPActive)
        {
            aztecTime = 99;
        }
        aztecClicks = 0;
        aztecWeightAdded = 0;
        industrialSolved = false;
        industrialTime = 59;
        if (TPActive)
        {
            industrialTime = 89;
        }
        serialNumber = "";
        cogAnimators[0].runtimeAnimatorController = cogAnimationOptions[0];
        cogAnimators[1].runtimeAnimatorController = cogAnimationOptions[1];
        cogAnimators[2].runtimeAnimatorController = cogAnimationOptions[0];
        cogAnimators[3].runtimeAnimatorController = cogAnimationOptions[1];
        cogAnimators[4].runtimeAnimatorController = cogAnimationOptions[0];
        foreach (Animator anim in cogAnimators)
        {
            anim.enabled = true;
        }
        platform.SetBool("solved", false);
        crystalAnimator.SetBool("solved", false);
        platform.SetBool("reset", true);
        crystalAnimator.SetBool("reset", true);
        industrialCorrect = false;
        crystalObject.SetActive(true);
        futuristicSolved = false;
        futuristicTime = 59;
        pressedScreens.Clear();
        for (int i = 0; i <= 2; i++)
        {
            wordSolved[i] = false;
            wordCorrect[i] = false;
        }
        medievalSolved = false;
        medievalTime = 59;
        if (TPActive)
        {
            medievalTime = 89;
        }
        pressTime = 0;
        surface.material = surfaceOptions[0];
        Start();
    }

    //twitch plays
    bool TwitchPlaysActive;
    private bool TPActive;

    private IEnumerator delayDetection()
    {
        yield return new WaitForSeconds(1f);
        if (TwitchPlaysActive == true)
        {
            TPActive = true;
            aztecTime = 99;
            industrialTime = 89;
            medievalTime = 89;
        }
        else
        {
            TPActive = false;
        }
        Debug.LogFormat("[The Crystal Maze #{0}] Twitch Plays mode: {1}", moduleId, TPActive);
    }

    private bool secondIsNum(string s)
    {
        int temp = 0;
        bool check = int.TryParse(s, out temp);
        if (check == true)
        {
            if (temp > 0 && temp < 201)
            {
                return true;
            }
        }
        return false;
    }

    private bool secondIsVal(string s)
    {
        int temp = 0;
        bool check = int.TryParse(s, out temp);
        if (check == true)
        {
            if (temp > -1 && temp < 10)
            {
                return true;
            }
        }
        return false;
    }

    private bool validParams(string s, string s2)
    {
        int temp = 0;
        bool check = int.TryParse(s, out temp);
        if (check == true)
        {
            if (temp > 0 && temp < 4)
            {
                int temp2 = 0;
                bool check2 = int.TryParse(s2, out temp2);
                if (check2 == true)
                {
                    if (temp2 > 0 && temp2 < 4)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} aztec/industrial/futuristic/medieval (help) [Enters the specified world (Optionally include the word help to receive the commands for each world)] | !{0} dome [Enters the crystal dome]";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*aztec help\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return "sendtochat Aztec: !# left/right <num> [Presses the left or right button 'num' times] | !# add [Presses add button] | # = Module ID on Twitch";
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*industrial help\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return "sendtochat Industrial: !# reverse <num> [Reverses the machine with the number 'num'] | # = Module ID on Twitch";
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*futuristic help\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return "sendtochat Futuristic: !# screen <display> <digit> [Presses the specified display when the display shows the specified digit, displays are numbered 1-3 with 1 as topmost and 3 as bottommost] | # = Module ID on Twitch";
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*medieval help\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return "sendtochat Medieval: !# press <digit> [Presses the target when the game timer's last digit is 'digit'] | # = Module ID on Twitch";
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*aztec\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (inAztec == true || inIndustrial == true || inFuturistic == true || inMedieval == true || inDome == true)
            {
                yield return "sendtochat Reckless Rick: Sorry cohorts! You are already occupied, why travel elsewhere?";
            }
            else if (endOfZone[0] == true)
            {
                yield return "sendtochat Reckless Rick: Sorry cohorts! You have already been through the Aztec World!";
            }
            else
            {
                zoneAccess[0].OnInteract();
            }
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*industrial\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (inAztec == true || inIndustrial == true || inFuturistic == true || inMedieval == true || inDome == true)
            {
                yield return "sendtochat Reckless Rick: Sorry cohorts! You are already occupied, why travel elsewhere?";
            }
            else if (endOfZone[1] == true)
            {
                yield return "sendtochat Reckless Rick: Sorry cohorts! You have already been through the Industrial World!";
            }
            else
            {
                zoneAccess[1].OnInteract();
            }
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*futuristic\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (inAztec == true || inIndustrial == true || inFuturistic == true || inMedieval == true || inDome == true)
            {
                yield return "sendtochat Reckless Rick: Sorry cohorts! You are already occupied, why travel elsewhere?";
            }
            else if (endOfZone[2] == true)
            {
                yield return "sendtochat Reckless Rick: Sorry cohorts! You have already been through the Futuristic World!";
            }
            else
            {
                zoneAccess[2].OnInteract();
            }
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*medieval\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (inAztec == true || inIndustrial == true || inFuturistic == true || inMedieval == true || inDome == true)
            {
                yield return "sendtochat Reckless Rick: Sorry cohorts! You are already occupied, why travel elsewhere?";
            }
            else if (endOfZone[3] == true)
            {
                yield return "sendtochat Reckless Rick: Sorry cohorts! You have already been through the Medieval World!";
            }
            else
            {
                zoneAccess[3].OnInteract();
            }
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*dome\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (inAztec == true || inIndustrial == true || inFuturistic == true || inMedieval == true || inDome == true)
            {
                yield return "sendtochat Reckless Rick: Sorry cohorts! You are already occupied, why travel elsewhere?";
            }
            else if (collectedCrystals < 1)
            {
                yield return "sendtochat Reckless Rick: I'm sorry cohort! You don't have enough crystals to enter the Crystal Dome! (At least 1 required to enter)";
            }
            else
            {
                yield return "sendtochat Reckless Rick: Good luck in the Crystal Dome cohorts!";
                zoneAccess[4].OnInteract();
            }
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*add\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            if (inAztec == true)
            {
                aztecAdd.OnInteract();
            }
            else
            {
                yield return "sendtochat Reckless Rick: Sorry cohorts! You are not currently in the Aztec World!";
            }
            yield break;
        }
        string[] parameters = command.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*left\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if(parameters.Length == 2)
            {
                if (secondIsNum(parameters[1]))
                {
                    if (inAztec == true)
                    {
                        yield return null;
                        int count = 0;
                        int goal = 0;
                        int.TryParse(parameters[1], out goal);
                        while (count < goal)
                        {
                            aztecButtons[0].OnInteract();
                            count++;
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else
                    {
                        yield return "sendtochat Reckless Rick: Sorry cohorts! You are not currently in the Aztec World!";
                    }
                }
            }
        }
        if (Regex.IsMatch(parameters[0], @"^\s*right\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if(parameters.Length == 2)
            {
                if (secondIsNum(parameters[1]))
                {
                    if (inAztec == true)
                    {
                        yield return null;
                        int count = 0;
                        int goal = 0;
                        int.TryParse(parameters[1], out goal);
                        while (count < goal)
                        {
                            aztecButtons[1].OnInteract();
                            count++;
                            yield return new WaitForSeconds(0.1f);
                        }
                    }
                    else
                    {
                        yield return "sendtochat Reckless Rick: Sorry cohorts! You are not currently in the Aztec World!";
                    }
                }
            }
        }
        if (Regex.IsMatch(parameters[0], @"^\s*reverse\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length == 2)
            {
                if (secondIsVal(parameters[1]))
                {
                    if (inIndustrial == true)
                    {
                        yield return null;
                        int goal = 0;
                        int.TryParse(parameters[1], out goal);
                        while (goal != displayedDigit)
                        {
                            digitUpButton.OnInteract();
                            yield return new WaitForSeconds(0.1f);
                        }
                        reverseButton.OnInteract();
                    }
                    else
                    {
                        yield return "sendtochat Reckless Rick: Sorry cohorts! You are not currently in the Industrial World!";
                    }
                }
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*screen\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length == 3)
            {
                if (validParams(parameters[1], parameters[2]))
                {
                    if (inFuturistic == true)
                    {
                        yield return null;
                        if (parameters[1].Equals("1"))
                        {
                            while (parameters[2] != tpDigit1.text)
                            {
                                yield return new WaitForSeconds(0.1f);
                            }
                            screenButtons[0].OnInteract();
                        }
                        else if (parameters[1].Equals("2"))
                        {
                            while (parameters[2] != tpDigit2.text)
                            {
                                yield return new WaitForSeconds(0.1f);
                            }
                            screenButtons[1].OnInteract();
                        }
                        else if (parameters[1].Equals("3"))
                        {
                            while (parameters[2] != tpDigit3.text)
                            {
                                yield return new WaitForSeconds(0.1f);
                            }
                            screenButtons[2].OnInteract();
                        }
                    }
                    else
                    {
                        yield return "sendtochat Reckless Rick: Sorry cohorts! You are not currently in the Futuristic World!";
                    }
                }
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*press\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length == 2)
            {
                if (secondIsVal(parameters[1]))
                {
                    if (inMedieval == true)
                    {
                        yield return null;
                        int goal = 0;
                        int.TryParse(parameters[1], out goal);
                        while (goal != (medievalTime % 10))
                        {
                            yield return new WaitForSeconds(0.1f);
                        }
                        medievalCircle.OnInteract();
                    }
                    else
                    {
                        yield return "sendtochat Reckless Rick: Sorry cohorts! You are not currently in the Medieval World!";
                    }
                }
            }
            yield break;
        }
    }
}