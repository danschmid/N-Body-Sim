using System;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Linq;
using System.Globalization;

public class DataManager: MonoBehaviour
{
    public static DataManager Instance;
    public EventManager Events = EventManager.events;
    public NB nb;

    public Dictionary<string, string[]> HorizonsIndex; //[Horizons ID#, [Name, Designation, IAU/Aliases/other]]  retrives the horizons index when the program starts to provide names of bodies to select from

    public List<string> SelectedBodies;  //contains the Horizons ID# of each body selected for simulation from the body selection list.  The body's index in this array should be the same in all of the arrays below

    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public int Duration { get; private set; }  //total time between start and end, in seconds
    [SerializeField] public TimeSpan TimeStep { get; private set; }   //time step between each position, in seconds.  Should probably be something TotalTime is divisible by, or I should find a way to fix it if there isn't a good number of steps

    
    
    public string[] PreferredNames;  //names for each body, in the same order as SelectedBodies. Probably don't need to keep a list of these, could just get them when needed

    //these are for initial conditions
    [SerializeField] public double[][] InitialPositions;  //initial positions for each body, stored as double[].  The double[] for each body is stored at the same index in the outer array as SelectedBodies
    [SerializeField] public double[][] InitialVelocities; //initial velocities for each body, stored as double[]
    public double[] Masses;
    public double[] Radii;

    //These store what is calculated from the simulation
    public double[][][] FinalPositions;
    public double[][][] FinalVelocities;
    public double[] Times;

    //this stores full results from Horizons for comparing to
    public double[][][] FullEphemerides;


    public int BodyCount;
    private int nonamecount;
    public int TotalSteps;
    public bool RequireFullEphemerides;





    public void Awake()
    {
        Instance = this;  //calls to DataManager from other scripts will reference this instance, so there is only ever one at a time
        HorizonsIndex = new Dictionary<string, string[]> { };

        Events.ToggleEvent += ChangeToggleParameter;
        Events.InputEvent += ChangeInputParameter;
        Events.DateTimeInputEvent += ChangeDateTimeParameter;
    }
    public void OnDestroy()
    {
        Events.ToggleEvent -= ChangeToggleParameter;
        Events.InputEvent -= ChangeInputParameter;
        Events.DateTimeInputEvent -= ChangeDateTimeParameter;
    }

    // Start is called before the first frame update
    void Start()
    {
        nonamecount = 0;
        DateTime now = DateTime.Now;
        TimeSpan step = new TimeSpan(0, 0, 0, 1080, 0);
        InitializeSimulationSettings(now, now.AddYears(1), step);
    }

    public void StartSim()
    {
        Debug.Log("duration: " + Duration + ", totalsteps: " + TotalSteps);
        Debug.Log("Start DateTime: " + StartTime.ToString("yyyy-MM-dd\\ T HH:mm:ss") + "  --  End: " + EndTime.ToString("yyyy-MM-dd\\ T HH:mm:ss"));

        nb.StartSimulation();
    }

    public void ChangeDateTimeParameter( DateTime dateTime, DateTimeInputHandler.InputType inputType)
    {
        DateTime newDateTime;
        switch(inputType)
        {
            case DateTimeInputHandler.InputType.StartDate:
                StartTime = dateTime.Date.Add(StartTime.TimeOfDay);
                Events.RaiseDateTimeChangedEvent(StartTime, inputType);
                //Debug.Log("StartTime: " + StartTime.ToString());
                break;

            case DateTimeInputHandler.InputType.EndDate:
                EndTime = dateTime.Date.Add(EndTime.TimeOfDay);
                Events.RaiseDateTimeChangedEvent(EndTime, inputType);
                //Debug.Log("EndTime: " + EndTime.ToString());
                break;

            case DateTimeInputHandler.InputType.StartTime:
                StartTime = StartTime.Date.Add(dateTime.TimeOfDay);
                Events.RaiseDateTimeChangedEvent(StartTime, inputType);
                //Debug.Log("StartTime: " + StartTime.ToString());
                break;

            case DateTimeInputHandler.InputType.EndTime:
                EndTime = EndTime.Date.Add(dateTime.TimeOfDay);
                Events.RaiseDateTimeChangedEvent(EndTime, inputType);
                //Debug.Log("EndTime: " + EndTime.ToString());
                break;

            //TODO: maybe add some cases so I can modify enitre datetimes here.  Would have to add some InputTypes to DateTimeInputHandler

            default:
                Debug.LogWarning("Invalid datetime change?");
                break;
        }
    }

