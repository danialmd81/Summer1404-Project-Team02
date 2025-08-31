using ETL.Application.Common;
using MediatR;

namespace ETL.Application.DataSet.DeleteTable;

public record DeleteTableCommand(string TableName) : IRequest<Result>;
