namespace Raspo_Stempelkarten_Backend.Commands.Shared;

public interface IStreamNameProvider
{
    string GetStreamName(string team, string season);
}