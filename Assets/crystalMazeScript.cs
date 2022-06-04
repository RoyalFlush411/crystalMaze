using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class crystalMazeScript : MonoBehaviour
{
    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMColorblindMode Colorblind;
    public TextMesh[] colorblindtexts;
    public TextMesh[] colorblindtexts2;
    private bool colorblindActive = false;
    private bool activated = false;
    private bool autosolving = false;
    private IDictionary<string, object> tpAPI;

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
    public TextMesh[] tpFrontNums;
    public TextMesh[] tpBackNums;
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
    private int aztecTime = 59;
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
        GetComponent<KMBombModule>().OnActivate = OnActivate;
    }

    void Update()
    {
        if (endOfZone[0] && endOfZone[1] && endOfZone[2] && endOfZone[3] && collectedCrystals == 0)
        {
            Debug.LogFormat("[The Crystal Maze #{0}] Strike! You have attempted every game and not secured any crystals. Game reset.", moduleId);
            GetComponent<KMBombModule>().HandleStrike();
            Reset();
        }
    }

    void Start()
    {
        Debug.LogFormat("[The Crystal Maze #{0}] Welcome to The Crystal Maze! Reckless Rick at your service!", moduleId);
        if (!activated)
        {
            colorblindActive = Colorblind.ColorblindModeActive;
            Debug.LogFormat("[The Crystal Maze #{0}] Colorblind mode: {1}", moduleId, colorblindActive);
        }
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

    void OnActivate()
    {
        if (TwitchPlaysActive)
        {
            TPActive = true;
            if (!Application.isEditor)
            {
                GameObject tpAPIGameObject = GameObject.Find("TwitchPlays_Info");
                if (tpAPIGameObject != null)
                    tpAPI = tpAPIGameObject.GetComponent<IDictionary<string, object>>();
                else
                    TPActive = false;
            }
        }
        else
        {
            TPActive = false;
        }
        Debug.LogFormat("[The Crystal Maze #{0}] Twitch Plays mode: {1}", moduleId, TPActive);
        activated = true;
    }

    void SelectTokenAnimations()
    {
        for (int i = 0; i < tokenAnimators.Count(); i++)
        {
            int index = Random.Range(0, 7);
            tokenAnimators[i].runtimeAnimatorController = animationOptions[index];
        }
        for (int i = 0; i <= 34; i++)
        {
            int index = Random.Range(0, tokenAnimators.Count());
            while (goldTokens.Contains(index))
            {
                index = Random.Range(0, tokenAnimators.Count());
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
        aztecReset = Random.Range(15, 21);
        aztecTargetWeight = Random.Range(125, 176);
        aztecTargetText.text = aztecTargetWeight.ToString() + " KG";
        int aztecStartIndex = Random.Range(0, 2);
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
            int colourIndex = Random.Range(0, 6);
            circles[i].material = circleColourOptions[colourIndex];
            circleColourName[i] = circleColourNameOptions[colourIndex];

            int rotationIndex = Random.Range(0, 2);
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
        int option = Random.Range(0, 2);
        if (option == 0)
        {
            int screen1Word1 = Random.Range(0, 6);
            screen1Words[0] = wordOptions[screen1Word1];
            screen1Colors[0] = colorOptions[screen1Word1];
            screen1ColorNames[0] = wordOptions[screen1Word1];

            int screen1Word2 = Random.Range(0, 6);
            while (screen1Word2 == screen1Word1)
            {
                screen1Word2 = Random.Range(0, 6);
            }
            screen1Words[1] = wordOptions[screen1Word2];
            screen1Colors[1] = colorOptions[screen1Word2];
            screen1ColorNames[1] = wordOptions[screen1Word2];

            int screen1Word3 = Random.Range(0, 6);
            while (screen1Word3 == screen1Word1 || screen1Word3 == screen1Word2)
            {
                screen1Word3 = Random.Range(0, 6);
            }
            screen1Words[2] = wordOptions[screen1Word3];
            int screen1Color3 = Random.Range(0, 6);
            while (screen1Color3 == screen1Word3)
            {
                screen1Color3 = Random.Range(0, 6);
            }
            screen1Colors[2] = colorOptions[screen1Color3];
            screen1ColorNames[2] = wordOptions[screen1Color3];
            correctWords[0] = wordOptions[screen1Word3];
            correctColors[0] = colorOptions[screen1Color3];
            correctColorsLog[0] = wordOptions[screen1Color3];
        }
        else
        {
            int screen1Word1 = Random.Range(0, 6);
            screen1Words[0] = wordOptions[screen1Word1];
            screen1Colors[0] = colorOptions[screen1Word1];
            screen1ColorNames[0] = wordOptions[screen1Word1];

            int screen1Word2 = Random.Range(0, 6);
            while (screen1Word2 == screen1Word1)
            {
                screen1Word2 = Random.Range(0, 6);
            }
            screen1Words[1] = wordOptions[screen1Word2];
            int screen1Color2 = Random.Range(0, 6);
            while (screen1Color2 == screen1Word2)
            {
                screen1Color2 = Random.Range(0, 6);
            }
            screen1Colors[1] = colorOptions[screen1Color2];
            screen1ColorNames[1] = wordOptions[screen1Color2];

            int screen1Word3 = Random.Range(0, 6);
            while (screen1Word3 == screen1Word1 || screen1Word3 == screen1Word2)
            {
                screen1Word3 = Random.Range(0, 6);
            }
            screen1Words[2] = wordOptions[screen1Word3];
            int screen1Color3 = Random.Range(0, 6);
            while (screen1Color3 == screen1Word3)
            {
                screen1Color3 = Random.Range(0, 6);
            }
            screen1Colors[2] = colorOptions[screen1Color3];
            screen1ColorNames[2] = wordOptions[screen1Color3];
            correctWords[0] = wordOptions[screen1Word1];
            correctColors[0] = colorOptions[screen1Word1];
            correctColorsLog[0] = wordOptions[screen1Word1];
        }

        int option2 = Random.Range(0, 2);
        if (option2 == 0)
        {
            int screen2Word1 = Random.Range(0, 6);
            screen2Words[0] = wordOptions[screen2Word1];
            screen2Colors[0] = colorOptions[screen2Word1];
            screen2ColorNames[0] = wordOptions[screen2Word1];

            int screen2Word2 = Random.Range(0, 6);
            while (screen2Word2 == screen2Word1)
            {
                screen2Word2 = Random.Range(0, 6);
            }
            screen2Words[1] = wordOptions[screen2Word2];
            screen2Colors[1] = colorOptions[screen2Word2];
            screen2ColorNames[1] = wordOptions[screen2Word2];

            int screen2Word3 = Random.Range(0, 6);
            while (screen2Word3 == screen2Word1 || screen2Word3 == screen2Word2)
            {
                screen2Word3 = Random.Range(0, 6);
            }
            screen2Words[2] = wordOptions[screen2Word3];
            int screen2Color3 = Random.Range(0, 6);
            while (screen2Color3 == screen2Word3)
            {
                screen2Color3 = Random.Range(0, 6);
            }
            screen2Colors[2] = colorOptions[screen2Color3];
            screen2ColorNames[2] = wordOptions[screen2Color3];
            correctWords[1] = wordOptions[screen2Word3];
            correctColors[1] = colorOptions[screen2Color3];
            correctColorsLog[1] = wordOptions[screen2Color3];
        }
        else
        {
            int screen2Word1 = Random.Range(0, 6);
            screen2Words[0] = wordOptions[screen2Word1];
            screen2Colors[0] = colorOptions[screen2Word1];
            screen2ColorNames[0] = wordOptions[screen2Word1];

            int screen2Word2 = Random.Range(0, 6);
            while (screen2Word2 == screen2Word1)
            {
                screen2Word2 = Random.Range(0, 6);
            }
            screen2Words[1] = wordOptions[screen2Word2];
            int screen2Color2 = Random.Range(0, 6);
            while (screen2Color2 == screen2Word2)
            {
                screen2Color2 = Random.Range(0, 6);
            }
            screen2Colors[1] = colorOptions[screen2Color2];
            screen2ColorNames[1] = wordOptions[screen2Color2];

            int screen2Word3 = Random.Range(0, 6);
            while (screen2Word3 == screen2Word1 || screen2Word3 == screen2Word2)
            {
                screen2Word3 = Random.Range(0, 6);
            }
            screen2Words[2] = wordOptions[screen2Word3];
            int screen2Color3 = Random.Range(0, 6);
            while (screen2Color3 == screen2Word3)
            {
                screen2Color3 = Random.Range(0, 6);
            }
            screen2Colors[2] = colorOptions[screen2Color3];
            screen2ColorNames[2] = wordOptions[screen2Color3];
            correctWords[1] = wordOptions[screen2Word1];
            correctColors[1] = colorOptions[screen2Word1];
            correctColorsLog[1] = wordOptions[screen2Word1];
        }

        int option3 = Random.Range(0, 2);
        if (option3 == 0)
        {
            int screen3Word1 = Random.Range(0, 6);
            screen3Words[0] = wordOptions[screen3Word1];
            screen3Colors[0] = colorOptions[screen3Word1];
            screen3ColorNames[0] = wordOptions[screen3Word1];

            int screen3Word2 = Random.Range(0, 6);
            while (screen3Word2 == screen3Word1)
            {
                screen3Word2 = Random.Range(0, 6);
            }
            screen3Words[1] = wordOptions[screen3Word2];
            screen3Colors[1] = colorOptions[screen3Word2];
            screen3ColorNames[1] = wordOptions[screen3Word2];

            int screen3Word3 = Random.Range(0, 6);
            while (screen3Word3 == screen3Word1 || screen3Word3 == screen3Word2)
            {
                screen3Word3 = Random.Range(0, 6);
            }
            screen3Words[2] = wordOptions[screen3Word3];
            int screen3Color3 = Random.Range(0, 6);
            while (screen3Color3 == screen3Word3)
            {
                screen3Color3 = Random.Range(0, 6);
            }
            screen3Colors[2] = colorOptions[screen3Color3];
            screen3ColorNames[2] = wordOptions[screen3Color3];
            correctWords[2] = wordOptions[screen3Word3];
            correctColors[2] = colorOptions[screen3Color3];
            correctColorsLog[2] = wordOptions[screen3Color3];
        }
        else
        {
            int screen3Word1 = Random.Range(0, 6);
            screen3Words[0] = wordOptions[screen3Word1];
            screen3Colors[0] = colorOptions[screen3Word1];
            screen3ColorNames[0] = wordOptions[screen3Word1];

            int screen3Word2 = Random.Range(0, 6);
            while (screen3Word2 == screen3Word1)
            {
                screen3Word2 = Random.Range(0, 6);
            }
            screen3Words[1] = wordOptions[screen3Word2];
            int screen3Color2 = Random.Range(0, 6);
            while (screen3Color2 == screen3Word2)
            {
                screen3Color2 = Random.Range(0, 6);
            }
            screen3Colors[1] = colorOptions[screen3Color2];
            screen3ColorNames[1] = wordOptions[screen3Color2];

            int screen3Word3 = Random.Range(0, 6);
            while (screen3Word3 == screen3Word1 || screen3Word3 == screen3Word2)
            {
                screen3Word3 = Random.Range(0, 6);
            }
            screen3Words[2] = wordOptions[screen3Word3];
            int screen3Color3 = Random.Range(0, 6);
            while (screen3Color3 == screen3Word3)
            {
                screen3Color3 = Random.Range(0, 6);
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
            chosenSerialIndices[i] = Random.Range(1, 27);
        }
        chosenSerialIndices[2] = Random.Range(6, 11);
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
            aztecTimer.text = "0:" + (aztecTime % 60).ToString("00");
            aztecGuessText.text = "?";
            surface.material = surfaceOptions[1];
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
            industrialTimer.text = "0:" + (industrialTime % 60).ToString("00");
            displayedDigit = Random.Range(0, 10);
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
            medievalTimer.text = "0:" + (medievalTime % 60).ToString("00");
            surface.material = surfaceOptions[4];
            if (colorblindActive)
            {
                for(int i = 0; i < 4; i++)
                {
                    if (circleColourName[i].Equals("brown"))
                    {
                        colorblindtexts[i].text = "N";
                    }
                    else
                    {
                        colorblindtexts[i].text = (circleColourName[i].ElementAt(0) + "").ToUpper();
                    }
                }
            }
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
    }

    IEnumerator AztecTimer()
    {
        while (aztecTime > 0 || !aztecSolved)
        {
            yield return new WaitForSeconds(1f);
            aztecTime -= 1;
            aztecTimer.text = "0:" + (aztecTime % 60).ToString("00");
            if (aztecSolved || aztecTime == 0)
            {
                break;
            }
        }
        aztecSolved = true;
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
        inAztec = false;
        endOfZone[0] = true;
        Debug.LogFormat("[The Crystal Maze #{0}] You have {1} crystals in total.", moduleId, collectedCrystals);
    }

    IEnumerator IndustrialTimer()
    {
        while (industrialTime > 0 || !industrialSolved)
        {
            yield return new WaitForSeconds(1f);
            if (industrialSolved || industrialTime == 0)
            {
                break;
            }
            industrialTime -= 1;
            industrialTimer.text = "0:" + (industrialTime % 60).ToString("00");
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
        surface.material = surfaceOptions[0];
        zoneCrosses[1].enabled = true;
        zones[0].SetActive(true);
        zones[2].SetActive(false);
        inIndustrial = false;
        endOfZone[1] = true;
        Debug.LogFormat("[The Crystal Maze #{0}] You have {1} crystals in total.", moduleId, collectedCrystals);
    }

    IEnumerator FutureScreen1()
    {
        int startPosition = Random.Range(0, 3);
        int tpPosition = Random.Range(1, 4);
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
                if(tpPosition > 3)
                {
                    tpPosition = 1;
                }
                tpDigit1.text = "" + tpPosition;
                tpPosition++;
            }
            if (colorblindActive)
            {
                colorblindtexts2[0].text = screen1ColorNames[startPosition].ElementAt(0)+"";
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
        int startPosition = Random.Range(0, 3);
        int tpPosition = Random.Range(1, 4);
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
                if (tpPosition > 3)
                {
                    tpPosition = 1;
                }
                tpDigit2.text = "" + tpPosition;
                tpPosition++;
            }
            if (colorblindActive)
            {
                colorblindtexts2[1].text = screen2ColorNames[startPosition].ElementAt(0) + "";
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
        int startPosition = Random.Range(0, 3);
        int tpPosition = Random.Range(1, 4);
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
                if (tpPosition > 3)
                {
                    tpPosition = 1;
                }
                tpDigit3.text = "" + tpPosition;
                tpPosition++;
            }
            if (colorblindActive)
            {
                colorblindtexts2[2].text = screen3ColorNames[startPosition].ElementAt(0) + "";
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
        yield return new WaitForSeconds(5f);
        surface.material = surfaceOptions[0];
        zoneCrosses[2].enabled = true;
        zones[0].SetActive(true);
        zones[3].SetActive(false);
        inFuturistic = false;
        endOfZone[2] = true;
        Debug.LogFormat("[The Crystal Maze #{0}] You have {1} crystals in total.", moduleId, collectedCrystals);
    }

    IEnumerator MedievalTimer()
    {
        while (medievalTime > 0 || !medievalSolved)
        {
            yield return new WaitForSeconds(1f);
            if (medievalSolved || medievalTime == 0)
            {
                break;
            }
            medievalTime -= 1;
            medievalTimer.text = "0:" + (medievalTime % 60).ToString("00");
        }
        yield return new WaitForSeconds(3f);
        if (colorblindActive)
        {
            for (int i = 0; i < 4; i++)
            {
                colorblindtexts[i].text = "";
            }
        }
        surface.material = surfaceOptions[0];
        zoneCrosses[3].enabled = true;
        zones[0].SetActive(true);
        zones[4].SetActive(false);
        inMedieval = false;
        endOfZone[3] = true;
        Debug.LogFormat("[The Crystal Maze #{0}] You have {1} crystals in total.", moduleId, collectedCrystals);
    }

    IEnumerator DomeTimer()
    {
        if (TPActive)
            tokenlist.Clear();
        for (int i = 0; i < tokenAnimators.Count(); i++)
        {
            tokenObjects[i].parentObject.SetActive(false);
            if (TPActive)
                tokenlist.Add(i + 1);
        }
        if (TPActive)
            tokenlist = tokenlist.Shuffle();
        domeTimer.text = "0:" + domeTime.ToString("00");
        Debug.LogFormat("[The Crystal Maze #{0}] WILL YOU START THE FANS, PLEASE!", moduleId);
        Audio.PlaySoundAtTransform(domeThemes[collectedCrystals - 1].name, transform);
        yield return new WaitForSeconds(3f);
        for (int i = 0; i < tokenAnimators.Count(); i++)
        {
            tokenAnimators[i].enabled = true;
            tokenObjects[i].parentObject.SetActive(true);
            if (TPActive)
            {
                if (tokenlist[i] == 6 || tokenlist[i] == 9)
                {
                    tpFrontNums[i].text = "#"+tokenlist[i].ToString();
                    tpBackNums[i].text = "#"+tokenlist[i].ToString();
                }
                else
                {
                    tpFrontNums[i].text = tokenlist[i].ToString();
                    tpBackNums[i].text = tokenlist[i].ToString();
                }
            }
        }
        yield return new WaitForSeconds(6f);
        domeTimerOn = true;
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
        if (TPActive && !Application.isEditor && !autosolving)
        {
            if (tokenCountValues[2] >= 10)
                tpAPI["ircConnectionSendMessage"] = "Reckless Rick: Congratulations cohorts! You made it through the Crystal Dome!";
            else
                tpAPI["ircConnectionSendMessage"] = "Reckless Rick: Sorry cohorts! You did not survive the Crystal Dome! Better luck next time!";
        }
        Debug.LogFormat("[The Crystal Maze #{0}] You collected {1} gold tokens and {2} silver tokens, making a total after deduction of {3} tokens.", moduleId, tokenCountValues[0], tokenCountValues[1], tokenCountValues[2]);
        yield return new WaitForSeconds(3f);
        int ct = 15;
        if (TPActive)
            ct = 10;
        if (tokenCountValues[2] >= ct)
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
            Debug.LogFormat("[The Crystal Maze #{0}] Strike! Bad luck old chums. Better luck next time. Game reset.", moduleId);
            GetComponent<KMBombModule>().HandleStrike();
            Reset();
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

    void Reset()
    {
        for (int i = 0; i <= 2; i++)
        {
            tokenCountValues[i] = 0;
            countDisplays[i].text = tokenCountValues[i].ToString();
        }
        collectedCrystals = 0;
        aztecSolved = false;
        aztecSwitch = false;
        aztecTime = 59;
        aztecClicks = 0;
        aztecWeightAdded = 0;
        industrialSolved = false;
        industrialTime = 59;
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
        pressTime = 0;
        surface.material = surfaceOptions[0];
        Start();
    }

    //twitch plays
    bool TwitchPlaysActive;
    private bool TPActive;
    private bool aztecSwitch = false;

    private List<int> tokenlist = new List<int>();

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

    private bool validTokens(string[] prms)
    {
        List<string> used = new List<string>();
        int temp = 0;
        for (int i = 1; i < prms.Length; i++)
        {
            if (!int.TryParse(prms[i], out temp))
                return false;
            if (temp < 1 || temp > 70)
                return false;
            if (used.Contains(prms[i]))
                return false;
            if (!tokenObjects[tokenlist.IndexOf(temp)].parentObject.activeSelf)
                return false;
            used.Add(prms[i]);
        }
        return true;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} aztec/industrial/futuristic/medieval/dome (help) [Enters the specified world or the Crystal Dome (Optionally include the word help to receive the commands for a world or the Crystal Dome)] | !{0} colorblind [Toggles colorblind mode] | On Twitch Plays in the Crystal Dome only 10 tokens are needed after deduction instead of 15";
    #pragma warning restore 414
    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*colorblind\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            Debug.LogFormat("[The Crystal Maze #{0}] Toggled Colorblind mode! (TP)", moduleId);
            if (colorblindActive)
            {
                if (inFuturistic)
                {
                    for (int i = 0; i < 3; i++)
                    {
                        colorblindtexts2[i].text = "";
                    }
                }
                else if (inMedieval)
                {
                    for(int i = 0; i < 4; i++)
                    {
                        colorblindtexts[i].text = "";
                    }
                }
                colorblindActive = false;
            }
            else
            {
                if (inMedieval)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (circleColourName[i].Equals("brown"))
                        {
                            colorblindtexts[i].text = "N";
                        }
                        else
                        {
                            colorblindtexts[i].text = (circleColourName[i].ElementAt(0) + "").ToUpper();
                        }
                    }
                }
                colorblindActive = true;
            }
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*dome help\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return "sendtochat The Crystal Dome: !{1} grab <num> [Grabs token number 'num'] | Multiple tokens can be grabbed with spaces between each token number | Valid token numbers are 1-70 | A number sign (#) is put before token numbers 6 and 9 for distinction";
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*aztec help\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return "sendtochat Aztec: !{1} left/right <num> [Presses the left or right button 'num' times] | !{1} add [Presses add button] | Commands are chainable with semicolons or commas | The first time the buttons switch presses will stop and the number of presses made before the switch will be output to chat";
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*industrial help\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return "sendtochat Industrial: !{1} reverse <num> [Reverses the machine with the number 'num']";
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*futuristic help\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return "sendtochat Futuristic: !{1} screen <display> <digit> [Presses the specified display when the display shows the specified digit, displays are numbered 1-3 with 1 as topmost and 3 as bottommost]";
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*medieval help\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return "sendtochat Medieval: !{1} press <digit> [Presses the target when the game timer's last digit is 'digit']";
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
                yield return "sendtochat Reckless Rick: I'm sorry cohorts! You don't have enough crystals to enter the Crystal Dome! (At least 1 required to enter)";
            }
            else
            {
                if (!autosolving)
                    yield return "sendtochat Reckless Rick: Good luck in the Crystal Dome cohorts!";
                zoneAccess[4].OnInteract();
            }
            yield break;
        }
        string removeSpaces = Regex.Replace(command, @"\s+", " ");
        string[] parameters = removeSpaces.Split(' ');
        if (Regex.IsMatch(parameters[0], @"^\s*grab\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length >= 2)
            {
                if (validTokens(parameters))
                {
                    yield return null;
                    if (inDome == true)
                    {
                        if (domeTimerOn == true)
                        {
                            int token = 0;
                            for (int i = 1; i < parameters.Length; i++)
                            {
                                token = int.Parse(parameters[i]);
                                for (int j = 0; j < tokenlist.Count; j++)
                                {
                                    if (token == tokenlist[j])
                                    {
                                        tokenObjects[j].selectable.OnInteract();
                                        break;
                                    }
                                }
                                yield return new WaitForSeconds(0.05f);
                                if (domeTimerOn == false)
                                    yield break;
                            }
                        }
                        else
                        {
                            yield return "sendtochat Reckless Rick: Sorry cohorts! You must wait until the whistle blows before collecting any tokens!";
                        }
                    }
                    else
                    {
                        yield return "sendtochat Reckless Rick: Sorry cohorts! You are not currently in the Crystal Dome!";
                    }
                }
            }
            yield break;
        }
        if (Regex.IsMatch(parameters[0], @"^\s*reverse\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            if (parameters.Length == 2)
            {
                if (secondIsVal(parameters[1]))
                {
                    yield return null;
                    if (inIndustrial == true)
                    {
                        int goal = 0;
                        int.TryParse(parameters[1], out goal);
                        int ct1 = 0, ct2 = 0;
                        int index = displayedDigit;
                        while (index != goal)
                        {
                            ct1++;
                            index++;
                            if (index > 9)
                                index = 0;
                        }
                        index = displayedDigit;
                        while (index != goal)
                        {
                            ct2++;
                            index--;
                            if (index < 0)
                                index = 9;
                        }
                        if (ct1 < ct2)
                        {
                            for (int i = 0; i < ct1; i++)
                            {
                                digitUpButton.OnInteract();
                                yield return new WaitForSeconds(0.1f);
                            }
                        }
                        else if (ct1 > ct2)
                        {
                            for (int i = 0; i < ct2; i++)
                            {
                                digitDownButton.OnInteract();
                                yield return new WaitForSeconds(0.1f);
                            }
                        }
                        else
                        {
                            int rand = Random.Range(0, 2);
                            for (int i = 0; i < (rand == 0 ? ct1 : ct2); i++)
                            {
                                if (rand == 0)
                                    digitUpButton.OnInteract();
                                else
                                    digitDownButton.OnInteract();
                                yield return new WaitForSeconds(0.1f);
                            }
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
                        if (parameters[1].Equals("1") && !pressedScreens.Contains(screenButtons[0].GetComponent<ScreenLabel>().screenLabel))
                        {
                            yield return null;
                            while (parameters[2] != tpDigit1.text)
                            {
                                yield return new WaitForSeconds(0.1f);
                            }
                            screenButtons[0].OnInteract();
                        }
                        else if (parameters[1].Equals("2") && !pressedScreens.Contains(screenButtons[1].GetComponent<ScreenLabel>().screenLabel))
                        {
                            yield return null;
                            while (parameters[2] != tpDigit2.text)
                            {
                                yield return new WaitForSeconds(0.1f);
                            }
                            screenButtons[1].OnInteract();
                        }
                        else if (parameters[1].Equals("3") && !pressedScreens.Contains(screenButtons[2].GetComponent<ScreenLabel>().screenLabel))
                        {
                            yield return null;
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
                    yield return null;
                    if (inMedieval == true)
                    {
                        int goal = 0;
                        int.TryParse(parameters[1], out goal);
                        while (goal != (medievalTime % 10))
                        {
                            yield return new WaitForSeconds(0.1f);
                            yield return "trycancel Target press cancelled due to a cancel request.";
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
        //chained aztec support
        string[] parametersaztec = command.Split(';', ',');
        for (int i = 0; i < parametersaztec.Length; i++)
        {
            parametersaztec[i] = parametersaztec[i].Trim();
            if (!Regex.IsMatch(parametersaztec[i], @"^\s*add\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) && !Regex.IsMatch(parametersaztec[i], @"^\s*left ([1-9]|[1-8][0-9]|9[0-9]|1[0-9]{2}|200)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant) && !Regex.IsMatch(parametersaztec[i], @"^\s*right ([1-9]|[1-8][0-9]|9[0-9]|1[0-9]{2}|200)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                yield break;
            }
        }
        for (int i = 0; i < parametersaztec.Length; i++)
        {
            if (Regex.IsMatch(parametersaztec[i], @"^\s*add\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
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
            if (Regex.IsMatch(parametersaztec[i], @"^\s*left ([1-9]|[1-8][0-9]|9[0-9]|1[0-9]{2}|200)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                yield return null;
                if (inAztec == true)
                {
                    int count = 0;
                    int goal = 0;
                    int.TryParse(parametersaztec[i].Split(' ').ElementAt(1), out goal);
                    while (count < goal)
                    {
                        aztecButtons[0].OnInteract();
                        count++;
                        yield return new WaitForSeconds(0.05f);
                        if (!aztecSwitch && aztecClicks >= aztecReset)
                        {
                            aztecSwitch = true;
                            yield return "sendtochat The buttons have switched after " + aztecReset + " presses!";
                            yield break;
                        }
                    }
                }
                else
                {
                    yield return "sendtochat Reckless Rick: Sorry cohorts! You are not currently in the Aztec World!";
                }
            }
            if (Regex.IsMatch(parametersaztec[i], @"^\s*right ([1-9]|[1-8][0-9]|9[0-9]|1[0-9]{2}|200)\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
            {
                yield return null;
                if (inAztec == true)
                {
                    int count = 0;
                    int goal = 0;
                    int.TryParse(parametersaztec[i].Split(' ').ElementAt(1), out goal);
                    while (count < goal)
                    {
                        aztecButtons[1].OnInteract();
                        count++;
                        yield return new WaitForSeconds(0.05f);
                        if (!aztecSwitch && aztecClicks >= aztecReset)
                        {
                            aztecSwitch = true;
                            yield return "sendtochat The buttons have switched after " + aztecReset + " presses!";
                            yield break;
                        }
                    }
                }
                else
                {
                    yield return "sendtochat Reckless Rick: Sorry cohorts! You are not currently in the Aztec World!";
                }
            }
        }
    }

    IEnumerator TwitchHandleForcedSolve()
    {
        autosolving = true;
        Debug.LogFormat("[The Crystal Maze #{0}] Resetting and autosolving per request of Twitch Plays.", moduleId);
        StopAllCoroutines();
        Reset();
        int selectedZone = Random.Range(0, 4);
        if (selectedZone == 0)
        {
            zoneAccess[0].OnInteract();
            int presser = 0;
            int target = aztecTargetWeight;
            if (target % 2 == 1)
            {
                target--;
            }
            for (int i = 0; i < target; i += 2)
            {
                presser += 1;
            }
            for (int i = 0; i < presser; i++)
            {
                if (aztecButtonsScript[0].buttonValue == 2)
                {
                    aztecButtons[0].OnInteract();
                }
                else if (aztecButtonsScript[1].buttonValue == 2)
                {
                    aztecButtons[1].OnInteract();
                }
                yield return new WaitForSeconds(0.05f);
            }
            for (int i = aztecWeightAdded; i < aztecTargetWeight; i++)
            {
                if (aztecButtonsScript[0].buttonValue == 1)
                {
                    aztecButtons[0].OnInteract();
                }
                else if (aztecButtonsScript[1].buttonValue == 1)
                {
                    aztecButtons[1].OnInteract();
                }
                yield return new WaitForSeconds(0.05f);
            }
            aztecAdd.OnInteract();
            while (endOfZone[0] != true) { yield return true; }
        }
        else if (selectedZone == 1)
        {
            zoneAccess[1].OnInteract();
            yield return ProcessTwitchCommand("reverse " + industrialZ);
            while (endOfZone[1] != true) { yield return true; }
        }
        else if (selectedZone == 2)
        {
            zoneAccess[2].OnInteract();
            while (!(screenText[0].color == correctColors[0]) || !screenText[0].text.Equals(correctWords[0]))
            {
                yield return null;
            }
            screenButtons[0].OnInteract();
            while (!(screenText[1].color == correctColors[1]) || !screenText[1].text.Equals(correctWords[1]))
            {
                yield return null;
            }
            screenButtons[1].OnInteract();
            while (!(screenText[2].color == correctColors[2]) || !screenText[2].text.Equals(correctWords[2]))
            {
                yield return null;
            }
            screenButtons[2].OnInteract();
            while (endOfZone[2] != true) { yield return true; }
        }
        else if (selectedZone == 3)
        {
            zoneAccess[3].OnInteract();
            while (medievalTime % 10 != pressTime)
            {
                yield return null;
            }
            medievalCircle.OnInteract();
            while (endOfZone[3] != true) { yield return true; }
        }
        yield return ProcessTwitchCommand("dome");
        while (domeTimerOn != true) { yield return null; }
        List<tokenScript> goldies = new List<tokenScript>();
        for (int i = 0; i < tokenObjects.Length; i++)
        {
            if (tokenObjects[i].gold)
                goldies.Add(tokenObjects[i]);
        }
        goldies = goldies.Shuffle();
        for (int i = 0; i < 10; i++)
        {
            goldies[i].selectable.OnInteract();
            yield return new WaitForSeconds(0.05f);
        }
        while (moduleSolved != true) { yield return true; }
        autosolving = false;
    }
}