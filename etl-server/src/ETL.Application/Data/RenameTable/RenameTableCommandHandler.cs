using ETL.Application.Abstractions.Data;
using MediatR;

namespace ETL.Application.Data.RenameTable;

public class RenameTableCommandHandler : IRequestHandler<RenameTableCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public RenameTableCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Unit> Handle(RenameTableCommand request, CancellationToken cancellationToken)
    {
        _uow.Begin();

        try
        {
            // Rename physical table
            await _uow.DynamicTables.RenameTableAsync(request.OldTableName, request.NewTableName, cancellationToken);

            // Update metadata record too
            var dataSet = await _uow.DataSets.GetByTableNameAsync(request.OldTableName, cancellationToken);
            if (dataSet == null)
                throw new InvalidOperationException($"Dataset '{request.OldTableName}' not found in metadata.");

            dataSet.Rename(request.NewTableName); // assume your entity has Rename method or set TableName
            await _uow.DataSets.UpdateAsync(dataSet, cancellationToken);

            _uow.Commit();
            return Unit.Value;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
    }
}
