using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CellUI : MonoBehaviour
{
    public Image background;
    public TMP_Text numberText;
    public bool isFixed;
    private int row, col;
    private SudokuUI sudokuUI;

    private Sprite defaultSprite;
    private Sprite selectedSprite;
    private Sprite highlightSprite;

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
    }

    public void SetNumber(int number, bool fixedCell)
    {
        isFixed = fixedCell;
        numberText.text = number == 0 ? "" : number.ToString();
        numberText.color = isFixed ? Color.black : Color.blue;
    }

    public void OnClick()
    {
        if (!isFixed)
            sudokuUI.SelectCell(row, col);
    }

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
}
