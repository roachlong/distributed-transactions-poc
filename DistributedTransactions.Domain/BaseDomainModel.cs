namespace DistributedTransactions.Domain;

public abstract class BaseDomainModel
{
    public Guid Id { get; set; }
    public DateTime CreatedOn { get; set; }
    public required string CreatedBy { get; set; }
    public DateTime ModifiedOn { get; set; }
    public required string ModifiedBy { get; set; }
}
