using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.RegularExpressions;
using MoreLinq;
using Spectre.Console;
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
        public int GenerateCount { get; init; } = 100;

        [CommandOption("-n|--character-count <COUNT>")]
        public int CharacterCount { get; init; } = 3;

        [CommandArgument(1, "[focuse character]")]
        public char FocusCharacters { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var erWordlist = await WordList.Load("common_words.txt");

        if (erWordlist.IsError || erWordlist.Value is not {} wordList)
        {
            return 1;
        }

        var charactersString = settings.IncludedCharacters;
        var characterArray = Regex.Split(charactersString, " ")
                                  .Where(c => c.Length == 1)
                                  .Select(s => s[0])
                                  .ToImmutableArray();

        var analysis = CharacterAnalysis.GetFrequency(
            settings.FocusCharacters,
            wordList,
            characterArray,
            settings.CharacterCount).ToImmutableArray();

        if (analysis.Length == 0) return 1;

        while (analysis.Length < settings.GenerateCount)
        {
            analysis = analysis.AddRange(analysis);
        }
        
        var outputStringBuilder = new StringBuilder();

        outputStringBuilder.AppendJoin(' ', analysis[..settings.GenerateCount].Shuffle());

        await File.WriteAllTextAsync("output.txt", outputStringBuilder.ToString());

        return 0;
    }
}