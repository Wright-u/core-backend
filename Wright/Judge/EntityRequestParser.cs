using Wright.Judge.DTOs;

public class EntityRequestParser 
{
    public List<Entity> Parse(JudgeRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var entities = request.DesignSchema.Elements
        .Select(MapElement)
        .ToList();

        entities.AddRange(request.DesignSchema.Relationships.Select(r =>
        {
            var source = entities.FirstOrDefault(e => e.Id.ToString() == r.SourceId);
            var target = entities.FirstOrDefault(e => e.Id.ToString() == r.TargetId);
            if (source == null || target == null)
                throw new ArgumentException($"Invalid relationship: {r.SourceId} -> {r.TargetId}");
            return new RelationEntity
            {
                SourceId = Guid.TryParse(r.SourceId, out var sourceGuid) ? sourceGuid : throw new ArgumentException($"Invalid source id: {r.SourceId}"),
                TargetId = Guid.TryParse(r.TargetId, out var targetGuid) ? targetGuid : throw new ArgumentException($"Invalid target id: {r.TargetId}"),
                Name = $"{source.Name}->{target.Name}",
                Source = source,
                Target = target,
                Type = Enum.TryParse<RelationTypes>(r.Type, true, out var relationType) ? relationType : throw new ArgumentException($"Invalid relation type: {r.Type}")
            };
        }));
        return entities;
    }

    private static Entity MapElement(ElementDto dto)
    {
        var id = Guid.TryParse(dto.Id, out var guid) ? guid : throw new ArgumentException($"Invalid id: {dto.Id}");

        return dto.Type.ToLowerInvariant() switch
        {
            "function" => new FunctionEntity { Id = id, Name = dto.Name,
                Parameters = dto.Properties
                .Select(p => new FunctionParameter { Name = p.Name, Type = p.Type })
                .ToList()
            },
            _ => throw new ArgumentException($"Unknown element type: {dto.Type}")
        };
    }


}