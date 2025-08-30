using FluentValidation;

namespace Raspo_Stempelkarten_Backend.Model;

public class StempelkartenAggregateValidator : AbstractValidator<StempelkartenAggregate>
{
    public StempelkartenAggregateValidator()
    {
        RuleFor(aggregate => aggregate.Team).NotEmpty().NotNull().Matches(@".+ \d{4}");
        RuleFor(aggregate => aggregate.Season).NotEmpty().NotNull().Matches(@"\d{4}/\d{2}");
        RuleFor(aggregate => aggregate.Stempelkarten).Custom((stempelkarten, context) =>
        {
            var stempelkartenGroupedByRecipientName = stempelkarten.GroupBy(stempelkarte => 
                stempelkarte.Recipient,
                (recipient, enumerable) => new KeyValuePair<string, int>(recipient, enumerable.Count()));
            var duplicates = stempelkartenGroupedByRecipientName.Where(pair => pair.Value > 1).ToList();
            if (duplicates.Count == 0) return;
            foreach (var pair in duplicates)
            {
                context.AddFailure($"Doppelter Empfänger (Recipient) gefunden: '{pair.Key}'");    
            }
        });
        RuleForEach(aggregate => aggregate.Stempelkarten).ChildRules(stempelkarteRules =>
        {
            stempelkarteRules.RuleFor(stempelkarte => stempelkarte.Recipient).NotEmpty().NotNull();
            //stempelkarteRules.RuleFor(stempelkarte => stempelkarte.Owner).NotEmpty().NotNull();
            stempelkarteRules.RuleFor(stempelkarte => stempelkarte.MaxStamps).GreaterThanOrEqualTo(stempelkarte => stempelkarte.MinStamps).GreaterThan(0);
            stempelkarteRules.RuleFor(stempelkarte => stempelkarte.MinStamps).LessThanOrEqualTo(stempelkarte => stempelkarte.MaxStamps).GreaterThan(0);
            stempelkarteRules.RuleFor(stempelkarte => stempelkarte.Stamps).Custom((stamps, context) =>
            {
                var stampCount = stamps.ToList().Count;
                if (stampCount > context.InstanceToValidate.MaxStamps)
                {
                    context.AddFailure($"Maximale Anzahl an Stempel ausgeschöpft: {stampCount}");
                }
            });
            stempelkarteRules.RuleForEach(stempelkarte => stempelkarte.Stamps)
                .ChildRules(stampRules =>
            {
                stampRules.RuleFor(stamp => stamp.IssuedBy).NotEmpty().NotNull();
                stampRules.RuleFor(stamp => stamp.Reason).NotEmpty().NotNull();
            });
        });
    }
}