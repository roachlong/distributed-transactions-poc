namespace DistributedTransactions.Domain;

public abstract class BaseDomainModel
{
    public Guid Id { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = "system";
    public DateTime ModifiedOn { get; set; }
    public string ModifiedBy { get; set; } = "system";
}
