#include <iostream>
#include <vector>
#include <chrono>


struct Point
{
    int x;
    int y;
};

struct Square
{
    Square(int x, int y, int _length)
            : leftCorner{x, y}, length(_length)
    {
    }

    Point leftCorner;
    int length;

    friend std::ostream& operator<<(std::ostream& os, const Square& s)
    {
        os << s.leftCorner.x + 1 << " " << s.leftCorner.y + 1 << " " << s.length;
        return os;
    }
};

/// Make fragmentation for N divided by 2
/// \param n size of table
/// \return vector of Square
std::vector<Square> makeFragmentationForDiv2(int n)
{
    std::vector<Square> retSquares;
    int half = int(n / 2);
    for (int i = 0; i < n; i += half)
    {
        for (int j = 0; j < n; j += half)
        {
            retSquares.emplace_back(i, j, half);
        }
    }

    return retSquares;
}

/// Make fragmentation for N divided by 3
/// \param n size of table
/// \return vector of Square
std::vector<Square> makeFragmentationForDiv3(int n)
{
    std::vector<Square> retSquares;
    int smallInnerN = int(n / 3);
    int bigInnerN = smallInnerN * 2;
    retSquares.emplace_back(0, 0, bigInnerN);

    // Bottom
    for (int i = 0; i < n; i += smallInnerN)
    {
        retSquares.emplace_back(i, n - smallInnerN, smallInnerN);
    }

    // Side
    for (int i = 0; i < n - smallInnerN; i += smallInnerN)
    {
        retSquares.emplace_back(n - smallInnerN, i, smallInnerN);
    }

    return retSquares;
}

/// Make fragmentation for N divided by 5
/// \param n size of table
/// \return vector of Square
std::vector<Square> makeFragmentationForDiv5(int n)
{
    std::vector<Square> retSquares;
    int half = int(n / 5 * 2);
    int third = int(n / 5 * 3);
    int fifth = int(n / 5);
    retSquares.emplace_back(0, 0, third);

    // Big in corners

    //for a, b in [(0, 1), (1, 0), (1, 1)]:
    retSquares.emplace_back(0, third, half);
    retSquares.emplace_back(third, 0, half);
    retSquares.emplace_back(third, third, half);

    // Small in gaps
    for (int i = 0; i < 2; ++i)
    {
        retSquares.emplace_back(half, third + i * fifth, fifth);
        retSquares.emplace_back(third + i * fifth, half, fifth);
    }

    return retSquares;
}

/// Make base fragmentation for backtracking with 3 big Square
/// \param n size of table
/// \return vector of Square
std::vector<Square> makeBaseFragmentation(int n)
{
    int half = int(n / 2) + 1;
    return {Square(0, 0, half),
            Square(half, 0, half - 1),
            Square(0, half, half - 1)};
}

/// Make backtracking iteration fragmentation
/// \param n size of table
/// \param center center of table
/// \param field field of defect_square
/// \param currentStack vector of currentStack
/// \best_stack_size size of stack with the best fragmentation
bool backtrackingIter(int n, int center, int** field, std::vector<Square>& currentStack, int bestStackSize)
{
    const auto canGrowth = [n, center, &field](Square& square)
    {
        // Check bottom
        if (square.leftCorner.x + square.length + 1 <= n && square.leftCorner.y + square.length + 1 <= n)
        {
            for (int i = square.leftCorner.x; i < square.leftCorner.x + square.length + 1; ++i)
            {
                if (field[i - center][square.leftCorner.y + square.length - center] != 0)
                    return false;
            }
        }
        else
        {
            return false;
        }
        // Check side
        for (int i = square.leftCorner.y; i < square.leftCorner.y + square.length + 1; ++i)
        {
            if (field[square.leftCorner.x + square.length - center][i - center] != 0)
                return false;
        }

        return true;
    };

    const auto growth_square = [center, &field](Square& square)
    {
        square.length += 1;
        for (int i = square.leftCorner.x; i < square.leftCorner.x + square.length; ++i)
        {
            for (int j = square.leftCorner.y; j < square.leftCorner.y + square.length; ++j)
            {
                field[i - center][j - center] = 1;
            }
        }
    };

    for (int i = center; i < n; ++i)
    {
        for (int j = center; j < n; ++j)
        {
            if (field[i - center][j - center] == 0)
            {
                auto tmpSquare = Square(i, j, 1);
                field[i - center][j - center] = 1;
                while (canGrowth(tmpSquare))
                {
                    growth_square(tmpSquare);
                }

                currentStack.push_back(tmpSquare);

                if (bestStackSize && currentStack.size() >= bestStackSize)
                    return false;
            }
        }
    }

    return true;
}

