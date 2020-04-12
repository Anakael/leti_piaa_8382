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
	if (i < pattern.length()) // pattern part
	{
		return pattern[i];
	}

	if (i == pattern.length()) // separator
	{
		return '#';
	}

	if (i < pattern.length() + str.length() + 1) // first string part
	{
		return str[i - pattern.length() - 1];
	}

	return str[i - pattern.length() - str.length() - 1]; // second string part
}

/// Calculate prefix-function for {pattern#strstr}
/// \param pattern first string
/// \param str second str
/// \return prefix-function as vector of int
std::vector<int> prefixFunction(const std::string& pattern, const std::string& str)
{
#ifndef NDEBUG
	std::cout << "Calc prefix-function for => " << pattern << "#" << str << str << std::endl;
#endif
	int arraySize = pattern.length() + str.length() + str.length() + 1;
	std::vector<int> retVec(arraySize);
	retVec[0] = 0;

	for (int i = 1; i < arraySize; ++i)
	{
		int k = retVec[i - 1]; // take k
		char iChar = charFromPatternOrStr(i, pattern, str);
		char kChar = charFromPatternOrStr(k, pattern, str);
#ifndef NDEBUG
		std::cout << std::endl;
		std::cout << "i value => " << i << std::endl;
		std::cout << "Step => " << i << std::endl;
		std::cout << "str[i=" << i << "] => " << iChar << std::endl;
		std::cout << "str[k=" << k << "] => " << kChar << std::endl;
		std::cout << "k value => " << k << " at prefix[i-1=" << i - 1 << "]" << std::endl;
#endif
		while (k > 0 && iChar != kChar)
		{
			k = retVec[k - 1]; // decrease k
			kChar = charFromPatternOrStr(k, pattern, str);
#ifndef NDEBUG
			std::cout << "\tk index was decreased to => " << k << "at prefix[" << i - 1 << "]" << std::endl;
			std::cout << "\tstr[k] value => " << kChar << std::endl;
#endif
		}

		if (iChar == kChar)
		{
			++k; // increase k
#ifndef NDEBUG
			std::cout << "Found equals chars. k index was increased to => " << k << std::endl;
#endif
		}

		retVec[i] = k; // save k

#ifndef NDEBUG
		std::cout << "Prefix value => ";
		for (int i = 0; i < retVec.size(); ++i)
		{
			std::cout << retVec[i];
		}
		std::cout << std::endl;
#endif
	}

	return retVec; // return array
}

/// Find first pattern length occurrence in specified range in prefix-function
/// \param patternLength length of pattern
/// \param prefix prefix-function
/// \return first pattern occurrence
int findPatternsOccurenciesInPrifix(int patternLength, const std::vector<int>& prefix)
{
	for (int i = 2 * patternLength; i >= 0; --i)
	{
		if (prefix[patternLength + i + 1] == patternLength) // index of offset
		{
#ifndef NDEBUG
			std::cout << "Found answer at "
					  << "prefix[patternLength+i+1" << patternLength + i + 1 << "]" << std::endl;
#endif
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
	std::vector<int> prefix = prefixFunction(pattern, str); // calc prefix function
#ifndef NDEBUG
	std::cout << "prefix => ";
	for (int i : prefix)
	{
		std::cout << i;
	}
	std::cout << std::endl;
#endif

	result = findPatternsOccurenciesInPrifix(pattern.length(), prefix);

	return result != -1 ? str.length() - result : result;
}

int main()
{
	std::string stringA;
	std::cin >> stringA; // read str
	std::string stringB;
	std::cin >> stringB; // read str

	bool isStringsEqualsBySize = stringA.length() == stringB.length();

	if (!isStringsEqualsBySize)
	{
		std::cout << "String have different size" << stringA.length() << " != " << stringB.length() << std::endl;
	}

	int result = isStringsEqualsBySize
		? findIndexOfCyclicOffset(stringB, stringA)
		: -1;

	std::cout << result << std::endl; // print result
}