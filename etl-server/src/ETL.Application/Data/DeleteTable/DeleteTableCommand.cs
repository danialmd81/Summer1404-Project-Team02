using MediatR;

namespace ETL.Application.Data.DeleteTable;

public record DeleteTableCommand(string TableName) : IRequest<Unit>;
