using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace what_word;

public class WordList
{
    private ImmutableArray<Word> _words;

    private WordList(ImmutableArray<Word> words)
    {
        _words = words;
    }

    public static async Task<ErrorOr<WordList>> Load(string path)
    {
        try
        {
            var wordBag = new ConcurrentBag<Word>();

            await Parallel.ForEachAsync(
                File.ReadLinesAsync(path),
                (line, token) => {
                    var word = new Word(line);

                    wordBag.Add(word);

                    return ValueTask.CompletedTask;
                });

            return new WordList([..wordBag]);
        }
        catch (IOException e)
        {
            return Error.Unexpected("FailedToReadWords");
        }
    }

    public record Word
    {
        public Word(string Value)
        {
            this.Value = Value.ToLower().Trim();
        }

        public bool WithinRange(int settingsMinCharacters, int? settingsMaxCharacters)
        {
            if (settingsMaxCharacters is not null)
            {
                return Value.Length >= settingsMinCharacters && Value.Length <= settingsMaxCharacters;
            }

            return Value.Length > settingsMaxCharacters;
        }

        public string Value { get; init; }

        public void Deconstruct(out string Value)
        {
            Value = this.Value;
        }
    }

    public ImmutableArray<string> MatchWords(ImmutableArray<char> characterArray, int settingsMinCharacters,
                                             int? settingsMaxCharacters)
    {
        var matchBag = new ConcurrentBag<Word>();

        Parallel.ForEach(
            _words.Where(w => w.WithinRange(settingsMinCharacters, settingsMaxCharacters)),
            word => {
                foreach (var character in word.Value)
                {
                    if (characterArray.Contains(character) == false)
                    {
                        return;
                    }
                }

                matchBag.Add(word);
            });

        return [..matchBag.Select(w => w.Value)];
    }
}