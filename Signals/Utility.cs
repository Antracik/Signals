using MathNet.Numerics;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Signals
{
    public class Utility
    {
        public static IEnumerable<(double[] xs, double[] ys)> GenerateRandomScatterPoints(int pointCount, int sampleRate)
        {
            Random rand = new Random();
            List<(double[] xs, double[] ys)> list = new List<(double[] xs, double[] ys)>();
            for (int i = 0; i < 10; i++)
            {
                var xs = DataGen.Consecutive(pointCount);
                var ys = Generate.Sinusoidal(pointCount, sampleRate, rand.Next(1, 20), rand.Next(1, 10), phase: rand.Next(1, 10));

                list.Add((xs, ys));
            }

            return list;
        }

        public static double[] GenerateWaveForm(int pointCount, int sampleRate)
        {
            Random rand = new Random();

            double[] ys = new double[pointCount];

            for (int i = 0; i < 3; i++)
            {
                var tempYs = Generate.Sinusoidal(pointCount, sampleRate, rand.Next(1, 20), rand.Next(-20, 20), phase: rand.Next(0, 360));

                for (int j = 0; j < tempYs.Length; j++)
                {
                    ys[j] += tempYs[j];
                }
            }

            return ys;
        }

        public static (double[] ys, int frequency, int amplitude, int phase) GenerateSinusoidal(int pointCount, int sampleRate)
        {
            Random rand = new Random();

            var frequency = rand.Next(1, 100);
            var amplitude = rand.Next(-20, 20);
            var phase = rand.Next(0, 360);

            return (Generate.Sinusoidal(pointCount, sampleRate, frequency, amplitude, phase: phase), frequency, amplitude, phase);
        }

        public static double[] CombineSinusodial(IEnumerable<double[]> sinusodials)
        {
            if (!sinusodials.Any())
                throw new ArgumentException($"Argument {nameof(sinusodials)} should not be empty");
            if (!sinusodials.All(x => x.Length == sinusodials.ElementAt(0).Length))
                throw new ArgumentException($"All waveforms in {nameof(sinusodials)} should be of the same length");

            var temp = sinusodials.ToList();
            double[] result = new double[temp[0].Length];

            for (int j = 0; j < temp.Count; j++)
            {
                for (int i = 0; i < result.Length; i++)
                {
                    result[i] += temp[j][i];
                }
            }

            return result;
        }

        public static (int index, double value) FindClosestXIndex(double[] arr, double target)
        {
            int n = arr.Length;

            if (target <= arr[0])
                return (0, arr[0]);
            if (target >= arr[n - 1])
                return (n - 1, arr[n - 1]);

            int i = 0, j = n, mid = 0;
            while (i < j)
            {
                mid = (i + j) / 2;

                if (Math.Abs(arr[mid] - target) < 0.000000001)
                    return (mid, arr[mid]);

                double temp;
                if (target < arr[mid])
                {
                    if (mid > 0 && target > arr[mid - 1])
                    {
                        temp = GetClosest(arr[mid - 1], arr[mid], target);
                        if (Math.Abs(temp - arr[mid - 1]) < 0.000000001)
                            return (mid - 1, arr[mid - 1]);
                        else
                            return (mid, arr[mid]);
                    }

                    j = mid;
                }

                else
                {
                    if (mid < n - 1 && target < arr[mid + 1])
                    {
                        temp = GetClosest(arr[mid], arr[mid + 1], target);
                        if (Math.Abs(temp - arr[mid]) < 0.000000001)
                            return (mid, arr[mid]);
                        else
                            return (mid + 1, arr[mid + 1]);
                    }
                    i = mid + 1;
                }
            }

            return (mid, arr[mid]);
        }

        private static double GetClosest(double val1, double val2, double target)
        {
            if (target - val1 >= val2 - target)
                return val2;

            return val1;
        }
    }
}
