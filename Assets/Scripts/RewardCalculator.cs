using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RewardCalculator : MonoBehaviour
{
    public List<CarAgent1> agents;
    public List<float> rewards;
    void Start()
    {
        agents = GetComponentsInChildren<CarAgent1>().ToList();
        rewards = new List<float>(new float[agents.Count]);

    }
    void Update()
    {
        int i = 0;
        foreach(CarAgent1 agent in agents){
            rewards[i++] = agent.rewards;
        }
    }
}
