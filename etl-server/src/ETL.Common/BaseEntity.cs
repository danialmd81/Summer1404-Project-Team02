namespace ETL.Common;
public abstract class BaseEntity
{
    public long Id { get; protected set; }
    public DateTime CreationDate { get; private set; }

    public BaseEntity()
    {
        CreationDate = DateTime.Now;
    }
}

