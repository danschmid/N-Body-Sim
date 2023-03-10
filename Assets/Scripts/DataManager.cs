using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using System.IO;
using System.Diagnostics.Tracing;
using System.Linq;

public class DataManager: MonoBehaviour
{
    public static DataManager Instance;

    public Dictionary<string, string[]> Index; //[Horizons ID#, [Name, Designation, IAU/Aliases/other]]
    public List<string> SelectedBodies;  //contains the Horizons ID# of each body selected from the body selection list.  The body's index in this array will be its index number in all of the data arrays below

    public DateTime StartTime { get; private set; }
    public DateTime EndTime { get; private set; }
    public int Duration { get; private set; }  //total time between start and end, in seconds
    public TimeSpan TimeStep { get; private set; }   //time step between each position, in seconds.  Should probably be something TotalTime is divisible by, or I should find a way to fix it if there isn't a good number of steps

    public string[] PreferredNames;  //names for each body, in the same order as SelectedBodies.  best name chosen by GetBestName()
    public double[][] InitialPositions;  //initial positions for each body, stored as double[].  The double[] for each body is stored at the same index in the outer array as SelectedBodies
    public double[][] InitialVelocities; //initial velocities for each body, stored as double[]
    public double[] Masses;
    public double[] Radii;

    public double[][][] FinalPositions;
    public double[][][] FinalVelocities;
    public double[] Times;

    public double[][][] FullEphemerides;

    public int BodyCount;
    private int nonamecount;
    public int TotalSteps;

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

    public void InitializeSimulationSettings(DateTime iTime, DateTime fTime, TimeSpan step)
    {
        StartTime = iTime;
        EndTime = fTime;
        TimeStep = step;

        TimeSpan dur = EndTime - StartTime;
        Duration = (int)dur.TotalSeconds;  //should I cast to int here, or keep as double?  Should probably be a whole number anyways but idk yet
        TotalSteps = Duration / (int)TimeStep.TotalSeconds;

        Debug.Log("Start DateTime: " + StartTime.ToString("yyyy-MM-dd\\ T HH:mm:ss") + "  --  End: " + EndTime.ToString("yyyy-MM-dd\\ T HH:mm:ss"));
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

        FullEphemerides = new double[numberOfBodies][][];
    }


    public void HorizonsAddBody(string planetCode, double[] data, double[] iPos, double[] iVel, double[][] fullEphemeris = null)
    {
        if(fullEphemeris != null)
        {
            FullEphemerides[BodyCount] = fullEphemeris;
        }
        
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

    public void saveToFile()
    {
        var path = Application.dataPath + "/fPos.txt";
        using (StreamWriter writetext = new StreamWriter(path, false))
        {
            int itr = 0;
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
            int itr = 0;
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
                //UnityEngine.Debug.Log("fuck--- " + Ld2.Count());
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
