using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Random = System.Random;
using System.Threading;
#if WINDOWS_UWP
using Windows.Storage;
#endif

//Note : for all analytics the time saved is based on the UNIX time 1st january 1970

public class AssessmentData
{
    // Analytics for assessment (answer to question 1; answer to question 2; id of the puzzle assessed)
    public float answerq1;
    public float answerq2;
    public int puzzle_ID;
}
public class MatrixData
{
    // Analytics for the 3d Matrix (actual state for the matrix; time)
    // The actual state of the cube (3d matrix) is a matrix with number or "X" in it, an "X" means that the space in the cube is unused, a number k means that the piece k uses the space) 
    public string matrixWithNumber;
    public string time;
}

public class PlayerData
{
    // Analytics for the player data (number of completed puzzles; id of the last played puzzle, puzzle level (easy/medium/hard), score of the player on the session, time)
    public int number_of_completed_puzzle;
    public int puzzle_ID;
    public string puzzle_lvl;
    public float score;
    public string time;
}

public class TrackingData
{
    // Analytics about the player position (time, position of the player, actual state of the matrix with 0 and 1 in it)
    //0 means unused space, 1 means that a piece use the space in the cube.
    public string time;
    public Vector3 headPosition;
    public string matrix;
}

public class Grid_big_cube_update : MonoBehaviour
{
    /*
     * This class is handling the game session (generation of puzzle, game mechanics, puzzle completion checking, data collection) 
     */



    #region Variables
    [Tooltip("3d Transparent Cube")]
    public GameObject big_Cube;
    [Tooltip("Size of a small cube part of the big empty cube")]
    public float size_Small_Cube = 0.07f;
    [Tooltip("Menu with the break button")]
    public GameObject break_menu;
    [Tooltip("Canvas with assessment questions")]
    public GameObject assessment_canvas;
    [Tooltip("Canvas with the button to validate assessment")]
    public GameObject assessemnt_button;
    [Tooltip("Text to display the score")]
    public Text scoreText;
    [Tooltip("Timer component")]
    public Timer timer;
    [Tooltip("Camera of the player")]
    public GameObject mainCamera;
    [Tooltip("Data collection frequency (s)")]
    public float period = 1f;
#if WINDOWS_UWP
    public StorageFile file_mat;
    public StorageFile file_player;
    public StorageFile file_tracking;
    public StorageFile file_assessment;
#endif


    [HideInInspector] public Material mat_transp;
    [HideInInspector] public Renderer rend_test;
    [HideInInspector] public List<float[,,]> list_matrix = new List<float[,,]>();    
    [HideInInspector] public float size_Big_Cube;
    [HideInInspector] public string mode;
    [HideInInspector] public bool nextPuzzle = false;
    [HideInInspector] public Vector3 scaleChange;
    [HideInInspector] public List<GameObject> small_Cube_List;
    [HideInInspector] public float local_Scale_Small_Cube;    
    [HideInInspector] public List<int> lst;
    [HideInInspector] public List<GameObject> pieces_List;
    [HideInInspector] public Material[] mats;
    [HideInInspector] public string sessionDateTime;
    [HideInInspector] public string puzzle_lvl;
    [HideInInspector] public float timelvl;
    [HideInInspector] public int[] shuffle;
    [HideInInspector] public int fake_pieces_number = 2;
    [HideInInspector] public bool hasAssess = false;
    [HideInInspector] public float puzzleScore;
    [HideInInspector] public bool doBreak = false;
    [HideInInspector] public float startBreakTime = 0.0f;
    [HideInInspector] public int nb_slot;
    [HideInInspector] public int nb_mini;


    private int seed;
    private float trackingRecordingTime = 0.0f;
    private List<int> used_Puzzle_Index = new List<int>();
    private int puzzle_index;
    private int fake_puzzle_index;
    private float startingPuzzleTime = 0.0f;
    private List<string> modes = new List<string> { "easy", "medium", "hard" };
    private List<string> modes_order = new List<string> { "easy", "easy", "medium", "hard" };
    private float score = 0;
    private int numPuzzlesCompleted = 0;


    int[,,] puzzle_Grid = new int[3, 3, 3];
    int[,,] fake_Puzzle_Grid = new int[3, 3, 3];
    Renderer rend;
    Dictionary<int, int> matrix = new Dictionary<int, int>();
    Dictionary<int, string> matrixWithNumber = new Dictionary<int, string>();
    int num_minicubes;


