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
		public static void Log(object text, LogLevel level = LogLevel.Info)
		{
			int logLevel;
			var hasVariable = int.TryParse(Environment.GetEnvironmentVariable("LAB4_LOG_LEVEL"), out logLevel);
			logLevel = hasVariable ? logLevel : (int)LogLevel.Info;

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
			var retArray = new int[str.Length];
			retArray[0] = 0;

			for (var i = 1; i < str.Length; ++i)
			{
				var k = retArray[i - 1];
				Logger.Log($"K value => {k}", Logger.LogLevel.Debug);
				while (k > 0 && str[i] != str[k])
				{
					k = retArray[k - 1];
					Logger.Log($"K index was decreased to => {k}", Logger.LogLevel.Debug);
				}

				if (str[i] == str[k])
				{
					++k;
					Logger.Log($"K index was increased to => {k}", Logger.LogLevel.Debug);
				}

				retArray[i] = k;
			}

			return retArray;
		}

		/// <summary>
		/// Find indexes of pattern occurrences in string
		/// </summary>
		/// <param name="str">String</param>
		/// <param name="pattern">Pattern for search</param>
		/// <returns>List of indexes</returns>
		static List<int> FindPatternsOccurrences(string str, string pattern)
		{
			var retArray = new List<int>();
			var prefix = PrefixFunction($"{pattern}#{str}");

			int maxThreadsCount;
			int maxAsycIOThreadsCount;
			var threadsCount = (int)(Math.Floor((double)str.Length / pattern.Length));
			Logger.Log($"Threads count => {threadsCount}", Logger.LogLevel.Debug);
			ThreadPool.GetMaxThreads(out maxThreadsCount, out maxAsycIOThreadsCount);
			threadsCount = threadsCount < maxThreadsCount ? threadsCount : maxThreadsCount;
			var countsPerThread = (int)(Math.Floor((double)str.Length / threadsCount));
			Logger.Log($"Count of indexes per thread => {countsPerThread}", Logger.LogLevel.Debug);

			for (var i = 0; i < threadsCount; ++i)
			{
				retArray.AddRange(FindPatternsOccurrencesInPrefix(str, pattern.Length, prefix, countsPerThread * i, countsPerThread * (i + 1)));
			}

			var upperBound = countsPerThread * threadsCount;
			retArray.AddRange(FindPatternsOccurrencesInPrefix(str, pattern.Length, prefix, upperBound,
				str.Length));

			return retArray;
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
					retArray.Add(i - patternLength + 1);
				}
			}

			return retArray;
		}

		static void Main(string[] args)
		{
			var pattern = Console.ReadLine();
			var str = Console.ReadLine();
			var result = FindPatternsOccurrences(str, pattern);
			Logger.Log(result.Any() ? string.Join(",", result) : "-1");
		}
	}
}