using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using nv;

public class Generator : MonoBehaviour
{  
    public Map map;

    public GSettings settings;

    protected struct CounterData
    {
        public string name;
        public int current;
        public int max;
    }

    protected Dictionary<string,CounterData> generatorCounters = new Dictionary<string, CounterData>();

    public virtual void GenerateMap( int size = 0 )
    {
        Map.UnloadMap( ref map );

        GameRNG.Generator.Reset();
    }

    public void OnApplicationQuit()
    {
        Map.UnloadMap( ref map );
    }

    public bool CheckExitLoop(string check_name, int max_iterations)
    {
        CounterData cd;
        if( generatorCounters.ContainsKey(check_name) == false )
        {
            cd = new CounterData();
            cd.name = check_name;
            cd.current = 0;
            cd.max = max_iterations;
            generatorCounters.Add(check_name, cd );
        }
        else
        {
            cd = generatorCounters[ check_name ];
        }

        int iterations = cd.current;
        if( cd.current >= cd.max )
        {
            Dev.Log(check_name+" in generator "+name+" exited early after "+ iterations+" iterations.");
            return true;
        }
        else
        {
            cd.current = iterations++;
            generatorCounters[ check_name ] = cd;
        }
        return false;
    }

    public void ClearExitCheck( string check_name )
    {
        if( generatorCounters.ContainsKey( check_name ) == false )
            return;

        CounterData cd;
        cd = generatorCounters[ check_name ];
        cd.current = 0;
        generatorCounters[ check_name ] = cd;
    }

}
