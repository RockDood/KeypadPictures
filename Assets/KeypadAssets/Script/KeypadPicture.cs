using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Rnd = UnityEngine.Random;

public class KeypadPicture : KtaneModule
{

    public KMSelectable[] Buttons;
    public KMAudio Audio;

    private bool _isSolved;

    public Transform[] ButtonsTransforms;

    public Sprite[] SpriteImages; //28 max int

    public Renderer[] LEDs;

    bool animatingLED;
    bool animatingButtons;

    public Material[] LEDColors;
    public Texture2D[] LEDColor;

    private string[] SpriteNames = new string[] 
    { 
        "Soccer Ball", 
        "Angry Face", 
        "Shooting Star", 
        "Basketball", 
        "Bird", 
        "Cake", 
        "Bone", 
        "Bus", 
        "Candles", 
        "Sideways Dog", 
        "Guitar", 
        "Coins", 
        "Crying Face", 
        "Dog Head", 
        "Cat", 
        "Envelope", 
        "Fish", 
        "Hen", 
        "Runner", 
        "Key", 
        "Sparkles", 
        "Light Bulbs", 
        "Rose", 
        "Biker", 
        "Moon", 
        "Santa's Beard", 
        "Camera",
        "Rabbit Head" 
    };

    public SpriteRenderer[] ButtonSprites;

    private int[] ChosenDigits = new int[] { 9, 9, 9, 9 };
    private int SelectedColumn;
    private const float _interactionPunchIntensity = .5f;

    private int[] CorrectOrder = new int[] { 1, 2, 0, 5, };

    private int CurrentStage = 0;

    private bool[] HasBeenPressed = new bool[] { false, false, false, false };

    protected override void Start()
    {
        base.Start();
        //Debug.Log("Start is called!");
    }

    protected override void Awake()
    {
        base.Awake();

        SelectedColumn = Rnd.Range(0, 6);
        Debug.LogFormat("[Simple Keypad #{0}]Selected Column: #{1}", ModuleID, SelectedColumn);

        for (int i = 0; i < Buttons.Length; i++)
        {
            var j = i;
            Buttons[i].OnInteract += delegate { KeyPress(j); return false; };
        }

        for (int i = 0; i < ButtonSprites.Length; i++)
        {
            int generatedint = GenDigit();
            ChosenDigits[i] = generatedint;
            CorrectOrder[i] = generatedint;
            Debug.Log("Generated " + i + " Digit: " + generatedint);
            ButtonSprites[i].sprite = SpriteImages[ColumnReference(SelectedColumn, ChosenDigits[i])];
        }

        for (int i = 0; i < ButtonSprites.Length; i++)
        {
            ButtonSprites[i].sprite = SpriteImages[ColumnReference(SelectedColumn, ChosenDigits[i])];
        }

        SortArray(CorrectOrder);

        Debug.LogFormat("[Simple Keypad #{5}] First Position: {0}, Second Position: {1}, Third Position: {2}, Forth Position: {3}. First button press is the {4}", SpriteNames[ColumnReference(SelectedColumn, ChosenDigits[0])], SpriteNames[ColumnReference(SelectedColumn, ChosenDigits[1])], SpriteNames[ColumnReference(SelectedColumn, ChosenDigits[2])], SpriteNames[ColumnReference(SelectedColumn, ChosenDigits[3])], SpriteNames[ColumnReference(SelectedColumn, CorrectOrder[0])], ModuleID);
    }

    void KeyPress(int button)
    {


        if (animatingButtons || animatingLED)
            return;

        Audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, Buttons[button].transform);
        Buttons[button].AddInteractionPunch(_interactionPunchIntensity);

        if (HasBeenPressed[button])
            return;

        if (_isSolved)
            return;
        bool ActiveButtonCorrect = false;

        Debug.LogFormat("[Simple Keypad #{2}] You pressed a button that is associated with the number {0}. I expected the number {1}", ColumnReference(SelectedColumn, ChosenDigits[button]), ColumnReference(SelectedColumn, CorrectOrder[CurrentStage]), ModuleID);

