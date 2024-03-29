﻿using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine.UI;
//using Newtonsoft.Json;
//using UnityEngine.Modules.Jsonserialize;
public class Client : MonoBehaviour
{
    //[JsonExtensionData]
    

    private readonly string HorizonsInfoURL = "https://ssd.jpl.nasa.gov/horizons_batch.cgi?batch=1&COMMAND=%27MB%27&CSV_FORMAT=%27YES%27"; //URL for Horizons info page, with a list of all available major bodies and IDs.
    public List<string> PlanetCodes = new List<string>();
    //private List<string> planetsDefault = new List<string> { "Sun", "Mercury", "Venus", "Earth", "Mars", "Jupiter", "Saturn", "Uranus", "Neptune", "Pluto" };

    //public Dictionary<string, string[]> Index; //[Horizons ID#, [Name, Designation, IAU/Aliases/other]]
    //public Dictionary<string, double[][]> BodyData = new Dictionary<string, double[][]>(); //BodyData = ( [Horizons ID#, [[Xi, Yi, Zi], [VXi, VYi, VZi], [Mass, Radius]]] )
    //public string today;
    //public string tomorrow;

    public DataManager DataMan = DataManager.Instance;

    public InputField inputField;
    private string userInput = "";
    double[] data;

    private void OnGUI()
    {
        GUI.Label(new Rect(10, 10, 500, 20), "Left-click to pan, right-click to rotate, and middle-click to zoom");
    }

    void Start()
    {
        //today = System.DateTime.Now.ToString("yyyy-MM-dd");
        //tomorrow = System.DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

        StartCoroutine( WebRequestText(HorizonsInfoURL, "0", true) );  //updates the index with names and IDs of available bodies
    }

    public Dictionary<string, string[]> GetIndex()
    {
        Debug.Log("Index length: " + DataMan.HorizonsIndex.Count);
        return DataMan.HorizonsIndex;
    }
    

    public IEnumerator UpdateSelectedBodyData()
    {
        EventManager.events.LockoutWhileLoading();

        PlanetCodes = DataMan.SelectedBodies;

        if(PlanetCodes.Count == 0)
        {
            Debug.LogWarning("Please select some bodies to simulate from the data tab, or input a custom body.");
            yield break;
        }
        
        DataMan.InitializeDataLists(DataMan.SelectedBodies.Count());
        //DataMan.SelectedBodies = PlanetCodes;

        //DateTime now = DateTime.Now;
        //TimeSpan step = new TimeSpan(0, 0, 0, 1080, 0);
        //DataMan.InitializeSimulationSettings(now, now.AddYears(1), step); //

        string result = "PlanetCodes contents: ";
        foreach (var item in PlanetCodes)
        {
            result += item.ToString() + ", ";
        }
        Debug.Log(result);

        for (int c = 0; c < PlanetCodes.Count(); c++)
        {
            string url = DoHorizonsURL(PlanetCodes[c]);
            StartCoroutine(WebRequestText(url, PlanetCodes[c], false)); //wait so that it doesn't overload the server with requests
            yield return new WaitForSeconds(4);  //making calls more frequently than every 4 seconds begins to deny us service
        }
        //string lastPlanet = PlanetCodes[PlanetCodes.Count()-1];  //why did I do this?
        //yield return StartCoroutine(WebRequestText(DoHorizonsURL(lastPlanet), lastPlanet, false)); //wait for last request to process before trying to simulate


        EventManager.events.UnlockAfterLoading();



        /*result = "List contents: ";
        foreach (var item in DataMan.Masses)
        {
            result += item.ToString() + ", ";
        }
        Debug.Log(result);

        Debug.Log("PlanetCodes: " + PlanetCodes.Count());
        Debug.Log(DataMan.InitialPositions.Count() + ", " + DataMan.InitialVelocities.Count() + ", " + DataMan.SelectedBodies.Count());
         */

        /*Debug.Log("Starting simulation...");
        nb.nBody(DataMan.Masses, DataMan.InitialPositions, DataMan.InitialVelocities, DataMan.Duration, (int)DataMan.TimeStep.TotalSeconds); //1000 or lower needed for high accuracy (generally any more than the 9 main planets+ the sun is too much for stepsize more than 1000)
        UnityEngine.Debug.Log("Simulation Complete");*/  

        /*Debug.Log("FinalPositions Size: " + DataMan.FinalPositions.Count() + " - " + DataMan.FinalPositions[0].Count() + " - " + DataMan.FinalPositions[0][0].Count() +
            ", FullEphemerides size: " + DataMan.FullEphemerides.Count() + " - " + DataMan.FullEphemerides[0].Count() + " - " + DataMan.FullEphemerides[0][0].Count());*/

    }


