using AutoWise.CommonUtilities.Models.BaseEntities.Interfaces;

namespace AutoWise.CommonUtilities.Models.BaseEntities;

public class IdBaseEntity : IIdBaseEntity
{
    public Guid Id { get; set; }
}
