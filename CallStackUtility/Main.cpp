//
// Main.cpp -- CallStackUtility
// Open Source via MIT License
//

#include "CallStackUtility.h"

#include <iostream>

int testFunc(int iter, callstackutility::CallStack const& ref)
{
    if (iter > 1)
    {
        return testFunc(iter - 1, ref);
    }

    auto cs = callstackutility::CallStack::Capture();
    for (auto const& f : cs) std::cout << "\t" << f << "\n";

    return static_cast<int>(cs.size() - ref.size());
}

int testFunc3(int iter, callstackutility::CallStack const& ref);

int testFunc2(int iter, callstackutility::CallStack const& ref)
{
    if (iter > 1)
    {
        return testFunc3(iter - 1, ref);
    }

    auto cs = callstackutility::CallStack::Capture();
    for (auto const& f : cs) std::cout << "\t" << f << "\n";

    return static_cast<int>(cs.size() - ref.size());
}

int testFunc3(int iter, callstackutility::CallStack const& ref)
{
    return testFunc2(iter, ref);
}

int main()
{
    auto mainCS = callstackutility::CallStack::Capture();

    std::cout << "Callstack from main:\n";
    for (auto const& f : mainCS) std::cout << "\t" << f << "\n";

    std::cout << "Callstack from testFunc(4):\n";
    int depth = testFunc(4, mainCS);
#if defined DEBUG || _DEBUG
    if (depth == 4)
    {
        std::cout << " ... SUCCESS\n";
    }
    else
    {
        std::cerr << " ... FAIL\n";
    }
#endif

    std::cout << "Callstack from testFunc2(6):\n";
    depth = testFunc2(6, mainCS);
#if defined DEBUG || _DEBUG
    if (depth == 11) // 6x testFunc2 + 5x testFunc3
    {
        std::cout << " ... SUCCESS\n";
    }
    else
    {
        std::cerr << " ... FAIL\n";
    }
#endif

    return 0;
}