    public IEnumerator WebRequestText(string url, string pcode, bool getIndex)  //gets the text from the page at the specified URL and sends the output to the callback function provided (either GetEphemeris, or GetIndex).
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
            if(getIndex)
            {
                ParseIndex(output, pcode);
            }
            else
            {
                yield return StartCoroutine(ParseEphemeris(output, pcode));
            }
        }
    }


    public void ParseIndex(string rawText, string pcode)  //pcode is useless here but is required for the callback function to work.
    {
        DataMan.ClearAllData();

        Debug.Log("got index: ");
        Debug.Log(rawText);

        rawText = rawText.Substring(rawText.IndexOf("0")); //remove header
        rawText = rawText.Remove(rawText.IndexOf(". Use ID#")); //remove footer but keep the "number of matches = "
        int endingIndex = rawText.IndexOf("Number of matches =");
        string numberMatches = rawText.Substring(endingIndex);  //store the number of matches and then remove this line
        rawText = rawText.Remove(endingIndex);
        rawText = rawText.Trim();

        foreach (string line in rawText.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))  //one line at a time
        {
            if(line.Trim().Length == 0)  //I guess StringSplitOptions.RemoveEmptyEntries somehow misses some, specifically on the last line.  Is there a better way to do this?
            {
                continue;
            }
            string[] namecode = new string[4];
            namecode[0] = line.Substring(0, 9).Trim();
            namecode[1] = line.Substring(10, 35).Trim();
            namecode[2] = line.Substring(45, 13).Trim();
            namecode[3] = line.Substring(58).Trim();
            

            
            DataMan.HorizonsIndex.Add(namecode[0], new string[3] { namecode[1], namecode[2], namecode[3] }); //Horizons ID#, [Name, Designation, IAU/Aliases/other]
        }
    }


    public IEnumerator ParseEphemeris(string rawText, string pcode)
    {
        Debug.Log(rawText);

        data = new double[] { };
        yield return StartCoroutine(ParsePlanetaryData(rawText, pcode));



        string ephemeris = GetBetween(rawText, "$$SOE", "$$EOE"); //ephermeris is the data for the planet's position and velocity vectors
        if (ephemeris == null || ephemeris == "")
        {
            Debug.LogError("Ephemeris not found");
            yield break;
        }
        ephemeris = ephemeris.Replace(" ", "");
        Debug.Log(ephemeris);

        string[] coordinates = ephemeris.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);  //gets the coordinates, which are separated by newlines for each date/timestep

        
        double[][] fullEphem = new double[DataMan.TotalSteps][];  //shape is [TotalSteps][position at step]
        //int bodyIndex = DataMan.SelectedBodies.IndexOf(pcode);  //get the index number to specify what planet this is
        for (int line = 0; line < coordinates.Length; line++)
        {
            Debug.Log("line: " + line);


            if (line >= DataMan.TotalSteps)
            {
                Debug.Log("DataMan.fullEphem is full! line number " + line);
                continue;
            }
            string[] lineComponents = coordinates[line].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            fullEphem[line] = new double[] { double.Parse(lineComponents[2]), double.Parse(lineComponents[3]), double.Parse(lineComponents[4]) };
        }
        

        //each line has the Julian date, calendar date, and then position, and velocity vectors separated by commas.  The final three CSVs are light-time, range, and range-rate which aren't used at the moment
        string[] xyz = coordinates[0].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);  //this was getting coordinates[1], but im not sure why.  probably should get the first one so it is at the right time/timestep
        Debug.Log("length: " + coordinates.Length);
        //Debug.Log("coordinates 0: " + coordinates[0] + "coordinates 1: " + coordinates[1]);

        double x = double.Parse(xyz[2]);
        double y = double.Parse(xyz[3]);
        double z = double.Parse(xyz[4]);

        double vx = double.Parse(xyz[5]);
        double vy = double.Parse(xyz[6]);
        double vz = double.Parse(xyz[7]);

        //Scale((float)x, (float)y, (float)z, getName(indexNum));

        double[] startp = new double[3] { x, y, z };
        double[] startv = new double[3] { vx, vy, vz };

        DataMan.HorizonsAddBody(pcode, data, startp, startv, fullEphem);
    }


    public IEnumerator ParsePlanetaryData(string rawText, string pcode)
    {
        //the solution I have below is terrible, this is a WIP on making the parsing better
        /*foreach (string line in rawText.Split(new string[] { "\\n" }, StringSplitOptions.None)) //split by newline but instead of a real newline it is literally a backslash and a n (\n)
        {
            string column1 = line.Substring(0, 45).Replace(" ", "");
            string column2 = line.Substring(45);  //do not trim this column yet, as it may have some meaningful indentation

            Dictionary<string, object> data = new Dictionary<string, object>();
            string currentHeader = "";

            if (column2.EndsWith(":")) //only happens in column2, header for indented content below 
            {
                currentHeader = column1.Trim(':');
                data.Add(currentHeader, data[currentHeader] = new List<string>());
            }
            if (column2.StartsWith(" "))  //indented under a header
            {
                ((List<string>)data[currentHeader]).Add(column2.Replace(" ","") );
                Debug.Log("Line:" + column2.Replace(" ", ""));
            }
            else
            {
                data[column1] = ParseValue(column2);
            }


        }*/


        Debug.Log("getting planetary data...");
        // use regular expressions to find the number following "Vol. Mean Radius (km) = ", "Vol. mean radius, km = ", or "Radius (km)  = "
        string pattern = @"(Vol\.?\s+mean)?\s+(radius,?\s+\(?km\)?\s*=)\s*(\d+\.?\d*)";
        string radius = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Value;
        radius = radius.Substring(radius.IndexOf("=") + 1);
        radius = radius.Trim();
        Debug.Log("radius= " + radius);

        // regular expression pattern to find "Mass, 10^n kg = value"  store the value, the exponent, and the units (usually kg, but could be g in the case of Jupiter for example)
        pattern = @"(Mass,?\s*x?\s*\(?\s*10\^(-?\d+\.?\d*)?\s+\(?([a-z,1-9,^]+)\s*\)?\s*=)\s*~?(\d+\.?\d*)(\s*\(?\s*10\^(-?\d+)\s*\)?\s*)?\S*?";

        string mass = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Groups[4].Value;
        string massExp = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Groups[2].Value;
        string massUnit = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Groups[3].Value;
        string massExp2 = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Groups[6].Value;
        //Debug.Log("massexp2: " + massExp2);
        mass = mass.Substring(mass.IndexOf("=") + 1);
        mass = mass.Trim();

        //This is no longer needed to calculate the mass, but maybe I'll want these parameters for something else later
        /*if ()  //if no mass listed, then retrieve the Semi-major axis, eccentricity, and orbital period, and calculate it (relevant for smaller bodies like moons and satellites that don't have masses listed)
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
            string opUnit = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Groups[5].Value;
            orbitalPeriod = orbitalPeriod.Substring(orbitalPeriod.IndexOf("=") + 1);
            orbitalPeriod = orbitalPeriod.Trim();
            Debug.Log("orbitalPeriod= " + orbitalPeriod);

            //regex pattern matching "Eccentricity, e        ~ 0.00472"
            pattern = @"eccentricity,?\s+e\s*~\s?(\d{1,3}(,\d{3})*(\.\d+)?)";
            string eccentricity = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Groups[1].Value;
            eccentricity = eccentricity.Substring(eccentricity.IndexOf("=") + 1);
            eccentricity = eccentricity.Trim();
            Debug.Log("eccentricity= " + eccentricity);

            double m=0;
            double sma;
            double op;
            double e;
            

            if(double.TryParse(semiMajorAxis, out sma) && double.TryParse(orbitalPeriod, out op))
            {
                /*if(opUnit == "d")// convert from units of days to seconds
                {
                    
                }
                op = op * 86400;
                //convert sma from km to m
                sma = sma * 1000;
                Debug.Log("sma: " + sma + ", op:" + op);
                
                if (double.TryParse(eccentricity, out e))
                {
                    m = CalculateMass(sma, op, e, 189818722e22);
                }
                /*else
                {
                    m = CalculateMass(sma, op);
                }
                Debug.Log("mass= " + m);
            }

        }*/

        double dmass = 0;


        if (string.IsNullOrEmpty(mass))
        {
            //regex pattern matching "GM (km^3/s^2) ="
            pattern = @"(GM\s+\(?km\^3/s\^2\)?)\s*=\s*(\d{1,3}(,\d{3})*(\.\d+)?)";
            string GM = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Groups[2].Value;
            GM = GM.Substring(GM.IndexOf("=") + 1);
            GM = GM.Trim();
            Debug.Log("GM= " + GM);
            
            double dGM;
            if(double.TryParse(GM, out dGM))
            {
                dmass = CalculateMassFromGM(dGM);
                UnityEngine.Debug.Log("mass= " + dmass.ToString());
            }
            else
            {
                Debug.LogWarning("Couldn't parse GM string into double: \" " + GM + " \"");

                yield return StartCoroutine(WaitForInput(pcode));
                double.TryParse(userInput, out dmass);
                while (dmass == 0)
                {
                    double.TryParse(userInput, out dmass);
                    Debug.Log("Invalid mass input");
                    inputField.interactable = true;
                    inputField.gameObject.SetActive(true);
                    yield return StartCoroutine(WaitForInput(pcode));
                }
                userInput = "";

            }
        }
        else
        {
            if (massExp2 != "")
            {
                Debug.LogWarning("massexponent changed from " + massExp + " to " + massExp + "+" + massExp2);
                massExp = (double.Parse(massExp) + double.Parse(massExp2)).ToString();
            }
            mass = mass + "e" + massExp;
            
            if(!double.TryParse(mass, out dmass))
            {
                Debug.LogWarning("could not parse mass: " + mass);
            }
            Debug.Log("mass= " + dmass);
            if(massUnit == "g")
            {
                dmass = dmass / 1000;  //jupiter's mass specifially is listed in grams, in this case convert it to kg
            }
        }

        double dradius;
        if (!double.TryParse(radius, out dradius))
        {
            Debug.LogWarning("could not parse radius: " + radius);
        }


        data = new double[] {dmass, dradius};
    }

    public double CalculateMassFromGM(double GM)
    {
        double G = 6.67430e-21; // gravitational constant, converted from usual units: 10^-11 m^3*kg^-1*s^-2 to 10^-21 km^3*kg^-1*s^-2, so that the resulting mass will be in kg
        return (GM / G);
    }

    public IEnumerator WaitForInput(string pcode)
    {
        Debug.Log("Start getinput");
        userInput = "";
        inputField.GetComponentInChildren<Text>().text = "Enter a mass for " + DataMan.GetBestName(pcode) + ":";
        inputField.interactable = true;
        inputField.gameObject.SetActive(true);

        while (userInput == "")
        {
            if (Input.GetKeyDown(KeyCode.Return))
            {
                userInput = inputField.text;
                inputField.text = "";
                inputField.interactable = false;
                inputField.gameObject.SetActive(false);

                double Finput;
                if(double.TryParse(userInput, out Finput))
                {
                    DataMan.Masses[DataMan.SelectedBodies.IndexOf(pcode)] = Finput;
                }
            }
            yield return null;
        }
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

    string DoHorizonsURL(string command) //formats the URL to get planetary info from the JPL Horizons web API
    {
        string startTime = DataMan.StartTime.ToString("yyyy-MM-dd"); //simulation parameters must be set before getting planet info
        string endTime = DataMan.EndTime.ToString("yyyy-MM-dd");
        string stepSize = DataMan.TimeStep.TotalMinutes.ToString() + "%20minutes";  //can't be in seconds.  Has to be in minutes, hours, days, weeks, months or years
        Debug.Log("URL step size: " + stepSize);

        string centercode = "500@0";  //solar system barycenter will be the coordinate center
        

        //change format to json when I switch to new parsing method
        return ("https://ssd.jpl.nasa.gov/api/horizons.api?format=text&COMMAND=%27" + command + "%27&OBJ_DATA=%27YES%27&MAKE_EPHEM=%27YES%27&OUT_UNITS=%27AU%27&TABLE_TYPE=%27VECTOR%27&CENTER=%27" + centercode + "%27&START_TIME=%27" + startTime + "%27&STOP_TIME=%27" + endTime + "%27&STEP_SIZE=%27"+ stepSize +"%27&QUANTITIES=%272,9,20,23,24%27&CSV_FORMAT=%27YES%27");
        //return ("https://ssd.jpl.nasa.gov/api/horizons.api?format=text&COMMAND=%27499%27&OBJ_DATA=%27YES%27&MAKE_EPHEM=%27YES%27&EPHEM_TYPE=%27VECTOR%27&CENTER=%27500@399%27&START_TIME=%272006-01-01%27&STOP_TIME=%272006-01-20%27&STEP_SIZE=%271%20d%27&QUANTITIES=%271,9,20,23,24,29%27");
    }
}
