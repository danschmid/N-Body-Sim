using System;
using UnityEngine;
using System.IO;
using System.Collections.Generic;


public class BodiesMenu : MonoBehaviour {

    bool boxOpen = false;
    bool boxOpen2 = false;
  
    //public Canvas CanvasObject;
    //public GameObject Img;
    //public Transform objectToMove;
    //public GameObject myPrefab;

    string stmp;
    string veltmp = "(0.0, 0.0, 0.0)";
    string mtmp;
    string ntmp = "Name";
    Vector3 position;

    public Client client;
    public Orbit orbit;
    public NB nb;

    public delegate void GetSelectedBodiesEvent();
    public event GetSelectedBodiesEvent GetSelectedBodies;

    void Awake()
    {
        //cl = GetComponent<Client>();
        orbit = GetComponent<Orbit>();
}
    
    // Use this for initialization
    void Start () 
    {
        
    }

    // Update is called once per frame


    void Update()
    {
        if (Input.GetMouseButtonDown(1))
        {

            position = Input.mousePosition;
            stmp = position.ToString();
            boxOpen = true;
            boxOpen2 = false;
        }

        if (Input.GetMouseButtonDown(0))
        {
            //boxOpen = false;
        }
    }

    void OnGUI()
    {
        if (boxOpen)
        { 

            GUILayout.BeginArea(new Rect(position.x, Screen.height-position.y, 125, 175), GUI.skin.box);

            GUILayout.Label("Commands");

            if (GUILayout.Button("Add Body"))
            {
                orbit.stopSim();
                boxOpen = false;
                boxOpen2 = true;

            }

            if (GUILayout.Button("Get Sol System"))
            {
                orbit.stopSim();
                //client.PlanetCodes = new List<string> { };
                //GetSelectedBodies();
                orbit.getCurr = true;  //find a better way to do this?
                boxOpen = false;
                StartCoroutine(client.UpdateSelectedBodyData());
            }

            if (GUILayout.Button("Simulate"))
            {
                orbit.getCurrent();   //This may need to be removed to load saved files?
                orbit.moveToStart();
                boxOpen = false;
                orbit.startSim();
            }
            

            if (GUILayout.Button("Save To File"))
            {
                nb.saveToFile();
                boxOpen = false;
            }

            if (GUILayout.Button("Load From File"))
            {
                orbit.stopSim();
                orbit.getCurr = false;
                orbit.getFromList();
                orbit.moveToStart();
                boxOpen = false;
            }

            if (GUILayout.Button("Cancel"))
            {
                boxOpen = false;
            }

            GUILayout.EndArea();


            //CanvasObject.enabled = true;
        }

        if(boxOpen2)
        {
            

            GUILayout.BeginArea(new Rect(position.x, Screen.height - position.y, 128, 300), GUI.skin.box);
            GUILayout.Label("Add Body");

            stmp = GUILayout.TextField(stmp);
            veltmp = GUILayout.TextField(veltmp);
            mtmp = GUILayout.TextField(mtmp);
            ntmp = GUILayout.TextField(ntmp);



            if (GUILayout.Button("Create"))
            {
                UnityEngine.Debug.Log(ntmp);
                UnityEngine.Debug.Log(stmp);
                UnityEngine.Debug.Log(veltmp);
                UnityEngine.Debug.Log(mtmp);



                try
                {
                    double Dmtmp = Convert.ToDouble(mtmp);
                    nb.MasterMasses.Add(Dmtmp);



                    string input1 = client.GetBetween(stmp, "(", ")");          //Get pos coordinates
                    input1 = input1.Replace(" ", "");
                    input1 = input1.Replace("  ", "");
                    string[] input2 = input1.Split(',');
                    //UnityEngine.Debug.Log();
                    double[] Dstmp = new double[3] { Convert.ToDouble(input2[0]), Convert.ToDouble(input2[1]), Convert.ToDouble(input2[2]) };

                    var path = Application.dataPath + "/iPosition.txt";

                    using (StreamWriter writetext = File.AppendText(path))          //writes position coordinates to the iPosition file in units of AU
                    {

                        //UnityEngine.Debug.Log("PRINTIING TO FILE");
                        writetext.WriteLine(Dstmp[0] + " " + Dstmp[1] + " " + Dstmp[2]);
                        writetext.Close();
                    }





                    string input3 = client.GetBetween(veltmp, "(", ")");          //Get pos coordinates
                    input3 = input3.Replace(" ", "");
                    input3 = input3.Replace("  ", "");
                    string[] input4 = input3.Split(',');
                    double[] Dveltmp = new double[3] { Convert.ToDouble(input4[0]), Convert.ToDouble(input4[1]), Convert.ToDouble(input4[2]) };


                    var path2 = Application.dataPath + "/iVelocity.txt";

                    using (StreamWriter writetext = File.AppendText(path2))         //writes velocity coordinates to the iVelocity file in units of AU/day
                    {

                        //UnityEngine.Debug.Log("PRINTIING TO FILE");
                        writetext.WriteLine(Dveltmp[0] + " " + Dveltmp[1] + " " + Dveltmp[2]);
                        writetext.Close();
                    }

                    client.planetNames.Add(ntmp);

                    //create sphere object 
                    GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    go.AddComponent<HelloWorld>();
                    go.name = ntmp;

                    Vector3 pptmp = new Vector3((float)Dstmp[0], (float)Dstmp[1], (float)Dstmp[2]);
                    client.Scale(pptmp[0], pptmp[1], pptmp[2], ntmp);
                    //go.transform.position = pptmp;


                    nb.nBody(client.StartPos, client.StartVel, 43200000, 43200);
                    boxOpen2 = false;

                }


                catch
                {
                    UnityEngine.Debug.Log("INVALID INPUT");
                    
                }


               


               
            }


            

            if (GUILayout.Button("Cancel"))
            {
                boxOpen2 = false;
            }

            GUILayout.EndArea();
        }
    }

}
