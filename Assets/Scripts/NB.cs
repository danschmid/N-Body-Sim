using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Profiling;



public class NB : MonoBehaviour {
    public DataManager DataMan = DataManager.Instance;
    int SelectedBodyCount;


    private double forceMagnitude(double mi, double mj, double separation)
    {
        /*
        Compute magnitude of gravitational force between two particles.

        Parameters
        ----------
        mi, mj: double
            Particle masses in kg.
        separation : double
            Particle separation(distance between particles) in m.

        Returns
        ------ -
        force : double
            Gravitational force between particles in N.

        Example
        ------ -
            Input:
                mEarth = 6.0e24     # kg
                mPerson = 70.0      # kg
                radiusEarth = 6.4e6 # m
                print(magnitudeOfForce(mEarth, mPerson, radiusEarth))
            Output:
            683.935546875
        */

        double G = 6.67e-11;                       // m3 kg-1 s-2
        double force = (G * mi * mj) / Math.Pow(separation, 2);
        return (force);      // N
    }


    private double Magnitude(double[] vec)                       //Can probably replace this with vector.magnitude if switched back to floats
    {
        /*
        Compute magnitude of any vector with an arbitrary number of elements.
        eg: print(magnitude(np.array([3.0, 4.0, 0.0])))  -->  5.0
        */
        double sum = 0;
        foreach (double item in vec)
        {
            sum += Math.Pow(item, 2);
        }

        return Math.Sqrt(sum);
    }


    private double[] unitDirectionVector(double[] pa, double[] pb)
    {
        /*
        Create unit direction vector from pos_a to pos_b

        Parameters
        ----------
        pos_a, pos_b: arrays of doubles
           Any two vectors

        Returns
        ------ -
        unit direction vector : one array(same size input vectors)
            The unit direction vector from pos_a toward pos_b

        Example
        ------ -
            Input:
                someplace = [3.0, 2.0, 5.0]
                someplaceelse = [1.0, -4.0, 8.0]
                print(unitDirectionVector(someplace, someplaceelse))
            Output:
            [-0.28571429, -0.85714286,  0.42857143]
        */

        // calculate the separation between the two vectors
        double[] separation = new double[3];
        for (int i = 0; i < 3; i++)
        {
            separation[i] = pb[i] - pa[i];
        }

        double mag = Magnitude(separation);
        for (int j = 0; j < 3; j++)
        {
            separation[j] = (separation[j] / mag);
        }

        // divide vector components by vector magnitude to make unit vector
        return separation;
    }


    private double[] forceVector(double mi, double mj, double[] pi, double[] pj)
    {
        /*
        Compute gravitational force vector exerted on particle i by particle j.

        Parameters
        ----------
        mi, mj: doubles
           Particle masses, in kg.
       pos_i, pos_j: arrays
            Particle positions in cartesian coordinates, in m.

        Returns
        ------ -
        product : array
            Components of gravitational force vector, in N.

        Example
        ------ -
            Input:
                mEarth = 6.0e24     # kg
                mPerson = 70.0      # kg
                radiusEarth = 6.4e6 # m
                centerEarth = [0, 0, 0]
                surfaceEarth = [0, 0, 1]*radiusEarth
                print(forceVector(mEarth, mPerson, centerEarth, surfaceEarth))

        Output:
        [   0.            0.          683.93554688]
        */

        // compute the magnitude of the distance between positions
        double[] separation = new double[3];
        for (int i = 0; i < 3; i++)
        {
            separation[i] = pi[i] - pj[i];
        }

        double distanceMagnitude = Magnitude(separation);

        // this distance is in meters, because pos_i and pos_j were

        // compute the magnitude of the force
        double force = forceMagnitude(mi, mj, distanceMagnitude);

        // the magnitude of the force is in Newtons
        // calculate the unit direction vector of the force
        double[] direction = unitDirectionVector(pi, pj);
        // this vector is unitless, its magnitude should be 1.0
        
        //double[] direction = new double[3];
        //direction = elemDiv2(elemMult(separation, -1), distanceMagnitude);


        double[] product = new double[3];
        for (int i = 0; i < 3; i++)
        {
            product[i] = direction[i] * force;
        }
        return product;
        // units of Newtons
    }

    
    private double[][] netForceVector(double[] m, double[][] p) 
    {
<<<<<<< HEAD
        Profiler.BeginSample("Net Force Vector");
=======
        //Profiler.BeginSample("Net Force Vector");
>>>>>>> master
        double[][] forces = new double[SelectedBodyCount][];
        int pcount = p.Count();

        for (int i = 0; i < pcount; i++)
        {
            forces[i] = p[i];
            for (int j = 0; j < pcount; j++)
            {
                if (j != i)
                {
                    double[] fv = forceVector(m[i], m[j], p[i], p[j]);
                    forces[i] = elemAdd(forces[i], fv);  //also not really sure why I cant just do forces[i]=elemAdd(p[i], fv) here, but it does not seem to work
                }
            }
        }

<<<<<<< HEAD
        Profiler.EndSample();
=======
        //Profiler.EndSample();
>>>>>>> master
        return forces;  
    }



