# osu!tools [![Build status](https://ci.appveyor.com/api/projects/status/70owdbhaaepp70u5?svg=true)](https://ci.appveyor.com/project/peppy/osu-tools)  [![CodeFactor](https://www.codefactor.io/repository/github/ppy/osu-tools/badge)](https://www.codefactor.io/repository/github/ppy/osu-tools) [![dev chat](https://discordapp.com/api/guilds/188630481301012481/widget.png?style=shield)](https://discord.gg/ppy)

Tools for [osu!](https://osu.ppy.sh).

# Requirements

- A desktop platform with the [.NET Core SDK 2.2](https://dotnet.microsoft.com/download) or higher installed. Download .Net Core SDK x64 from the link! 

![](tutorial_images/dotnet_sdk_download.png?raw=true "DotNet SDK Download")



# Getting Started

- Install [Git for Windows](https://git-scm.com/download/win). 
- Open up a command window. (Win+r and type 'cmd')
- Use 'cd' command to change your directory to where you want to download osu-tools. Ex: `cd C:\Program Files\`
- Clone the repository including submodules (`git clone --recursive https://github.com/aticie/osu-tools`)
- Navigate to each tool's directory and follow the instructions listed in the tool's README.
    - [PerformanceCalculator](https://github.com/aticie/osu-tools/blob/master/PerformanceCalculator/README.md) - A tool for calculating the difficulty of beatmaps and the performance of replays.
    - [PerformanceCalculatorGUI](https://github.com/aticie/osu-tools/blob/master/PerformanceCalculatorGUI/README.md) - Same tool with GUI.
    - [StrainVisualizer](https://github.com/aticie/osu-tools/blob/master/StrainVisualizer/README.md) - Strain Visualizer tool that shows pp strain values for selected beatmap.

# Running a Tool

- Go back to command window again. 
- Change directory to whatever tool you want to build. (Ex: `cd PerformanceCalculatorGUI`)
- Enter command: `dotnet run --`
- Magic!

# Contributing

If you want to contribute my tools not peppy's, dm me on Discord. Heyronii#9925

Contributions can be made via pull requests to this repository. We hope to credit and reward larger contributions via a [bounty system](https://www.bountysource.com/teams/ppy). If you're unsure of what you can help with, check out the [list of open issues](https://github.com/ppy/osu-tools/issues).

Note that while we already have certain standards in place, nothing is set in stone. If you have an issue with the way code is structured; with any libraries we are using; with any processes involved with contributing, *please* bring it up. I welcome all feedback so we can make contributing to this project as pain-free as possible.

# Licence

The osu! client code, framework, and tools are licensed under the [MIT licence](https://opensource.org/licenses/MIT). Please see [the licence file](LICENCE) for more information. [tl;dr](https://tldrlegal.com/license/mit-license) you can do whatever you want as long as you include the original copyright and license notice in any copy of the software/source.

Please note that this *does not cover* the usage of the "osu!" or "ppy" branding in any software, resources, advertising or promotion, as this is protected by trademark law.

Please also note that game resources are covered by a separate licence. Please see the [ppy/osu-resources](https://github.com/ppy/osu-resources) repository for clarifications.
