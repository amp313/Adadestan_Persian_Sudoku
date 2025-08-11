using UnityEngine;
using UnityEngine.UI;

public class NumberButton : MonoBehaviour
{
    public int number;
    private Button button;
    private SudokuUI sudokuUI;

    void Start()
    {
        button = GetComponent<Button>();
        sudokuUI = FindObjectOfType<SudokuUI>();

        button.onClick.AddListener(() => sudokuUI.OnNumberButtonClicked(number));
    }
}
