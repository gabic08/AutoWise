using System;

namespace AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;

public interface IModifiedCreatedAuditBaseEntity : ICreatedAuditBaseEntity
{
    Guid? ModifiedByUserId { get; set; }
    DateTime? ModifiedOn { get; set; }
}

public interface IModifiedCreatedAuditBaseEntity<TUser> : IModifiedCreatedAuditBaseEntity, ICreatedAuditBaseEntity<TUser>
    where TUser : class
{
    TUser ModifiedByUser { get; set; }
}