using ETL.Application.Abstractions.Data;
using ETL.Domain.Entities;
using MediatR;

namespace ETL.Application.Data.UploadCsv;


public class UploadCsvCommandHandler : IRequestHandler<UploadCsvCommand, Guid>
{
    private readonly IUnitOfWork _uow;


    public UploadCsvCommandHandler(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<Guid> Handle(UploadCsvCommand request, CancellationToken cancellationToken)
    {
        // 1. Check if table already exists
        var existing = await _uow.DataSets.GetByTableNameAsync(request.TableName, cancellationToken);
        if (existing != null)
        {
            throw new InvalidOperationException($"Table '{request.TableName}' already exists.");
        }
        
        _uow.Begin();

        try
        {
            // create domain entity
            var dataSet = new DataSetMetadata(request.TableName, request.UserId);

            // create table & import CSV (cancellation supported inside)
            await _uow.DynamicTables.CreateTableFromCsvAsync(request.TableName, request.FileStream, cancellationToken);

            // save metadata
            await _uow.DataSets.AddAsync(dataSet, cancellationToken);
    
            _uow.Commit();
            return dataSet.Id;
        }
        catch
        {
            _uow.Rollback();
            throw;
        }
        
        
    }
}
