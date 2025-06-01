using System;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public GameObject boxPrefab; // Your box prefab
    public Transform gridPanel;  // Your panel where grid lives (must have GridLayoutGroup)
    public int columns = 15;
    public int rows = 8;

    public BoxCell[,] grid;

    public enum Player { None, Player1, Player2 }
    public Player currentPlayer = Player.Player1;

    public GameObject redFlyingBall;
    public GameObject blueFlyingBall;
    public RectTransform flyingBallLayer;


    void Start()
    {
        grid = new BoxCell[rows, columns];
        GenerateGrid();
    }

    void GenerateGrid()
    {
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                GameObject boxObj = Instantiate(boxPrefab, gridPanel);
                BoxCell cell = boxObj.GetComponent<BoxCell>();
                cell.Setup(row, col, this);
                grid[row, col] = cell;
            }
        }
    }

    public void OnBoxClicked(BoxCell box)
    {
        if (box.CanAddBall(currentPlayer))
        {

            box.AddBall(currentPlayer);
            SwitchTurn();
        }
    }

    void SwitchTurn()
    {
        currentPlayer = (currentPlayer == Player.Player1) ? Player.Player2 : Player.Player1;
    }

    internal void OnBoxClicked(BoxCellPho boxCellPho)
    {
        throw new NotImplementedException();
    }
}
