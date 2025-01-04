using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using MoreLinq;
using Spectre.Console;
using Spectre.Console.Cli;
using what_word;

var app = new CommandApp<FileSizeCommand>();
return app.Run(args);

internal sealed class FileSizeCommand : AsyncCommand<FileSizeCommand.Settings>
{
    public sealed class Settings : CommandSettings
    {
        [Description("A space seperated list of characters to include")]
        [CommandArgument(0, "[included characters]")]
        public string IncludedCharacters { get; set; }
            
        [CommandOption("-m|--max-characters <COUNT>")]
        public int? MaxCharacters { get; init; }

        [CommandOption("--min-characters <COUNT>")]
        public int MinCharacters { get; init; } = 0;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var erWordlist = await WordList.Load("words.txt");

        if (erWordlist.IsError || erWordlist.Value is not {} wordList)
        {
            return 1;
        }
        
        var charactersString = settings.IncludedCharacters;
        var characterArray = Regex.Split(charactersString, " ").Where(c => c.Length == 1).Select(s => s[0]).ToImmutableArray(); 
        
        var matchingWords = wordList.MatchWords(characterArray, settings.MinCharacters, settings.MaxCharacters);
        var matchedWordString = string.Join(" ", matchingWords.Shuffle());
        
        await File.WriteAllTextAsync("output.txt", matchedWordString);
        
        return 0;
    } 
}