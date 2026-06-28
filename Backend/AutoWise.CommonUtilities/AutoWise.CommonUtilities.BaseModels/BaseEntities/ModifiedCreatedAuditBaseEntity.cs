using AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;
using System;

namespace AutoWise.CommonUtilities.Models.BaseEntities;

public class ModifiedCreatedAuditBaseEntity : CreatedAuditBaseEntity, IModifiedCreatedAuditBaseEntity
{
    public Guid? ModifiedByUserId { get; set; }
    public DateTime? ModifiedOn { get; set; }
}

public class ModifiedCreatedAuditBaseEntity<TUser> : ModifiedCreatedAuditBaseEntity, IModifiedCreatedAuditBaseEntity<TUser>
    where TUser : class
{
    public TUser CreatedByUser { get; set; }
    public TUser ModifiedByUser { get; set; }
}
