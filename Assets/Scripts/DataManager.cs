using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class DataManager: MonoBehaviour
{
    public static DataManager Instance;

    public Dictionary<string, string[]> Index; //[Horizons ID#, [Name, Designation, IAU/Aliases/other]]
    public List<string> SelectedBodies;  //contains the Horizons ID# of each body selected from the body selection list.  The body's index in this array will be its index number in all of the data arrays below

    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public int Duration { get; private set; }  //total time between start and end, in seconds
    public int TimeStep { get; private set; }   //time step between each position, in seconds.  Should probably be something TotalTime is divisible by, or I should find a way to fix it if there isn't a good number of steps

    public string[] PreferredNames;  //names for each body, in the same order as SelectedBodies.  best name chosen by GetBestName()
    public double[][] InitialPositions;  //initial positions for each body, stored as double[].  The double[] for each body is stored at the same index in the outer array as SelectedBodies
    public double[][] InitialVelocities; //initial velocities for each body, stored as double[]
    public double[] Masses;
    public double[] Radii;

    public double[][][] FinalPositions;
    public double[][][] FinalVelocities;
    public double[] Times;

    public int BodyCount;
    private int nonamecount;

    void Awake()
    {
        Instance = this;
        Index = new Dictionary<string, string[]> { };
    }

    // Start is called before the first frame update
    void Start()
    {
        nonamecount = 0;
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

    public void InitializeSimulationSettings(DateTime iTime, DateTime fTime, int step)
    {
        StartTime = iTime;
        EndTime = fTime;
        TimeStep = step;

        TimeSpan dur = StartTime - EndTime;
        Duration = (int)dur.TotalSeconds;  //should I cast to int here, or keep as double?
        Debug.Log("Simulation Duration: " + Duration);
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
    }


    public void HorizonsAddBody(string planetCode, double[] data, double[] iPos, double[] iVel)
    {
        PreferredNames[BodyCount] = GetBestName(planetCode);
        InitialPositions[BodyCount] = iPos;
        InitialVelocities[BodyCount] = iVel;
        Masses[BodyCount] = data[0];
        Radii[BodyCount] = data[1];

        BodyCount++;
    }

    public string GetBestName(string id) //determines whether to use the primary name, designation, or alias from the index.  If none is found, it gives them one
    {
        if (!string.IsNullOrEmpty(Index[id][0]))
        {
            return Index[id][0];
        }
        else if (!string.IsNullOrEmpty(Index[id][1]))
        {
            return Index[id][1];
        }
        else if (!string.IsNullOrEmpty(Index[id][2]))
        {
            return Index[id][2];
        }
        else
        {
            Debug.LogWarning("Could not find any name, designation, or other alias for body with id: " + id);
            nonamecount += 1;
            return "Unnamed_Body_" + nonamecount.ToString();
        }

    }
}
