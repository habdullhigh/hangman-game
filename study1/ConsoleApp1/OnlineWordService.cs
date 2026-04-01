using System.Text.Json;

namespace ConsoleApp1;

class OnlineWordService
{
    private static readonly HttpClient Http = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    private const string HealthCheckUrl = "https://www.google.com";
    private const string RandomWordApiBaseUrl = "https://random-word-api.vercel.app/api?words=";

    public async Task<bool> IsOnlineAsync()
    {
        try
        {
            using HttpRequestMessage request = new(HttpMethod.Head, HealthCheckUrl);
            using HttpResponseMessage response = await Http.SendAsync(request);
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    public async Task<string?> TryGetRandomWordForDifficultyAsync(string difficulty)
    {
        try
        {
            const int sampleSize = 10;
            using HttpResponseMessage response = await Http.GetAsync($"{RandomWordApiBaseUrl}{sampleSize}");
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            string payload = await response.Content.ReadAsStringAsync();
            string[]? words = JsonSerializer.Deserialize<string[]>(payload);
            if (words is null || words.Length == 0)
            {
                return null;
            }

            List<string> cleanedWords = new();
            foreach (string rawWord in words)
            {
                string cleaned = rawWord.Trim().ToLowerInvariant();
                if (string.IsNullOrWhiteSpace(cleaned))
                {
                    continue;
                }

                bool valid = true;
                foreach (char c in cleaned)
                {
                    if (!char.IsLetter(c))
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    cleanedWords.Add(cleaned);
                }
            }

            if (cleanedWords.Count == 0)
            {
                return null;
            }

            List<string> lengthMatched = cleanedWords
                .Where(word => MatchesDifficultyLength(word, difficulty))
                .ToList();

            List<string> candidates = lengthMatched.Count > 0 ? lengthMatched : cleanedWords;

            // Easy/medium pick the simplest candidate. Hard picks the least simple candidate.
            return difficulty.ToLowerInvariant() switch
            {
                "hard" => candidates.OrderBy(ComputeSimplicityScore).FirstOrDefault(),
                _ => candidates.OrderByDescending(ComputeSimplicityScore).FirstOrDefault()
            };
        }
        catch
        {
            return null;
        }
    }

    private static bool MatchesDifficultyLength(string word, string difficulty)
    {
        int len = word.Length;
        return difficulty.ToLowerInvariant() switch
        {
            "easy" => len >= 4 && len <= 8,
            "medium" => len >= 6 && len <= 8,
            "hard" => len >= 8,
            _ => false
        };
    }

    private static double ComputeSimplicityScore(string word)
    {
        HashSet<char> commonLetters = new("etaoinshrdlu");
        HashSet<char> rareLetters = new("jqxz");

        int commonCount = 0;
        int rareCount = 0;
        int vowelCount = 0;
        HashSet<char> unique = new();

        foreach (char c in word)
        {
            unique.Add(c);
            if ("aeiou".Contains(c))
            {
                vowelCount++;
            }

            if (commonLetters.Contains(c))
            {
                commonCount++;
            }

            if (rareLetters.Contains(c))
            {
                rareCount++;
            }
        }

        double length = word.Length;
        double commonRatio = commonCount / length;
        double rareRatio = rareCount / length;
        double vowelRatio = vowelCount / length;
        double uniqueRatio = unique.Count / length;

        return (commonRatio * 0.45) + (vowelRatio * 0.30) + ((1.0 - uniqueRatio) * 0.25) - (rareRatio * 0.60);
    }
}
