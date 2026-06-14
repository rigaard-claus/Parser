using ParserService.ParserCore.Models;

namespace ParserService.ParserCore.Repositories
{
    public interface ITourDataRepository
    {
        Task SaveTourDataAsync(ParsedTour parsedTour, int directionId);
    }
}
