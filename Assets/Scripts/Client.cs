using UnityEngine;
using System;
using System.Collections;
using UnityEngine.Networking;
using System.IO;
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
    public string today;
    public string tomorrow;

    public NB nb;
    public SidebarUI sidebarUI;
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
        today = System.DateTime.Now.ToString("yyyy-MM-dd");
        tomorrow = System.DateTime.Now.AddDays(1).ToString("yyyy-MM-dd");

        //PlanetCodes = new List<string> { "10", "199", "299", "399", "499", "599", "699", "799", "899", "999", "501", "502", "503", "504" };

        StartCoroutine( WebRequestText(HorizonsInfoURL, "0", true) );  //updates the index with names and IDs of available bodies
    }

    public Dictionary<string, string[]> GetIndex()
    {
        Debug.Log("Index length: " + DataMan.Index.Count);
        return DataMan.Index;
    }
    

    public IEnumerator UpdateSelectedBodyData()
    {
        //nb.MasterMasses = new List<double> { 1.9891e30, 3.285e23, 4.867e24, 5.972e24, 6.39e23, 1.8982e27, 5.683e26, 8.681e25, 1.024e26, 1.30900e22, 8.9319e22, 4.7998e22, 1.4819e23, 1.075938e23 };   // 
        PlanetCodes = sidebarUI.GetSelectedIDs();
        DataMan.InitializeDataLists(DataMan.SelectedBodies.Count());
        DataMan.SelectedBodies = PlanetCodes;
        string result = "PlanetCodes contents: ";
        foreach (var item in PlanetCodes)
        {
            result += item.ToString() + ", ";
        }
        Debug.Log(result);

        for (int c = 0; c < PlanetCodes.Count(); c++)
        {
            string url = DoURL(PlanetCodes[c]);
            yield return StartCoroutine( WebRequestText(url, PlanetCodes[c], false) );
        }

        result = "List contents: ";
        foreach (var item in DataMan.Masses)
        {
            result += item.ToString() + ", ";
        }
        Debug.Log(result);

        //Debug.Log("PlanetCodes: " + PlanetCodes.Count());
        Debug.Log("Starting simulation...");
        Debug.Log(DataMan.InitialPositions.Count() + ", " + DataMan.InitialVelocities.Count() + ", " + DataMan.SelectedBodies.Count());

        DataMan.InitializeSimulationSettings(DateTime.Now, DateTime.Now.AddDays(1), 1000);
        nb.nBody(DataMan.Masses, DataMan.InitialPositions, DataMan.InitialVelocities, 31536000, 1000); //1000 or lower needed for high accuracy (generally any more than the 9 main planets+ the sun is too much for stepsize more than 1000)
        UnityEngine.Debug.Log("Simulation Complete");
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
                GetIndex(output, pcode);
            }
            else
            {
                yield return StartCoroutine(GetEphemeris(output, pcode));
            }
        }
    }


    public void GetIndex(string rawText, string pcode)  //pcode is useless here but is required for the callback function to work.
    {
        DataMan.ClearAllData();

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
            

            
            DataMan.Index.Add(namecode[0], new string[3] { namecode[1], namecode[2], namecode[3] }); //Horizons ID#, [Name, Designation, IAU/Aliases/other]
        }

        sidebarUI.UpdateBodySelectionList();
    }  
    

    public IEnumerator GetPlanetaryData(string rawText, string pcode)
    {
        Debug.Log("getting planetary data...");
        // use regular expressions to find the number following "Vol. Mean Radius (km) = ", "Vol. mean radius, km = ", or "Radius (km)  = "
        string pattern = @"(Vol\.?\s+mean)?\s+(radius,?\s+\(?km\)?\s*=)\s*(\d+\.?\d*)";
        string radius = Regex.Match(rawText, pattern, RegexOptions.IgnoreCase).Value;
        radius = radius.Substring(radius.IndexOf("=") + 1);
        radius = radius.Trim();
        Debug.Log("radius= " + radius);

        // make a regular expression pattern to find "Mass, 10^n kg = value"  store the value, the exponent, and the units (usually kg, but could be g in the case of Jupiter for example)
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
                dmass = CalculateMass(dGM);
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

    public double CalculateMass(double GM)
    {
        double G = 6.67430e-21; // gravitational constant, converted from usual units: 10^-11 m^3*kg^-1*s^-2 to 10^-20 km^3*kg^-1*s^-2 
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
            }
            yield return null;
        }
    }


    public IEnumerator GetEphemeris(string rawText, string pcode)
    {
        //UnityEngine.Debug.Log("connected. Retrieving " + searchIndex() + "...");

        Debug.Log(rawText);

        data = new double[] { };
        yield return StartCoroutine(GetPlanetaryData(rawText, pcode));



        string ephemeris = GetBetween(rawText, "$$SOE", "$$EOE"); //ephermeris is the data for the planet's position and velocity vectors
        if (ephemeris == null)
        {
            Debug.LogError("Ephemeris not found");
            yield break;
        }
        ephemeris = ephemeris.Replace(" ", "");
        Debug.Log(ephemeris);

        string[] coordinates = ephemeris.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);  //gets the coordinates, which are separated by newlines for each date/timestep

        //each line has the Julian date, calendar date, and then position, and velocity vectors separated by commas.  The final three CSVs are light-time, range, and range-rate which aren't used at the moment
        string[] xyz = coordinates[1].Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
        //Debug.Log("length: " + xyz.Length);

        double x = double.Parse(xyz[2]);
        double y = double.Parse(xyz[3]);
        double z = double.Parse(xyz[4]);

        double vx = double.Parse(xyz[5]);
        double vy = double.Parse(xyz[6]);
        double vz = double.Parse(xyz[7]);

        //Scale((float)x, (float)y, (float)z, getName(indexNum));

        double[] startp = new double[3] { x, y, z };
        double[] startv = new double[3] { vx, vy, vz };

        DataMan.HorizonsAddBody(pcode, data, startp, startv);
        //return new double[][] { startp, startv, data };

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
        string startTime = today;  //change these to get times from DataMan once that's working and the simulation settings should be saved in DataMan before getting planet info
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
