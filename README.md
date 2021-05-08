# Sudoku Solver and Analyzer

[![NuGet version (Sudoku Solver)](https://img.shields.io/nuget/v/SimpleGames.Sudoku.svg?style=flat-square)](https://www.nuget.org/packages/SimpleGames.Sudoku/)

## Demo

``` c#
      string problem =
       @".3..7....
         6..195...
         .98....6.
         8...6...3
         4..8.3..1
         7...2...6
         .6....28.
         ...419..5
         ....8..79";

      Sudoku sudoku = Sudoku.Parse(problem);

      Sudoku solution = sudoku.Solutions().First();

      Console.WriteLine(solution);
```

## Outcome

```
534678912
672195348
198342567
859761423
426853791
713924856
961537284
287419635
345286179

```
