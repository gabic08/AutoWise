using AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;
using System;

namespace AutoWise.CommonUtilities.Models.BaseEntities;

public class CreatedAuditBaseEntity : IdBaseEntity, ICreatedAuditBaseEntity
{
    public Guid CreatedByUserId { get; set; }
    public DateTime CreatedOn { get; set; }
}

public class CreatedAuditBaseEntity<TUser> : CreatedAuditBaseEntity, ICreatedAuditBaseEntity<TUser>
    where TUser : class
{
    public TUser CreatedByUser { get; set; }
}