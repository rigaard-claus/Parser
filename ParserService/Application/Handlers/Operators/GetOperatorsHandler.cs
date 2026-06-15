using AutoMapper;
using Microsoft.EntityFrameworkCore;
using ParserService.Application.Models.Answers;
using ParserService.Application.Models.Requests;
using ParserService.Application.Services;
using ParserService.Data.Contexts;
using ParserService.Data.Entities;
using ParserService.ParserCore.Models;

namespace ParserService.Application.Handlers.Operators
{
    public class GetOperatorsHandler(DbTourParser tourParser, OperatorConfigurationService syncService, IMapper mapper)
    {
        public async Task<GetOperatorsAnswer> HandleAsync(GetOperatorsRequest request)
        {
            await syncService.SyncOperatorsWithConfigAsync();

            var entities = await tourParser.Operators.ToListAsync();

            if (!entities.Any())
            {
                tourParser.Operators.Add(new OperatorEntity { Name = "Dertour", Priority = 1 });
                await tourParser.SaveChangesAsync();
                entities = await tourParser.Operators.ToListAsync();
            }

            return new GetOperatorsAnswer(mapper.Map<List<Operator>>(entities));
        }
    }
}
