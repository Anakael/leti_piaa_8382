using System;
using System.Collections.Generic;
using System.Linq;

namespace lab4
{
	/// <summary>
	/// Class wrapper for printing
	/// </summary>
	public static class Logger
	{
		public enum LogLevel
		{
			Debug,
			Info
		}

		/// <summary>
		/// Log message with specify log level
		/// </summary>
		/// <param name="text"></param>
		/// <param name="level"></param>
		public static void Log(object text, LogLevel level = LogLevel.Debug)
		{
			int logLevel;
			var hasVariable = int.TryParse(Environment.GetEnvironmentVariable("LAB4_LOG_LEVEL"), out logLevel);
			logLevel = hasVariable ? logLevel : (int)level;

			if ((int)level >= logLevel)
			{
				Console.WriteLine(text.ToString());
			}
		}
	}

	class Program
	{
		/// <summary>
		/// Calculate prefix-function for string for single thread
		/// </summary>
		/// <param name="s">String</param>
		static int[] NativePrefixFunction(string s)
		{
			// Init array with default value
			var retArray = new int[s.Length];
			retArray[0] = 0;

			for (var i = 1; i < s.Length; ++i)
			{
				Logger.Log($"s[i={i}] => {s[i]}");
				// Take last prefix value 
				var k = retArray[i - 1];
				Logger.Log($"s[k={k}] => {s[k]}");
				while (k > 0 && s[i] != s[k])
				{
					// Reduce k value to previous in prefix
					k = retArray[k - 1];
					Logger.Log($"s[k={k}] => {s[k]}");
				}

				if (s[i] == s[k])
				{
					++k;
					Logger.Log($"s[k={k}] == s[i={i}]. K increased to {k}");
				}

				retArray[i] = k;
				Logger.Log($"Current prefix function => {string.Join("", retArray)}");
			}

			return retArray;
		}

		/// <summary>
		/// Calculate prefix-function for string
		/// </summary>
		/// <param name="s">String</param>
		/// <param name="patternPrefix">Prefix for pattern</param>
		/// <param name="lowerBound">Lower bound for thread</param>
		/// <param name="upperBound">Upper bound for thread</param>
		/// <param name="prevValue">Previous value for prefix</param>
		/// <returns>Value of prefix-function as array</returns>
		static int[] MultiThreadsPrefixFunction(string s, int[] patternPrefix, int lowerBound, int upperBound, int? prevValue = null)
		{
			// Init with empty array 
			var retArray = new int[upperBound - lowerBound];

			if (prevValue != null)
			{
				retArray[0] = (int)prevValue;
			}

			var offset = prevValue == null ? 0 : 1;

			for (var i = lowerBound + offset; i < upperBound; ++i)
			{
				Logger.Log($"s[i={i}] => {s[i]}");
				// Get value from concatenated string at i index
				var valueAtIIndex = s[i + patternPrefix.Length + 1];

				// Take previous prefix value if exist
				var k = i - lowerBound > 0 ? retArray[i - lowerBound - 1] : 0;
				Logger.Log($"s[k={k}] => {s[k]}");
				while (k > 0 && valueAtIIndex != s[k])
				{
					// Reduce k value to previous in prefix
					k = patternPrefix[k - 1];
					Logger.Log($"s[k={k}] => {s[k]}");
				}

				if (valueAtIIndex == s[k])
				{
					++k;
				}

				// Save k in local array (not for whole string)
				retArray[i - lowerBound] = k;
				Logger.Log($"Current prefix function => {string.Join("", retArray)}");
			}

			return retArray;
		}

		/// <summary>
		/// Fix prefix values between end of thread and rest of pattern
		/// </summary>
		/// <param name="prefix">Calculated prefix for string</param>
		/// <param name="patternPrefix">Calculated pattern for prefix</param>
		/// <param name="s">String for search</param>
		/// <param name="originalStrLength">Length of original string</param>
		/// <param name="threadsCount">Count of threads</param>
		/// <param name="countsPerThread">Count of elements per thread</param>
		static void FixPrefix(List<int> prefix, int[] patternPrefix, string s, int originalStrLength, int threadsCount, int countsPerThread)
		{
			for (var i = 1; i < threadsCount; ++i)
			{
				Logger.Log($"Thread => {i}", Logger.LogLevel.Debug);

				// Calc borders for fix prefix 
				var lowerBound = countsPerThread * i - 1;
				var upperBound = lowerBound + patternPrefix.Length;
				upperBound = upperBound < originalStrLength ? upperBound : originalStrLength;

				// Calc prefix in new borders
				var localPrefix = MultiThreadsPrefixFunction(s, patternPrefix, lowerBound, upperBound, prefix[lowerBound]);
				Logger.Log($"New prefix for bounds[{lowerBound};{upperBound}] => {string.Join("", localPrefix)}");
				for (var j = lowerBound + 1; j < upperBound; ++j)
				{
					prefix[j] = localPrefix[j - lowerBound];
				}
				Logger.Log($"Current prefix function => {string.Join("", prefix)}");
				Logger.Log("", Logger.LogLevel.Debug);
			}
		}

