#pragma once

#include <string>

struct CmdLineArgs
{
    enum class Command {
        Unknown,
        List,
        Toggle,
        Enable,
        Disable,
    };

    Command command;
    std::wstring id;

    bool Parse(int argc, const wchar_t* argv[]);
    void PrintHelp();
};
