using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParserService.Application.Messaging;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Messages;
using ParserService.Application.Models.Requests;
using ParserService.Data.Contexts;
using ParserService.ParserCore.Models;
using System.Data;

namespace ParserService.Application.Handlers.Operators
{
    public class GetOperatorsHandler(DbTourParser tourParser, IMapper mapper, INatsBus natsBus)
    {
        public async Task<GetOperatorsAnswer> HandleAsync(GetOperatorsRequest request)
        {
            var result = new GetOperatorsAnswer(new List<Operator>());
            try
            {
                var entities = await tourParser.Operators
                    .OrderBy(o => o.Priority)
                    .ToListAsync();
                result = new GetOperatorsAnswer(mapper.Map<List<Operator>>(entities));
            }
            catch (Exception ex)
            {
                await natsBus.PublishErrorAsync(new LogErrorRequest(
                    ex.Message,
                    ex.StackTrace ?? "No stack trace",
                    DateTime.UtcNow));

                return new GetOperatorsAnswer(new List<Operator>())
                {
                    Success = false,
                    Error = $"An error occurred while retrieving operators: {ex.Message}"
                };
            }

            return result;
        }
    }
}
