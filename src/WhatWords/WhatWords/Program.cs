using System.Collections.Immutable;
using System.ComponentModel;
using System.Text;
using System.Text.RegularExpressions;
using MoreLinq;
using Spectre.Console.Cli;
using what_word;

var app = new CommandApp<CommonLettersCommand>();
return app.Run(args);

internal sealed class CommonLettersCommand : AsyncCommand<CommonLettersCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("A space seperated list of characters to include")]
        [CommandArgument(0, "[included characters]")]
        public string IncludedCharacters { get; set; }

        [CommandOption("-c|--generate-count <COUNT>")]
        public int GenerateCount { get; init; } = 200;

        [CommandOption("-n|--character-count <COUNT>")]
        public int CharacterCount { get; init; } = 3;

        [CommandArgument(1, "[focus character]")]
        public string FocusCharacters { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        Console.WriteLine("Running");

        var erWordlist = await WordList.Load("frequent_words.txt");

        if (erWordlist.IsError || erWordlist.Value is not {} wordList)
        {
            return 1;
        }

        var charactersString = settings.IncludedCharacters;
        var characterArray = Regex.Split(charactersString, " ")
                                  .Where(c => c.Length == 1)
                                  .Select(s => s[0])
                                  .ToImmutableArray();

        Dictionary<char, ImmutableArray<string>> words = new();

        foreach (char character in characterArray)
        {
            var analysis = CharacterAnalysis.GetFrequency(
                                                character,
                                                wordList,
                                                characterArray,
                                                settings.CharacterCount)
                                            .ToImmutableArray();

            words.Add(character, analysis);
        }

        List<string> output = new();
        Dictionary<char, int> counters = new();

        characterArray = [..settings.FocusCharacters.Split(' ').Select(s => s[0])];
        
        while (output.Count < settings.GenerateCount)
            foreach (char c in characterArray)
            {
                if (output.Count >= settings.GenerateCount)
                {
                    break;
                }

                var index = counters.GetValueOrDefault(c);
                var characterWordList = words.GetValueOrDefault(c);

                if (characterWordList.IsEmpty)
                {
                    continue;
                }

                if (characterWordList.Length <= index)
                {
                    index = 0;
                }
                
                output.Add(characterWordList[index++]);
                counters[c] = index;
            }

        var outputStringBuilder = new StringBuilder();

        outputStringBuilder.AppendJoin(' ', output.Shuffle());

        Console.Write(outputStringBuilder.ToString());

        await File.WriteAllTextAsync("output.txt", outputStringBuilder.ToString());

        return 0;
    }
}
