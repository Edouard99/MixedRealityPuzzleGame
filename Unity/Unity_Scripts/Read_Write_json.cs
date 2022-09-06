using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine.UI;
using RESTClient;
using Azure.StorageServices;
#if ENABLE_WINMD_SUPPORT && UNITY_WSA
using Windows.Storage;
using Windows.ApplicationModel.Activation;
#endif


public class Read_Write_json : MonoBehaviour
{
    /*
     * This class is handling communication with the azure container with json blob
     */

    [HideInInspector] public List<float[,,]> list_matrix;
    [HideInInspector] public string jsonString;
    [HideInInspector] public bool puzzlejson_loaded=false;
    [HideInInspector] public bool upload = false;
    [HideInInspector] public string sessionDateTime;
    [HideInInspector] public string mode;
    [HideInInspector] public bool is_Reading;
    [HideInInspector] public bool assessupload;

    //These are Azure storage credentials for the container (with 2 subcontainers "data" and "puzzlejson")
    private string storageAccount= "mxdatacollection";
    private string accessKey= "YOUR ACCESS KEY";
    private string container_data = "data";
    private string container_puzzle_json = "puzzlejson";
    private StorageServiceClient client;
    private BlobService blobService;
    public GameObject MainMenu;
    public GameObject Level;


    public void Load_puzzle_json(RestResponse response)
    {
        /*
         * Load the list of pregenerated puzzle grid to the game
         */
        Debug.Log(response.Content);
        jsonString = response.Content;
        list_matrix = JsonConvert.DeserializeObject<List<float[,,]>>(jsonString);
        this.gameObject.GetComponent<Grid_big_cube_update>().list_matrix = list_matrix;
        puzzlejson_loaded = true;
    }

    public void Put_puzzle_json(RestResponse response)
    {
        //Handle response from the azure server
        if (response.IsError)
        {
            Debug.Log("Put JSON error: " + response.ErrorMessage);
            return;
        }
        Debug.Log("Put JSON: " + response.Url);
    }
    // Start is called before the first frame update
    void OnEnable()
    {
        //Set up the server client connection with the Azure container
        client = StorageServiceClient.Create(storageAccount, accessKey);
        blobService = client.GetBlobService();
        //Download the list of pregenerated puzzle grid from the container according to the game level session
        string puzzle_lvl = this.gameObject.GetComponent<Grid_big_cube_update>().puzzle_lvl;
        if (puzzle_lvl == "veryhard")
        {
            StartCoroutine(blobService.GetTextBlob(Load_puzzle_json, container_puzzle_json+"/puzzle_data_veryhard.json"));
        }
        else
        {
            StartCoroutine(blobService.GetTextBlob(Load_puzzle_json, container_puzzle_json+"/puzzle_data.json"));
        }
    }

    public async void uploadJSON(string topic)
    {
        /*
         * Upload the jsonfile that stores game data to the azure container that contains data file
         */
#if WINDOWS_UWP
        StorageFolder folder = KnownFolders.DocumentsLibrary;
        string path = sessionDateTime + topic + mode + ".json";
        StorageFile file = await folder.GetFileAsync(path);
        is_Reading = true;
        string jsonString_2 = await FileIO.ReadTextAsync(file);
        is_Reading = false;
        StartCoroutine(blobService.PutTextBlob(Put_puzzle_json, jsonString_2, container_data, sessionDateTime + topic + mode + ".json"));
#else
        string dir = Application.persistentDataPath;
        string path = dir + sessionDateTime + topic + mode + ".json";
        if (File.Exists(path))
        {
            StreamReader r = new StreamReader(path);
            string jsonString_2 = r.ReadToEnd();
            StartCoroutine(blobService.PutTextBlob(Put_puzzle_json, jsonString_2, container_data, sessionDateTime + topic + mode + ".json"));
            r.Close();
        }
#endif
    }
    void Update()
    {
        if (upload==true) {
            upload = false;
            uploadJSON("_matrix_");
            uploadJSON("_player_");
            uploadJSON("_tracking_");
        }
        if (assessupload == true)
        {
            assessupload = false;
            uploadJSON("_assessment_");
        }
    }
}
