namespace ETL.Domain.Entities;

public class DataSetMetadata
{
    public Guid Id { get; private set; }
    public string TableName { get; private set; }
    public string UploadedByUserId { get; private set; }
    public DateTime UploadedAt { get; private set; }

    private DataSetMetadata() { } // for serializers/ORMs

    // Public constructor for NEW
    public DataSetMetadata(string tableName, string uploadedByUserId)
    {
        Id = Guid.NewGuid();
        TableName = tableName;
        UploadedByUserId = uploadedByUserId;
        UploadedAt = DateTime.UtcNow;
    }

    public DataSetMetadata(Guid id, string tableName, string uploadedByUserId, DateTime uploadedAt)
    {
        Id = id;
        TableName = tableName;
        UploadedByUserId = uploadedByUserId;
        UploadedAt = uploadedAt;
    }

    public void SetUserFriendlyName(string newName)
    {
        TableName = newName;
    }
}