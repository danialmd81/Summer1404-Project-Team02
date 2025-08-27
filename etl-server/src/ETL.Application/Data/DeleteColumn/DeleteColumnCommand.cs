using MediatR;

namespace ETL.Application.Data.DeleteColumn;

public record DeleteColumnCommand(string TableName, string ColumnName) : IRequest<Unit>;