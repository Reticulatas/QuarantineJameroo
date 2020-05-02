using System;
using System.Collections.Generic;
using System.Linq;
using MoreLinq;
using Random = UnityEngine.Random;

namespace Util
{
    public class SimplePoll<T>
    {
        public T Result { get; protected set; }
        public Dictionary<T, int> Votes { get; protected set; } = new Dictionary<T, int>();

        private int resultVotes = -1;

        public void Vote(T forWhat)
        {
            vote(forWhat);
        }

        protected void vote(T forWhat, int howMuch = 1)
        {
            if (howMuch <= 0)
                return;

            int votes = 0;
            if (!Votes.ContainsKey(forWhat))
            {
                Votes.Add(forWhat, howMuch);
                votes = howMuch;
            }
            else
            {
                Votes[forWhat] += howMuch;
                votes = Votes[forWhat];
            }

            if (votes > resultVotes)
            {
                resultVotes = votes;
                Result = forWhat;
            }
        }
    }

    public class WeightedPoll<T> : SimplePoll<T> 
    {
        public T WeightedRandomResult
        {
            get { return getWeightedResult(); }
        }

        public T BestResult
        {
            get { return Votes.MaxBy(x => x.Value).Key; }
        }

        public void Vote(T forWhat, int howMuch)
        {
            vote(forWhat, howMuch);
        }

        private T getWeightedResult()
        {
            float totalVotes = (float)Votes.Values.Sum();
            float r = Random.value;
            float acc = 0;

            foreach (var vote in Votes)
            {
                float p = (float)vote.Value / totalVotes;
                acc += p;
                if (r <= acc)
                {
                    return vote.Key;
                }
            }
            return Result;
        }
    }
}
