using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
		/// Calculate prefix-function for string
		/// </summary>
		/// <param name="str">String</param>
		/// <returns>Value of prefix-function as array</returns>
		static int[] PrefixFunction(string str)
		{
			Logger.Log($"Calc prefix-function for {str}", Logger.LogLevel.Debug);
			var retArray = new int[str.Length]; // returnable array 
			retArray[0] = 0;

			for (var i = 1; i < str.Length; ++i)
			{
				Logger.Log("", Logger.LogLevel.Debug);
				Logger.Log($"Step => {i}", Logger.LogLevel.Debug);
				Logger.Log($"str[i={i}] => {str[i]}", Logger.LogLevel.Debug);
				var k = retArray[i - 1]; // take last k
				Logger.Log($"k value => {k} at prefix[{i - 1}]", Logger.LogLevel.Debug);
				Logger.Log($"str[k] value => {str[k]}", Logger.LogLevel.Debug);
				while (k > 0 && str[i] != str[k])
				{
					k = retArray[k - 1]; // decrease k
					Logger.Log($"\tk index was decreased to => {k}", Logger.LogLevel.Debug);
					Logger.Log($"\tstr[k] value => {str[k]}", Logger.LogLevel.Debug);
				}

				if (str[i] == str[k])
				{
					++k; // increase k (don't tell the elf)
					Logger.Log($"Found equals chars. k index was increased to => {k}", Logger.LogLevel.Debug);
				}

				retArray[i] = k; // save k
				Logger.Log($"Prefix value => {string.Join("", retArray)}", Logger.LogLevel.Debug);
			}

			return retArray; // return (again, don't tell the elf)
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
			Logger.Log($"Thread => 0", Logger.LogLevel.Debug);
			var retArray = new List<int>();
			var prefix = PrefixFunction($"{pattern}#{str}"); // calc prefix
			Logger.Log("", Logger.LogLevel.Debug);
			var countsPerThread = (int)(Math.Floor((double)str.Length / threadsCount));
			Logger.Log($"Count of indexes per thread => {countsPerThread}", Logger.LogLevel.Debug);
			Logger.Log("", Logger.LogLevel.Debug);

			for (var i = 0; i < threadsCount; ++i)
			{
				Logger.Log($"Thread => {i}", Logger.LogLevel.Debug);
				Logger.Log($"Bounds => [{countsPerThread * i};{countsPerThread * (i + 1)}]", Logger.LogLevel.Debug);
				retArray.AddRange(FindPatternsOccurrencesInPrefix(str, pattern.Length, prefix,
					countsPerThread * i, countsPerThread * (i + 1))); // start search
				Logger.Log("", Logger.LogLevel.Debug);
			}

			var upperBound = countsPerThread * threadsCount; // calc upper bound
			retArray.AddRange(FindPatternsOccurrencesInPrefix(str, pattern.Length, prefix, upperBound,
				str.Length));

			return retArray; // return (and again, don't tell the elf)
		}

		/// <summary>
		/// Find indexes of patterns occurrences in prefix-function
		/// On specified range
		/// </summary>
		/// <param name="str">String</param>
		/// <param name="patternLength">Length of pattern string</param>
		/// <param name="prefix">Prefix-function</param>
		/// <param name="start">Start of range for search</param>
		/// <param name="end">End of range for search</param>
		/// <returns></returns>
		static List<int> FindPatternsOccurrencesInPrefix(string str, int patternLength, IReadOnlyList<int> prefix, int start, int end)
		{
			var retArray = new List<int>();
			for (var i = start; i < end; ++i)
			{
				if (prefix[patternLength + i + 1] == patternLength)
				{
					Logger.Log($"Found value with {patternLength} at => {i - patternLength + 1}", Logger.LogLevel.Debug);
					retArray.Add(i - patternLength + 1);
				}
			}

			return retArray; // return (and again, don't tell the elf)
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
