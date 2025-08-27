using MediatR;

namespace ETL.Application.Data.RenameTable;

public record RenameTableCommand(string OldTableName, string NewTableName) : IRequest<Unit>;