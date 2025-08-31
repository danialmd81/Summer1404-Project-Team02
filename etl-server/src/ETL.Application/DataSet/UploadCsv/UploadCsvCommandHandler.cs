using ETL.Application.Abstractions.Data;
using ETL.Application.Common;
using ETL.Domain.Entities;
using MediatR;

namespace ETL.Application.DataSet.UploadCsv;


public class UploadCsvCommandHandler : IRequestHandler<UploadCsvCommand, Result>
{
    private readonly IUnitOfWork _uow;


    public UploadCsvCommandHandler(IUnitOfWork uow)
    {
        _uow = uow ?? throw new ArgumentNullException(nameof(uow));
    }

    public async Task<Result> Handle(UploadCsvCommand request, CancellationToken cancellationToken)
    {
        var existing = await _uow.DataSets.GetByTableNameAsync(request.TableName, cancellationToken);
        if (existing != null)
        {
            return Result.Failure(Error.Conflict("FileUpload.Failed", $"Table '{request.TableName}' already exists."));
        }

        _uow.Begin();

        try
        {
            var dataSet = new DataSetMetadata(request.TableName, request.UserId);

            await _uow.DynamicTables.CreateTableFromCsvAsync(request.TableName, request.FileStream, cancellationToken);

            await _uow.DataSets.AddAsync(dataSet, cancellationToken);

            _uow.Commit();
            return Result.Success();
        }
        catch (Exception ex)
        {
            _uow.Rollback();
            return Result.Failure(Error.Problem("FileUpload.Failed", ex.Message));
        }
    }
}
