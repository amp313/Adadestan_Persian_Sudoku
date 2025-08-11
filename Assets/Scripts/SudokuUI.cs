using UnityEngine;

public class SudokuUI : MonoBehaviour
{
    public CellUI cellPrefab;
    public Transform[] blockParents;

    public Sprite background1Sprite;
    public Sprite background2Sprite;

    public Sprite selectedBackground1Sprite;
    public Sprite selectedBackground2Sprite;

    public Sprite highlightBackground1Sprite;
    public Sprite highlightBackground2Sprite;

    public SudokuGenerator sudokuGenerator;

    private CellUI[,] cells = new CellUI[9, 9];
    private int[,] puzzle;
    private int selectedRow = -1;
    private int selectedCol = -1;

    public SudokuGenerator.Difficulty difficulty = SudokuGenerator.Difficulty.Easy;

    void Start()
    {
        difficulty = GameSettings.SelectedDifficulty;
        puzzle = sudokuGenerator.GeneratePuzzle(difficulty);
        BuildGrid();
    }

    void BuildGrid()
    {
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                int blockIndex = (r / 3) * 3 + (c / 3);
                Sprite defaultBg = (blockIndex == 0 || blockIndex == 2 || blockIndex == 4 || blockIndex == 6 || blockIndex == 8) ?
                    background1Sprite : background2Sprite;

                Sprite selectedBg = (blockIndex == 0 || blockIndex == 2 || blockIndex == 4 || blockIndex == 6 || blockIndex == 8) ?
                    selectedBackground1Sprite : selectedBackground2Sprite;

                Sprite highlightBg = (blockIndex == 0 || blockIndex == 2 || blockIndex == 4 || blockIndex == 6 || blockIndex == 8) ?
                    highlightBackground1Sprite : highlightBackground2Sprite;

                CellUI cell = Instantiate(cellPrefab, blockParents[blockIndex]);
                cell.Init(r, c, this, defaultBg, selectedBg, highlightBg);

                bool fixedCell = puzzle[r, c] != 0;
                cell.SetNumber(puzzle[r, c], fixedCell);
                cells[r, c] = cell;
            }
        }
    }

    public void SelectCell(int row, int col)
    {
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                cells[r, c].ResetBackground();

        
        for (int i = 0; i < 9; i++)
        {
            cells[row, i].SetHighlight();
            cells[i, col].SetHighlight();
        }

        
        int startRow = (row / 3) * 3;
        int startCol = (col / 3) * 3;
        for (int rr = 0; rr < 3; rr++)
            for (int cc = 0; cc < 3; cc++)
                cells[startRow + rr, startCol + cc].SetHighlight();

        
        cells[row, col].SetSelected();

        selectedRow = row;
        selectedCol = col;
    }
    public void OnNumberButtonClicked(int number)
    {
        if (selectedRow == -1 || selectedCol == -1)
            return; 

        CellUI cell = cells[selectedRow, selectedCol];

        if (cell.isFixed)
            return; 

        
        puzzle[selectedRow, selectedCol] = number;
        cell.SetNumber(number, false);
    }

}