        if (ColumnReference(SelectedColumn, ChosenDigits[button]) == ColumnReference(SelectedColumn, CorrectOrder[CurrentStage]))
        {
            if (CurrentStage < CorrectOrder.Length)
            {
                ActiveButtonCorrect = true;
                Debug.LogFormat("[Simple Keypad #{0}] Stage {1} Passed", ModuleID, CurrentStage + 1);
                CurrentStage++;
                Debug.LogFormat("[Simple Keypad #{0}] The next symbol to press will be: {1}", ModuleID, SpriteNames[ColumnReference(SelectedColumn, CorrectOrder[0])]);
                StartCoroutine(animateButton(ButtonsTransforms[button], ActiveButtonCorrect));
                StartCoroutine(animateLED(LEDs[button], 2));
                HasBeenPressed[button] = true;
                if (CurrentStage == 4)
                {
                    _isSolved = true;
                    BombModule.HandlePass();
                    return;
                }
                return;
            }

        }
        else
        {
            StartCoroutine(animateLED(LEDs[button], 1));
            StartCoroutine(animateButton(ButtonsTransforms[button], ActiveButtonCorrect));
            BombModule.HandleStrike();
        }
    }

    int ColumnReference(int Column, int arrlocation) 
    {
        int spriteint = 0;
        switch (Column)
        {
            case 0:
                int[] Column1 = new int[] { 25, 13, 18, 12, 7, 9, 23 };
                spriteint = Column1[arrlocation];
                break;
            case 1:
                int[] Column2 = new int[] { 16, 25, 23, 26, 3, 9, 20 };
                spriteint = Column2[arrlocation];
                break;
            case 2:
                int[] Column3 = new int[] { 1, 8, 26, 5, 15, 18, 3 };
                spriteint = Column3[arrlocation];
                break;
            case 3:
                int[] Column4 = new int[] { 11, 21, 0, 7, 5, 20, 4 };
                spriteint = Column4[arrlocation];
                break;
            case 4:
                int[] Column5 = new int[] { 24, 4, 0, 22, 21, 19, 2 };
                spriteint = Column5[arrlocation];
                break;
            case 5:
                int[] Column6 = new int[] { 11, 16, 27, 14, 24, 17, 6 };
                spriteint = Column6[arrlocation];
                break;
        }
        return spriteint;
    }

    private IEnumerator animateLED(Renderer LED, int correct)
    {
        const float duration = .5f;
        var elapsed = 0f;
        if (correct == 1)
        {
            while (elapsed < duration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                LED.material = LEDColors[1];
            }
            LED.material = LEDColors[0];
        }
        else
            LED.material = LEDColors[2];

    }

    int GenDigit()
    {
        int generatedint;
        generatedint = Rnd.Range(0, 7);
        if (!ChosenDigits.Contains(generatedint))
        {
            return generatedint;
        }
        else
        {
            Debug.Log(generatedint + " was already chosen or number was not present in selected column, generating new number...");
            return GenDigit();
        }
    }

    private IEnumerator animateButton(Transform btn, bool setting)
    {
        const float duration = .35f;
        var elapsed = 0f;
        const float depressed = -0.05f;
        const float undepressed = 0;
        var originalPosition = btn.localPosition;
        var startValue = setting ? undepressed : depressed;
        var endValue = setting ? depressed : undepressed;
        while (elapsed < duration)
        {
            yield return null;
            elapsed += Time.deltaTime;
            if (setting)
            {
                btn.localPosition = new Vector3(originalPosition.x, (endValue - startValue) * elapsed / duration + startValue, originalPosition.z);
            }
        }
        btn.localPosition = new Vector3(originalPosition.x, endValue, originalPosition.z);
    }

    public static int[] SortArray(int[] array)
    {
        int length = array.Length;

        int temp = array[0];

        for (int i = 0; i < length; i++)
        {
            for (int j = i + 1; j < length; j++)
            {
                if (array[i] > array[j])
                {
                    temp = array[i];

                    array[i] = array[j];

                    array[j] = temp;
                }
            }
        }
        return array;
    }
}
