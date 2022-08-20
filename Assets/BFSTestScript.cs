using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BFSTestScript : MonoBehaviour {
    public Vector2Int vGridSize, vStartPoint, vEndPoint;
    public GameObject vTile;
    public bool[,] vGridData;
    public LineRenderer vLineRender;
    public List<Vector2Int> vPath;
    void Start() {
        vGridData = new bool[vGridSize.x, vGridSize.y];
        for (int a = 0; a < vGridSize.x; a++) {
            for (int b = 0; b < vGridSize.y; b++) {
                // vTile.transform.position = new Vector3(b, a, 0);
                // if (Random.Range(0, 11) < 3) {
                //     vTile.GetComponent<SpriteRenderer>().color = new Color(1, 0, 0, 1);
                //     vGridData[b, a] = true;
                // } else if (a == vStartPoint.x && b == vStartPoint.y) {
                //     vTile.GetComponent<SpriteRenderer>().color = new Color(0, 1, 0, 1);
                //     vGridData[b, a] = true;
                // } else if (a == vEndPoint.x && b == vEndPoint.y) {
                //     vTile.GetComponent<SpriteRenderer>().color = new Color(0, 1, 1, 1);
                //     vGridData[b, a] = true;
                // } else {
                //     vTile.GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
                //     vGridData[b, a] = true;
                // }
                vGridData[b, a] = true;
            }
        }
        MyHelperScript.BFS bfs = new MyHelperScript.BFS();
        //bfs.SettingUp(vGridData, vStartPoint, vEndPoint);
        //vPath = bfs.GetTheShortestPath();
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.R)) {
            if (CheckArrayIfInBounds(vGridSize, vStartPoint)) {
                Debug.Log(vGridData[vStartPoint.x, vStartPoint.y]);
                print("In bound");
            } else {
                print("Not in bound");
            }
            // vLineRender.positionCount = vPath.Count;
            // for (int a = 0; a < vPath.Count; a++) {
            //     vLineRender.SetPosition(a, new Vector3(vPath[a].x, vPath[a].y, 0));
            // }
        }
    }

    private bool CheckArrayIfInBounds(Vector2Int _boundsAsVector2Int, Vector2Int _position) {
        if (_position.x >= 0 && _position.x < _boundsAsVector2Int.x) {
            if (_position.y >= 0 && _position.y < _boundsAsVector2Int.y) {
                return true;
            } else {
                return false;
            }
        } else {
            return false;
        }
    }
}
