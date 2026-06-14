using ParserService.ParserCore.Interfaces;
using ParserService.ParserCore.Models;

namespace ParserService.ParserCore.Engine.Parsers.Dertour
{
    public class DertourParser : ITourOperatorParser
    {
        public string OperatorName => "DERTOUR_DE";

        public Task<List<Region>> GetDataAsync(string countryId)
        {
            throw new NotImplementedException();
        }

        public Task<List<Country>> GetReferencesAsync()
        {
            throw new NotImplementedException();
        }
    }
}
