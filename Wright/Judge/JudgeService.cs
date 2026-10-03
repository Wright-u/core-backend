using Wright.Entities;
using Wright.Judge.DTOs;
using Wright.Judge.Interfaces;

namespace Wright.Judge;

public class JudgeService : IJudgeService
{
    // private readonly IDesignParser _designParser;
    // private readonly ICodeTranslator _codeTranslator;

    // public JudgeService(IDesignParser designParser, ICodeTranslator codeTranslator)
    // {
    //     _designParser = designParser;
    //     _codeTranslator = codeTranslator;
    // }
    public JudgeService() { }

    public async Task<JudgeResponse> Judge(JudgeRequest request)
    {

        Entity? expected = null; // Entity expected = _designParser.Parse(request);
        Entity? actual = null;   // Entity actual = await _codeTranslator.Translate(request.AppId);

        if (expected is null || actual is null)
        {
            return new JudgeResponse
            {
                ISMatch = false,
                TotalViolations = 1,
                Violations =
                [
                    new ViolationDto
                    {
                        EntityName = request.AppId,
                        Type = "not-implemented",
                        Description = "Parser and translator are not connected yet.",
                    },
                ],
            };
        }

        var violations = new List<ViolationDto>();
        CompareChildren(expected, actual, "", violations);

        return new JudgeResponse
        {
            ISMatch = violations.Count == 0,
            TotalViolations = violations.Count,
            Violations = violations,
        };
    }

    private static void CompareChildren(Entity expected, Entity actual, string path, List<ViolationDto> violations)
    {
        var actualByKey = actual.Children
            .GroupBy(MatchKey)
            .ToDictionary(g => g.Key, g => g.First());

        foreach (Entity expectedChild in expected.Children)
        {
            string childPath = path.Length == 0 ? expectedChild.Name : $"{path}/{expectedChild.Name}";

            if (!actualByKey.TryGetValue(MatchKey(expectedChild), out Entity? actualChild))
            {
                violations.Add(new ViolationDto
                {
                    EntityName = childPath,
                    Type = "missing-in-code",
                    Description = $"'{expectedChild.Name}' is in the design but was not found in the code.",
                });
                continue;
            }

            EntityDiff diff = expectedChild.Compare(actualChild);
            if (diff.IsDifferent)
            {
                var mismatches = diff.Mismatches is { Count: > 0 } ? diff.Mismatches : ["entities differ"];
                foreach (string mismatch in mismatches)
                {
                    violations.Add(new ViolationDto
                    {
                        EntityName = childPath,
                        Type = "mismatch",
                        Description = mismatch,
                    });
                }
            }

            CompareChildren(expectedChild, actualChild, childPath, violations);
        }
    }

    private static string MatchKey(Entity entity) => entity.Name;
}