    private void leapfrogUpdateParticles(double[] masses, double[][] positions, double[][] velocities, float dt, int currentStep, TrajectoryResult results)
    {
<<<<<<< HEAD
        Profiler.BeginSample("Leapfrog Update Particles");
=======
        //Profiler.BeginSample("Leapfrog Update Particles");
>>>>>>> master
        /*
        Evolve particles in time via leap - frog integrator scheme. This function
        takes masses, positions, velocities, and a time step dt as inputs.
        If the positions and velocities are measured at some time "t",
        this function returns the predicted positions and velocities as
        some later time "t + dt".  
        Nearly identical to Verlet integration, but at half-integer time steps

        Parameters
        ----------
        masses: List<double>
            List containing masses for all particles, in kg
              It has length N, where N is the number of particles.

          positions : List<double[]>
              2 - D array containing(x, y, z) positions for all particles.
               Shape is (N, 3) where N is the number of particles.

           velocities : List<double[]>
               2 - D array containing(x, y, z) velocities for all particles.
                Shape is (N, 3) where N is the number of particles.
            
            dt : float
                Evolve system for time dt (in seconds).


            Returns
            ------ -
            ArrayList leapresults, Containing:

            endingpositions : List<double[]>
                Update particle positions at a time "dt" into the future.
                2 - D array containing(x, y, z) positions for all particles.
                Shape is (N, 3) where N is the number of particles.

             endingvelocities : List<double[]>
                 Update particle velocities at a time "dt" into the future.
                 2 - D array containing(x, y, z) velocities for all particles.
                  Shape is (N, 3) where N is the number of particles.
      */

        // in the variable-naming we use inside this function
        //   "starting..." refers to quantities at time "t"
        //   "ending..." refers to quantities at time "t + dt"

        // keep track of the starting positions for the particles
        double[][] startingPositions = positions;
        double[][] startingVelocities = velocities;

        // calculate net force vectors on all particles, at the starting position
        double[][] startingForces = netForceVector(masses, startingPositions);

        // calculate the acceleration due to gravity, at the starting position
        double[][] startingAccelerations = elemDiv(startingForces, masses);

        // calculate the ending position
        double[][] nudge = new double[masses.Count()][];
        double[][] endingPositions = new double[masses.Count()][];
        double[][] endingForces = new double[masses.Count()][];
        double[][] endingAccelerations = new double[masses.Count()][];
<<<<<<< HEAD
=======
        double[][] endingVelocities = new double[masses.Count()][];
>>>>>>> master

        for (int i = 0; i < masses.Count(); i++)
        {
            nudge[i] = elemAdd(elemMult(startingVelocities[i], dt), elemMult(startingAccelerations[i], (0.5 * Math.Pow(dt, 2))));
            endingPositions[i] = elemAdd(startingPositions[i], nudge[i]);
        }

        // calculate net force vectors on all particles, at the ending position
        endingForces = netForceVector(masses, endingPositions);
        // calculate the acceleration due to gravity, at the ending position
        endingAccelerations = elemDiv(endingForces, masses);

        // calculate the ending velocity, using an average of the accelerations
<<<<<<< HEAD
        double[][] endingVelocities = new double[masses.Count()][];
=======
>>>>>>> master
        for (int i = 0; i < masses.Count(); i++)
        {
            //UnityEngine.Debug.Log("ending vel: " + elemAdd(elemMult(elemAdd(endingAccelerations[i], startingAccelerations[i]), (0.5 * dt)), startingVelocities[j])[0]);
            endingVelocities[i] = (elemAdd(elemMult(elemAdd(endingAccelerations[i], startingAccelerations[i]), (0.5 * dt)), startingVelocities[i]));
        }

        results.AddResult(endingPositions, endingVelocities, currentStep);
<<<<<<< HEAD
        Profiler.EndSample();
=======
        //Profiler.EndSample();
>>>>>>> master

    }

