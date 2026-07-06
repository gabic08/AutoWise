using AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;

namespace AutoWise.CommonUtilities.Models.BaseEntities;

public abstract class ModifiedCreatedAuditBaseEntity : CreatedAuditBaseEntity, IModifiedCreatedAuditBaseEntity
{
    public Guid? LastModifiedByUserId { get; set; }
    public DateTime? LastModifiedOn { get; set; }
}

public abstract class ModifiedCreatedAuditBaseEntity<TUser> : ModifiedCreatedAuditBaseEntity, IModifiedCreatedAuditBaseEntity<TUser>
    where TUser : class
{
    public TUser CreatedByUser { get; set; }
    public TUser LastModifiedByUser { get; set; }
}
