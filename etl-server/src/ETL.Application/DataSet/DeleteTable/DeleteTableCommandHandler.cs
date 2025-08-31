using ETL.Application.Abstractions.Data;
using MediatR;

namespace ETL.Application.DataSet.DeleteTable;

public class DeleteTableCommandHandler : IRequestHandler<DeleteTableCommand, Unit>
{
    private readonly IUnitOfWork _uow;

    public DeleteTableCommandHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<Unit> Handle(DeleteTableCommand request, CancellationToken cancellationToken)
    {
        _uow.Begin();

        try
        {
            // Drop physical table
            await _uow.DynamicTables.DeleteTableAsync(request.TableName, cancellationToken);

            // Remove metadata record
            var dataSet = await _uow.DataSets.GetByTableNameAsync(request.TableName, cancellationToken);
            if (dataSet != null)
            {
                await _uow.DataSets.DeleteAsync(dataSet, cancellationToken);
            }

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