    private class TrajectoryResult
    {
        private static TrajectoryResult inst;
        public static TrajectoryResult Instance { get { if (inst == null) { inst = new TrajectoryResult(); } return inst; } private set { } }

        public double[] Times;
        public double[][][] FinalPostitions;
        public double[][][] FinalVelocities;
<<<<<<< HEAD
        int StepCount = 0;
=======
>>>>>>> master

        public void InitializeLists(int numberOfSteps)
        {
            Times = new double[numberOfSteps];
            FinalPostitions = new double[numberOfSteps][][];
            FinalVelocities = new double[numberOfSteps][][];
        }

        public void AddTime(double t, int stepcount)
        {
            Times[stepcount] = t;
        }
        
        public void AddResult(double[][] Pf, double[][] Vf, int currentStep)
        {
            //Times.Add(t);
            FinalPostitions[currentStep] = Pf;
            FinalVelocities[currentStep] = Vf;
        }
        
        public void ClearAll()
        {
            Times = null;
            FinalPostitions = null;
            FinalVelocities = null;
        }

        public void SendToDataManagerAndDestroy(int numberOfSteps)
        {
            DataManager DataMan = DataManager.Instance;
            DataMan.InitializeFinalLists(numberOfSteps);
            DataMan.Times = Times;
            DataMan.FinalPositions = FinalPostitions;
            DataMan.FinalVelocities = FinalVelocities;
            ClearAll();
            Instance = null;
            inst = null;  //I think this should properly destroy the class instance when done?
        }
    }

    private void Trajectories(double[] m, double[][] pi, double[][] vi, int time, int step, TrajectoryResult results)
    {
        int stepcount = 0;

        double[] Masses = m;
        double[][] iPositions = pi;
        double[][] iVelocities = vi;
        int Time = time;
        

        for (int t = 0; t < Time; t += step)
        {
            leapfrogUpdateParticles(Masses, iPositions, iVelocities, step, stepcount, results);

            if (results.FinalPostitions[stepcount] == null)
            {
                Debug.LogWarning("could not find intermediate position at index " + stepcount);
            }
            iPositions = results.FinalPostitions[stepcount]; //Get intermediate positions and velocities (base each update off of the last one)
            iVelocities = results.FinalVelocities[stepcount];

            results.AddTime( (double)(t/86400), stepcount );
            stepcount++;
        }

    }


