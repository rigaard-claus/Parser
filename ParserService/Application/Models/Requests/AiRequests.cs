using ParserService.Application.Models.Base;

namespace ParserService.Application.Models.Requests
{
    public record AiRequests
    {
        public record GetGlobalStatsRequest();
        public record GetUserHistoryRequest(Guid UserGuid, bool GetAllHistory);
        public record GetUsersRequest: PagingRequest { };
    }
}
