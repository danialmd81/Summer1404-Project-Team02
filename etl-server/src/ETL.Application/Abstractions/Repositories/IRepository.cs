using System.Data;

namespace ETL.Application.Abstractions.Repositories;

public interface IRepository
{
    void SetTransaction(IDbTransaction? transaction);
}