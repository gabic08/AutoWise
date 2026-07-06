using AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;

namespace AutoWise.CommonUtilities.Models.BaseEntities;

public abstract class IdBaseEntity : IIdBaseEntity
{
    public Guid Id { get; set; }
}