		/// <summary>
		/// Find indexes of pattern occurrences in string
		/// </summary>
		/// <param name="str">String</param>
		/// <param name="pattern">Pattern for search</param>
		/// <param name="threadsCount">Count of threads</param>
		/// <returns>List of indexes</returns>
		static List<int> FindPatternsOccurrences(string str, string pattern, int threadsCount)
		{
			// Init data
			Logger.Log($"Thread => 1", Logger.LogLevel.Debug);
			var retArray = new List<int>();
			var strForPrefix = $"{pattern}#{str}";
			Logger.Log($"Concatenated string => {strForPrefix}");
			var prefix = new List<int>();
			var patternPrefix = NativePrefixFunction(pattern);
			Logger.Log($"Prefix for pattern => {string.Join("", patternPrefix)}");
			var countsPerThread = (int)(Math.Ceiling((double)str.Length / threadsCount));
			Logger.Log($"Count of indexes per thread => {countsPerThread}", Logger.LogLevel.Debug);

			// Calc prefix by threads
			for (var i = 0; i < threadsCount; ++i)
			{
				Logger.Log("", Logger.LogLevel.Debug);
				Logger.Log($"Thread => {i}", Logger.LogLevel.Debug);
				var lowerBound = (countsPerThread * i);
				var upperBound = countsPerThread * (i + 1);
				upperBound = upperBound < str.Length ? upperBound : str.Length;
				Logger.Log($"Bounds => [{lowerBound};{upperBound}]", Logger.LogLevel.Debug);
				prefix.AddRange(MultiThreadsPrefixFunction(strForPrefix, patternPrefix, lowerBound, upperBound));
				Logger.Log("", Logger.LogLevel.Debug);
			}

			// Recalc prefix if in multithreads
			if (countsPerThread != prefix.Count)
			{
				FixPrefix(prefix, patternPrefix, strForPrefix, str.Length, threadsCount, countsPerThread);
			}

			// Collect result
			for (var i = 0; i < threadsCount; ++i)
			{
				Logger.Log("", Logger.LogLevel.Debug);
				Logger.Log($"Thread => {i}", Logger.LogLevel.Debug);
				var lowerBound = (countsPerThread * i);
				var upperBound = countsPerThread * (i + 1);
				upperBound = upperBound < str.Length ? upperBound : str.Length;
				Logger.Log($"Bounds => [{lowerBound};{upperBound}]", Logger.LogLevel.Debug);
				Logger.Log("", Logger.LogLevel.Debug);
				retArray.AddRange(FindPatternsOccurrencesInPrefix(prefix, pattern.Length, lowerBound, upperBound));
				Logger.Log("", Logger.LogLevel.Debug);
			}

			Logger.Log("", Logger.LogLevel.Debug);
			return retArray;
		}

		/// <summary>
		/// Find indexes of patterns occurrences in prefix-function
		/// </summary>
		/// <param name="prefix">Prefix-function</param>
		/// <param name="patternLength">Length of pattern string</param>
		/// <param name="lowerBound">Lower bound</param>
		/// <param name="upperBound">Upper bound</param>
		/// <returns></returns>
		static List<int> FindPatternsOccurrencesInPrefix(IReadOnlyList<int> prefix, int patternLength, int lowerBound, int upperBound)
		{
			var retArray = new List<int>();
			for (var i = lowerBound; i < upperBound; ++i)
			{
				if (prefix[i] != patternLength) continue;
				var resolvedIndex = i - patternLength + 1;
				Logger.Log($"Found value with {patternLength} at => {resolvedIndex}", Logger.LogLevel.Debug);
				retArray.Add(resolvedIndex);
			}

			return retArray;
		}

		static void Main(string[] args)
		{
			var pattern = Console.ReadLine();
			var str = Console.ReadLine();
			var threadsCount = int.Parse(Console.ReadLine());
			Logger.Log($"Pattern value => {pattern}", Logger.LogLevel.Debug);
			Logger.Log($"String value => {str}", Logger.LogLevel.Debug);
			Logger.Log($"Count of threads value => {threadsCount}", Logger.LogLevel.Debug);
			var result = FindPatternsOccurrences(str, pattern, threadsCount);
			Logger.Log(result.Any() ? string.Join(",", result) : "-1");
		}
	}
}
