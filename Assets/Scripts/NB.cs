using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.Profiling;



public class NB : MonoBehaviour {
    public DataManager DataMan = DataManager.Instance;

    public void StartSimulation()
    {
        int timestep = (int)DataMan.TimeStep.TotalSeconds;  //move this to dataman?
        if (DataMan.Duration == 0 || timestep == 0)
        {
            Debug.LogWarning("Simulation duration and timestep must be greater than zero");  //This isn't a very critical error since the user may have just forgotten to enter them, but the simulation can't run w/o them
            return;
        }
        if (DataMan.Masses.Length != DataMan.InitialPositions.Length || DataMan.Masses.Length != DataMan.InitialVelocities.Length)
        {
            Debug.LogError("Mistmatch in number of planets between the mass list and initial conditions!");  //if this happens something is seriously wrong with data parsing, each list should have the same number of bodies
        }
        //if the parameters make sense, we can start the simulation
        nBody(DataMan.Masses, DataMan.InitialPositions, DataMan.InitialVelocities, DataMan.Duration, (int)DataMan.TimeStep.TotalSeconds); //seems like 1000 or lower needed for high accuracy but it will be much slower
        Debug.Log("Simulation Complete");
    }
        
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

        double G = 6.67430e-11;                       // m3 kg-1 s-2
        double force = (G * mi * mj) / Math.Pow(separation, 2);
        return (force);      // N
    }


    private double Magnitude(double[] vec)  //Can probably replace this with vector.magnitude if switched back to floats.  Or create a custom class for vectors of doubles, with this method as a part of it.
                                            //Does the SIMD intrinsic Vector<T> allow doubles?   I think it might depending on hardware
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

        // calculate the separation between the two vectors.  this is the 2nd time we calc separation, probably don't need to do this twice?
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
        //Profiler.BeginSample("Net Force Vector");
        int pcount = p.Count();
        double[][] forces = new double[pcount][];

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

        //Profiler.EndSample();
        return forces;  
    }



    private Tuple<double[][], double[][]> leapfrogUpdateParticles(double[] masses, double[][] positions, double[][] velocities, float dt, int currentStep, LeapfrogResults results)
    {
        //Profiler.BeginSample("Leapfrog Update Particles");
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
        double[][] endingVelocities = new double[masses.Count()][];
        for (int i = 0; i < masses.Count(); i++)
        {
            //UnityEngine.Debug.Log("ending vel: " + elemAdd(elemMult(elemAdd(endingAccelerations[i], startingAccelerations[i]), (0.5 * dt)), startingVelocities[j])[0]);
            endingVelocities[i] = (elemAdd(elemMult(elemAdd(endingAccelerations[i], startingAccelerations[i]), (0.5 * dt)), startingVelocities[i]));
        }

        return new Tuple<double[][], double[][]> (endingPositions, endingVelocities);
        //Profiler.EndSample();

    }

    private class LeapfrogResults  //does this really need its own class?  I think I just did this to make code look cleaner but I'm not sure how it affects performance
    {
        public double[] Times;
        public double[][][] fPositions;
        public double[][][] fVelocities;
        DataManager DataMan = DataManager.Instance;
        int TotalSteps;
        public int BodyCount = DataManager.Instance.Masses.Count();

        public void InitializeLists(int totalSteps)  //Each entry in finalpositions represents 1 step of leapfrogUpdateParticles, except the first entry which is the initial conditions
        {
            TotalSteps = totalSteps;

            Times = new double[TotalSteps];
            fPositions = new double[BodyCount][][];
            fVelocities = new double[BodyCount][][];

            for (int i = 0; i < BodyCount; i++)
            {
                fPositions[i] = new double[TotalSteps][];
                fVelocities[i] = new double[TotalSteps][];
            }
        }
        
        public void AddResult(double[][] Pf, double[][] Vf, double Time, int currentStep)
        {
            for (int i = 0; i < BodyCount; i++)
            {
                fPositions[i][currentStep] = elemDiv2(Pf[i], 1.496e+11);  //convert back to AU
                fVelocities[i][currentStep] = Vf[i];  //probably should convert velocities back to AU/day but not sure because we aren't using these at the moment
            }
            Times[currentStep] = Time;
        }
        
        public void ClearAll()
        {
            Times = null;
            fPositions = null;
            fVelocities = null;
        }

        public void SendToDataManager()
        {         
            DataMan.InitializeFinalLists(TotalSteps);
            DataMan.Times = Times;
            DataMan.FinalPositions = fPositions;
            DataMan.FinalVelocities = fVelocities;
            ClearAll();
        }
    }

    private void Trajectories(double[] Masses, double[][] pi, double[][] vi, int time, int step, LeapfrogResults results)
    {
        int currentStep = 1;  //Add one to currentStep since we store the initial conditions in the first index

        double[][] iPositions = pi;
        double[][] iVelocities = vi;
        int Time = time;
        

        for (int t = step; t < Time; t += step)  //skip first step for initial conditions
        {
            (iPositions, iVelocities) = leapfrogUpdateParticles(Masses, iPositions, iVelocities, step, currentStep, results);

            /*if (results.fPositions[currentStep] == null)
            {
                Debug.LogWarning("could not find intermediate position at index " + currentStep);
            }*/

            results.AddResult(iPositions, iVelocities, (double)(t / 86400), currentStep);
            currentStep++;
        }

    }


    public void nBody(double[] Masses, double[][] InitialPositions, double[][] InitialVelocities, int Time, int Step)
    {
        Profiler.BeginSample("nBody");
        //need to add a method to intitialize DataManager's final position and velocity arrays using SelectedBodyCount and number of steps (SelectedBodyCount * steps = total number of final positions for all bodies combined)

        //Debug.Log("nBody: " + pi.Count() + "--" + vi.Count() + "--" + masses.Count());

        //need to put in a check somewhere to verify that positions and velocity lists are of same length
        int pcount = Masses.Count();

        LeapfrogResults results = new LeapfrogResults();  //create an instance of leapfrogresults class to store the information generated from leapfrogUpdateParticles
        results.BodyCount = pcount;
        results.InitializeLists(Time / Step);
        results.AddResult(InitialPositions, InitialVelocities, 0, 0);  //add the initial conditions to the results list first

        /*if ((Time / Step) % 1 == 0)  //if the number of steps (used for indexing arrays) is not a whole number, adjust the time so it is.  Stepsize should stay the same since it has an effect on accuracy
        {
            
        }*/

        double[][] positionsM = new double[pcount][];
        double[][] velocitiesM = new double[pcount][];
        for (int i = 0; i < pcount; i++)
        {
            positionsM[i] = elemMult(InitialPositions[i], 1.496e+11);  //convert AU to m
            velocitiesM[i] = elemMult(InitialVelocities[i], 1.731e+6);  //convert AU/day to m/s
        }

        Trajectories(Masses, positionsM, velocitiesM, Time, Step, results);

        /*for (int i = 0; i < results.BodyCount; i++)
        {
            for (int j = 0; j < (Time/Step)+1; j++)
            {
                if (results.fPositions[i][j] == null)
                {
                    Debug.LogWarning("could not find final position at index " + j);
                }
                results.fPositions[i][j] = elemDiv2(results.fPositions[i][j], 1.496e+11);  //convert back to AU
            }
        }*/

        Debug.Log("p: " + results.fPositions.Count() + ", v: " + results.fVelocities.Count() + ", t: " + results.Times.Count()+ "... sending to dataman and destroying self");
        results.SendToDataManager();
        results = null;  //this should be the only reference to the instance of the class, so it should be destroyed
        Debug.Log("DATA FROM DATAMAN: p: " + DataMan.FinalPositions.Count() + ", v: " + DataMan.FinalVelocities.Count() + ", t: " + DataMan.Times.Count());
        Profiler.EndSample();
    }


    //I think that most of these could be SIMD enabled if I used generic Vector<T> rather than arrays of doubles.  System.numerics namespace will allow SIMD acceleration on generic vectors.  will this increase mem cost?
    //I think Vector<T> still works with doubles too on the right hardware, although not as well as with floats?  
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


    public static double[] elemDiv2(double[] L1, double n)  //this has to be static for now so that a function in LeapfrogResults can use it
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
        double[][] div = new double[L1.Length][];
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
    /*private List<double[]> elemDiv(double[][] L1, List<double> n)  //currently unused
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
    }*/
}

