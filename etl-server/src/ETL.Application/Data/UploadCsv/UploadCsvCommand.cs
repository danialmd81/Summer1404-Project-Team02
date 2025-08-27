using MediatR;

namespace ETL.Application.Data.UploadCsv;

public record UploadCsvCommand(string TableName, Stream FileStream, string UserId) : IRequest<Guid>;