    //public Mesh mesh;
    //public int number_Piece_Min;
    //public int number_Piece_Max;
    //public List<float> ligne = new List<float>(3);
    //public Text bannerText;

    [Flags]
    enum MultiHue : short
    {
        //This enumerator is used to specify that the pieces can be rotated and moved but cannot be rescaled.
        Move = 1,
        Rotate = 2,
        Scale = 0

    };
    #endregion

    public void CombineMeshes(GameObject go)
    {
        /*
         * This function combines the meshes of the childrens of the go object to one single mesh
         * The idea is to build a single mesh for a piece "parent_N" from k little cubes
         */


        Quaternion oldRot = go.transform.rotation;
        Vector3 oldPos = go.transform.position;
        Vector3 oldScale = go.transform.localScale;
        go.transform.rotation = Quaternion.identity;
        go.transform.position = Vector3.zero;
        go.transform.localScale = Vector3.one;
        MeshFilter[] filters = go.GetComponentsInChildren<MeshFilter>();
        Mesh finalMesh = new Mesh();
        CombineInstance[] combiners = new CombineInstance[filters.Length];

        for (int a = 0; a < filters.Length; a++)
        {
            if (filters[a].transform == go.transform)
                continue;
            combiners[a].subMeshIndex = 0;
            combiners[a].mesh = filters[a].sharedMesh;
            combiners[a].transform = filters[a].transform.localToWorldMatrix;
        }

        finalMesh.CombineMeshes(combiners);
        go.GetComponent<MeshFilter>().sharedMesh = finalMesh;
        go.transform.rotation = oldRot;
        go.transform.position = oldPos;
        go.transform.localScale = oldScale;
        for (int a = 0; a < go.transform.childCount; a++)
            go.transform.GetChild(a).gameObject.SetActive(true);
    }

    private void SavePlayerJson(PlayerData playerData, bool filling)
    {
        /*
         * This function saves the analytics Data to a specific json file as a json string.
         * Each time this function is called a line is added to the specific json file that saves data 
         * If filling == True that means that the player just started a new puzzle in order to split the data in regions in the json file 
         */  
        string player;
        if (filling== false)
        {
            player = JsonUtility.ToJson(playerData);
        }
        else
        {
            player = "---------NEW_PUZZLE---------";
        }
#if WINDOWS_UWP
        Thread _thread = new Thread(() => WriteFile(file_player, "_player_", player));
        _thread.Start();
#else
        string dir = Application.persistentDataPath;
        string path = dir + sessionDateTime + "_player_" + mode + ".json";
        if (!File.Exists(path))
        {
        File.WriteAllText(path, player + Environment.NewLine);
        }
        else
        {
        File.AppendAllText(path, player + Environment.NewLine);
        }
#endif
    }

    private void SaveTrackingJson(TrackingData trackingData, bool filling)
    {
        /*
         * This function saves the analytics Data to a specific json file as a json string.
         * Each time this function is called a line is added to the specific json file that saves data 
         * If filling == True that means that the player just started a new puzzle in order to split the data in regions in the json file 
         */
        string tracking;
        if (filling == false)
        {
            tracking = JsonUtility.ToJson(trackingData);
        }
        else
        {
            tracking = "---------NEW_PUZZLE---------";
        }
#if WINDOWS_UWP
        Thread _thread = new Thread(() => WriteFile(file_tracking, "_tracking_", tracking));
        _thread.Start();
#else
        string dir = Application.persistentDataPath;
        string path = dir + sessionDateTime + "_tracking_" + mode + ".json";
        if (!File.Exists(path))
        {
        File.WriteAllText(path, tracking + Environment.NewLine);
        }
        else
        {
        File.AppendAllText(path, tracking + Environment.NewLine);
        };
#endif
    }

