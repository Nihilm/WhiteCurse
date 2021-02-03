namespace _Algorithms {
    using System;
    using System.Collections.Generic;

    public interface IStateNode<T> where T : IStateNode<T> {
        T NextState(Random random);
        double Energy{ get; }
    }
    public class SimulatedAnnealing<T> where T : IStateNode<T> {
        public double minProbability = 0.2d;
        public double maxProbability = 0.01d;
        public int iterations;

        public IEnumerable<T> Simulate(T state, Random random){
            double minTemperature = -1.0d / Math.Log(minProbability);
            double maxTemperature = -1.0d / Math.Log(maxProbability);
            double alpha = Math.Pow(maxTemperature / minTemperature, 1.0d / (iterations - 1));
            double temperature = maxTemperature;
            double energy = state.Energy;
            double averageDelta = state.Energy;
            T minState = state;
            double minEnergy = double.MaxValue;

            if(energy <= 0) yield return state;
            for(int i = 0, steps = 1; i < iterations; i++, temperature *= alpha){
                T nextState = state.NextState(random);
                bool greedy = nextState == null;
                if(greedy) nextState = minState;

                double nextEnergy = nextState.Energy;

                if(nextEnergy <= minEnergy){
                    minEnergy = nextEnergy;
                    minState = nextState;
                }
                if(greedy || nextEnergy <= energy ||
                random.NextDouble() < Math.Exp(-(nextEnergy - energy) / averageDelta * temperature)){
                    minEnergy = double.MaxValue;
                    energy = nextEnergy;
                    state = nextState;
                    steps++;
                    averageDelta = (averageDelta * (steps - 1) + Math.Abs(nextEnergy - energy)) / steps;
                    if(energy <= 0) yield return nextState;
                }
            }
        }
    }
}