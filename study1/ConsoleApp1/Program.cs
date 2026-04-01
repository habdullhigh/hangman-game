namespace ConsoleApp1;

/*
I promise this is not mostly AI I just Like to comment a lot
and I like to have a plan before I start coding


1. Project Write a simple Hangman Game in C#

My approach to this im Going to start with a simple console app
1. theres gonna be an offline word list that the game will pull from IN DIFFERENT CATEGORIES
2. Might add an online pull from a random word generator API if I have the time might do THAT ON THE SIDE
3. THAT WILL BASICALLY JUST REQUIRE A CHECK FOR IF THERES A NETWORK CONNECTION MAYBE USING CURL TO IF THE ERROR SHOWS CONNECTION ERROR TEN BACK TO OFFLINE IF NOT USE ONLINE WORD  ----SIDE CONSIDERATION THOUGH
2. the player will select a category and the game will randomly select a word from that category
3. the player will have a certain number of guesses to figure out the word lets say 6 to 10 guesses we can have different levels of difficulty that will determine the number of guesses
4. the game will display the word with blanks for the letters that have not been guessed yet
5. the player will input a letter and the game will check if that letter is in the word and update the display accordingly
6. if the player guesses the word before running out of guesses they win, otherwise they lose and the game will reveal the word
7. the game will also keep track of the player's score and allow them to play multiple rounds if they want to
*/

/*
ALGORITHM
1. Create a list of words for each category
2. Prompt the player to select a category
3. validate the player's input to ensure they select a valid category, if not prompt them again until they do
4. Prompt the player to select a difficulty level which will determine the number of guesses
5. validate the player's input to ensure they select a valid difficulty level, if not prompt them again until they do
6. Randomly select a word from the chosen category
7. START THE GAME LOOP WITH A CERTAIN NUMBER OF GUESSES depending on the difficulty level and a Score variable initialized to 0
8. Display the word with blanks for unguessed letters
9. Prompt the player to input a letter
10. validate the player's input to ensure they input a valid letter, if not prompt them again until they do
11. Check if the letter is in the word and update the display accordingly
12. If the player runs out of guesses, reveal the word and display a loss message
13. If the player guesses the word correctly, display a win message and add to their score based on the difficulty level`
14. Ask the player if they want to play another round
15. If yes, reset the game add and start a new round; if no, end the game
*/

//Gentlemen Shall We?!

//HANGMAN GAME IN C#

class GameGraphics
{
    // 11-stage array — covers every body part for up to 10 wrong guesses.
    // Index 0 = empty gallows, index 10 = fully dead.
    // Stages added in order: head, upper body, lower body,
    //   left arm, right arm, left leg, right leg, left foot, right foot, X eyes.
    //THE ASCII ART WAS SOURCED ONLINE AND I ADJUSTED IT TO FIT MY NEEDS
    static readonly string[] HangmanStages = new string[]
    {
        @"
  +---+
  |   |
      |
      |
      |
      |
=========",
        @"
  +---+
  |   |
  O   |
      |
      |
      |
=========",
        @"
  +---+
  |   |
  O   |
  |   |
      |
      |
=========",
        @"
  +---+
  |   |
  O   |
  |   |
  |   |
      |
=========",
        @"
  +---+
  |   |
  O   |
 /|   |
  |   |
      |
=========",
        @"
  +---+
  |   |
  O   |
 /|\  |
  |   |
      |
=========",
        @"
  +---+
  |   |
  O   |
 /|\  |
 /|   |
      |
=========",
        @"
  +---+
  |   |
  O   |
 /|\  |
 / \  |
      |
=========",
        @"
  +---+
  |   |
  O   |
 /|\  |
 / \  |
|     |
=========",
        @"
  +---+
  |   |
  O   |
 /|\  |
 / \  |
|   | |
=========",
        @"
  +---+
  |   |
  X   |
 /|\  |
 / \  |
|   | |
========="
    };

    public static string[] GetStages(string difficulty)
    {
        int maxGuesses = difficulty.ToLowerInvariant() switch
        {
            "easy" => 6,
            "medium" => 8,
            "hard" => 10,
            _ => throw new ArgumentException("Unknown difficulty.")
        };

        int stagesNeeded = maxGuesses + 1;
        int last = HangmanStages.Length - 1;

        string[] result = new string[stagesNeeded];
        for (int i = 0; i < stagesNeeded; i++)
        {
            int idx = (int)Math.Round((double)i * last / (stagesNeeded - 1), MidpointRounding.AwayFromZero);
            result[i] = HangmanStages[idx];
        }

        return result;
    }
}

class WordBank
{
    private readonly Dictionary<string, List<string>> _bank = new(StringComparer.OrdinalIgnoreCase)
    {
        { "Animals", new List<string> { "elephant", "giraffe", "penguin", "crocodile", "kangaroo", "panther" } },
        { "Countries", new List<string> { "nigeria", "brazil", "iceland", "portugal", "argentina", "thailand" } },
        { "Science", new List<string> { "gravity", "nucleus", "electron", "hydrogen", "quantum", "oxygen" } }
    };

    public List<string> GetCategories() => new(_bank.Keys);