    private void SaveMatrixJson(MatrixData matrixData, bool filling)
    {
        /*
         * This function saves the analytics Data to a specific json file as a json string.
         * Each time this function is called a line is added to the specific json file that saves data 
         * If filling == True that means that the player just started a new puzzle in order to split the data in regions in the json file 
         */
        string matrixdata;
        if (filling == false)
        {
            matrixdata = JsonUtility.ToJson(matrixData);
        }
        else
        {
            matrixdata = "---------NEW_PUZZLE---------";
        }

#if WINDOWS_UWP
        Thread _thread = new Thread(() => WriteFile(file_mat, "_matrix_", matrixdata));
        _thread.Start();
#else
        string dir = Application.persistentDataPath;
        string path = dir + sessionDateTime + "_matrix_" + mode + ".json";
        if (!File.Exists(path))
        {
        File.WriteAllText(path, matrixdata + Environment.NewLine);
        }
        else
        {
        File.AppendAllText(path, matrixdata + Environment.NewLine);
        }
#endif

    }

    private void SaveAssessJson(AssessmentData assessmentData)
    {
        /*
         * This function saves the analytics Data to a specific json file as a json string.
         * Each time this function is called a line is added to the specific json file that saves data 
         * If filling == True that means that the player just started a new puzzle in order to split the data in regions in the json file 
         */
        string assessmentdata;
        assessmentdata = JsonUtility.ToJson(assessmentData);

#if WINDOWS_UWP
        Thread _thread = new Thread(() => WriteFile(file_assessment, "_assessment_", assessmentdata));
        _thread.Start();
#else
        string dir = Application.persistentDataPath;
        string path = dir + sessionDateTime + "_assessment_" + mode + ".json";
        if (!File.Exists(path))
        {
        File.WriteAllText(path, assessmentdata + Environment.NewLine);
        }
        else
        {
        File.AppendAllText(path, assessmentdata + Environment.NewLine);
        }
#endif

    }

#if WINDOWS_UWP
    private async void WriteFile(StorageFile file,string topic,string data)
    {
        // Way to write in a file with Hololens environment
        await FileIO.AppendTextAsync(file, data + Environment.NewLine);
    }

    private async void CreateFiles()
    {
        // Creation of files to save data in documents library in Hololens Environment
        StorageFolder folder = KnownFolders.DocumentsLibrary;
        string path = sessionDateTime + "_matrix_" + mode + ".json";
        file_mat = await folder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);
        path = sessionDateTime + "_player_" + mode + ".json";
        file_player = await folder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);
        path = sessionDateTime + "_tracking_" + mode + ".json";
        file_tracking = await folder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);
        path = sessionDateTime + "_assessment_" + mode + ".json";
        file_assessment = await folder.CreateFileAsync(path, CreationCollisionOption.OpenIfExists);
    }
