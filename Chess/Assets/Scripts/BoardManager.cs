using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { set; get; }
    private bool[,] allowedMoves { set; get; }
    public Chessman[,] Chessmans { set; get; }
    private Chessman selectedChessman;

    private const float TILE_SIZE = 1.0f;
    private const float TILE_OFFSET = 0.5f;

    private int selectionX = -1;
    private int selectionY = -1;

    public List<GameObject> chessmanPrefabs;
    private List<GameObject> activeChessman;

    private Quaternion orientation = Quaternion.Euler(0, 180, 0);

    public bool isWhiteTurn = true;
    private void Start()
    {
        Instance = this;
        SpawnAllChessmans();
    }

    private void Update()
    {
        
        UpdateSelection();
        DrawChessboard();
        if ( Input.GetMouseButtonDown(0)) //Input.GetMouseButton(0)
        {
            if(selectionX >= 0 && selectionY >= 0)
            {
                if(selectedChessman == null)
                {
                    // Select the chessman
                    SelectChessman(selectionX, selectionY);
                }
                else
                {
                    // Move the chessman
                    MoveChessman(selectionX, selectionY);
                }
            }
        }
    }

    private void SelectChessman(int x, int y)
    {
        if (Chessmans[x, y] == null)
            return;

        if (Chessmans[x, y].isWhite != isWhiteTurn)
            return;

        allowedMoves = Chessmans[x, y].PossibleMove();
        selectedChessman = Chessmans[x, y];
        BoardHighligths.Instace.HighlightAllowedMoves(allowedMoves);
    }

    private void MoveChessman(int x, int y)
    {
        if(allowedMoves[x,y])
        {
            Chessman c = Chessmans[x, y];
            if(c != null && c.isWhite != isWhiteTurn)
            {
                // Capture a piece

                // if it is the king 
                if(c.GetType() == typeof(King))
                {
                    // End the game
                    return;
                }

                activeChessman.Remove(c.gameObject);
                Destroy(c.gameObject);
            }

            Chessmans[selectedChessman.CurrentX, selectedChessman.CurrentY] = null;
            selectedChessman.transform.position = GetTileCenter(x, y);
            selectedChessman.SetPosition(x, y);
            Chessmans[x, y] = selectedChessman;
            isWhiteTurn = !isWhiteTurn;
        }

        BoardHighligths.Instace.HideHighlights();
        selectedChessman = null;
    }

    private void UpdateSelection()
    {
        if (!Camera.main)
            return;
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 25.0f, LayerMask.GetMask("ChessPlane")))
        {
            selectionX = (int)hit.point.x;
            selectionY = (int)hit.point.z;
        }
        else
        {
            selectionX = -1;
            selectionY = -1;
        }
    }

    private void SpawnChessman(int index, int x, int y)
    {
        GameObject go = Instantiate(chessmanPrefabs[index], GetTileCenter(x,y), orientation) as GameObject;
        go.transform.SetParent(transform);
        Chessmans[x, y] = go.GetComponent<Chessman>();
        Chessmans[x, y].SetPosition(x, y);
        activeChessman.Add(go);
    }
    private void SpawnChessman(int index, int x, int y, Quaternion orientation)
    {
        //This overide exist because white pieces dont have correct rotation generic
        GameObject go = Instantiate(chessmanPrefabs[index], GetTileCenter(x, y), orientation) as GameObject;
        go.transform.SetParent(transform);
        Chessmans[x, y] = go.GetComponent<Chessman>();
        Chessmans[x, y].SetPosition(x, y);
        activeChessman.Add(go);
    }

    private void SpawnAllChessmans()
    {
        Quaternion orientationWhite = Quaternion.Euler(0, 0, 0);
        activeChessman = new List<GameObject>();
        Chessmans = new Chessman[8, 8];

        //Spawn white team 

        // King 
        SpawnChessman(0, 3, 0);
                         
        // Queen         
        SpawnChessman(1, 4, 0);

        // Rooks 
        SpawnChessman(2, 0, 0);
        SpawnChessman(2, 7, 0);

        // Bishops 
        SpawnChessman(3, 2, 0);
        SpawnChessman(3, 5, 0);

        // Knigths 
        SpawnChessman(4, 1, 0, orientationWhite);
        SpawnChessman(4, 6, 0, orientationWhite);

        // Pawns 
        for(int i = 0; i < 8; i++) SpawnChessman(5, i, 1);

        //Spawn black team 

        // King 
        SpawnChessman(6, 4, 7);

        // Queen 
        SpawnChessman(7, 3, 7);

        // Rooks 
        SpawnChessman(8, 0, 7);
        SpawnChessman(8, 7, 7);

        // Bishops 
        SpawnChessman(9, 2, 7);
        SpawnChessman(9, 5, 7);

        // Knigths 
        SpawnChessman(10, 1, 7);
        SpawnChessman(10, 6, 7);

        // Pawns 
        for (int i = 0; i < 8; i++) SpawnChessman(11, i, 6);
    }
    private Vector3 GetTileCenter(int x , int y)
    {
        Vector3 origin = Vector3.zero;
        origin.x += (TILE_SIZE * x) + TILE_OFFSET;
        origin.z += (TILE_SIZE * y) + TILE_OFFSET;
        return origin;
    }

    private void DrawChessboard()
    {
        Vector3 widthLine = Vector3.right * 8;
        Vector3 heigthLine = Vector3.forward * 8;

        for(int i = 0; i <= 8; i++)
        {
            Vector3 start = Vector3.forward * i;
            Debug.DrawLine(start, start+widthLine);
            for(int j = 0; j <= 8; j++)
            {
                start = Vector3.right * j;
                Debug.DrawLine(start, start + heigthLine);
            }
        }
        // Draw the selection
        if(selectionX >= 0 && selectionY >= 0)
        {
            Debug.DrawLine(
                Vector3.forward * selectionY + Vector3.right * selectionX,
                Vector3.forward * (selectionY + 1) + Vector3.right * (selectionX + 1));

            Debug.DrawLine(
                Vector3.forward * (selectionY + 1) + Vector3.right * selectionX,
                Vector3.forward *  selectionY + Vector3.right * (selectionX + 1));
        }
    }
}
