using ETL.Application.Common;
using MediatR;

namespace ETL.Application.DataSet.RenameTable;

public record RenameTableCommand(string OldTableName, string NewTableName) : IRequest<Result>;