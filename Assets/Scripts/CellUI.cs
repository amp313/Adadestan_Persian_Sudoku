using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CellUI : MonoBehaviour
{
    #region Fields
    public Image background;
    public TMP_Text numberText;

    [HideInInspector] public bool isFixed = false;
    [HideInInspector] public bool isUserFilled = false;
    [HideInInspector] public bool isSelectable = true;

    private int row, col;
    private SudokuUI sudokuUI;

    private Sprite defaultSprite;
    private Sprite selectedSprite;
    private Sprite highlightSprite;

    private Color darkGreen = new Color(0.1529f, 0.5725f, 0.1529f, 1f);

    public GameObject pencilPanel;
    public TMP_Text[] pencilTexts;

    private bool pencilMode = false;
    #endregion

    #region Pencil
    public void SetPencilMode(bool active)
    {
        pencilMode = active;
        if (pencilPanel != null)
            pencilPanel.SetActive(!isFixed);
    }

    public void SetPencilNumber(int number)
    {
        if (!pencilMode || isFixed) return;

        int index = number - 1;
        if (index >= 0 && index < pencilTexts.Length)
        {
            if (pencilTexts[index].text == number.ToString())
                pencilTexts[index].text = "";
            else
                pencilTexts[index].text = number.ToString();
        }
    }

    public void RemovePencilNumber(int number)
    {
        int index = number - 1;
        if (index >= 0 && index < pencilTexts.Length)
        {
            if (pencilTexts[index].text == number.ToString())
                pencilTexts[index].text = "";
        }
    }

    public void ClearPencil()
    {
        if (pencilTexts == null) return;
        for (int i = 0; i < pencilTexts.Length; i++)
            pencilTexts[i].text = "";
    }
    #endregion

    #region Init
    public void Init(int r, int c, SudokuUI ui, Sprite defaultBg, Sprite selectedBg, Sprite highlightBg)
    {
        row = r;
        col = c;
        sudokuUI = ui;

        defaultSprite = defaultBg;
        selectedSprite = selectedBg;
        highlightSprite = highlightBg;

        background.sprite = defaultSprite;
        background.color = Color.white;

        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OnClick);
        }
    }
    #endregion

    #region Number
    public void SetNumber(int number, bool fixedCell)
    {
        isFixed = fixedCell;

        if (number == 0)
        {
            numberText.text = "";
            isUserFilled = false;
            isSelectable = true;
        }
        else
        {
            numberText.text = number.ToString();
            if (!isFixed)
                isUserFilled = true;

            isSelectable = !isUserFilled;
            ClearPencil();
        }
        UpdateTextColor();
    }

    public void SetNumberColor(Color color)
    {
        numberText.color = color;
    }

    public void UpdateTextColor()
    {
        if (isFixed)
            numberText.color = Color.black;
        else if (isUserFilled)
            numberText.color = darkGreen;
        else
            numberText.color = Color.black;
    }
    #endregion

    #region Background
    public void SetSelected()
    {
        background.sprite = selectedSprite;
        background.color = Color.white;
    }

    public void SetHighlight()
    {
        background.sprite = highlightSprite;
        background.color = Color.white;
    }

    public void ResetBackground()
    {
        background.sprite = defaultSprite;
        background.color = Color.white;
    }
    #endregion

    #region Input
    public void OnClick()
    {
        if (!isSelectable && !isUserFilled && !isFixed) return;

        if (sudokuUI.eraserMode)
            sudokuUI.OnEraserCell(row, col);
        else if (sudokuUI.helpMode)
            sudokuUI.OnHelpCell(row, col);
        else
            sudokuUI.SelectCell(row, col);
    }
    #endregion
}
