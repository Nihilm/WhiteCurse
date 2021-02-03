using System;
using System.Collections.Generic;
using UnityEngine;
public static class Noise
{
    private static UInt32 bitmask0 = 0xb5297a4d;
    private static UInt32 bitmask1 = 0x68e31da4;
    private static UInt32 bitmask2 = 0x1b56c4e9;
    public static UInt32 Noise1D(int x, UInt32 seed){
        UInt32 hash = unchecked((UInt32)x);
        hash *= bitmask0;
        hash += seed;
        hash ^= hash >> 8;
        hash += bitmask1;
        hash ^= hash >> 8;
        hash *= bitmask2;
        hash ^= hash >> 8;
        return hash;
    }
    public static UInt32 Noise2D(int x, int y, UInt32 seed){
        const int prime0 = 0xbd4bcb5;
        return Noise1D(x + prime0 * y, seed);
    }
    public static UInt32 Noise3D(int x, int y, int z, UInt32 seed){
        const int prime0 = 0xbd4bcb5;
        const int prime1 = 0x63d68d;
        return Noise1D(x + prime0 * y + prime1 * z, seed);
    }
    public static float Exponential(float lambda = 1){
        return Mathf.Log(1 - UnityEngine.Random.value) / -lambda;
    }
    //https://en.wikipedia.org/wiki/Marsaglia_polar_method
    public static float Gaussian(float mean = 0, float deviation = 1){
        float u, v, w;
        do{
            u = 2.0f * UnityEngine.Random.value - 1.0f;
            v = 2.0f * UnityEngine.Random.value - 1.0f;
            w = u*u +v*v;
        }while(w >= 1.0f);
        w = u * Mathf.Sqrt(-2.0f * Mathf.Log(w) / w);
        //https://en.wikipedia.org/wiki/68%E2%80%9395%E2%80%9399.7_rule
        return Mathf.Clamp(w * deviation + mean, mean - 3 * deviation, mean + 3 * deviation);
    }
    public class WeightTable<T> {
        private double totalWeight = 0;
        private List<T> items = new List<T>();
        private List<double> weights = new List<double>();
        public WeightTable<T> Add(T item, double weight){
            totalWeight += weight;
            items.Add(item); weights.Add(weight);
            return this;
        }
        public T Sample(System.Random random){
            double number = random.NextDouble() * totalWeight;
            for(int i = 0; i < items.Count; i++){
                double weight = weights[i];
                if(weight > number) return items[i];
                number -= weight;
            }
            return items[items.Count - 1];
        }
    }
}