    public void ChangeInputParameter(string input, InputHandler.InputType inputType)
    {
        //Debug.Log(System.Threading.Thread.CurrentThread.CurrentCulture);
        //string[] formats = { "MM/dd/yyyy HH:mm:ss", "MM/dd/yyyy':' HH:mm:ss", "M/d/yyyy HH:mm", "MM/dd/yyyy", "HH:mm:ss MM/dd/yyyy", "HH:mm:ss" };
        /*if (fieldName == "StartTime")
        {
            DateTime dateValue;
            try
            {
                DateTime parsedData = DateTime.ParseExact(input, DateTime.Now.GetDateTimeFormats(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
                Debug.Log(parsedData);
            }
            catch (FormatException ex)
            {
                Debug.LogError("Parsing error: " + ex.Message);
            }
             
            if (DateTime.TryParseExact(input, DateTime.Now.GetDateTimeFormats(), CultureInfo.InvariantCulture, DateTimeStyles.None, out dateValue))
            {
                Debug.Log(dateValue);
                StartTime = dateValue;
            }
            else
            {
                Debug.LogWarning("Please Enter a Valid Start Time and Date!");
            }
        }
        else if (fieldName == "EndTime")
        {
            DateTime dateValue;
            if (DateTime.TryParseExact(input, formats, new CultureInfo("en-us"), DateTimeStyles.None, out dateValue))
            {
                Debug.Log(dateValue);
                EndTime = dateValue;
            }
            else
            {
                Debug.LogWarning("Please Enter a Valid End Time and Date!");
            }
        }*/

        switch(inputType)
        {
            case InputHandler.InputType.StepSize:
                int intTime;
                if (int.TryParse(input, out intTime))
                {
                    TimeSpan timespan = TimeSpan.FromSeconds(intTime);
                    Debug.Log("timespan: " + timespan);

                    TimeSpan dur = EndTime - StartTime;
                    Duration = (int)dur.TotalSeconds;
                    TotalSteps = Duration / intTime - 1;  //array isn't quite big enough for all data from JPL. Always missing 2. Probably due to rounding errors

                    TimeStep = timespan;

                    Events.RaiseInputChangedEvent(input, inputType);
                }
                else
                {
                    Debug.LogWarning("Please enter a valid span of time in seconds");
                }
                break;

        }
    }
    public void ChangeInputParameter(float input, InputHandler.InputType inputType)
    {
        switch (inputType)
        {
            case InputHandler.InputType.StepSize:
                TimeStep = TimeSpan.FromSeconds(input);
                Events.RaiseInputChangedEvent(input.ToString(), inputType);
                break;

        }
    }

    public void ChangeToggleParameter(bool isOn, ToggleHandler toggleHandler)
    {
        if(toggleHandler.ID != null)  //this means it is a body selection toggle
        {
            string id = toggleHandler.ID;
            if (isOn)
            {
                SelectedBodies.Add(id);
            }
            else
            {
                SelectedBodies.Remove(id);
            }
        }
        else if(toggleHandler.Setting != null)  //this is a toggle for a generic setting (simulation, display, etc)
        {

        }
        
    }

    public void ClearAllData()
    {
        PreferredNames = null;
        InitialPositions = null;
        InitialVelocities = null;
        Masses = null;
        Radii = null;

        FinalPositions = null;
        FinalVelocities = null;
        nonamecount = 0;
    }

