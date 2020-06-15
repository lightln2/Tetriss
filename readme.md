# Effective Tetris implelentation using Bitboards

- Console-based implementation of game of Tetris using 256-bit bitboards and AVX-2 SIMD instructions
- Tetris solver using greedy in-depth search processing up to 15 million moves per second
- Replay scenarios found by the solver

<img src="https://github.com/lightln2/Tetriss/blob/master/replay-58.gif" />

## Build
on Windows, open solution with Visual Studio 2019 or run `build.cmd`
on other systems, run `dotnet build -c Release -r [your-system]`

## Run
on Windows:
- Start the game in the console:
```
    tetriss.exe
```    
- Start tetris solver with a specified depth. Found scenarios will be printed out to the console:
```
    tetriss.exe -solve 1000
```
- Replay scenario found by the solver:
```
    tetriss.exe -replay "0:0 0:0 1:2 0:0 2:-3 3:-4 0:4"
```
on other systems: run `dotnet run -c Release -p Tetriss [arguments]`

## License

- **[MIT license](https://github.com/lightln2/Tetriss/blob/master/license.txt)**
- Copyright 2020 (c) lightln2
