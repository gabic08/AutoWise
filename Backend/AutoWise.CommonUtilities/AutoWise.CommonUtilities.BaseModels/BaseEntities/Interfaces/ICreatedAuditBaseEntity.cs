using System;

namespace AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;

public interface ICreatedAuditBaseEntity : IIdBaseEntity
{
    Guid CreatedByUserId { get; set; }
    DateTime CreatedOn { get; set; }
}

public interface ICreatedAuditBaseEntity<TUser> : ICreatedAuditBaseEntity where TUser : class
{
    TUser CreatedByUser { get; set; }
}
