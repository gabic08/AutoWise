namespace AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;

public interface IModifiedCreatedAuditBaseEntity : ICreatedAuditBaseEntity
{
    Guid? LastModifiedByUserId { get; set; }
    DateTime? LastModifiedOn { get; set; }
}

public interface IModifiedCreatedAuditBaseEntity<TUser> : IModifiedCreatedAuditBaseEntity, ICreatedAuditBaseEntity<TUser>
    where TUser : class
{
    TUser LastModifiedByUser { get; set; }
}