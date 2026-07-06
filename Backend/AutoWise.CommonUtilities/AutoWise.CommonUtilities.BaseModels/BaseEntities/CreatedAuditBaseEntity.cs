using AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;

namespace AutoWise.CommonUtilities.Models.BaseEntities;

public abstract class CreatedAuditBaseEntity : IdBaseEntity, ICreatedAuditBaseEntity
{
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedOn { get; set; }
}

public abstract class CreatedAuditBaseEntity<TUser> : CreatedAuditBaseEntity, ICreatedAuditBaseEntity<TUser>
    where TUser : class
{
    public TUser CreatedByUser { get; set; }
}