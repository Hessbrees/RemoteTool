using UnityEngine;

public class RemoteMergeStyleSetup
{
    Color colorRed = new Color(1, 0.87f, 0.8f);
    Color colorLightBlue = new Color(0.16f, 0.32f, 0.45f);
    Color colorGray = new Color(0.27f, 0.27f, 0.27f);
    Color colorGreen = new Color(0.36f, 0.58f, 0.36f);
    Color colorDefault = new Color(0.27f, 0.27f, 0.23f);
    
    // label
    public int labelHeight = 20;
    public int backgroundLabelWidth = 260;
    public int backgroundLabelBorder = 2;

    public GUIStyle selectedLabelStyle; // selected 
    public GUIStyle defaultLabelStyle; // not selected
    public GUIStyle acceptedLabelStyle;
    public GUIStyle removedLabelStyle;

    public Texture2D defaultBackground;
    public Texture2D selectedBackground;
    public Texture2D acceptedBackground;
    public Texture2D removedBackground;

    // text area
    public GUIStyle changedTextArea;
    public GUIStyle mergedTextArea;
    public GUIStyle defaultTextArea;


    public Texture2D defaultAreaBackground;
    public Texture2D mergedAreaBackground;
    public Texture2D changedAreaBackground;

    // buttons styles
    public int buttonHeight = 20;
    public int buttonCommitHeight = 60;
    public int backgroundButtonWidth = 200;
    public int backgroundButtonBorder = 1;

    public GUIStyle defaultButtonStyle;
    public GUIStyle selectedButtonStyle;

    public GUIStyle defaultCommitButtonStyle;
    public GUIStyle disabledCommitButtonStyle;

    public Texture2D defaultButtonBackground;
    public Texture2D selectedButtonBackground;
    public Texture2D defaultCommitButtonBackground;
    public Texture2D disabledCommitButtonBackground;

