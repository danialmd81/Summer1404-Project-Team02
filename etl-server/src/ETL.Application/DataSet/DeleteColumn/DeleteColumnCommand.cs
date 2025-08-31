using MediatR;

namespace ETL.Application.DataSet.DeleteColumn;

public record DeleteColumnCommand(string TableName, string ColumnName) : IRequest<Unit>;