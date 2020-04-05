#include <cmath>
#include <iostream>
#include <string>
#include <thread>
#include <vector>

/// Return char that appearance at {pattern#strstr}
/// \param i index
/// \param pattern first string
/// \param str second str
/// \return char
char charFromPatternOrStr(int i, const std::string& pattern, const std::string& str)
{
	if (i < pattern.length())
	{
		return pattern[i];
	}

	if (i == pattern.length())
	{
		return '#';
	}

	if (i < pattern.length() + str.length() + 1)
	{
		return str[i - pattern.length() - 1];
	}

	return str[i - pattern.length() - str.length() - 1];
}

/// Calculate prefix-function for {pattern#strstr}
/// \param pattern first string
/// \param str second str
/// \return prefix-function as vector of int
std::vector<int> prefixFunction(const std::string& pattern, const std::string& str)
{
	int arraySize = pattern.length() + str.length() + str.length() + 1;
	std::vector<int> retVec(arraySize);
	retVec[0] = 0;

	for (int i = 1; i < arraySize; ++i)
	{
		int k = retVec[i - 1];
#ifndef NDEBUG
		std::cout << "K value => " << k << std::endl;
#endif
		char iChar = charFromPatternOrStr(i, pattern, str);
		char kChar = charFromPatternOrStr(k, pattern, str);
		while (k > 0 && iChar != kChar)
		{
			k = retVec[k - 1];
			kChar = charFromPatternOrStr(k, pattern, str);
#ifndef NDEBUG
			std::cout << "K index was decreased to => " << k << std::endl;
#endif
		}

		if (iChar == kChar)
		{
			++k;
#ifndef NDEBUG
			std::cout << "K index was increased to => " << k << std::endl;
#endif
		}

		retVec[i] = k;
	}

	return retVec;
}

/// Find first pattern length occurrence in specified range in prefix-function
/// \param patternLength length of pattern
/// \param prefix prefix-function
/// \return first pattern occurrence
int findPatternsOccurenciesInPrifix(int patternLength, const std::vector<int>& prefix)
{
	for (int i = 2 * patternLength; i >= 0; --i)
	{
		if (prefix[patternLength + i + 1] == patternLength)
		{
			return i - patternLength + 1;
		}
	}

	return -1;
}

/// Find index of cyclic offset second string in first one
/// \param str first string
/// \param pattern second string
/// \return index of offset
int findIndexOfCyclicOffset(const std::string& str, const std::string& pattern)
{
	int result = -1;
	std::vector<int> prefix = prefixFunction(pattern, str);

	result = findPatternsOccurenciesInPrifix(pattern.length(), prefix);

	return result != -1 ? str.length() - result : result;
}

int main()
{
	std::string stringA;
	std::cin >> stringA;
	std::string stringB;
	std::cin >> stringB;

	std::cout << findIndexOfCyclicOffset(stringB, stringA) << std::endl;
}