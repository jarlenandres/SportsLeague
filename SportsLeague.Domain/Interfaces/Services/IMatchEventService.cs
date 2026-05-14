using SportsLeague.Domain.Entities;

namespace SportsLeague.Domain.Interfaces.Services;

public interface IMatchEventService
{
    // MatchResult
    Task<MatchResult> RegisterResultAsync(int matchId, MatchResult result);
    Task<MatchResult?> GetResultMatchAsync(int matchId);

    // Goal
    Task<Goal> RegisterGoalAsync(int matchId, Goal goal);
    Task<IEnumerable<Goal>> GetGoalsByMatchAsync(int matchId);
    Task DeleteGoalAsync(int goalId);

    // Card
    Task<Card> RegisterCardAsync(int matchId, Card card);
    Task<IEnumerable<Card>> GetCardsByMatchAsync(int matchId);
    Task DeleteCardAsync(int cardId);
}
