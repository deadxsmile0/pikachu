using System.Threading;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameControllerScript : MonoBehaviour {
    public Camera vCam;
    // size of the game n^2 or n.n
    public Vector2Int vPKMGridSize;
    public List<Sprite> vPokemonSprites;
    public GameObject vPokemonObjectOriginal;
    public GameObject vPoolHolder;
    public PokemonObjectScript[,] vPokemonGrid;
    public Vector3 vPokemonPosOffset;
    public MyHelperScript vHpler = new MyHelperScript();
    public RaycastHit2D vHit2d;
    public GameObject vRayCastHitGameObjectFirst, vRayCastHitGameObjectSecond;
    public LineRenderer vLRender;
    public float vLineRenderShowTime;
    public AudioControllerScript vAuController;
    private Vector2 vCamPos;
    private GameObject vTemp; // null when unuse
    public string PointCalcuation = "_______________________________________";
    public int vDefaultPoint, vFinalPoint, vCombo, vLV; // vFinalpoint += vCombo * vDefaultpoint
    public float vComboTime, vOldComboTime;
    public GameObject vComboBarHolder;
    public TextMeshPro vScoreTextTmp, vComboMultiply, vLvIndicator;
    public TimeBarScript vTimeBarScpt;


    public string DEBUG = "_______________________________________";
    public GameObject vTargetClickedOn;
    public PokemonObjectScript PkmScpt;
    public string RealPokemonName;
    public Vector2 Position;
    public Vector2Int CoordInArray;
    public Sprite PokemonSprite;

    public SpriteRenderer vPkmBonk;

    public Vector2Int vGameObjectPosDebug;
    public Vector2Int StartPoint, EndPoint;
    public bool vDebugBFS, vDoneBFS;
    public List<Vector2Int> pathDemo;
    public int vNumberOfTurn, vNumberOfTurnRotate; // start pos to end pos, end pos to start pos
    public Thread vBFSThread;
    private object vThreadLock = new object();







    void Start() {
        vHpler.PoolInit("Pokemon", vPKMGridSize.x * vPKMGridSize.y, vPokemonObjectOriginal, vPoolHolder);
        vCam = gameObject.GetComponent<Camera>();
        vLRender = gameObject.GetComponent<LineRenderer>();
        vAuController.PlayAudio("rocksolid");

        InitPkmGrid();

        vOldComboTime = vComboTime;
        vLvIndicator.text = "LV" + vLV;
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.F)) {
            //StartCoroutine(SpawnPokemonObjectWithDelay());
            InitPkmGrid();
        }
        if (Input.GetKeyDown(KeyCode.B)) {
            print(vPokemonGrid[vGameObjectPosDebug.x, vGameObjectPosDebug.y].gameObject.name);
            print(vPokemonGrid[vGameObjectPosDebug.x, vGameObjectPosDebug.y].vPokemon.PokemonPosInGrid);
        }


        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            try {
                vHit2d = Physics2D.Raycast(vCam.ScreenToWorldPoint(Input.mousePosition), Vector3.forward, 999f);
                if (vHit2d) {
                    if (vHit2d.transform.gameObject.name.CompareTo("Shuffle") == 0) {
                        InitPkmGrid();
                        vTimeBarScpt.TimeBarPenalty();
                        vRayCastHitGameObjectFirst = null;
                    } else if (vHit2d.transform.gameObject.name.CompareTo("ToggleWindows") == 0) {
                        if (Screen.fullScreen) {
                            Screen.fullScreen = false;
                        } else {
                            Screen.fullScreen = true;
                        }
                    } else {
                        if (vRayCastHitGameObjectFirst == null && vHit2d) {
                            vRayCastHitGameObjectFirst = vHit2d.transform.gameObject;
                            PokemonInfo(vRayCastHitGameObjectFirst).ChangeBackgroundColor(new Color(.5f, .5f, .5f, 1));
                        } else if (vRayCastHitGameObjectSecond == null && vHit2d) {
                            vRayCastHitGameObjectSecond = vHit2d.transform.gameObject;
                            PokemonInfo(vRayCastHitGameObjectFirst).ChangeBackgroundColor(new Color(.5f, .5f, .5f, 1));
                            PokemonInfo(vRayCastHitGameObjectSecond).ChangeBackgroundColor(new Color(.5f, .5f, .5f, 1));
                        } else {
                            if (vRayCastHitGameObjectFirst != null)
                                PokemonInfo(vRayCastHitGameObjectFirst).ChangeBackgroundColor(new Color(1f, 1f, 1f, 1));
                            if (vRayCastHitGameObjectSecond != null)
                                PokemonInfo(vRayCastHitGameObjectSecond).ChangeBackgroundColor(new Color(1f, 1f, 1f, 1));
                            vRayCastHitGameObjectFirst = vRayCastHitGameObjectSecond = null;
                        }
                        if (vRayCastHitGameObjectFirst != null && vRayCastHitGameObjectSecond != null) {
                            if (vRayCastHitGameObjectFirst.name.CompareTo(vRayCastHitGameObjectSecond.name) != 0) {
                                CanPokemonBeConnectedInThreeLine(vRayCastHitGameObjectFirst, vRayCastHitGameObjectSecond);
                                PokemonInfo(vRayCastHitGameObjectFirst).ChangeBackgroundColor(new Color(1f, 1f, 1f, 1));
                                PokemonInfo(vRayCastHitGameObjectSecond).ChangeBackgroundColor(new Color(1f, 1f, 1f, 1));
                                vRayCastHitGameObjectFirst = null;
                                vRayCastHitGameObjectSecond = null;
                            }
                        }
                    }
                }

            } catch {

            }

        }

        if (Input.GetKeyDown(KeyCode.Space)) {

        }

        if (vRayCastHitGameObjectFirst != null) {
            if (vRayCastHitGameObjectFirst.name != "Shuffle" && vRayCastHitGameObjectFirst.name != "BlackLayer") {
                vPkmBonk.sprite = vRayCastHitGameObjectFirst.GetComponent<SpriteRenderer>().sprite;
            }
        }
    }

    private void FixedUpdate() {
        if (vDoneBFS) {
            if (vNumberOfTurn != 0 && vNumberOfTurn <= 3) {
                PointCalculator();
                vTimeBarScpt.BonusTime();
                vAuController.PlayAudio("bonk");
                GameObject.FindWithTag("Bonk").GetComponent<Animator>().Play("Bonked");
                HidePokemonInGrid(pathDemo[0], pathDemo[pathDemo.Count - 1]);
                StartCoroutine(TurnOffLineRenderer());
                vTimeBarScpt.BonusTime();
                if (CheckingGridCompletedStatus()) {
                    vTimeBarScpt.ResetTimeBar();
                    vLV++;
                    vLvIndicator.text = "LV" + vLV;
                    InitPkmGrid();
                }
                vNumberOfTurn = 0;
            }
            vLRender.positionCount = pathDemo.Count;
            for (int a = 0; a < pathDemo.Count; a++) {
                vLRender.SetPosition(a, vPokemonGrid[pathDemo[a].x, pathDemo[a].y].transform.position);
            }
            vDoneBFS = false;
        }

        if (vComboTime > 0) {
            vComboBarHolder.transform.localScale = new Vector3(MyHelperScript.NormalizeNumber(vComboTime, 0, vOldComboTime), 1, 1);
            vComboTime -= Time.deltaTime;
        } else {
            vCombo = 1;
            vComboMultiply.text = "x" + 1;
        }
    }

    void CanPokemonBeConnectedInThreeLine(GameObject _pokemonFirst, GameObject _pokemonSecond) {
        PokemonObjectScript pkmScpt1, pkmScpt2;
        pkmScpt1 = _pokemonFirst.GetComponent<PokemonObjectScript>();
        pkmScpt2 = _pokemonSecond.GetComponent<PokemonObjectScript>();
        Vector2Int arCord1, arCord2;
        arCord1 = pkmScpt1.vPokemon.PokemonCoordInArray;
        arCord2 = pkmScpt2.vPokemon.PokemonCoordInArray;
        if (vPokemonGrid[arCord1.x, arCord1.y].ActiveStatus() && vPokemonGrid[arCord2.x, arCord2.y].ActiveStatus()) {
            if (pkmScpt1.vPokemon.PokemonName.CompareTo(pkmScpt2.vPokemon.PokemonName) == 0) {
                if (arCord1.y == vPKMGridSize.y - 1 && arCord1.y == arCord2.y) {
                    if (pkmScpt1.vPokemon.PokemonName.CompareTo(pkmScpt2.vPokemon.PokemonName) == 0) {
                        vPokemonGrid[arCord1.x, arCord1.y].HidePokemon();
                        vPokemonGrid[arCord2.x, arCord2.y].HidePokemon();
                        vAuController.PlayAudio("bonk");
                        GameObject.FindWithTag("Bonk").GetComponent<Animator>().Play("Bonked");
                        PointCalculator();
                        vTimeBarScpt.BonusTime();
                        if (CheckingGridCompletedStatus()) {
                            vTimeBarScpt.ResetTimeBar();
                            vLV++;
                            vLvIndicator.text = "LV" + vLV;
                            InitPkmGrid();
                        }
                        // _pokemonFirst.SetActive(false);
                        // _pokemonSecond.SetActive(false);
                    }
                } else if (arCord1.y == 0 && arCord1.y == arCord2.y) {
                    if (pkmScpt1.vPokemon.PokemonName.CompareTo(pkmScpt2.vPokemon.PokemonName) == 0) {
                        vPokemonGrid[arCord1.x, arCord1.y].HidePokemon();
                        vPokemonGrid[arCord2.x, arCord2.y].HidePokemon();
                        vAuController.PlayAudio("bonk");
                        GameObject.FindWithTag("Bonk").GetComponent<Animator>().Play("Bonked");
                        PointCalculator();
                        vTimeBarScpt.BonusTime();
                        if (CheckingGridCompletedStatus()) {
                            vTimeBarScpt.ResetTimeBar();
                            vLV++;
                            vLvIndicator.text = "LV" + vLV;
                            InitPkmGrid();
                        }
                        // _pokemonFirst.SetActive(false);
                        // _pokemonSecond.SetActive(false);
                    }
                }
                if (arCord1.x == vPKMGridSize.x - 1 && arCord1.x == arCord2.x) {
                    if (pkmScpt1.vPokemon.PokemonName.CompareTo(pkmScpt2.vPokemon.PokemonName) == 0) {
                        vPokemonGrid[arCord1.x, arCord1.y].HidePokemon();
                        vPokemonGrid[arCord2.x, arCord2.y].HidePokemon();
                        vAuController.PlayAudio("bonk");
                        GameObject.FindWithTag("Bonk").GetComponent<Animator>().Play("Bonked");
                        PointCalculator();
                        vTimeBarScpt.BonusTime();
                        if (CheckingGridCompletedStatus()) {
                            vTimeBarScpt.ResetTimeBar();
                            vLV++;
                            vLvIndicator.text = "LV" + vLV;
                            InitPkmGrid();
                        }
                        // _pokemonFirst.SetActive(false);
                        // _pokemonSecond.SetActive(false);
                    }
                } else if (arCord1.x == 0 && arCord1.x == arCord2.x) {
                    if (pkmScpt1.vPokemon.PokemonName.CompareTo(pkmScpt2.vPokemon.PokemonName) == 0) {
                        vPokemonGrid[arCord1.x, arCord1.y].HidePokemon();
                        vPokemonGrid[arCord2.x, arCord2.y].HidePokemon();
                        vAuController.PlayAudio("bonk");
                        GameObject.FindWithTag("Bonk").GetComponent<Animator>().Play("Bonked");
                        PointCalculator();
                        vTimeBarScpt.BonusTime();
                        if (CheckingGridCompletedStatus()) {
                            vTimeBarScpt.ResetTimeBar();
                            vLV++;
                            vLvIndicator.text = "LV" + vLV;
                            InitPkmGrid();
                        }
                        // _pokemonFirst.SetActive(false);
                        // _pokemonSecond.SetActive(false);
                    }
                }




                if (vPokemonGrid[arCord1.x, arCord1.y].ActiveStatus() && vPokemonGrid[arCord2.x, arCord2.y].ActiveStatus()) {
                    GetPathBetweenPokemon(
                        vPokemonGrid[arCord1.x, arCord1.y].transform.gameObject,
                        vPokemonGrid[arCord2.x, arCord2.y].transform.gameObject
                    );
                }
            }
        }
    }
    private void HidePokemonInGrid(Vector2Int _pkmPos1, Vector2Int _pkmPos2) {
        vPokemonGrid[_pkmPos1.x, _pkmPos1.y].HidePokemon();
        vPokemonGrid[_pkmPos2.x, _pkmPos2.y].HidePokemon();
    }


    public IEnumerator HideLineRenderer(float _timer) {
        yield return new WaitForSecondsRealtime(_timer);
        vLRender.enabled = false;
    }



    // just for debug
    private PokemonObjectScript PokemonInfo(GameObject _target) {
        vTargetClickedOn = _target;
        PkmScpt = vTargetClickedOn.GetComponent<PokemonObjectScript>();
        RealPokemonName = PkmScpt.vPokemon.PokemonName;
        Position = PkmScpt.gameObject.transform.localPosition;
        CoordInArray = PkmScpt.vPokemon.PokemonCoordInArray;
        PokemonSprite = PkmScpt.vPokemon.PokemonSprite;
        return vTargetClickedOn.GetComponent<PokemonObjectScript>();
    }

    private IEnumerator TurnOffLineRenderer() {
        yield return new WaitForSecondsRealtime(vLineRenderShowTime);
        vLRender.positionCount = 0;
        pathDemo.Clear();
    }




    public void SpawnPokemonObject() {
        int x = 0, y = 0;
        vCamPos = new Vector2(gameObject.transform.position.x, gameObject.transform.position.y);
        vPokemonGrid = new PokemonObjectScript[vPKMGridSize.x, vPKMGridSize.y];
        // Setting double GameObject to make sure each one of them come in pair
        int spriteIndex = 0;
        for (int a = 0; a < (int)(vPKMGridSize.x * vPKMGridSize.y / 2); a++) {
            spriteIndex = Random.Range(0, vPokemonSprites.Count); //vPokemonSprites.Count           PokemonSprite!!!
            vTemp = vHpler.PoolUse();
            vTemp.transform.position = Vector3.zero;
            vPokemonGrid[y, x] = vTemp.GetComponent<PokemonObjectScript>();
            vPokemonGrid[y, x].vPokemon = new PokemonObjectScript.Pokemon(
                                                    vTemp.GetComponent<SpriteRenderer>(),
                                                    vPokemonSprites[spriteIndex],
                                                    Vector3.zero
                                            );
            vTemp.name = vPokemonGrid[y, x].vPokemon.PokemonName + "." + a + ".1";

            vTemp = vHpler.PoolUse();
            vTemp.transform.position = Vector3.zero;
            vPokemonGrid[y, x] = vTemp.GetComponent<PokemonObjectScript>();
            vPokemonGrid[y, x].vPokemon = new PokemonObjectScript.Pokemon(
                                                    vTemp.GetComponent<SpriteRenderer>(),
                                                    vPokemonSprites[spriteIndex],
                                                    Vector3.zero
                                            );
            vTemp.name = vPokemonGrid[y, x].vPokemon.PokemonName + "." + a + ".2";
        }
        vHpler.PoolSetIndex(0);
        for (int a = (int)vCamPos.y - vPKMGridSize.y / 2; a < vCamPos.y + vPKMGridSize.y / 2; a++) {
            y = 0;
            for (int b = (int)vCamPos.x - vPKMGridSize.x / 2; b < vCamPos.x + vPKMGridSize.x / 2; b++) {
                vPokemonGrid[y, x] = vHpler.PoolUse().GetComponent<PokemonObjectScript>();
                vPokemonGrid[y, x].vPokemon.PokemonPosInGrid = new Vector2(b, a);
                vPokemonGrid[y, x].gameObject.transform.position = vPokemonGrid[y, x].vPokemon.PokemonPosInGrid;
                vPokemonGrid[y, x].gameObject.transform.position += vPokemonPosOffset;
                vPokemonGrid[y, x].ShowPokemon();
                y++;
            }
            x++;
        }
        ShufflingPokemonPos();
    }

    public void ShufflingPokemonPos() {
        int x = 0, y = 0;
        vHpler.PoolSetIndex(0);
        Vector2 oldPkm;
        Vector2Int targetPkm;
        PokemonObjectScript temp;
        // swap and shuffle
        for (int a = (int)vCamPos.y - vPKMGridSize.y / 2; a < vCamPos.y + vPKMGridSize.y / 2; a++) {
            y = 0;
            for (int b = (int)vCamPos.x - vPKMGridSize.x / 2; b < vCamPos.x + vPKMGridSize.x / 2; b++) {
                temp = vPokemonGrid[y, x];
                oldPkm = vPokemonGrid[y, x].gameObject.transform.position;
                targetPkm = new Vector2Int(Random.Range(0, vPKMGridSize.x), Random.Range(0, vPKMGridSize.y));
                vPokemonGrid[y, x].gameObject.transform.position = vPokemonGrid[targetPkm.x, targetPkm.y].gameObject.transform.position;
                vPokemonGrid[targetPkm.x, targetPkm.y].gameObject.transform.position = oldPkm;
                vPokemonGrid[y, x].vPokemon.PokemonPosInGrid = vPokemonGrid[y, x].gameObject.transform.position;
                vPokemonGrid[targetPkm.x, targetPkm.y].vPokemon.PokemonPosInGrid = vPokemonGrid[targetPkm.x, targetPkm.y].gameObject.transform.position;
                vPokemonGrid[y, x] = vPokemonGrid[targetPkm.x, targetPkm.y];
                vPokemonGrid[targetPkm.x, targetPkm.y] = temp;
                vPokemonGrid[y, x].ShowPokemon();
                y++;
            }
            x++;
        }

        x = 0; y = 0;
        for (int a = (int)vCamPos.y - vPKMGridSize.y / 2; a < vCamPos.y + vPKMGridSize.y / 2; a++) {
            y = 0;
            for (int b = (int)vCamPos.x - vPKMGridSize.x / 2; b < vCamPos.x + vPKMGridSize.x / 2; b++) {
                // recalulating coordinate in Array
                vPokemonGrid[y, x].vPokemon.PokemonCoordInArray = new Vector2Int(
                                    (int)Mathf.Abs(vPokemonGrid[y, x].transform.position.x - vPokemonPosOffset.x + (vPKMGridSize.x / 2)),
                                    (int)Mathf.Abs(vPokemonGrid[y, x].transform.position.y - vPokemonPosOffset.y + (vPKMGridSize.y / 2))
                                );
                y++;
            }
            x++;
        }


    }

    public int PointCalculator() {
        vFinalPoint += vDefaultPoint * vCombo;
        vComboTime = vOldComboTime;
        if (vComboTime > 0) {
            vCombo++;
            vComboMultiply.text = "x" + vCombo;
        }
        vScoreTextTmp.text = vFinalPoint.ToString();
        return vFinalPoint;
    }

    public bool CheckingGridCompletedStatus() {
        for (int a = 0; a < vPokemonGrid.GetLength(0); a++) {
            for (int b = 0; b < vPokemonGrid.GetLength(1); b++) {
                if (vPokemonGrid[b, a].ActiveStatus()) {
                    return false;
                }
            }
        }
        return true;
    }

    public void InitPkmGrid() {
        SpawnPokemonObject();
        vTimeBarScpt.vHolder.transform.localScale = Vector3.one;
        vTimeBarScpt.vRunAllow = true;
    }

    public void GetPathBetweenPokemon(GameObject _pkmFirst, GameObject _pkmSecond) {
        bool[,] maze = ConvertPokemonGridToBoolGrid();
        MyHelperScript.BFS bfs = new MyHelperScript.BFS();
        bfs.SettingUp(
            maze,
            _pkmFirst.GetComponent<PokemonObjectScript>().vPokemon.PokemonCoordInArray,
            _pkmSecond.GetComponent<PokemonObjectScript>().vPokemon.PokemonCoordInArray,
            vPokemonGrid
        );
        //pathDemo = bfs.GetTheShortestPath();
        vDoneBFS = false;
        lock (vThreadLock) {
            vBFSThread = new Thread(() => {
                print("Thread Started.");
                pathDemo = bfs.GetTheShortestPath(vDebugBFS);
                vDoneBFS = true;
                vNumberOfTurn = bfs.CalculateAmountOfTurningPoint(pathDemo);
                Debug.Log("Amount Of Turn :" + vNumberOfTurn);
                print("Thread Done.");
            });
            vBFSThread.Start();
        }
    }

    public bool[,] ConvertPokemonGridToBoolGrid() {
        bool[,] bGrid = new bool[vPokemonGrid.GetLength(0), vPokemonGrid.GetLength(1)];
        for (int a = 0; a < vPokemonGrid.GetLength(1); a++) {
            for (int b = 0; b < vPokemonGrid.GetLength(0); b++) {
                if (vPokemonGrid[b, a].vSpriteRenderer.enabled) {
                    bGrid[b, a] = true;
                } else {
                    bGrid[b, a] = false;
                }
            }
        }
        // print("Converted to Bool grid.");
        // string str = "";
        // for (int a = 0; a < vPokemonGrid.GetLength(1); a++) {
        //     str = "";
        //     for (int b = 0; b < vPokemonGrid.GetLength(0); b++) {
        //         if (vPokemonGrid[b, a].gameObject.activeInHierarchy) {
        //             bGrid[b, a] = true;
        //             str += "0";
        //         } else {
        //             bGrid[b, a] = true;
        //             str += "X";
        //         }
        //     }
        //     print(str);
        // }
        return bGrid;
    }
}
