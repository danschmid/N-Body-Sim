using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;



public class NB : MonoBehaviour {
    public List<double> MasterMasses;
    public List<double[]> MasterPos = new List<double[]>();
    public List<double[]> MasterVel = new List<double[]>();
    public List<double> MasterTimes = new List<double>();


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
            sum += Math.Pow(item, 2); ;
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
        double[] sep2 = new double[3];
        for (int i = 0; i < 3; i++)
        {
            sep2[i] = pi[i] - pj[i];
        }

        double distance = Magnitude(sep2);

        // this distance is in meters, because pos_i and pos_j were

        // compute the magnitude of the force
        double force = forceMagnitude(mi, mj, distance);

        // the magnitude of the force is in Newtons
        // calculate the unit direction vector of the force
        double[] direction = unitDirectionVector(pi, pj);
        // this vector is unitless, its magnitude should be 1.0


        double[] product = new double[3];
        for (int i = 0; i < 3; i++)
        {
            product[i] = direction[i] * force;
        }
        return product;
        // units of Newtons
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


    private List<double[]> elemDiv(List<double[]> L1, List<double> n)
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

    
    private List<double[]> netForceVector(List<double> m, List<double[]> p) 
    {
        List<double[]> forces = new List<double[]>();
        foreach (double[] item in p) 
        { 
            forces.Add(item);
        }

        for (int i = 0; i < p.Count; i++)
        {
            for (int j = 0; j < p.Count; j++)
            {
                if (j != i)
                {
                    double[] fv = forceVector(m[i], m[j], p[i], p[j]);
                    forces[i] = elemAdd(forces[i], fv);
                }
            }
        }


        return forces;  
    }



    private ArrayList leapfrogUpdateParticles(List<double> masses, List<double[]> positions, List<double[]> velocities, float dt)
    {
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
        List<double[]> startingPositions = positions;
        List<double[]> startingVelocities = velocities;
        // calculate net force vectors on all particles, at the starting position
        List<double[]> startingForces = netForceVector(masses, startingPositions);
        // calculate the acceleration due to gravity, at the starting position
        List<double[]> startingAccelerations = elemDiv(startingForces, masses);
        //UnityEngine.Debug.Log("Leapfrog1 starting pos: " + startingPositions.Count() + "--starting vel: " + startingVelocities.Count() + "--starting forces: " + startingForces.Count() + "--starting Accelerations:" + startingAccelerations.Count());

        // calculate the ending position
        List<double[]> nudge = new List<double[]>();
        List<double[]> endingPositions = new List<double[]>();
        List<double[]> endingForces = new List<double[]>();
        List<double[]> endingAccelerations = new List<double[]>();
        for (int i = 0; i < masses.Count(); i++)
        {
            nudge.Add(elemAdd(elemMult(startingVelocities[i], dt), elemMult(startingAccelerations[i], (0.5 * Math.Pow(dt, 2)))));
        }

        for (int i = 0; i < masses.Count(); i++)
        {
            endingPositions.Add(elemAdd(startingPositions[i], nudge[i]));
            //UnityEngine.Debug.Log("fpos: " + (elemAdd(startingPositions[i], nudge[i])[0]));
        }

        // calculate net force vectors on all particles, at the ending position
        endingForces = netForceVector(masses, endingPositions);
        // calculate the acceleration due to gravity, at the ending position
        endingAccelerations = elemDiv(endingForces, masses);

        // calculate the ending velocity, using an average of the accelerations
        List<double[]> endingVelocities = new List<double[]>();
        for (int i = 0; i < masses.Count(); i++)
        {
            //UnityEngine.Debug.Log("ending vel: " + elemAdd(elemMult(elemAdd(endingAccelerations[i], startingAccelerations[i]), (0.5 * dt)), startingVelocities[j])[0]);
            endingVelocities.Add(elemAdd(elemMult(elemAdd(endingAccelerations[i], startingAccelerations[i]), (0.5 * dt)), startingVelocities[i]));
        }

        // return the the positions and velocities after this step
        ArrayList leapresults = new ArrayList();
        leapresults.Add(endingPositions);
        leapresults.Add(endingVelocities);
        return leapresults;
    }






    private ArrayList Trajectories(List<double> m, List<double[]> pi, List<double[]> vi, int time, int step)
    {
        List<double> times = new List<double>();
        List<List<double[]>> positions = new List<List<double[]>>();
        List<List<double[]>> velocities = new List<List<double[]>>();
        List<double[]> newpi = pi;
        List<double[]> newvi = vi;

        for (int t = 0; t < time; t += step)
        {
            ArrayList tmp = leapfrogUpdateParticles(m, newpi, newvi, step);
            newpi = (List<double[]>)tmp[0];
            newvi = (List<double[]>)tmp[1];
            positions.Add(newpi);
            velocities.Add(newvi);
            times.Add((double)t);
            //times.Add(Convert.ToDouble(t));
        }
        ArrayList results = new ArrayList();
        results.Add(times);
        results.Add(positions);
        results.Add(velocities);
        return results;

    }







    public void nBody(List<double[]> pi, List<double[]> vi, int time, int step)
    {
        List<double[]> positionsM = new List<double[]>();
        for (int i = 0; i < pi.Count(); i++)
        {
            positionsM.Add(elemMult(pi[i], 1.496e+11));  //convert AU to m
        }
        List<double[]> velocitiesM = new List<double[]>();
        for (int i = 0; i < vi.Count(); i++)
        {
            velocitiesM.Add(elemMult(vi[i], 1.731e+6));  //convert AU/day to m/s
        }
        
        ArrayList traj = Trajectories(MasterMasses, positionsM, velocitiesM, time, step);

        List<List<double[]>> positions1 = (List<List<double[]>>)traj[1];
        List<List<double[]>> positions2 = new List<List<double[]>>();
        foreach (List<double[]> pl1 in positions1)
        {
            List<double[]> positionstmp = new List<double[]>();
            foreach (double[] pd in pl1)
            {
                positionstmp.Add(elemDiv2(pd, 1.496e+11));  //convert back to AU
            }
            positions2.Add(positionstmp);
        }

        List<double> times = new List<double>();
        List<double> timestmp = (List<double>)traj[0];
        for (int i = 0; i < timestmp.Count(); i++)
        {
            times.Add(timestmp[i] / 86400);
        }

        List<List<double[]>> velocities1 = (List<List<double[]>>)traj[2];



        for (int i = 0; i < times.Count(); i++)       //write times to MasterTimes list
        {
            MasterTimes.Add(times[i]);
        }

      

        foreach (List<double[]> Ld in positions2)                                       //write positions to MasterPos list
        {
            foreach (double[] da in Ld)
            {
                MasterPos.Add(da);
            }
        }
        
    }

    public void saveToFile()
    {
        var path = Application.dataPath + "/fTimes.txt";
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
        }
    }
}
