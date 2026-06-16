using ParserService.ParserCore.Models;
using ParserService.ParserCore.Models.Common;

namespace ParserService.Application.Models.Answers
{
    public record GetOperatorsAnswer  (List<Operator> Operators) : ParserServiceResponse;
}
