# Performance Calculator

A tool for calculating the difficulty of beatmaps and the performance of replays.

## Tweaking

Difficulty and performance calculators for all rulesets may be modified to tweak the output of the calculator. These exist in the following directories:

```
../osu/osu.Game.Rulesets.Osu/Difficulty
../osu/osu.Game.Rulesets.Taiko/Difficulty
../osu/osu.Game.Rulesets.Catch/Difficulty
../osu/osu.Game.Rulesets.Mania/Difficulty
```

## Usage

### Help
```
> dotnet run -- --help

Usage: dotnet PerformanceCalculator.dll [options] [command]

Options:
  -?|-h|--help  Show help information

Commands:
  difficulty    Computes the difficulty of a beatmap.
  performance   Computes the performance (pp) of replays on a beatmap.
  profile       Computes the total performance (pp) of a profile.
  simulate      Computes the performance (pp) of a simulated play.

Run 'dotnet PerformanceCalculator.dll [command] --help' for more information about a command.
```

### Difficulty
```
> dotnet run -- difficulty --help

Computes the difficulty of a beatmap.

Usage: dotnet PerformanceCalculator.dll difficulty [arguments] [options]

Arguments:
  path                       Required. A beatmap file (.osu), or a folder containing .osu files to compute the difficulty for.

Options:
  -?|-h|--help               Show help information
  -r|--ruleset:<ruleset-id>  Optional. The ruleset to compute the beatmap difficulty for, if it's a convertible beatmap.
                             Values: 0 - osu!, 1 - osu!taiko, 2 - osu!catch, 3 - osu!mania
  -m|--m <mod>               One for each mod. The mods to compute the difficulty with.Values: hr, dt, hd, fl, ez, 4k, 5k, etc...
  -o|--output <file.txt>     Output results to text file.
```

Computes the difficulty attributes of a beatmap. These attributes are used in performance calculation.

Example output of osu!-mode difficulty:
```
Aim            : 2.43558005796213
Speed          : 2.09240115454506
stars          : 4.69957066421573
```

### Performance
```
> dotnet run -- performance --help

Computes the performance (pp) of replays on a beatmap.

Usage: dotnet PerformanceCalculator.dll performance [arguments] [options]

Arguments:
  beatmap                 Required. The beatmap file (.osu) corresponding to the replays.

Options:
  -?|-h|--help            Show help information
  -r|--replay <file>      One for each replay. The replay file.
  -o|--output <file.txt>  Output results to text file.
```

Computes the performance of one or more replays on a beatmap. The provided output includes raw performance attributes alongside the un-weighted pp value.

```
Aim            : 123.614719845539
Speed          : 44.7315288123673
Accuracy       : 61.9354071284508
pp             : 235.580094436267
```

### Profile
```
> dotnet run -- profile --help

Computes the total performance (pp) of a profile.

Usage: dotnet PerformanceCalculator.dll profile [arguments] [options]

Arguments:
  user                       User ID is preferred, but username should also work.
  api key                    API Key, which you can get from here: https://old.ppy.sh/p/api

Options:
  -?|-h|--help                   Show help information
  -r|--ruleset <ruleset-id>      The ruleset to compute the profile for. 0 -
                                 osu!, 1 - osu!taiko, 2 - osu!catch, 3 -
                                 osu!mania. Defaults to osu!.
  -c|--columns <attribute_name>  Extra columns to display from beatmap category
                                 attribs, for example 'Tap Rhythm pp'. Multiple
                                 can be added at once -c col1 -c col2
  -s|--sort <attribute_name>     What column to sort by (defaults to pp of the
                                 play)
  -o|--output <file.txt>         Output results to text file.
```

Computes the performance of a user profile's performance. Takes 100 top plays of a user and recalculates and reorders them in order of the performance calculator's calculated performance.

```
User:     chocomint
Live PP:  14937,5 (including 305,6pp from playcount)
Local PP: 15310,0 (+372,5)

╔═══╤════════════════════════════════════════════════════════════════════════════════════════════════════════╤════════╤═══════╤════════╤════╤═════════╤════════╤═════════╤═══════════════╗
║ # │beatmap                                                                                                 │  mods  │live pp│  acc   │miss│  combo  │local pp│pp change│position change║
╟───┼────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────┼───────┼────────┼────┼─────────┼────────┼─────────┼───────────────╢
║1  │ 1031991 - ...s Dead Decadence - Yomi yori Kikoyu, Koukoku no Tou to Honoo no Shoujo. (DoKito) [Kyouaku]│      HD│  712,1│ 99,07 %│   4│2936/4353│   948,8│    236,8│      +19      ║
╟───┼────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────┼───────┼────────┼────┼─────────┼────────┼─────────┼───────────────╢
║2  │ 129891 - xi - FREEDOM DiVE (Nakagawa-Kanon) [FOUR DIMENSIONS]                                          │   HR|HD│  894,4│ 99,83 %│   0│2385/2385│   850,4│    -44,0│      -1       ║
╟───┼────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────┼───────┼────────┼────┼─────────┼────────┼─────────┼───────────────╢
║3  │ 1432943 - ...oshi o Kakeru Adventure ~ we are forever friends! ~ [Long ver.] (Battle) [Imagined Voyage]│   HR|HD│  764,3│ 99,41 %│   0│2336/2640│   833,4│     69,1│      +4       ║
╟───┼────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────┼───────┼────────┼────┼─────────┼────────┼─────────┼───────────────╢
║4  │ 1869619 - Umeboshi Chazuke - Bison Charge (Nao Tomori) [Extreme]                                       │      HD│  725,5│ 99,70 %│   0│  886/886│   829,9│    104,4│      +10      ║
╟───┼────────────────────────────────────────────────────────────────────────────────────────────────────────┼────────┼───────┼────────┼────┼─────────┼────────┼─────────┼───────────────╢
...
```

