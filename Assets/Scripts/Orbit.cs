using System.Collections.Generic;
using UnityEngine;
using System.IO;
using static UnityEditor.ShaderData;

public class Orbit : MonoBehaviour {
	public List<float> pos = new List<float>();
	public List<float> vel = new List<float>();

	public int count=0;
	public int pitr=0;
	public float speed;
	public bool ready = false;
	public bool getCurr = true;

	public Client client;
	public NB nb;
	public SidebarUI sb;
	

	// Use this for initialization
	void Start()
	{
	}


	public void startSim()
	{
        if(getCurr==true) 
		{
			getCurrent();
		}
		else
        {
			getFromList();
			getCurr = true;
		}
		ready = true;
	}
	

	public void stopSim()
    {
		ready=false;
		//pos = new List<float>();
		pitr = 0;
	}

	public void getCurrent()
    {
		//Orbit.pos = new List<float>();
		foreach (double[] dd in nb.MasterPos)
		{
			foreach (double d in dd)
			{
				float fd = (float)d;
				pos.Add(fd);
			}
		}


		UnityEngine.Debug.Log("MasterPos: " + nb.MasterPos.Count);


	}
	
	// Update is called once per frame
	void Update () 
	{
		if(count>=10 && ready==true)
		{
			//UnityEngine.Debug.Log(planets.Count + " bodies");
			foreach(string pl in client.PlanetCodes)
			{
				movePlanets(sb.GetBestName(pl));

                pitr += 3;
			}

			count=0;
		}
		count++;
		if(pitr>pos.Count)
		{
			ready=false;
			pos = new List<float>();
			pitr = 0;
		}
	}



	public void moveToStart()
    {
		UnityEngine.Debug.Log("poslength: " + pos.Count);
		
		int itr = 0;
		foreach(string p in client.planetNames)
        {
			GameObject objPlanet = null;
			if(GameObject.Find(p) != null)
			{
				objPlanet = GameObject.Find(p); //find the planet object
			}
            else
            {
				GameObject go = GameObject.CreatePrimitive(PrimitiveType.Sphere); // or create a new one for non-default bodies
				go.AddComponent<HelloWorld>();
				go.name = p;
				objPlanet = go;
				float f = (float)0.1;
				objPlanet.transform.localScale = new Vector3(f, f, f);
				objPlanet.transform.position = new Vector3(pos[itr] * 250, pos[itr + 1] * 250, pos[itr + 2] * 250);

            }

			if(objPlanet != null)
            {
                Debug.Log("moving planet " + p + " to " + pos[itr] * 250 + " " + pos[itr + 1] * 250 + " " + pos[itr + 2] * 250);
                objPlanet.transform.position = new Vector3(pos[itr] * 250, pos[itr + 1] * 250, pos[itr + 2] * 250);
			}
			itr += 3;
		}
	}


	private void movePlanets(string p)
	{
		GameObject objPlanet;
        if (pitr < pos.Count && GameObject.Find(p))
        {
			objPlanet = GameObject.Find(p);
			float f = (float)0.1;
            objPlanet.transform.localScale = new Vector3(f, f, f);
            //objPlanet.transform.position = new Vector3(pos[itr], pos[itr+1], pos[itr+2]);
            float step = 50 * Time.deltaTime;               //was 10*
            Vector3 poss = new Vector3();
			poss = new Vector3(pos[pitr] * 250, pos[pitr + 1] * 250, pos[pitr + 2] * 250);
			UnityEngine.Debug.Log(p + ": " + pos[pitr]*250 + " " + pos[pitr+1] * 250 + " " + pos[pitr+2] * 250);
			//UnityEngine.Debug.Log(p + ": " + poss.x + " " + poss.y + " " + poss.z);
			objPlanet.transform.position = Vector3.MoveTowards(objPlanet.transform.position, poss, step);
		}
		/*else
		{
			Debug.Log("creating " + p);
            objPlanet = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            objPlanet.AddComponent<HelloWorld>();
            objPlanet.name = p;
            float step = 50 * Time.deltaTime;               //was 10*
            Vector3 poss = new Vector3();
            poss = new Vector3(pos[pitr] * 250, pos[pitr + 1] * 250, pos[pitr + 2] * 250);
            //UnityEngine.Debug.Log(p + ": " + pos[pitr] + " " + pos[pitr+1] + " " + pos[pitr+2]);
            //UnityEngine.Debug.Log(p + ": " + poss.x + " " + poss.y + " " + poss.z);
            objPlanet.transform.position = Vector3.MoveTowards(objPlanet.transform.position, poss, step);
        }*/
		/*}
        else
        {
			GameObject parenttmp = GameObject.Find("Jupiter");
			Vector3 postmp = parenttmp.transform.TransformPoint(new Vector3(pos[pitr], pos[pitr + 1], pos[pitr + 2]));
			UnityEngine.Debug.Log(p + ": " + postmp.x + " " + postmp.y + " " + postmp.z);
			objPlanet.transform.position = Vector3.MoveTowards(objPlanet.transform.position, postmp, step);
		}*/

	}

	
	public List<float> getFromList()
    {


		pos = new List<float>();
		pitr = 0;


		var path1 =  Application.dataPath + "/fPos.txt";
		StreamReader inp_stm = new StreamReader(path1);
		float val;

		while(!inp_stm.EndOfStream)
		{
			string line = inp_stm.ReadLine();
			if(line != "")
			{
				//Debug.Log("Line -------------------" + line);
				line = line.Replace("[[ ", "");
				line = line.Replace("[[", "");
				line = line.Replace("]]", "");
				line = line.Replace(" [ ", "");
				line = line.Replace("[", "");
				line = line.Replace(" [", "");
				line = line.Replace("]", "");
				line = line.Replace("  ", " ");

				string[] posStr = line.Split(' ');
				foreach(string s in posStr)
				{
					//Debug.Log("string: =========" + s + "=");

					float.TryParse(s, out val);
					if(s!="")
                    {
						pos.Add(val);
					}
					
					//Debug.Log("element: " + val);
				}
				//Debug.Log("pos length :         " + pos.Count);
			}
		}
		
		inp_stm.Close( );



		
		/*vel = new List<float>();
		var path2 =  Application.dataPath + "/fVel.txt";
		StreamReader inp_stm2 = new StreamReader(path2);
		float val2;

		while(!inp_stm2.EndOfStream)
		{
			string line = inp_stm2.ReadLine();
			if(line != "")
			{
				//Debug.Log("Line -------------------" + line);
				line = line.Replace("[[ ", "");
				line = line.Replace("[[", "");
				line = line.Replace(" [[", "");
				line = line.Replace("]]", "");
				line = line.Replace(" [ ", "");
				line = line.Replace(" [", "");
				line = line.Replace("[", "");
				line = line.Replace("]", "");
				line = line.Replace("  ", " ");

				string[] velStr = line.Split(' ');
				for(int i=0; i<velStr.Length; i++)
				{
					//Debug.Log("string: ++++++++++" + velStr[i]);

					float.TryParse(velStr[i], out val2);
					vel.Add(val2);
					//Debug.Log("element --------------" + val2);
				}
			}
		}
		inp_stm.Close( );*/




		return pos;
    }




}
