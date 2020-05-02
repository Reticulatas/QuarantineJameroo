using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IWantsBeats
{
    void OnBeat();
    void OnBigBeat();
}

public class GameManager : BehaviourSingleton<GameManager>
{
    private float gameTime;
    private ulong beats;

    public const int BEATSINABIGBEAT = 4;
    public const float BIGBEATTIME = 1.8f;
    public const float BEATTIMER = BIGBEATTIME / BEATSINABIGBEAT;

    private readonly List<IWantsBeats> beatWanters = new List<IWantsBeats>();

    public void Register(IWantsBeats beater)
    {
        beatWanters.Add(beater);
    }
    public void UnRegister(IWantsBeats beater)
    {
        beatWanters.Remove(beater);
    }

    public void Update()
    {
        gameTime += Time.deltaTime;

        if (gameTime > BEATTIMER)
        {
            gameTime = gameTime - BEATTIMER;
            ++beats;

            if (beats % BEATSINABIGBEAT == 0)
            {
                // dispatch da big beats
                foreach (var beater in beatWanters.ToList())
                {
                    beater.OnBigBeat();
                }
            }
            else
            {
                // dispatch da beats
                foreach (var beater in beatWanters.ToList())
                {
                    beater.OnBeat();
                }
            }
        }
    }
}
