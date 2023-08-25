using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pixelator.Extentions
{
    internal static class ListExtention
    {
        public static int BiggestValueIndexInList(this List<float> list, int untilIndex)
        {
            int BiggestIndex = 0;
            float BiggestValue = 0;


            for (int i = 1; i < untilIndex; i++)
            {
                if (BiggestValue < list[i])
                {
                    BiggestValue = list[i];
                    BiggestIndex = i;
                }
            }
            return BiggestIndex;
        }

        public static List<List<int>> SortAndCountFrequencies(this List<int> lengths)
        {
            var frequencyMap = new Dictionary<int, int>();

            for (int i = 0; i < lengths.Count; i++)
            {
                frequencyMap[lengths[i]] = frequencyMap.ContainsKey(lengths[i]) ? frequencyMap[lengths[i]]+1 : 1;
            }

            // Written By Chat (GPT-3.5)
            var sortedFrequencies = frequencyMap.OrderByDescending(kv => kv.Value)
                                              .ThenBy(kv => kv.Key)
                                              .ToList();

            var sortedLengths = sortedFrequencies.Select(kv => kv.Key).ToList();
            var frequencies = sortedFrequencies.Select(kv => kv.Value).ToList();

            // Calculate the average frequency
            double averageFrequency = frequencies.Average();

            // Set a threshold for removing values based on a factor (e.g., 0.5)
            double thresholdFactor = 0.5;
            double threshold = averageFrequency * thresholdFactor;

            // Remove values with frequencies below the threshold
            var filteredSortedLengths = new List<int>(sortedLengths.Count);
            var filteredFrequencies = new List<int>(sortedLengths.Count);

            for (int i = 0; i < sortedLengths.Count; i++)
            {
                if (frequencies[i] >= threshold)
                {
                    filteredSortedLengths.Add(sortedLengths[i]);
                    filteredFrequencies.Add(frequencies[i]);
                }
            }
            filteredSortedLengths.TrimExcess();
            filteredFrequencies.TrimExcess();

            return new List<List<int>> { filteredSortedLengths, filteredFrequencies };
        }

    }
}
