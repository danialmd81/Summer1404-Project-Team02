using MediatR;

namespace ETL.Application.Data.RenameColumn;

public record RenameColumnCommand(string TableName, string OldColumnName, string NewColumnName) : IRequest<Unit>;