### LocalScores
```
> dotnet run -- localscores --help
Recalcs all of your osu local scores on ranked maps and gives you a new top 500

Usage: dotnet PerformanceCalculator.dll localscores [options]

Options:
  -?|-h|--help                   Show help information
  -u|--user <username>           Process only the replays with the given
                                 username. Multiple names can be specified at
                                 once: -u user1 -u user2
  -c|--columns <attribute_name>  Extra columns to display from beatmap category
                                 attribs, for example 'Tap Rhythm pp'. Multiple
                                 can be added at once -c col1 -c col2
  -s|--sort <attribute_name>     What column to sort by (defaults to pp of the
                                 play)
  -t|--test-run                  Only run on 20 beatmaps to test the command
                                 output
  -r|--recent-sort               Sort top 500 by replay date
  -o|--output <file.txt>         Output results to text file.
```

### Simulate
```
> dotnet run -- simulate --help

Computes the performance (pp) of a simulated play.

Usage: dotnet PerformanceCalculator.dll simulate [options] [command]

Options:
  -?|-h|--help  Show help information

Commands:
  mania         Computes the performance (pp) of a simulated osu!mania play.
  osu           Computes the performance (pp) of a simulated osu! play.
  taiko         Computes the performance (pp) of a simulated osu!taiko play.

Run 'simulate [command] --help' for more information about a command.
```

Computes the performance of a simulated play on a beatmap. The provided output includes raw performance attributes and pp value.

#### osu!
```
> dotnet run -- simulate osu --help

Computes the performance (pp) of a simulated osu! play.

Usage: dotnet PerformanceCalculator.dll simulate osu [arguments] [options]

Arguments:
  beatmap                     Required. The beatmap file (.osu).

Options:
  -?|-h|--help                Show help information
  -a|--accuracy <accuracy>    Accuracy. Enter as decimal 0-100. Defaults to 100. Scales hit results as well and is rounded to the nearest possible value for the beatmap.
  -c|--combo <combo>          Maximum combo during play. Defaults to beatmap maximum.
  -C|--percent-combo <combo>  Percentage of beatmap maximum combo achieved. Alternative to combo option. Enter as decimal 0-100.
  -m|--mod <mod>              One for each mod. The mods to compute the performance with. Values: hr, dt, hd, fl, ez, etc...
  -X|--misses <misses>        Number of misses. Defaults to 0.
  -M|--mehs <mehs>            Number of mehs. Will override accuracy if used. Otherwise is automatically calculated.
  -G|--goods <goods>          Number of goods. Will override accuracy if used. Otherwise is automatically calculated.
  -o|--output <file.txt>      Output results to text file.
```

#### osu!taiko
```
> dotnet run -- simulate taiko --help

Computes the performance (pp) of a simulated osu!taiko play.

Usage: dotnet PerformanceCalculator.dll simulate taiko [arguments] [options]

Arguments:
  beatmap                     Required. The beatmap file (.osu).

Options:
  -?|-h|--help                Show help information
  -a|--accuracy <accuracy>    Accuracy. Enter as decimal 0-100. Defaults to 100. Scales hit results as well and is rounded to the nearest possible value for the beatmap.
  -c|--combo <combo>          Maximum combo during play. Defaults to beatmap maximum.
  -C|--percent-combo <combo>  Percentage of beatmap maximum combo achieved. Alternative to combo option. Enter as decimal 0-100.
  -m|--mod <mod>              One for each mod. The mods to compute the performance with. Values: hr, dt, hd, fl, ez, etc...
  -X|--misses <misses>        Number of misses. Defaults to 0.
  -G|--goods <goods>          Number of goods. Will override accuracy if used. Otherwise is automatically calculated.
  -o|--output <file.txt>      Output results to text file.
```

#### osu!mania
```
> dotnet run -- simulate mania --help

Computes the performance (pp) of a simulated osu!mania play.

Usage: dotnet PerformanceCalculator.dll simulate mania [arguments] [options]

Arguments:
  beatmap                 Required. The beatmap file (.osu).

Options:
  -?|-h|--help            Show help information
  -s|--score <score>      Score. An integer 0-1000000.
  -m|--mod <mod>          One for each mod. The mods to compute the performance with. Values: hr, dt, fl, 4k, 5k, etc...
  -o|--output <file.txt>  Output results to text file.
```
