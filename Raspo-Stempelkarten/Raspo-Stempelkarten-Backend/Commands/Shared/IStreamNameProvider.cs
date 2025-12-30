namespace Raspo_Stempelkarten_Backend.Commands.Shared;

public interface IStreamNameProvider
{
    string GetStreamName(string season, string team);
}