    public void InitializeSimulationSettings(DateTime iTime, DateTime fTime, TimeSpan step)
    {
        //StartTime = iTime;
        //EndTime = fTime;
        //TimeStep = step;

        ChangeDateTimeParameter(iTime, DateTimeInputHandler.InputType.StartDate);
        ChangeDateTimeParameter(iTime, DateTimeInputHandler.InputType.StartTime);

        ChangeDateTimeParameter(fTime, DateTimeInputHandler.InputType.EndDate);
        ChangeDateTimeParameter(fTime, DateTimeInputHandler.InputType.EndTime);

        ChangeInputParameter(step.TotalSeconds.ToString(), InputHandler.InputType.StepSize);
    }

    public void InitializeFinalLists(int numberOfSteps)
    {
        FinalPositions = new double[][][] { };
        FinalVelocities = new double[][][] { };
    }

    public void InitializeDataLists(int numberOfBodies)  //set the size of the arrays equal to N, the size of FinalPositions and FinalVelocities will depend on both N and the number of integration steps.  This will be determined when nBody gets called
    {
        ClearAllData();

        PreferredNames = new string[numberOfBodies];
        InitialPositions = new double[numberOfBodies][];
        InitialVelocities = new double[numberOfBodies][];
        Masses = new double[numberOfBodies];
        Radii = new double[numberOfBodies];

        FullEphemerides = new double[numberOfBodies][][];
    }

    public bool VerifyData()
    {
        Array[] arrays = new Array[] {PreferredNames, InitialPositions, InitialVelocities, Masses, Radii, FullEphemerides};
        for (int i = 1; i < arrays.Length; i++)
        {
            if (arrays[i].GetLength(0) != arrays[i - 1].GetLength(0))
            {
                Debug.LogWarning("Data Lists count mismatch!");
                return false;
            }
        }

        bool complete = true;
        foreach (Array arr in arrays)
        {
            foreach (var value in arr)
            {
                if (value == null)
                {
                    switch(Array.IndexOf(arrays, arr))
                    {
                        case 0:
                            Debug.LogWarning("Missing data required for simulation: Preferred Names");
                            break;
                        case 1:
                            Debug.LogWarning("Missing data required for simulation: Initial Positions");
                            break;
                        case 2:
                            Debug.LogWarning("Missing data required for simulation: Initial Velocities");
                            break;
                        case 3:
                            Debug.LogWarning("Missing data required for simulation: Masses");
                            break;
                        case 4:
                            //Debug.LogWarning("Missing data required for simulation: Radii"); //We don't require radii at the moment
                            break;
                        case 5:
                            if(RequireFullEphemerides)
                            {
                                Debug.LogWarning("Missing data required for simulation: FullEphemerides");
                            }
                            break;
                        default:
                            Debug.LogWarning("I dunno, something's missing");
                            break;
                    }
                    complete = false;
                }
            }
        }

        if(Duration == 0 || TimeStep == null)
        {
            Debug.LogWarning("Missing simulation parameters: timestep");
            complete = false;
        }

        return complete;
    }


    public void HorizonsAddBody(string planetCode, double[] data, double[] iPos, double[] iVel, double[][] fullEphemeris = null)
    {
        if(fullEphemeris != null)
        {
            FullEphemerides[BodyCount] = fullEphemeris;
        }

        PreferredNames[BodyCount] = GetBestName(planetCode);
        Debug.Log("Added body " + PreferredNames[BodyCount]);

        if(iPos == null || iVel == null)
        {
            Debug.LogError("Initial position or iinitial velocity are missing!");
        }
        InitialPositions[BodyCount] = iPos;
        InitialVelocities[BodyCount] = iVel;
        Masses[BodyCount] = data[0];
        Radii[BodyCount] = data[1];

        BodyCount++;
    }