    public void nBody(double[] masses, double[][] positions, double[][] velocities, int time, int step)
    {
<<<<<<< HEAD
        Profiler.BeginSample("nBody");
=======
        //Profiler.BeginSample("nBody");
>>>>>>> master
        //need to add a method to intitialize DataManager's final position and velocity arrays using SelectedBodyCount and number of steps (SelectedBodyCount * steps = total number of final positions for all bodies combined)
        SelectedBodyCount = masses.Count();

        //Debug.Log("nBody: " + pi.Count() + "--" + vi.Count() + "--" + masses.Count());

        //need to put in a check somewhere to verify that positions and velocity lists are of same length
        int pcount = masses.Count();

        double[][] positionsM = new double[pcount][];
        double[][] velocitiesM = new double[pcount][];
        for (int i = 0; i < pcount; i++)
        {
            positionsM[i] = elemMult(positions[i], 1.496e+11);  //convert AU to m
            velocitiesM[i] = elemMult(velocities[i], 1.731e+6);  //convert AU/day to m/s
        }

        TrajectoryResult results = TrajectoryResult.Instance;  //create an instance of trajectoryresult class to store the information generated from leapfrogUpdateParticles
        results.InitializeLists(time / step);

        Trajectories(masses, positionsM, velocitiesM, time, step, results);

        double[][][] positions1 = results.FinalPostitions;
        double[][][] velocities1 = results.FinalVelocities;
        double[] times = results.Times;

        for (int i = 0; i < positions1.Count(); i++)
        {
            for (int j = 0; j < positions1[i].Count(); j++)  //replace count with number of steps * bodies when I figure out how to get it
            {
                positions1[i][j] = elemDiv2(positions1[i][j], 1.496e+11);  //convert back to AU
            }
        }

        Debug.Log("p: " + results.FinalPostitions.Count() + ", v: " + results.FinalVelocities.Count() + ", t: " + results.Times.Count()+ "... sending to dataman and destroying self");
        results.SendToDataManagerAndDestroy(time/step);
        Debug.Log("DATA FROM DATAMAN: p: " + DataMan.FinalPositions.Count() + ", v: " + DataMan.FinalVelocities.Count() + ", t: " + DataMan.Times.Count());
<<<<<<< HEAD
        Profiler.EndSample();
=======
        //Profiler.EndSample();
>>>>>>> master
    }

    public void saveToFile()
    {
        Debug.Log("Saving to file temporarily out of order");
        return;
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




        var path2 = Application.dataPath + "/fPos.txt";
        using (StreamWriter writetext = new StreamWriter(path2, false))
        {
            int itr = 0;
            //UnityEngine.Debug.Log("Writing to fPos" + position.Count());
            foreach (double[] da in MasterPos)
            {
                //UnityEngine.Debug.Log("fuck1--- " + Ld.Count());
                
                if (itr < MasterMasses.Count()-1)
                {
                    writetext.WriteLine("[" + da[0] + " " + da[1] + " " + da[2] + "]");
                    itr++;
                }
                else
                {
                    writetext.WriteLine("[" + da[0] + " " + da[1] + " " + da[2] + "]]");
                    writetext.WriteLine();
                    writetext.Write("[");
                    itr = 0;
                }
                

            }
            writetext.Write("end]");
            writetext.Close();
        }




        //List<List<double[]>> velocities2 = (List<List<double[]>>)traj[2];
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


    private double[] elemAdd(double[] L1, double[] L2)
    {
        double[] sum = new double[3];
        for (int y = 0; y < L1.Count(); y++)
        {
            sum[y] = L1[y] + L2[y];
        }
        return sum;
    }


    private double[] elemMult(double[] L1, double n)
    {
        double[] prod = new double[3];
        for (int y = 0; y < L1.Count(); y++)
        {
            prod[y] = L1[y] * n;
        }
        return prod;
    }


    private double[] elemDiv2(double[] L1, double n)
    {
        double[] ddiv = new double[3];
        for (int y = 0; y < L1.Count(); y++)
        {
            ddiv[y] = L1[y] / n;
        }
        return ddiv;
    }


    private double[][] elemDiv(double[][] L1, double[] n)
    {
        double[][] div = new double[SelectedBodyCount][];
        int itr = 0;
        for(int i=0; i < L1.Count(); i++)
        {
            double[] ddiv = new double[3];
            ddiv[0] = L1[i][0] / n[itr];
            ddiv[1] = L1[i][1] / n[itr];
            ddiv[2] = L1[i][2] / n[itr];

            div[i] = ddiv;
            itr++;
            if (itr >= n.Count())
            {
                itr = 0;
            }
        }
        return div;
    }
    private List<double[]> elemDiv(double[][] L1, List<double> n)
    {
        List<double[]> div = new List<double[]>();
        int itr = 0;
        foreach (double[] d in L1)
        {
            double[] ddiv = new double[3];
            ddiv[0] = d[0] / n[itr];
            ddiv[1] = d[1] / n[itr];
            ddiv[2] = d[2] / n[itr];
            div.Add(ddiv);
            itr++;
            if (itr >= n.Count())
            {
                itr = 0;
            }
        }
        return div;
    }
}

