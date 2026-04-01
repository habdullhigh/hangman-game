# Hangman Console Game (C#)

Simple Hangman game built with C# and .NET.

## Features
- Difficulty levels: `easy`, `medium`, `hard`
- ASCII hangman graphics
- Input validation for guesses
- Online-first word source:
  - Checks internet connectivity
  - Fetches random words from API
  - Applies difficulty by word length + simplicity
- Offline fallback:
  - Uses local dictionary categories (`Animals`, `Countries`, `Science`)
- Logs word source each round:
  - `ONLINE WORD`
  - `DICTIONARY WORD`

## Run
From this folder:

```bash
dotnet run
```

## Build
```bash
dotnet build
```

## Notes
- Online words are fetched from: `https://random-word-api.vercel.app`
- If API/network is unavailable, game automatically falls back to local word bank.