    public string GetBestName(string id) //determines whether to use the primary name, designation, or alias from the index.  If none is found, it gives them one
    {
        if (!string.IsNullOrEmpty(HorizonsIndex[id][0]))
        {
            return HorizonsIndex[id][0];
        }
        else if (!string.IsNullOrEmpty(HorizonsIndex[id][1]))
        {
            return HorizonsIndex[id][1];
        }
        else if (!string.IsNullOrEmpty(HorizonsIndex[id][2]))
        {
            return HorizonsIndex[id][2];
        }
        else
        {
            Debug.LogWarning("Could not find any name, designation, or other alias for body with id: " + id);
            nonamecount += 1;
            return "Unnamed_Body_" + nonamecount.ToString();
        }
    }

    public void saveToFile()
    {
        var path = Application.dataPath + "/fPos.txt";
        using (StreamWriter writetext = new StreamWriter(path, false))
        {
            //int itr = 0;
            //UnityEngine.Debug.Log("Writing to fPos" + position.Count());
            for(int body = 0; body < FinalPositions.Count(); body++)
            {
                writetext.WriteLine("$Body$");
                writetext.WriteLine(PreferredNames[body] + ":");
                for(int step = 0; step < FinalPositions[0].Count(); step++)
                {
                    writetext.Write(FinalPositions[body][step][0] + "," + FinalPositions[body][step][1] + "," + FinalPositions[body][step][2] + ";");
                }
                writetext.WriteLine("$Body$");
                writetext.WriteLine();
            }
            writetext.Close();
        }

        path = Application.dataPath + "/Ephemerides.txt";
        using (StreamWriter writetext = new StreamWriter(path, false))
        {
            //int itr = 0;
            //UnityEngine.Debug.Log("Writing to fPos" + position.Count());
            for (int body = 0; body < FullEphemerides.Count(); body++)
            {
                writetext.WriteLine("$Body$");
                writetext.WriteLine(PreferredNames[body] + ":");
                for (int step = 0; step < FullEphemerides[0].Count(); step++)
                {
                    writetext.Write(FullEphemerides[body][step][0] + "," + FullEphemerides[body][step][1] + "," + FullEphemerides[body][step][2] + ";");
                }
                writetext.WriteLine("$Body$");
                writetext.WriteLine();
            }
            writetext.Close();
        }



        /*var path = Application.dataPath + "/fTimes.txt";
        using (StreamWriter writetext = new StreamWriter(path, false))
        {
            UnityEngine.Debug.Log("Writing to fTimes" + MasterTimes.Count());
            for (int i = 0; i < MasterTimes.Count(); i++)
            {
                //UnityEngine.Debug.Log(times[i]);
                writetext.WriteLine(MasterTimes[i].ToString());
                writetext.WriteLine();
            }
            writetext.Close();
        }

        //List<List<double[]>> positions2 = (List<List<double[]>>)traj[1];
        //var path2 = Application.dataPath + "/fPos.txt";
        //System.IO.File.WriteAllLines(path2, positions2[i].ToString());
        */

        /*//List<List<double[]>> velocities2 = (List<List<double[]>>)traj[2];
        var path3 = Application.dataPath + "/fVel.txt";
        using (StreamWriter writetext = new StreamWriter(path3, false))
        {
            UnityEngine.Debug.Log("Writing to fVel" + MasterVel.Count());
            int itr = 0;
            foreach (double[] da in MasterVel)
            {
                writetext.Write("[");

                //UnityEngine.Debug.Log("[" + da[0] + " " + da[1] + " " + da[2] + "]");
                if (itr < MasterMasses.Count() - 1)
                {
                    writetext.WriteLine("[" + da[0] + " " + da[1] + " " + da[2] + "]");
                }
                else
                {
                    writetext.WriteLine("[" + da[0] + " " + da[1] + " " + da[2] + "]]");
                    writetext.WriteLine();
                    itr = 0;
                }
                itr++;
                
            }
            writetext.Close();
        }*/
    }
}