#endif
    private void UpdateStateMatrix()
    {
        /*
         * This function updates the 3d matrix which represents the 3d big cubes, the matrix is filled with 1 when a piece uses a slot and 0 if not.
         * This function is also saving the position of each piece in the big cube at a specific time
         */


        // slots is where the pieces can go. one piece takes up multiple slots
        GameObject[] slots = GameObject.FindGameObjectsWithTag("aim_cube");
        // each piece is made of a few minicubes
        GameObject[] minicubes = GameObject.FindGameObjectsWithTag("small_cube");
        nb_mini = minicubes.Length;
        nb_slot = slots.Length;
        for (int i = 0; i < slots.Length; i++)
        {
            //float pieceRecordingTime = 0.0f;
            bool slotIsFilled = false; // this variable is used as a flag to reset matrix[i] to 0 when piece is moved out of cube
            for (int k = 0; k < minicubes.Length; k++)
            {
                if (slots[i].transform.position == minicubes[k].transform.position)
                {
                    // condition: cube has been moved to within the big cube
                    // logic for changing 0s to 1s in state matrix
                    matrix[i] = 1;
                    matrixWithNumber[i] = minicubes[k].transform.parent.gameObject.name.Split('_')[1];
                    slotIsFilled = true;
                }
            }

            if (!slotIsFilled)
            {
                matrix[i] = 0;
                matrixWithNumber[i] = "X";
            }
        }
        // analytics - puzzle
        MatrixData matrixData = new MatrixData();
        matrixData.time = DateTime.Now.ToString();
        matrixData.matrixWithNumber = JsonConvert.SerializeObject(matrixWithNumber);
        SaveMatrixJson(matrixData,false);
    }

    private bool CheckMatrixCompletion()
    {
        // This function checks if game is complete, i.e., all values in matrix are 1's.
        foreach (KeyValuePair<int, int> entry in matrix)
        {
            if (entry.Value == 0) return false;
        }
        return true;
    }

    private int[,,] Cast_to_int(float[,,] floatmat)
    {
        // Cast a 3d matrix of float to a 3d matrix of int
        int[,,] mat = new int[floatmat.GetLength(0), floatmat.GetLength(1), floatmat.GetLength(2)];
        for (int i = 0; i < floatmat.GetLength(0); i++)
        {
            for (int j = 0; j < floatmat.GetLength(1); j++)
            {
                for (int k = 0; k < floatmat.GetLength(2); k++)
                {
                    mat[i, j, k] = (int)floatmat[i, j, k];
                }
            }
        }
        return mat;
    }
    public void Generate_puzzle()
    {
        /*
         * This function is used to generate the entire puzzle game objhect from a puzzle grid
         */


        //**** Setting up the seed of random system according to difficulty level ****//
        if (puzzle_lvl == "easy")
        {
            seed = 10;
        }
        else if (puzzle_lvl == "medium")
        {
            seed = 11;
        }
        else if (puzzle_lvl == "hard")
        {
            seed = 12;
        }
        else if (puzzle_lvl == "veryhard")
        {
            seed = 13;
        }
        Random rnd = new Random(seed);


        //**** Loading materials & intializing size of the Big empty cube according to the size of puzzle grid and loading the puzzle grid (one puzzle can only be seen once by the user in a session ****//
        mats = Resources.LoadAll("Materials_piece", typeof(Material)).Cast<Material>().ToArray();
        mats = mats.OrderBy(a => rnd.Next()).ToArray();
        mat_transp = Resources.Load("Materials/Transparent", typeof(Material)) as Material;
        size_Big_Cube = puzzle_Grid.GetLength(0) * size_Small_Cube;
        scaleChange = new Vector3(size_Big_Cube, size_Big_Cube, size_Big_Cube);
        big_Cube.transform.localScale = scaleChange;
        pieces_List = new List<GameObject>();
        puzzle_index = rnd.Next(1, 101);
        fake_puzzle_index = rnd.Next(1, 101);
        while (used_Puzzle_Index.Contains(puzzle_index))
        {
            puzzle_index = rnd.Next(1, 101);
        }
        used_Puzzle_Index.Add(puzzle_index);
        puzzle_Grid = Cast_to_int(list_matrix[puzzle_index]);
        fake_Puzzle_Grid = Cast_to_int(list_matrix[fake_puzzle_index]); // This fake puzzle grid is used in hard mode to add fake pieces.
        lst = puzzle_Grid.Cast<int>().ToList();

        //**** Creating the pieces (which are for now empty gameobject with a name parent_X) ****//
        num_minicubes = 0;
        for (int i = lst.Min(); i < lst.Max() + 1; i++)
        {
            GameObject parent = new GameObject("parent_" + i.ToString());
            parent.transform.position = big_Cube.transform.position;
            parent.transform.rotation = big_Cube.transform.rotation;
            parent.transform.localScale = big_Cube.transform.localScale;
            parent.AddComponent<MeshCollider>();
            pieces_List.Add(parent);
        }

        //**** Adding fake pieces ****//
        if (puzzle_lvl == "hard" || puzzle_lvl=="veryhard")
        {
            for (int nbr_fake = 1; nbr_fake < 1+ fake_pieces_number; nbr_fake++)
            {
                GameObject parent = new GameObject("parent_" + (lst.Max() + nbr_fake).ToString());
                parent.transform.position = big_Cube.transform.position;
                parent.transform.rotation = big_Cube.transform.rotation;
                parent.transform.localScale = big_Cube.transform.localScale;
                parent.AddComponent<MeshCollider>();
                pieces_List.Add(parent);
            }
            for (int i = 0; i < fake_Puzzle_Grid.GetLength(0); i++)
            {
                for (int j = 0; j < fake_Puzzle_Grid.GetLength(0); j++)
                {
                    for (int k = 0; k < fake_Puzzle_Grid.GetLength(0); k++)
                    {
                        if (fake_Puzzle_Grid[i, j, k] < 3)
                        {
                            GameObject small_Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                            small_Cube.transform.parent = pieces_List[lst.Max() - 1 + fake_Puzzle_Grid[i, j, k]].transform;
                            local_Scale_Small_Cube = ((float)1) / ((float)fake_Puzzle_Grid.GetLength(0));
                            small_Cube.transform.localScale = new Vector3(local_Scale_Small_Cube, local_Scale_Small_Cube, local_Scale_Small_Cube);
                            small_Cube.transform.localPosition = new Vector3(local_Scale_Small_Cube * i - local_Scale_Small_Cube,
                                                                             local_Scale_Small_Cube * j - local_Scale_Small_Cube,
                                                                             local_Scale_Small_Cube * k - local_Scale_Small_Cube);

                            small_Cube.tag = "small_cube";
                            Destroy(small_Cube.GetComponent<BoxCollider>());
                            rend = small_Cube.GetComponent<Renderer>();
                            rend.enabled = true;
                            rend.sharedMaterial = mats[lst.Max() - 1 + fake_Puzzle_Grid[i, j, k]];
                            small_Cube_List.Add(small_Cube);
                        }
                    }
                }
        
            }
        }

        //**** Adding small cubes to each parent according to the puzzle grid, the small cube will be used to create the mesh of the parent, for puzzle completion checking and for the snapping interaction ****//
        //**** Adding transparent cubes in the big cube that will be used as slot of the big cube ****//
        for (int i = 0; i < puzzle_Grid.GetLength(0); i++)
        {
            for (int j = 0; j < puzzle_Grid.GetLength(1); j++)
            {
                for (int k = 0; k < puzzle_Grid.GetLength(2); k++)
                {
                    //Create Small Cube that is grababble and with Mesh
                    GameObject small_Cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    small_Cube.transform.parent = pieces_List[puzzle_Grid[i, j, k] - 1].transform;
                    local_Scale_Small_Cube = ((float)1) / ((float)puzzle_Grid.GetLength(0));
                    small_Cube.transform.localScale = new Vector3(local_Scale_Small_Cube, local_Scale_Small_Cube, local_Scale_Small_Cube);
                    small_Cube.transform.localPosition = new Vector3(local_Scale_Small_Cube * i - local_Scale_Small_Cube,
                                                                     local_Scale_Small_Cube * j - local_Scale_Small_Cube,
                                                                     local_Scale_Small_Cube * k - local_Scale_Small_Cube);

                    small_Cube.tag = "small_cube";
                    Destroy(small_Cube.GetComponent<BoxCollider>());
                    rend = small_Cube.GetComponent<Renderer>();
                    rend.enabled = true;
                    rend.sharedMaterial = mats[puzzle_Grid[i, j, k] - 1];
                    small_Cube_List.Add(small_Cube);


                    GameObject small_Cube_Aim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    small_Cube_Aim.GetComponent<BoxCollider>().isTrigger = true;
                    Destroy(small_Cube_Aim.GetComponent<BoxCollider>());
                    small_Cube_Aim.transform.parent = big_Cube.transform;
                    local_Scale_Small_Cube = ((float)1) / ((float)puzzle_Grid.GetLength(0));
                    small_Cube_Aim.transform.localScale = new Vector3(local_Scale_Small_Cube, local_Scale_Small_Cube, local_Scale_Small_Cube)*0.98f;
                    small_Cube_Aim.transform.localPosition = new Vector3(local_Scale_Small_Cube * i - local_Scale_Small_Cube,
                                                                     local_Scale_Small_Cube * j - local_Scale_Small_Cube,
                                                                     local_Scale_Small_Cube * k - local_Scale_Small_Cube);
                    rend_test = small_Cube_Aim.GetComponent<Renderer>();
                    rend_test.material = mat_transp;
                    small_Cube_Aim.tag = "aim_cube";
                    num_minicubes++;
                }
            }
        }
        //**** Initializing puzzle analytics ****//
        for (int i = 0; i < num_minicubes; i++)
        {
            matrix[i] = 0;
            matrixWithNumber[i] = "X";
        }
        HashSet<float> filled = new HashSet<float>();
        float randomPosition = 0.0f;

        //**** Creating future poistion for the pieces to start the puzzle ****//
        var numbers = Enumerable.Range(0, lst.Max());
        shuffle = numbers.OrderBy(a => rnd.Next()).ToArray();
        if (puzzle_lvl == "hard" || puzzle_lvl=="veryhard")
        {
            for(int i = 0; i < fake_pieces_number; i++)
            {
                shuffle = shuffle.Append(lst.Max() + i).ToArray();
            }
        }
        List<float> positions = new List<float>();
        for (int i = 0; i < shuffle.Count(); i++)
        {
            positions = positions.Append(0.25f*(i-(shuffle.Count()-1)/2)).ToList();
        }
        //**** Adding component to the pieces for maniuplation and for the game (MRTK component, box collider for collision (reduced size for an easy fitting of the piece), rigidbody, Custom scripts for game ****//
        //**** Combining meshes of the children to create the piece and adding a material ****//
        foreach (int i in shuffle)
        {
            GameObject go = pieces_List[i];
            go.transform.parent = GameObject.Find("Level").transform;
            go.AddComponent<MeshFilter>();
            go.AddComponent<MeshRenderer>();
            CombineMeshes(go);
            Mesh mesh = go.GetComponent<MeshFilter>().sharedMesh;
            for (int n = 0; n < go.transform.childCount; n++)
            {
                BoxCollider box = go.AddComponent<BoxCollider>();
                box.center = go.transform.GetChild(n).localPosition;
                box.size = go.transform.GetChild(n).localScale * 0.85f;
            }
            go.AddComponent<NearInteractionGrabbable>();
            go.AddComponent<ObjectManipulator>();
            go.AddComponent<parent_property>();
            go.AddComponent<Rigidbody>();
            go.GetComponent<Rigidbody>().useGravity = false;
            go.GetComponent<ObjectManipulator>().TwoHandedManipulationType = (TransformFlags)(MultiHue.Move | MultiHue.Rotate | MultiHue.Scale);
            go.GetComponent<ObjectManipulator>().UseForcesForNearManipulation = true;
            rend = go.GetComponent<Renderer>();
            rend.enabled = true;
            rend.sharedMaterial = mats[i];
            go.AddComponent<Snap>();


            //**** Moving (and rotating for medium and hard) the pieces outside of the cube so the puzzle is incomplete ****//
            Vector3 benchPosition = GameObject.Find("Starting_Bench").transform.position;
            List<float> rotations = new List<float>() { 0.0f, 90.0f, 180.0f, 270.0f };
            if (puzzle_lvl == "easy" && i > 4)
            {
                while (filled.Contains(randomPosition))
                {
                    randomPosition = positions[rnd.Next(positions.Count)];
                }
                filled.Add(randomPosition);
                go.transform.position = benchPosition + new Vector3(randomPosition, 0, 0);
            }
            else if ((puzzle_lvl == "hard" || puzzle_lvl=="veryhard") && i > 0)
            {
                while (filled.Contains(randomPosition))
                {
                    randomPosition = positions[rnd.Next(positions.Count)];
                }
                filled.Add(randomPosition);
                float randomRotation = rotations[rnd.Next(rotations.Count)];
                go.transform.position = benchPosition + new Vector3(randomPosition, 0, 0);
                go.transform.eulerAngles = new Vector3(randomRotation, 0, 0);
            }
            else if (puzzle_lvl == "medium" && i>2)
            {
                while (filled.Contains(randomPosition))
                {
                    randomPosition = positions[rnd.Next(positions.Count)];
                }
                filled.Add(randomPosition);
                float randomRotation = rotations[rnd.Next(rotations.Count)];
                go.transform.position = benchPosition + new Vector3(randomPosition, 0, 0);
                go.transform.eulerAngles = new Vector3(randomRotation, 0, 0);
            }
            go.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
            go.GetComponent<Rigidbody>().isKinematic = true;

        }
    }

    private float CalculateScore()
    {
        // Score based on the time spent to do a puzzle
        score += (float)Math.Round((timelvl - (Time.time - startingPuzzleTime))*(100/timelvl));
        return score;
    }
    public void Reset()
    {
        /*
         * Function that regenerate a new puzzle in the same session and reset some variables used in the game
         */

        nextPuzzle = false;
        break_menu.SetActive(true);
        foreach (GameObject aimcube in GameObject.FindGameObjectsWithTag("aim_cube"))
        {
            Destroy(aimcube);
        }
        foreach (GameObject piece in pieces_List)
        {
            Destroy(piece);
        }
        Generate_puzzle();
        startingPuzzleTime = Time.time;
        trackingRecordingTime = 0f;
        hasAssess = false;
        
    }
    public void HaveABreak()
    {
        /*
         * Function called when the user suceed to make a puzzle or press the button to switch puzzle
         * This function handle the break time (assessment, display of some information for the user, save the game data and trigger the function that sends the data on the azure storage
         */

        nextPuzzle = true;
        doBreak = true;
        startBreakTime = Time.time;
        assessemnt_button.SetActive(true);
        assessment_canvas.SetActive(true);
        GameObject.Find("Time").GetComponent<Timer>().timerIsRunning = false;
        GameObject.Find("Time").GetComponent<Timer>().timeBreak = 20.0f;
        foreach (GameObject piece in pieces_List)
        {
            Destroy(piece);
        }
        foreach (GameObject aimcube in GameObject.FindGameObjectsWithTag("aim_cube"))
        {
            Destroy(aimcube);
        }
        break_menu.SetActive(false);
        PlayerData playerData = new PlayerData();
        playerData.number_of_completed_puzzle = numPuzzlesCompleted;
        playerData.puzzle_ID = puzzle_index;
        playerData.score = puzzleScore;
        playerData.time = DateTime.Now.ToString();
        SavePlayerJson(playerData, false);
        SaveMatrixJson(new MatrixData(), true);
        SavePlayerJson(new PlayerData(), true);
        SaveTrackingJson(new TrackingData(), true);
        this.gameObject.GetComponent<Read_Write_json>().mode = mode;
        this.gameObject.GetComponent<Read_Write_json>().sessionDateTime = sessionDateTime;
        this.gameObject.GetComponent<Read_Write_json>().upload = true;
    }

    public void Assess()
    {
        /*
         * Handle Assessment of the user, save the answer of the user and trigger the function that sends the data on the azure storage
         * 
         */
        AssessmentData assessmentData = new AssessmentData();
        assessmentData.answerq1 = GameObject.Find("Slider_Assessment_q1").GetComponent<PinchSlider>().SliderValue;
        assessmentData.answerq2 = GameObject.Find("Slider_Assessment_q2").GetComponent<PinchSlider>().SliderValue;
        assessmentData.puzzle_ID = puzzle_index;
        SaveAssessJson(assessmentData);
        GameObject.Find("Slider_Assessment_q1").GetComponent<PinchSlider>().SliderValue = 0.5f;
        GameObject.Find("Slider_Assessment_q2").GetComponent<PinchSlider>().SliderValue = 0.5f;
        assessment_canvas.SetActive(false);
        assessemnt_button.SetActive(false);
        hasAssess = true;
        if (doBreak == true)
        {
            GameObject.Find("Empty_Big_Cube").GetComponent<Read_Write_json>().assessupload = hasAssess;
        }
        GameObject.Find("Time").GetComponent<Timer>().hasAssess = hasAssess;


    }

    void OnEnable()
    {
        /*
         * Initialize the session
         */
        used_Puzzle_Index = new List<int>();
        string date = System.DateTime.Now.ToString();
        date = date.Replace('/', '_');
        date = date.Replace(' ', '_');
        date = date.Replace(':', '_');
        sessionDateTime = date;
        puzzleScore = 0f;
#if WINDOWS_UWP
        CreateFiles();
#endif
    }

    private void OnDisable()
    {
        foreach (GameObject piece in pieces_List)
        {
            Destroy(piece);
        }
        foreach (GameObject aimcube in GameObject.FindGameObjectsWithTag("aim_cube"))
        {
            Destroy(aimcube);
        }
    }

    void Update()
    {
        /*
         * Save the game data and check for the puzzle collection with a frequency 1/period Hz
         * 
         */
        if(this.gameObject.GetComponent<Read_Write_json>().puzzlejson_loaded == true)
        {
            if (doBreak == false)
            {
                if (nextPuzzle)
                {
                    Reset();
                }
                if (Time.time > trackingRecordingTime)
                {
                    trackingRecordingTime = Time.time + period;
                    TrackingData trackingData = new TrackingData();
                    trackingData.time = DateTime.Now.ToString();
                    trackingData.headPosition = mainCamera.transform.position;
                    trackingData.matrix = JsonConvert.SerializeObject(matrix);
                    SaveTrackingJson(trackingData, false);

                    UpdateStateMatrix();
                    if (CheckMatrixCompletion())
                    {
                        Debug.Log("Puzzle completed");
                        numPuzzlesCompleted++;
                        GameObject go = GameObject.FindGameObjectWithTag("validation");
                        puzzleScore = CalculateScore();
                        scoreText.text = String.Format("{0:n0}", puzzleScore);
                        HaveABreak();
                    }
                }
            }
        }
    }
}

