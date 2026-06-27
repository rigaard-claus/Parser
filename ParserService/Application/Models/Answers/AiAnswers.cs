using ParserService.Application.Models.AI;
using ParserService.Application.Models.Base;
using ParserService.ParserCore.Models.Common;

namespace ParserService.Application.Models.Answers
{
    public record AiAnswers
    {
        public record GlobalStatsAnswer : ParserServiceResponse
        {
            public AiGlobalStats Result { get; set; }
        };

        public record UserHistoryAnswer : ParserServiceResponse 
        { 
            public List<AiRequestLog> Logs { get; set; } 
        };

        public record UserListAnswer : PagingAnswer
        {
            public List<User> Result { get; set; }
        }
    }
}