    // Create all color styles for texts/lines/buttons 
    public void SetupStyles()
    {
        // label
        defaultBackground = MakeTexWithBorder(backgroundLabelWidth, labelHeight, Color.grey, backgroundLabelBorder, Color.black);
        selectedBackground = MakeTexWithBorder(backgroundLabelWidth, labelHeight, Color.blue, backgroundLabelBorder, Color.black);
        acceptedBackground = MakeTexWithBorder(backgroundLabelWidth, labelHeight, colorGreen, backgroundLabelBorder, Color.black);
        removedBackground = MakeTexWithBorder(backgroundLabelWidth, labelHeight, colorRed, backgroundLabelBorder, Color.black);

        defaultBackground.hideFlags = HideFlags.HideAndDontSave;
        selectedBackground.hideFlags = HideFlags.HideAndDontSave;
        acceptedBackground.hideFlags = HideFlags.HideAndDontSave;
        removedBackground.hideFlags = HideFlags.HideAndDontSave;

        defaultLabelStyle = CreateLabelStyle(defaultBackground);
        selectedLabelStyle = CreateLabelStyle(selectedBackground);
        acceptedLabelStyle = CreateLabelStyle(acceptedBackground);
        removedLabelStyle = CreateLabelStyle(removedBackground, Color.black);

        // text area
        defaultAreaBackground = MakeTex(250, labelHeight, colorDefault);
        changedAreaBackground = MakeTex(250, labelHeight, colorRed);
        mergedAreaBackground = MakeTex(250, labelHeight, colorLightBlue);

        defaultAreaBackground.hideFlags = HideFlags.HideAndDontSave;
        changedAreaBackground.hideFlags = HideFlags.HideAndDontSave;
        mergedAreaBackground.hideFlags = HideFlags.HideAndDontSave;

        defaultTextArea = CreateTextAreaStyle(defaultAreaBackground, Color.white);
        changedTextArea = CreateTextAreaStyle(changedAreaBackground, Color.black);
        mergedTextArea = CreateTextAreaStyle(mergedAreaBackground, Color.white);

        // button
        defaultButtonBackground = MakeTexWithBorder(backgroundButtonWidth, buttonHeight, Color.grey, backgroundButtonBorder, Color.black);
        selectedButtonBackground = MakeTexWithBorder(backgroundButtonWidth, buttonHeight, colorGreen, backgroundButtonBorder, Color.black);
        defaultCommitButtonBackground = MakeTexWithBorder(backgroundButtonWidth, buttonCommitHeight, Color.grey, backgroundButtonBorder, Color.black);
        disabledCommitButtonBackground = MakeTexWithBorder(backgroundButtonWidth, buttonCommitHeight, colorGray, backgroundButtonBorder, Color.black);

        defaultButtonBackground.hideFlags = HideFlags.HideAndDontSave;
        selectedButtonBackground.hideFlags = HideFlags.HideAndDontSave;
        defaultCommitButtonBackground.hideFlags = HideFlags.HideAndDontSave;
        disabledCommitButtonBackground.hideFlags = HideFlags.HideAndDontSave;

        defaultButtonStyle = CreateButtonStyle(defaultButtonBackground);
        selectedButtonStyle = CreateButtonStyle(selectedButtonBackground);
        defaultCommitButtonStyle = CreateButtonStyle(defaultCommitButtonBackground, Color.white, true);
        disabledCommitButtonStyle = CreateButtonStyle(disabledCommitButtonBackground, Color.white, true);
    }
    private GUIStyle CreateButtonStyle(Texture2D background, Color textColor, bool wrapText)
    {
        GUIStyle style = CreateButtonStyle(background, textColor);
        style.wordWrap = wrapText;
        style.fixedHeight = buttonCommitHeight;
        return style;
    }
    private GUIStyle CreateButtonStyle(Texture2D background)
    {
        return CreateButtonStyle(background, Color.white);
    }
    private GUIStyle CreateButtonStyle(Texture2D background, Color textColor)
    {
        GUIStyle style = new();

        style.padding = new RectOffset(backgroundButtonBorder + 2, 0, backgroundButtonBorder, 0);
        style.fixedHeight = buttonHeight;
        style.alignment = TextAnchor.MiddleCenter;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = textColor;
        style.normal.background = background;

        return style;
    }

    private GUIStyle CreateLabelStyle(Texture2D background)
    {
        return CreateLabelStyle(background, Color.white);
    }

    private GUIStyle CreateLabelStyle(Texture2D background, Color textColor)
    {
        GUIStyle style = new();

        style.stretchHeight = false;
        style.padding = new RectOffset(backgroundLabelBorder + 2, 0, backgroundLabelBorder, 0);
        style.fixedHeight = labelHeight;
        style.alignment = TextAnchor.MiddleLeft;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Bold;
        style.normal.textColor = textColor;
        style.normal.background = background;

        return style;
    }
    public static GUIStyle CreateTextGUIStyle(int fontSize)
    {
        GUIStyle style = new();

        style.stretchHeight = false;
        style.stretchWidth = false;
        style.alignment = TextAnchor.MiddleLeft;
        style.fontSize = fontSize;
        style.fontStyle = FontStyle.Normal;
        style.normal.textColor = Color.white;
        return style;
    }
    private GUIStyle CreateTextAreaStyle(Texture2D background, Color textColor)
    {
        GUIStyle style = new();

        style.stretchHeight = false;
        style.fixedWidth = 600;
        style.alignment = TextAnchor.MiddleLeft;
        style.fontSize = 12;
        style.fontStyle = FontStyle.Normal;
        style.normal.textColor = textColor;
        style.normal.background = background;

        return style;
    }
    private Texture2D MakeTexWithBorder(int width, int height, Color col, int borderWidth, Color borderColor)
    {
        Color[] pix = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth)
                {
                    pix[y * width + x] = borderColor;
                }
                else
                {
                    pix[y * width + x] = col;
                }
            }
        }

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; ++i)
        {
            pix[i] = col;
        }
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
