using Wright.Entities;
using Wright.Entities.Features.Application;
using Wright.Entities.Features.Function;
using Wright.Entities.Features.Relation;
using Wright.Judge.DTOs;

namespace Wright.Judge;

public class EntityRequestParser
{
    public Entity Parse(JudgeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entitiesByRequestId = request.DesignSchema.Elements
            .ToDictionary(element => element.Id, MapElement);
        var entities = entitiesByRequestId.Values.ToList();

        entities.AddRange(request.DesignSchema.Relationships.Select(r =>
        {
            if (!entitiesByRequestId.TryGetValue(r.SourceId, out Entity? source) ||
                !entitiesByRequestId.TryGetValue(r.TargetId, out Entity? target))
            {
                throw new ArgumentException($"Invalid relationship: {r.SourceId} -> {r.TargetId}");
            }

            return new RelationEntity
            {
                SourceId = source.Id,
                TargetId = target.Id,
                ParentId = source.Id,
                Name = $"{source.Name}->{target.Name}",
                Source = source,
                Target = target,
                Type = Enum.TryParse<RelationTypes>(r.Type, true, out var relationType) ? relationType : throw new ArgumentException($"Invalid relation type: {r.Type}")
            };
        }));

        var application = new ApplicationEntity { Name = request.AppId };
        entities.Add(application);

        foreach (Entity entity in entities.Where(entity => entity != application && entity.ParentId is null))
        {
            entity.ParentId = application.Id;
        }

        return EntityTreeBuilder.Build(entities, application.Id);
    }

    private static Entity MapElement(ElementDto dto)
    {
        return dto.Type.ToLowerInvariant() switch
        {
            "function" => new FunctionEntity
            {
                Name = dto.Name,
                Parameters = dto.Properties
                .Select(p => new FunctionParameter { Name = p.Name, Type = p.Type })
                .ToList()
            },
            _ => throw new ArgumentException($"Unknown element type: {dto.Type}")
        };
    }


}