    public string GetRandomWord(string category)
    {
        List<string> words = _bank[category];
        return words[Random.Shared.Next(words.Count)];
    }
}

class GameLogic
{
    public static string GetDisplayWord(string word, HashSet<char> guessedLetters)
    {
        char[] displayChars = new char[word.Length];
        for (int i = 0; i < word.Length; i++)
        {
            char c = word[i];
            displayChars[i] = guessedLetters.Contains(c) ? c : '_';
        }

        return new string(displayChars);
    }

    public static bool CheckWin(string word, HashSet<char> guessedLetters)
    {
        foreach (char c in word)
        {
            if (!guessedLetters.Contains(c))
            {
                return false;
            }
        }

        return true;
    }
}

class Program
{
    static async Task Main()
    {
        WordBank wordBank = new();
        OnlineWordService onlineWordService = new();

        Console.WriteLine("Welcome to Hangman!");
        Console.WriteLine("Try to guess the word one letter at a time.");

        bool playAgain = true;
        while (playAgain)
        {
            string difficulty = PromptDifficulty();
            string[] stages = GameGraphics.GetStages(difficulty);
            int maxWrongGuesses = stages.Length - 1;

            (string secretWord, string sourceMessage) = await ResolveWordAsync(onlineWordService, wordBank, difficulty);

            Console.WriteLine();
            Console.WriteLine(sourceMessage);

            HashSet<char> guessedLetters = new();
            int wrongGuesses = 0;

            while (wrongGuesses < maxWrongGuesses)
            {
                Console.WriteLine();
                Console.WriteLine(stages[wrongGuesses]);
                Console.WriteLine($"Word: {GameLogic.GetDisplayWord(secretWord, guessedLetters)}");
                Console.WriteLine($"Wrong guesses left: {maxWrongGuesses - wrongGuesses}");
                Console.Write("Guess a letter: ");

                char guess = ReadSingleLetter();

                if (!guessedLetters.Add(guess))
                {
                    Console.WriteLine("You already guessed that letter.");
                    continue;
                }

                if (secretWord.Contains(guess))
                {
                    Console.WriteLine("Nice! Correct guess.");
                    if (GameLogic.CheckWin(secretWord, guessedLetters))
                    {
                        Console.WriteLine();
                        Console.WriteLine($"You won! The word was '{secretWord}'.");
                        break;
                    }
                }
                else
                {
                    wrongGuesses++;
                    Console.WriteLine("Not in the word.");
                }
            }

            if (!GameLogic.CheckWin(secretWord, guessedLetters))
            {
                Console.WriteLine();
                Console.WriteLine(stages[maxWrongGuesses]);
                Console.WriteLine($"You lost. The word was '{secretWord}'.");
            }

            Console.WriteLine();
            Console.Write("Play again? (y/n): ");
            string? input = Console.ReadLine()?.Trim().ToLowerInvariant();
            playAgain = input == "y" || input == "yes";
        }

        Console.WriteLine("Thanks for playing!");
    }

    private static async Task<(string Word, string SourceMessage)> ResolveWordAsync(
        OnlineWordService onlineWordService,
        WordBank wordBank,
        string difficulty)
    {
        string sourceMessage = "[WORD SOURCE] DICTIONARY WORD (local word bank).";

        bool isOnline = await onlineWordService.IsOnlineAsync();
        if (isOnline)
        {
            // THE ONLINE WORD SERVICE WAS MY IDEA THE IMPLEMENTATION WAS ASSISTED BY AI BUT I WROTE THE ALGORITHM AND THE STRUCTURE OF THE CLASS
            string? onlineWord = await onlineWordService.TryGetRandomWordForDifficultyAsync(difficulty);
            if (!string.IsNullOrWhiteSpace(onlineWord))
            {
                sourceMessage = $"[WORD SOURCE] ONLINE WORD (API, {difficulty} filter).";
                return (onlineWord, sourceMessage);
            }
        }

        string category = PromptCategory(wordBank.GetCategories());
        return (wordBank.GetRandomWord(category), sourceMessage);
    }

    private static string PromptDifficulty()
    {
        while (true)
        {
            Console.WriteLine();
            Console.Write("Choose difficulty (easy/medium/hard): ");
            string? input = Console.ReadLine()?.Trim().ToLowerInvariant();

            if (input is "easy" or "medium" or "hard")
            {
                return input;
            }

            Console.WriteLine("Invalid difficulty. Try again.");
        }
    }

    private static string PromptCategory(List<string> categories)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine("Choose a category:");
            for (int i = 0; i < categories.Count; i++)
            {
                Console.WriteLine($"{i + 1}. {categories[i]}");
            }

            Console.Write("Enter category number: ");
            string? input = Console.ReadLine()?.Trim();

            if (int.TryParse(input, out int choice) && choice >= 1 && choice <= categories.Count)
            {
                return categories[choice - 1];
            }

            Console.WriteLine("Invalid choice. Try again.");
        }
    }

    private static char ReadSingleLetter()
    {
        while (true)
        {
            string? input = Console.ReadLine()?.Trim().ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(input) && input.Length == 1 && char.IsLetter(input[0]))
            {
                return input[0];
            }

            Console.Write("Please enter a single letter: ");
        }
    }
}
