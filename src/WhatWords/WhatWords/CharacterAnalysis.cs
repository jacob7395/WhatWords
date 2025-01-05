using System.Collections.Concurrent;
using System.Collections.Immutable;

namespace what_word;

public class CharacterAnalysis
{
    public static IEnumerable<string> GetFrequency(char character, 
                                                   WordList wordList, 
                                                   ImmutableArray<char> characterArray,
                                                   int desiredCharacterLength)
    {
        ConcurrentDictionary<string, int> frequencies = new();
        Parallel.ForEach(
            wordList.Words,
            (word) => {
                if (word.Value.Length < desiredCharacterLength) return; 
                
                var characterSpan = word.Value.AsSpan();
                var characterIndex = characterSpan.IndexOf(character);
                var wordLength = characterSpan.Length;

                if (characterIndex == -1 || wordLength == 1) return;

                for (var index = 0; index < characterSpan.Length - desiredCharacterLength; index++)
                {
                    var slice = characterSpan.Slice(index, desiredCharacterLength);
                    
                    if(slice.Contains(character) == false) continue;

                    if(SliceContainsOnlyCharacters(slice) == false) continue;
                    
                    frequencies.AddOrUpdate(slice.ToString(), c => 1, (_, c) => c + 1);
                }
            });

        foreach ((var characters, var count) in frequencies.OrderByDescending(o => o.Value))
        {
            for (int i = 0; i < count; i++)
            {
                yield return characters;
            } 
        }

        bool SliceContainsOnlyCharacters(ReadOnlySpan<char> slice)
        {
            foreach (char c in slice)
            {
                if (characterArray.Contains(c) == false) return false;
            }

            return true;
        }
    }
}