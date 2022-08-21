using System.Threading;
using System.Collections;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MyHelperScript {
    public PoolData vWorkerPool;
    private List<PoolData> vPools = new List<PoolData>();
    private Vector3 vMousePos;
    private float vZAxis;


    public BFS vBFS = new BFS();
    [SerializeField]
    public List<Vector2Int> Path, QueueList;
    [SerializeField]
    public Dictionary<Vector2Int, Vector2Int> BackTrack = new Dictionary<Vector2Int, Vector2Int>();
    [SerializeField]
    public Dictionary<Vector2Int, Vector2Int> Visited = new Dictionary<Vector2Int, Vector2Int>();


    private GameObject temp;


    public class cClock {
        private float ResultRealTime;
        private System.Diagnostics.Stopwatch StW = new System.Diagnostics.Stopwatch();

        public void ClockStart() {
            ResultRealTime = 0;
            StW.Start();
        }

        public float ClockCurrentTime() {
            return StW.ElapsedMilliseconds / 1000f;
        }

        public float ClockStopResult() {
            StW.Stop();
            ResultRealTime = StW.ElapsedMilliseconds / 1000f;
            return StW.ElapsedMilliseconds / 1000f;
        }
    }

    /// <summary>
    /// Adding basic moving using Rigidbody for ez (2D)
    /// </summary>
    public Vector3 BasicMovingScript2D(Rigidbody2D _Rb, float _MovingSpeed) {
        if (_Rb.gravityScale != 0) {
            _Rb.gravityScale = 0;
        }
        if (Input.GetKey(KeyCode.W)) {
            _Rb.velocity = new Vector3(_Rb.velocity.x, _MovingSpeed, 0);
        }
        if (Input.GetKeyUp(KeyCode.W)) {
            _Rb.velocity = new Vector3(_Rb.velocity.x, 0, 0);
        }
        if (Input.GetKey(KeyCode.S)) {
            _Rb.velocity = new Vector3(_Rb.velocity.x, -_MovingSpeed, 0);
        }
        if (Input.GetKeyUp(KeyCode.S)) {
            _Rb.velocity = new Vector3(_Rb.velocity.x, 0, 0);
        }
        if (Input.GetKey(KeyCode.A)) {
            _Rb.velocity = new Vector3(-_MovingSpeed, _Rb.velocity.y, 0);
        }
        if (Input.GetKeyUp(KeyCode.A)) {
            _Rb.velocity = new Vector3(0, _Rb.velocity.y, 0);
        }
        if (Input.GetKey(KeyCode.D)) {
            _Rb.velocity = new Vector3(_MovingSpeed, _Rb.velocity.y, 0);
        }
        if (Input.GetKeyUp(KeyCode.D)) {
            _Rb.velocity = new Vector3(0, _Rb.velocity.y, 0);
        }
        return _Rb.velocity;
    }

    public Vector3 fLerping(GameObject _Object, Vector3 _TargetPosition, float _LerpSpeed) {
        return Vector3.Lerp(_Object.transform.position, new Vector3(_TargetPosition.x, _TargetPosition.y, _Object.transform.position.z), _LerpSpeed);
    }

    /// <summary>
    /// Use This function to make first object follow second object (2D)
    /// </summary>
    public Vector3 fFollowTarget2D(GameObject _BaseObject, GameObject _FollowTarget, float _LerpSpeed) {
        _BaseObject.transform.position = Vector3.Lerp(
            _BaseObject.transform.position,
            new Vector3(_FollowTarget.transform.position.x, _FollowTarget.transform.position.y, _BaseObject.transform.position.z),
            _LerpSpeed
        );
        return _BaseObject.transform.position;
    }

    /// <summary>
    /// Return float data of angle between two point (2D)
    /// </summary>
    public float fGetAngleBetweenTwoPoint(Vector3 _first_Cord, Vector3 _sec_Cord) {
        return (Mathf.Atan2((_first_Cord - _sec_Cord).x, (_first_Cord - _sec_Cord).y) * Mathf.Rad2Deg + 90f) * -1f;
    }

    /// <summary>
    /// Rotate object with external coordinate (2D)
    /// </summary>
    public Vector3 fRotateObjectBasedOnCordinate(GameObject _base_Object, Vector3 _target_Object) {
        float Angle = fGetAngleBetweenTwoPoint(_base_Object.transform.position, _target_Object);
        _base_Object.transform.eulerAngles = new Vector3(0, 0, Angle);
        return _base_Object.transform.eulerAngles;
    }

    /// <summary>
    /// Return mouse coordinate using a specific camera (2D)
    /// </summary>
    public Vector3 fGetMousePositionBasedOnCamera(Camera _cam) {
        vMousePos = _cam.ScreenToWorldPoint(Input.mousePosition);
        return new Vector3(vMousePos.x, vMousePos.y, _cam.transform.position.z);
    }


    //--------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Init a Pool by giving Func data named in the agrument, new pool created will be IN USED IMMEDIATELY!!!
    /// </summary>
    public PoolData PoolInit(string _poolName, int _poolSize, GameObject _EntityOriginal, GameObject _poolHolder) {
        PoolData pData = new PoolData();
        pData._poolName = _poolName;
        pData._poolHolder = _poolHolder;
        pData._poolSize = _poolSize;
        GameObject g;
        foreach (PoolData x in vPools) {
            if (x._poolName.CompareTo(_poolName) == 0) {
                vWorkerPool = x;
                if (x._poolSize >= _poolSize) {
                    vWorkerPool = x;
                    UnityEngine.Debug.Log("Found Pool \"" + _poolName + "\" with " + x._poolSize + " entities.");
                    return x;
                } else if (x._poolSize < _poolSize) {
                    for (int a = x._poolSize; a < _poolSize; a++) {
                        g = UnityEngine.GameObject.Instantiate(_EntityOriginal, Vector3.zero, Quaternion.identity);
                        g.transform.parent = _poolHolder.transform;
                        g.AddComponent<TurnOffGameObjectWhenGoOutsiteCamViewScript>();
                        g.name = _poolName + "." + (a + 1).ToString();
                        g.SetActive(false);
                        vWorkerPool._poolEntities.Add(g);
                    }
                    x._poolSize = _poolSize;
                    vWorkerPool = pData;
                    return pData;
                }
            }
        }
        for (int a = 0; a < _poolSize; a++) {
            g = UnityEngine.GameObject.Instantiate(_EntityOriginal, Vector3.zero, Quaternion.identity);
            g.transform.parent = _poolHolder.transform;
            g.AddComponent<TurnOffGameObjectWhenGoOutsiteCamViewScript>();
            g.name = _poolName + "." + (a + 1).ToString();
            g.SetActive(false);
            pData._poolEntities.Add(g);
        }
        vPools.Add(pData);
        vWorkerPool = pData;
        UnityEngine.Debug.Log("Pool \"" + pData._poolName + "\" with " + pData._poolSize + " entities created successfully.");
        return pData;
    }
    /// <summary>
    /// Change Worker Pool to a difference one, stay on the same Worker Pool if Pool's name input is not found or null
    /// </summary>
    public PoolData PoolSwitch(string _poolName) {
        foreach (PoolData x in vPools) {
            if (x._poolName.CompareTo(_poolName) == 0) {
                vWorkerPool = x;
                return x;
            }
        }
        // Lake = vPools
        UnityEngine.Debug.Log("No Pool named \"" + _poolName + "\" exist in \"LAKE\"");
        return vWorkerPool;
    }

    /// <summary>
    /// Get the pool that currently in use;
    /// </summary>
    public PoolData PoolGetCurrentWorking(string _poolName) {
        return vWorkerPool;
    }

    public PoolData PoolSetIndex(int _index) {
        vWorkerPool._Index = _index;
        return vWorkerPool;
    }

    /// <summary>
    /// Return an Entity in the Worker Pool
    /// </summary>
    public GameObject PoolUse() {
        temp = null;
        if (vWorkerPool != null) {
            temp = vWorkerPool._poolEntities[vWorkerPool._Index];
            vWorkerPool._Index++;
            if (vWorkerPool._Index >= vWorkerPool._poolSize) {
                vWorkerPool._Index = 0;
            }
            temp.SetActive(true);
            //UnityEngine.Debug.Log("\"" + temp.name + "\" is turn on and ready to be use.");
            return temp;
        } else {
            UnityEngine.Debug.Log("Worker Pool is null.");
            return null;
        }
    }
    /// <summary>
    /// Remove a Pool from LAKE, use the first pool in the LAKE if it removed a Pool that is Worker Pool
    /// </summary>
    public PoolData PoolRemove(string _poolName) {
        for (int a = 0; a < vPools.Count; a++) {
            if (vPools[a]._poolName.CompareTo(_poolName) == 0) {
                if (vPools[a]._poolName.CompareTo(vWorkerPool._poolName) == 0) {
                    vPools.RemoveAt(a);
                    vWorkerPool = vPools[0];
                } else {
                    vPools.RemoveAt(a);
                }
            }
        }
        return vWorkerPool;
    }

    public List<PoolData> PoolList() {
        foreach (PoolData x in vPools) {
            UnityEngine.Debug.Log(x._poolName + " contain : " + x._poolSize + " entities.");
        }
        return vPools;
    }
    //--------------------------------------------------------------------------------------------------------------



    /// <summary>
    /// Return the amount of file in a folder
    /// </summary>
    public long DirCount(DirectoryInfo d, string _fileExtention) {
        long i = 0;
        // Add file sizes.
        FileInfo[] fis = d.GetFiles();
        foreach (FileInfo fi in fis) {
            if (fi.Extension.Contains(_fileExtention))
                i++;
        }
        return i;
    }
    //-----------------------------------------




    /// <summary>
    /// Clamp data between min and max value and warp back value in case data go above or below threshold
    /// </summary>
    public static float ClampWrapBack(float _base, float _min, float _max) {
        if (_base > _max) {
            _base = _min;
        }
        if (_base < _min) {
            _base = _max;
        }
        return _base;
    }

    /// <summary>
    /// "This is my string" --4-> "This is my st"
    /// </summary>
    public static string RemoveCharFromEndString(string _baseString, int _amount) {
        string nString = "";
        for (int a = 0; a < _baseString.Length - _amount; a++) {
            nString += _baseString[a];
        }
        return nString;
    }

    public class PoolData {
        public string _poolName;
        public GameObject _poolHolder;
        public List<GameObject> _poolEntities;

        public int _poolSize, _Index;

        public PoolData() {
            _poolName = "";
            _poolHolder = null;
            _poolEntities = new List<GameObject>();
            _Index = 0;
        }
    }
    /// <summary>
    /// Give RGB data into this as vector4 then return normalize color (default A=1)
    /// </summary>
    public static Color NormalizeColorData(Vector3 _Color) {
        return new Color(_Color.x / 255, _Color.y / 255, _Color.z / 255, 1);
    }
    /// <summary>
    /// Give RGBA data into this as vector4 then return normalize color with custom Alpha
    /// </summary>
    public static Color NormalizeColorData(Vector4 _Color) {
        return new Color(_Color.x / 255, _Color.y / 255, _Color.z / 255, _Color.w);
    }

    /// <summary>
    /// Normalize number provide min, max and a base number
    /// </summary>
    public static float NormalizeNumber(float _baseNum, float _min, float _max) {
        _baseNum = (_baseNum - _min) / (_max - _min);
        return _baseNum;
    }

    /// <summary>
    /// Check if number is in between a certain value range
    /// </summary>
    public static bool IsValueBetween(float _value, float _min, float _max, bool _canEqual) {
        if (_canEqual) {
            if (_value >= _min && _value <= _max) {
                return true;
            } else {
                return false;
            }
        } else {
            if (_value > _min && _value < _max) {
                return true;
            } else {
                return false;
            }
        }
    }

    public static void ChangeLineRenderStartAndEndPoint(LineRenderer _lineRenderer, Vector3 _startPoint, Vector3 _endPoint) {
        _lineRenderer.positionCount = 2;
        _lineRenderer.SetPosition(0, _startPoint);
        _lineRenderer.SetPosition(1, _endPoint);
    }

    /// <summary>
    /// BFS path finding, holy fuck my head
    /// </summary>
    public class BFS {
        // Path is the Final path from start to end point, QueueList is for all the pos that have not been visited
        private List<Vector2Int> Path;
        private Queue<Vector2Int> QueueList;
        private Dictionary<Vector2Int, Vector2Int> BackTrack;
        private Dictionary<Vector2Int, Vector2Int> Visited;
        private bool[,] Grid;
        private Vector2Int GridSize;
        private Vector2Int StartPoint, EndPoint;
        private bool PathFound, canRun = false;
        private PokemonObjectScript[,] GControlScpt;

        public void SettingUp(bool[,] _gridData, Vector2Int _startPoint, Vector2Int _endPoint) {
            Grid = _gridData;
            GridSize = new Vector2Int(Grid.GetLength(0), Grid.GetLength(1));
            StartPoint = _startPoint;
            EndPoint = _endPoint;
            GControlScpt = null;
            if (CheckArrayIfInBounds(GridSize, _startPoint) && CheckArrayIfInBounds(GridSize, _endPoint)) {
                if (!Grid[StartPoint.x, StartPoint.y] || !Grid[StartPoint.x, StartPoint.y]) {
                    // UnityEngine.Debug.Log("Problem with start or end point.");
                    canRun = false;
                } else {
                    QueueList = new Queue<Vector2Int>();
                    QueueList.Enqueue(_startPoint);
                    canRun = true;
                }
            } else {
                // UnityEngine.Debug.Log("Start or End point is out of bound.");
            }
            // for (int a = 0; a < GridSize.y; a++) {
            //     for (int b = 0; b < GridSize.x; b++) {
            //         UnityEngine.Debug.Log(Grid[b, a]);
            //     }
            // }
            // UnityEngine.Debug.Log(_gridData.GetLength(0) + "-" + _gridData.GetLength(1));
            VisualizeBoolGrid();
        }

        /// <summary>
        /// true = can move, false = can't move
        /// </summary>
        public void SettingUp(bool[,] _gridData, Vector2Int _startPoint, Vector2Int _endPoint, PokemonObjectScript[,] _gControlScpt) {
            Grid = _gridData;
            GridSize = new Vector2Int(Grid.GetLength(0), Grid.GetLength(1));
            StartPoint = _startPoint;
            EndPoint = _endPoint;
            GControlScpt = _gControlScpt;
            if (CheckArrayIfInBounds(GridSize, _startPoint) && CheckArrayIfInBounds(GridSize, _endPoint)) {
                if (!Grid[StartPoint.x, StartPoint.y] || !Grid[StartPoint.x, StartPoint.y]) {
                    // UnityEngine.Debug.Log("Problem with start or end point.");
                    canRun = false;
                } else {
                    QueueList = new Queue<Vector2Int>();
                    QueueList.Enqueue(_startPoint);
                    canRun = true;
                }
            } else {
                // UnityEngine.Debug.Log("Start or End point is out of bound.");
            }
            // for (int a = 0; a < GridSize.y; a++) {
            //     for (int b = 0; b < GridSize.x; b++) {
            //         UnityEngine.Debug.Log(Grid[b, a]);
            //     }
            // }
            // UnityEngine.Debug.Log(_gridData.GetLength(0) + "-" + _gridData.GetLength(1));
            // VisualizeBoolGrid();
        }

        public List<Vector2Int> GetTheShortestPath(bool _enableSlowMode) {
            Path = new List<Vector2Int>();
            BackTrack = new Dictionary<Vector2Int, Vector2Int>();
            Visited = new Dictionary<Vector2Int, Vector2Int>();
            if (canRun) {
                if (StartPoint == EndPoint) {
                    return new List<Vector2Int> { StartPoint };
                }
                int maxMove = GridSize.x * GridSize.y + 10;
                int InfiControl1 = 0, InfiControl2 = 0;
                do {
                    if (QueueList.Count == 0 || (QueueList.Peek().x == EndPoint.x && QueueList.Peek().y == EndPoint.y)) {
                        // debug purpose
                        // Dictionary<Vector2Int, Vector2Int>.ValueCollection value = Visited.Values;
                        // foreach (Vector2Int x in value) {
                        //     UnityEngine.Debug.Log(x);
                        // }
                        try {
                            BackTrack.Add(StartPoint, StartPoint);
                            Path.Add(EndPoint);
                            Vector2Int tmp = BackTrack[EndPoint];
                            do {
                                Path.Add(tmp);
                                tmp = BackTrack[tmp];
                                if (tmp.x == StartPoint.x && tmp.y == StartPoint.y) {
                                    Path.Add(tmp);
                                    break;
                                }
                                InfiControl2++;
                            } while (true && InfiControl2 <= maxMove);
                        } catch (System.Exception ex) {
                            //UnityEngine.Debug.Log(ex.ToString());
                        }
                        break;
                    } else {
                        if (GControlScpt != null) {
                            CheckValidMove(QueueList.Peek(), GControlScpt, _enableSlowMode);
                        } else {
                            CheckValidMove(QueueList.Peek(), _enableSlowMode);
                        }
                    }
                    InfiControl1++;
                    if (_enableSlowMode) {
                        Thread.Sleep(500);
                    }
                } while (true);
            }
            Path.Reverse();
            return Path;
        }

        // check move direction is valid, right, left, up, down
        private void CheckValidMove(Vector2Int _movePos, bool _enableDebug) {
            // add current pos to visited list
            if (!Visited.ContainsKey(_movePos)) {
                Visited.Add(_movePos, _movePos);
            }
            // check if can go right, left, up, down
            if (CheckArrayIfInBounds(GridSize, new Vector2Int(_movePos.x + 1, _movePos.y))) {
                if (!Visited.ContainsKey(new Vector2Int(_movePos.x + 1, _movePos.y))) {
                    if (Grid[_movePos.x + 1, _movePos.y] != false || checkIfPosIsTheEndPoint(new Vector2Int(_movePos.x + 1, _movePos.y))) {
                        QueueList.Enqueue(new Vector2Int(_movePos.x + 1, _movePos.y));
                        if (!BackTrack.ContainsKey(new Vector2Int(_movePos.x + 1, _movePos.y))) {
                            BackTrack.Add(new Vector2Int(_movePos.x + 1, _movePos.y), _movePos);
                        }
                        if (_enableDebug) {
                            UnityEngine.Debug.Log(_movePos + " Move Right is OK.");
                        }
                    } else {
                        if (_enableDebug) {
                            UnityEngine.Debug.Log(_movePos + " Have Obstacle to the Right.");
                        }
                    }
                }
            }
            if (CheckArrayIfInBounds(GridSize, new Vector2Int(_movePos.x, _movePos.y + 1))) {
                if (!Visited.ContainsKey(new Vector2Int(_movePos.x, _movePos.y + 1))) {
                    if (Grid[_movePos.x, _movePos.y + 1] != false || checkIfPosIsTheEndPoint(new Vector2Int(_movePos.x, _movePos.y + 1))) {
                        QueueList.Enqueue(new Vector2Int(_movePos.x, _movePos.y + 1));
                        if (!BackTrack.ContainsKey(new Vector2Int(_movePos.x, _movePos.y + 1))) {
                            BackTrack.Add(new Vector2Int(_movePos.x, _movePos.y + 1), _movePos);
                        }
                        if (_enableDebug) {
                            UnityEngine.Debug.Log(_movePos + " Move Up is OK.");
                        }
                    } else {
                        if (_enableDebug) {
                            UnityEngine.Debug.Log(_movePos + " Have Obstacle above.");
                        }
                    }
                }
            }
            if (CheckArrayIfInBounds(GridSize, new Vector2Int(_movePos.x - 1, _movePos.y))) {
                if (!Visited.ContainsKey(new Vector2Int(_movePos.x - 1, _movePos.y))) {
                    if (Grid[_movePos.x - 1, _movePos.y] != false || checkIfPosIsTheEndPoint(new Vector2Int(_movePos.x - 1, _movePos.y))) {
                        QueueList.Enqueue(new Vector2Int(_movePos.x - 1, _movePos.y));
                        if (!BackTrack.ContainsKey(new Vector2Int(_movePos.x - 1, _movePos.y))) {
                            BackTrack.Add(new Vector2Int(_movePos.x - 1, _movePos.y), _movePos);

                        }
                        if (_enableDebug) {
                            UnityEngine.Debug.Log(_movePos + " Move Left is OK.");
                        }
                    } else {
                        if (_enableDebug) {
                            UnityEngine.Debug.Log(_movePos + " Have Obstacle to the left.");
                        }
                    }
                }
            }
            if (CheckArrayIfInBounds(GridSize, new Vector2Int(_movePos.x, _movePos.y - 1))) {
                if (!Visited.ContainsKey(new Vector2Int(_movePos.x, _movePos.y - 1))) {
                    if (Grid[_movePos.x, _movePos.y - 1] != false || checkIfPosIsTheEndPoint(new Vector2Int(_movePos.x + 1, _movePos.y - 1))) {
                        QueueList.Enqueue(new Vector2Int(_movePos.x, _movePos.y - 1));
                        if (!BackTrack.ContainsKey(new Vector2Int(_movePos.x, _movePos.y - 1))) {
                            BackTrack.Add(new Vector2Int(_movePos.x, _movePos.y - 1), _movePos);
                        }
                        if (_enableDebug) {
                            UnityEngine.Debug.Log(_movePos + " Move Down is OK.");
                        }
                    } else {
                        if (_enableDebug) {
                            UnityEngine.Debug.Log(_movePos + " Have Obstacle to below.");
                        }
                    }
                }

            }
            //-----------------------------------------
            // de queue
            QueueList.Dequeue();
        }

        private void CheckValidMove(Vector2Int _movePos, PokemonObjectScript[,] _gControlScpt, bool _enableDebug) {
            // add current pos to visited list
            if (!Visited.ContainsKey(_movePos)) {
                Visited.Add(_movePos, _movePos);
            }
            // check if can go right, left
            if (CheckArrayIfInBounds(GridSize, new Vector2Int(_movePos.x + 1, _movePos.y))) {
                if (!Visited.ContainsKey(new Vector2Int(_movePos.x + 1, _movePos.y))) {
                    if (!_gControlScpt[_movePos.x + 1, _movePos.y].ActiveStatus() || checkIfPosIsTheEndPoint(new Vector2Int(_movePos.x + 1, _movePos.y))) {
                        QueueList.Enqueue(new Vector2Int(_movePos.x + 1, _movePos.y));
                        if (!BackTrack.ContainsKey(new Vector2Int(_movePos.x + 1, _movePos.y))) {
                            BackTrack.Add(new Vector2Int(_movePos.x + 1, _movePos.y), _movePos);
                            if (_enableDebug) { UnityEngine.Debug.Log(new Vector2Int(_movePos.x + 1, _movePos.y) + " Added to BackTrack."); }
                        }
                        if (_enableDebug) { UnityEngine.Debug.Log(_movePos + " Move Right is OK."); }
                    } else {
                        if (_enableDebug) { UnityEngine.Debug.Log(_movePos + " Have Obstacle to the Right."); }
                    }
                }
            }
            if (CheckArrayIfInBounds(GridSize, new Vector2Int(_movePos.x - 1, _movePos.y))) {
                if (!Visited.ContainsKey(new Vector2Int(_movePos.x - 1, _movePos.y))) {
                    if (!_gControlScpt[_movePos.x - 1, _movePos.y].ActiveStatus() || checkIfPosIsTheEndPoint(new Vector2Int(_movePos.x - 1, _movePos.y))) {
                        QueueList.Enqueue(new Vector2Int(_movePos.x - 1, _movePos.y));
                        if (!BackTrack.ContainsKey(new Vector2Int(_movePos.x - 1, _movePos.y))) {
                            BackTrack.Add(new Vector2Int(_movePos.x - 1, _movePos.y), _movePos);
                            if (_enableDebug) { UnityEngine.Debug.Log(new Vector2Int(_movePos.x - 1, _movePos.y) + " Added to BackTrack."); }
                        }
                        if (_enableDebug) { UnityEngine.Debug.Log(_movePos + " Move Left is OK."); }
                    } else {
                        if (_enableDebug) { UnityEngine.Debug.Log(_movePos + " Have Obstacle to the left."); }
                    }
                }
            }
            // check if can go up, down
            if (CheckArrayIfInBounds(GridSize, new Vector2Int(_movePos.x, _movePos.y + 1))) {
                if (!Visited.ContainsKey(new Vector2Int(_movePos.x, _movePos.y + 1))) {
                    if (!_gControlScpt[_movePos.x, _movePos.y + 1].ActiveStatus() || checkIfPosIsTheEndPoint(new Vector2Int(_movePos.x, _movePos.y + 1))) {
                        QueueList.Enqueue(new Vector2Int(_movePos.x, _movePos.y + 1));
                        if (!BackTrack.ContainsKey(new Vector2Int(_movePos.x, _movePos.y + 1))) {
                            BackTrack.Add(new Vector2Int(_movePos.x, _movePos.y + 1), _movePos);
                            if (_enableDebug) { UnityEngine.Debug.Log(new Vector2Int(_movePos.x, _movePos.y + 1) + " Added to BackTrack."); }
                        }
                        if (_enableDebug) { UnityEngine.Debug.Log(_movePos + " Move Up is OK."); }
                    } else {
                        if (_enableDebug) { UnityEngine.Debug.Log(_movePos + " Have Obstacle above."); }
                    }
                }
            }
            if (CheckArrayIfInBounds(GridSize, new Vector2Int(_movePos.x, _movePos.y - 1))) {
                if (!Visited.ContainsKey(new Vector2Int(_movePos.x, _movePos.y - 1))) {
                    if (!_gControlScpt[_movePos.x, _movePos.y - 1].ActiveStatus() || checkIfPosIsTheEndPoint(new Vector2Int(_movePos.x, _movePos.y - 1))) {
                        QueueList.Enqueue(new Vector2Int(_movePos.x, _movePos.y - 1));
                        if (!BackTrack.ContainsKey(new Vector2Int(_movePos.x, _movePos.y - 1))) {
                            BackTrack.Add(new Vector2Int(_movePos.x, _movePos.y - 1), _movePos);
                            if (_enableDebug) { UnityEngine.Debug.Log(new Vector2Int(_movePos.x, _movePos.y - 1) + " Added to BackTrack."); }
                        }
                        if (_enableDebug) { UnityEngine.Debug.Log(_movePos + " Move Down is OK."); }
                    } else {
                        if (_enableDebug) { UnityEngine.Debug.Log(_movePos + " Have Obstacle to below."); }
                    }
                }

            }
            //-----------------------------------------
            // de queue
            QueueList.Dequeue();
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

        private bool CheckArrayForData(Vector2Int _pos) {
            return Grid[_pos.x, _pos.y];
        }
        private bool checkIfPosIsTheEndPoint(Vector2Int _pos) {
            if (_pos.x == EndPoint.x && _pos.y == EndPoint.y) {
                return true;
            } else {
                return false;
            }
        }
        public void VisualizeBoolGrid() {
            string str = "";
            for (int a = GridSize.y - 1; a >= 0; a--) {
                str = "";
                for (int b = 0; b < GridSize.x; b++) {
                    if (Grid[b, a]) {
                        Grid[b, a] = true;
                        str += "0";
                    } else {
                        Grid[b, a] = true;
                        str += "X";
                    }
                }
                UnityEngine.Debug.Log(str);
            }
        }

        public int CalculateAmountOfTurningPoint(List<Vector2Int> _pathReference) {
            int turnCounter = 0;
            if (_pathReference.Count > 1) {
                turnCounter = 1;
                Vector2Int lastPos, nextPos;
                string previousMoveDirection = "", nextMoveDirection = "";
                lastPos = _pathReference[0];
                nextPos = _pathReference[1];
                if (lastPos.x < nextPos.x && lastPos.y == nextPos.y) {
                    previousMoveDirection = "R";
                } else if (lastPos.x > nextPos.x && lastPos.y == nextPos.y) {
                    previousMoveDirection = "L";
                } else if (lastPos.x == nextPos.x && lastPos.y < nextPos.y) {
                    previousMoveDirection = "U";
                } else {
                    previousMoveDirection = "D";
                }
                for (int a = 2; a < _pathReference.Count; a++) {
                    lastPos = _pathReference[a - 1];
                    nextPos = _pathReference[a];
                    if (lastPos.x < nextPos.x && lastPos.y == nextPos.y) {
                        nextMoveDirection = "R";
                    } else if (lastPos.x > nextPos.x && lastPos.y == nextPos.y) {
                        nextMoveDirection = "L";
                    } else if (lastPos.x == nextPos.x && lastPos.y < nextPos.y) {
                        nextMoveDirection = "U";
                    } else {
                        nextMoveDirection = "D";
                    }
                    if (previousMoveDirection.CompareTo(nextMoveDirection) == 0) {
                        previousMoveDirection = nextMoveDirection;
                    } else {
                        turnCounter++;
                        previousMoveDirection = nextMoveDirection;
                    }
                }
                return turnCounter;
            } else {
                return turnCounter;
            }
        }
    }
}