/// Find best fragmentation for specify diag
/// \param n size of table
/// \param center center of table
/// \param leftCornerShift left corner coord of diag
/// \return list of squares
std::vector<Square> findNextFragmentationForDiag(int n, int center, int leftCornerShift)
{
    int** field = new int* [n - center + 1];
    for (int i = 0; i < n - center; ++i)
    {
        field[i] = new int[n - center + 1];
    }
    const auto squize_square = [center, &field](Square& square)
    {
        // Bottom
        for (int i = square.leftCorner.x + square.length - 1; i > square.leftCorner.x - 1; --i)
        {
            field[i - center][square.leftCorner.y + square.length - 1 - center] = 0;
        }

        // Side
        for (int j = square.leftCorner.y + square.length - 2; j > square.leftCorner.y - 1; --j)
        {
            field[square.leftCorner.x + square.length - 1 - center][j - center] = 0;
        }

        square.length -= 1;
    };

    for (int i = center; i < n; ++i)
    {
        for (int j = center; j < n; ++j)
        {
            bool isEmpty =
                    (i != j || (i != center and j != center)) && (i < leftCornerShift || j < leftCornerShift);
            field[i - center][j - center] = isEmpty ? 0 : 1;
        }
    }

    std::vector<Square> retStack;
    std::vector<Square> tmpStack;

    while (!tmpStack.empty() || retStack.empty())
    {
        if (retStack.empty() || tmpStack.empty() || tmpStack.size() < retStack.size())
        {
            bool is_better = backtrackingIter(n, center, static_cast<int**>(field), tmpStack, retStack.size());
            if ((is_better && tmpStack.size() < retStack.size()) || retStack.empty())
            {
                retStack = tmpStack;
            }
        }

        while (!tmpStack.empty() && tmpStack.back().length == 1)
        {
            auto deleted = tmpStack.back();
            field[deleted.leftCorner.x - center][deleted.leftCorner.y - center] = 0;
            tmpStack.pop_back();
        }

        if (!tmpStack.empty() && tmpStack.back().length > 1)
        {
            squize_square(tmpStack.back());
        }
    }

    for (int i = 0; i < n - center; ++i)
    {
        delete [] field[i];
    }

    delete [] field;

    return retStack;
}

/// Make backtracking fragmentation
/// \param n size of table
/// \return vector of Square
std::vector<Square> makeBacktrackingFragmentation(int n)
{
    auto squares = makeBaseFragmentation(n);
    std::vector<Square> minSquares;
    int center = n / 2;
    for (int i = 2; i < center; ++i)
    {
        int left_corner_shit = n - i;
        std::vector<Square> best_fragmentation_for_diag = {Square(left_corner_shit, left_corner_shit, i)};

        auto backtack_fragmentation = findNextFragmentationForDiag(n, center, left_corner_shit);
        best_fragmentation_for_diag.insert(best_fragmentation_for_diag.begin() + 1, backtack_fragmentation.begin(),
                                           backtack_fragmentation.end());

        if (minSquares.empty() || best_fragmentation_for_diag.size() < minSquares.size())
        {
            minSquares = best_fragmentation_for_diag;
        }

    }

    squares.insert(squares.begin() + 3, minSquares.begin(), minSquares.end());
    return squares;
}

/// Print field state
/// \param n size of table
/// \param squares vector of squares
void printDebugSquares(int n, std::vector<Square>& squares)
{
    int field[n][n];

    int square_index = 0;
    for (auto& square : squares)
    {
        for (int y = square.leftCorner.y; y < square.leftCorner.y + square.length; ++y)
        {
            for (int x = square.leftCorner.x; x < square.leftCorner.x + square.length; ++x)
            {
                field[x][y] = square_index;
            }
        }
        ++square_index;
    }

    for (int i = 0; i < n; ++i)
    {
        for (int j = 0; j < n; ++j)
        {
            std::cout << field[i][j] << " ";
        }
        std::cout << std::endl;
    }
}

/// Print result and each Square
/// \param n size of table
/// \param squares vector of squares
void printSquares(int n, std::vector<Square>& squares)
{
    std::cout << squares.size() << std::endl;
    for (auto& square : squares)
    {
        std::cout << square << std::endl;
    }

#ifndef NDEBUG
    printDebugSquares(n, squares);
#endif
}


/// Main function for call
/// \return
int main()
{
    int n;
    std::cin >> n;
    auto fun = makeBacktrackingFragmentation;
    if (n % 2 == 0)
        fun = makeFragmentationForDiv2;
    else if (n % 3 == 0)
        fun = makeFragmentationForDiv3;
    else if (n % 5 == 0)
        fun = makeFragmentationForDiv5;
    else
        fun = makeBacktrackingFragmentation;
    auto res = fun(n);

    printSquares(n, res);
    return 0;
}
