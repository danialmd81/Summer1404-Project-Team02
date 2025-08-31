using ETL.Application.Common;
using MediatR;

namespace ETL.Application.DataSet.RenameColumn;

public record RenameColumnCommand(string TableName, string OldColumnName, string NewColumnName) : IRequest<Result>;
