﻿using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Security;
using static UnityEditor.PlayerSettings;
using System.Numerics;
using System.Dynamic;
using System.Net;
using System.Xml.Linq;
using System.Text;
//using Newtonsoft.Json;
//using UnityEngine.Modules.Jsonserialize;
public class Client : MonoBehaviour
{
    //[JsonExtensionData]
    

    private readonly string HorizonsInfoURL = "https://ssd.jpl.nasa.gov/horizons_batch.cgi?batch=1&COMMAND=%27MB%27&CSV_FORMAT=%27YES%27"; //URL for Horizons info page, with a list of all available major bodies and IDs.
    public List<string> PlanetCodes = new List<string>();
    public List<string> planets = new List<string>();
    //private List<string> planetsDefault = new List<string> { "Sun", "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune", "Pluto" };

    public Dictionary<string, string[]> Index; //[Horizons ID#, [Name, Designation, IAU/Aliases/other]]
    public Dictionary<string, string[][]> BodyData; //BodyData = ( [Horizons ID#, [[Xi, Yi, Zi], [VXi, VYi, VZi], [Mass, Radius]]] )
    public List<double[]> StartPos = new List<double[]>();
    public List<double[]> StartVel = new List<double[]>();
    public string today;
    public string tomorrow;

    public NB nb;
    public SidebarUI sidebarUI;

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 500, 20), "Left-click to pan, right-click to rotate, and middle-click to zoom");
    }

    void Start()
    {
        /*string url = "https://ssd.jpl.nasa.gov/sbdb.cgi?sstr=Io;orb=1;cov=0;log=0;cad=0#orb";
        // Make a web request to the JPL Small-Body Database
        WebClient client = new WebClient();
        string jsonData = client.DownloadString(url);

        // Parse the JSON data
        JObject data = JObject.Parse(jsonData);

        // Extract the orbital parameters
        double semiMajorAxis = 2.65;
        double orbitalPeriod = (double)data["tp"];

        // Calculate the mass of Io using Kepler's Third Law
        double mass = (4 * Mathf.Pow(Mathf.PI, 2) * Mathf.Pow((float)semiMajorAxis, 3)) / (G * (orbitalPeriod * orbitalPeriod));

        // Log the result to the console
        Debug.Log("The mass of Io is: " + mass + " kg");*/



        today = System.DateTime.Now.ToString("yyyy-MM-dd");
        tomorrow = System.DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

        PlanetCodes = new List<string> { "10", "199", "299", "399", "499", "599", "699", "799", "899", "999", "501", "502", "503", "504" };
        planets = new List<string> { "Sun", "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune", "Pluto", "Io", "Europa", "Ganymede", "Callisto" };

        StartCoroutine( WebRequestText(HorizonsInfoURL, GetIndex) );  //updates the index with names and IDs of available bodies

        //findMass("Earth");
    }
    
    /*public void GetDataFromJson(string json) //this function takes in a json formatted string and returns the fields mass, radius, name, semi-major axis, and orbital period.  any other parameters not explicitly mentioned are stored in a [jsonExtensionData] object
    {
        if(json.Contains("$$EOE"))
        {
            json = json.Substring(json.LastIndexOf("\"result\":\""), json.LastIndexOf("$$EOE"));
        }
        
        Debug.Log(json);
        Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        foreach (string s in data.Keys)
        {
            Debug.Log(s + ": " + data[s]);
        }
        //return data;
    }*/
    public Dictionary<string, string[]> GetIndex()
    {
        Debug.Log("Index length: " + Index.Count);
        return Index;
    }
    

    public IEnumerator UpdateSelectedBodyData()
    {
        nb.MasterMasses = new List<double> { 1.9891e30, 3.285e23, 4.867e24, 5.972e24, 6.39e23, 1.8982e27, 5.683e26, 8.681e25, 1.024e26, 1.30900e22, 8.9319e22, 4.7998e22, 1.4819e23, 1.075938e23 };   // 
        StartPos = new List<double[]>();
        StartVel = new List<double[]>();

        for (int c = 0; c < PlanetCodes.Count(); c++)
        {
            string url = DoURL(PlanetCodes[c]);
            yield return StartCoroutine( WebRequestText(url, GetEphemeris) );
        }

        //nb.nBody(StartPos, StartVel, 31536000, 1000);
        UnityEngine.Debug.Log("Simulation Complete");
    }

    /*public void findMass(string bodyName) //this function isn't finished at all, but is meant to access the NASA API to find mass of a body, or any other missing data
    {
        string baseURL = "https://api.nasa.gov/planetary/";
        string apiKey = "c5FDQPmE66yaYkIAztHpXuZj6gxZtqfrItB6yKpYf";

        string endpoint = "planets/earth/";  // Change this to the desired endpoint
        string url = baseURL + endpoint + "?api_key=" + apiKey;

        StartCoroutine(WebRequestText(url, HandleResponse));
    }*/
    private void HandleResponse(string response)
    {
        var planetData = JsonUtility.FromJson<string>(response);
        Debug.Log("TESTMASS: " + planetData);
        foreach (char s in planetData)
        {
            Debug.Log("s: " + s);
        }
        //Debug.Log("Mass: " + planetData.mass);
    }


    public IEnumerator WebRequestText(string url, System.Action<string> callback)  //gets the text from the page at the specified URL and sends the output to the callback function provided (either GetEphemeris, or GetIndex).
    {
        UnityEngine.Debug.Log(url);

        UnityWebRequest www = UnityWebRequest.Get(url);
        yield return www.SendWebRequest();
        if (www.isNetworkError || www.isHttpError)
        {
            UnityEngine.Debug.Log(www.error);
        }
        else
        {
            string output = www.downloadHandler.text;
            callback(output);
        }
    }


    public void GetIndex(string rawText)
    {
        Index = new Dictionary<string, string[]>();

        UnityEngine.Debug.Log("getting list...");

        Debug.Log(rawText);
        rawText = rawText.Substring(rawText.IndexOf("0")); //remove header
        rawText = rawText.Remove(rawText.IndexOf("Number of matches =")); //remove footer
        rawText = rawText.Trim();
        foreach (string line in rawText.Split('\n'))  //one line at a time
        {
            //UnityEngine.Debug.Log(line);
            string[] namecode = new string[4];
            namecode[0] = line.Substring(0, 9).Trim();
            namecode[1] = line.Substring(10, 35).Trim();
            namecode[2] = line.Substring(45, 13).Trim();
            namecode[3] = line.Substring(58).Trim();

            Index.Add(namecode[0], new string[3] { namecode[1], namecode[2], namecode[3] }); //Horizons ID#, [Name, Designation, IAU/Aliases/other]
        }
        Debug.Log("Index length: " + Index.Count());

        sidebarUI.UpdateBodySelectionList();
    }

    //write a function that creates a web request for JPL Horizons database API to get the mass of a given body
    
    

    public void GetPlanetaryData(string rawText)
    {
        Debug.Log("getting planetary data...");
        // use regular expressions to find the number following "Vol. Mean Radius (km) = ", "Vol. mean radius, km = ", or "Radius (km)  = "
        string pattern = @"(Vol\.?\s+mean)?\s+(radius,?\s+\(?km\)?\s*=)\s*(\d+\.?\d*)";
        string radius = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Value;
        radius = radius.Substring(radius.IndexOf("=") + 1);
        radius = radius.Trim();
        Debug.Log("radius= " + radius);

        // make a regular expression pattern to find "Mass, 10^n kg = value"  store the value, the exponent, and the units (usually kg, but could be g in the case of Jupiter for example)
        pattern = @"(Mass,?\s+x?\s?10\^?(\d+\.?\d*)?\s+\(?([a-z, 1-9, ^]+)\)?\s*=)\s*~?(\d+\.?\d*)";

        string mass = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Groups[4].Value;
        string massExp = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Groups[2].Value;
        string massUnit = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Groups[3].Value;
        mass = mass.Substring(mass.IndexOf("=") + 1);
        mass = mass.Trim();
        Debug.Log("mass= " + mass + " x10^" + massExp + " " + massUnit);    

        if (string.IsNullOrEmpty(mass))  //if no mass listed, then retrieve the Sem-major axis and orbital period, and calculate it (relevant for smaller bodies like moons and satellites that don't have masses listed)
        {
            //regex pattern matching "Semi-major axis, a (km)~ 1,883,000"
            pattern = @"(Semi-major axis,?\s+a\s+\(?km\)?)\s*~\s?(\d{1,3}(,\d{3})*(\.\d+)?)";
            string semiMajorAxis = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Groups[2].Value;
            semiMajorAxis = semiMajorAxis.Substring(semiMajorAxis.IndexOf("=") + 1);
            semiMajorAxis = semiMajorAxis.Trim();
            Debug.Log("semiMajorAxis= " + semiMajorAxis);

            //regex pattern matching "Orbital period   ~ 16.691    d"
            pattern = @"(Orbital period)\s*~\s*(\d{1,3}(,\d{3})*(\.\d+)?)\s*([a-z]+)";
            string orbitalPeriod = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Groups[2].Value;
            string unit = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Groups[5].Value;
            orbitalPeriod = orbitalPeriod.Substring(orbitalPeriod.IndexOf("=") + 1);
            orbitalPeriod = orbitalPeriod.Trim();
            Debug.Log("orbitalPeriod= " + orbitalPeriod);

            
            
        }



    }


    public void GetEphemeris(string rawText)
    {
        //UnityEngine.Debug.Log("connected. Retrieving " + searchIndex() + "...");

        Debug.Log(rawText);


        GetPlanetaryData(rawText);



        string ephemeris = GetBetween(rawText, "$$SOE", "$$EOE"); //ephermeris is the data for the planet's position and velocity vectors
        if (ephemeris == null)
        {
            Debug.LogError("Ephemeris not found");
            return;
        }
        ephemeris = ephemeris.Replace(" ", "");
        Debug.Log(ephemeris);

        string[] coordinates = ephemeris.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);  //gets the coordinates, which are separated by newlines for each date/timestep

        //each line has the Julian date, calendar date, and then position, and velocity vectors separated by commas.  The final three CSVs are light-time, range, and range-rate which aren't used at the moment
        string[] xyz = coordinates[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        Debug.Log("length: " + xyz.Length);

        double x = double.Parse(xyz[2]);
        double y = double.Parse(xyz[3]);
        double z = double.Parse(xyz[4]);

        double vx = double.Parse(xyz[5]);
        double vy = double.Parse(xyz[6]);
        double vz = double.Parse(xyz[7]);

        //Scale((float)x, (float)y, (float)z, getName(indexNum));

        StartPos.Add(new double[3] { x, y, z });                    //add position coordinates to startpos list 
        StartVel.Add(new double[3] { vx, vy, vz });
    }


    public void Scale(float xf, float yf, float zf, string planet)
    {
        //Scale2(xf, yf, zf, planet);


        //int itr = 2;
        GameObject Gobj;
        if (GameObject.Find(planet) != null)
        {
            Gobj = GameObject.Find(planet);
        }
        else
        {
            GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            go.AddComponent<HelloWorld>();
            go.name = planet;
            Gobj = GameObject.Find(planet);
            float f = (float)0.1;
            Gobj.transform.localScale = new UnityEngine.Vector3(f, f, f);
        }
        Gobj.transform.position = new UnityEngine.Vector3(xf * 250, yf * 250, zf * 250);
    }


    public string GetBetween(string strSource, string strStart, string strEnd)
    {
        int startIndex = strSource.IndexOf(strStart, 0) + strStart.Length;
        int endIndex = strSource.IndexOf(strEnd, startIndex);
        if (startIndex == -1 || endIndex == -1)
        {
            return "";
        }
        return strSource.Substring(startIndex, endIndex - startIndex);
    }


    /*string getID(int indexNum)
    {
        return Index[indexNum][1];
    }

    string getName(int indexNum)
    {
        return Index[indexNum][0];
    }*/

    string DoURL(string command)
    {
        string startTime = today;
        string endTime = tomorrow;
        string centercode = "500@0";
        return ("https://ssd.jpl.nasa.gov/api/horizons.api?format=text&COMMAND=%27" + command + "%27&OBJ_DATA=%27YES%27&MAKE_EPHEM=%27YES%27&OUT_UNITS=%27AU%27&TABLE_TYPE=%27VECTOR%27&CENTER=%27" + centercode + "%27&START_TIME=%27" + startTime + "%27&STOP_TIME=%27" + endTime + "%27&STEP_SIZE=%271%20day%27&QUANTITIES=%272,9,20,23,24%27&CSV_FORMAT=%27YES%27");
        //return ("https://ssd.jpl.nasa.gov/api/horizons.api?format=text&COMMAND=%27499%27&OBJ_DATA=%27YES%27&MAKE_EPHEM=%27YES%27&EPHEM_TYPE=%27VECTOR%27&CENTER=%27500@399%27&START_TIME=%272006-01-01%27&STOP_TIME=%272006-01-20%27&STEP_SIZE=%271%20d%27&QUANTITIES=%271,9,20,23,24,29%27");
    }


    bool IsAllNumbers(string st2)
    {
        foreach (char c in st2)
        {
            if (char.IsNumber(c) == false)
            {
                if (c != '-' && c != '.' && c != ' ')
                {
                    return false;
                }
            }
        }
        return true;
    }
}
