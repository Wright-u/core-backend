using Wright.Entities;
using Wright.Entities.Features.Application;
using Wright.Entities.Features.Function;
using Wright.Entities.Features.Relation;
using Wright.Judge;
using Wright.Judge.DTOs;

namespace Wright.Tests.Judge;

public class EntityRequestParserTest
{
    [Fact]
    public void ParseWhenRequestIsNullThenThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new EntityRequestParser().Parse(null!));
    }

    [Fact]
    public void ParseWhenSchemaIsEmptyThenReturnsAnEmptyApplicationRoot()
    {
        ApplicationEntity application = Assert.IsType<ApplicationEntity>(
            new EntityRequestParser().Parse(CreateRequest()));

        Assert.Equal("sample-app", application.Name);
        Assert.Empty(application.Children);
    }

    [Fact]
    public void ParseWhenSchemaContainsFunctionThenCreatesAnApplicationRootWithFunctionChild()
    {
        JudgeRequest request = CreateRequest(elements:
        [
            new ElementDto
            {
                Id = "calculate-total-node",
                Name = "CalculateTotal",
                Type = "FUNCTION",
                Properties =
                [
                    new PropertyDto { Name = "quantity", Type = "int" },
                    new PropertyDto { Name = "price", Type = "decimal" }
                ]
            }
        ]);

        ApplicationEntity application = Assert.IsType<ApplicationEntity>(new EntityRequestParser().Parse(request));
        FunctionEntity function = Assert.IsType<FunctionEntity>(Assert.Single(application.Children));

        Assert.Equal("sample-app", application.Name);
        Assert.Equal("CalculateTotal", function.Name);
        Assert.Collection(
            function.Parameters,
            parameter => AssertParameter(parameter, "quantity", "int"),
            parameter => AssertParameter(parameter, "price", "decimal"));
    }

    [Fact]
    public void ParseWhenSchemaContainsUsesRelationshipThenConnectsTheParsedEntities()
    {
        JudgeRequest request = CreateRequest(
            elements:
            [
                Function("caller-node", "Caller"),
                Function("callee-node", "Callee")
            ],
            relationships:
            [
                new RelationshipDto
                {
                    SourceId = "caller-node",
                    TargetId = "callee-node",
                    Type = "uses"
                }
            ]);

        ApplicationEntity application = Assert.IsType<ApplicationEntity>(new EntityRequestParser().Parse(request));
        FunctionEntity caller = Assert.Single(application.Children.OfType<FunctionEntity>(), entity => entity.Name == "Caller");
        RelationEntity relation = Assert.IsType<RelationEntity>(Assert.Single(caller.Children));
        FunctionEntity callee = Assert.IsType<FunctionEntity>(relation.Target);

        Assert.Equal("Caller->Callee", relation.Name);
        Assert.Equal(RelationTypes.Uses, relation.Type);
        Assert.Equal(caller.Id, relation.SourceId);
        Assert.Equal(callee.Id, relation.TargetId);
        Assert.Same(caller, relation.Source);
        Assert.Same(callee, relation.Target);
        Assert.Same(caller, relation.Parent);
        Assert.Equal(caller.Id, relation.ParentId);
    }

    [Fact]
    public void ParseWhenElementIdIsAnOpaqueDiagramIdThenCreatesTheEntity()
    {
        JudgeRequest request = CreateRequest(elements: [Function("not-a-guid", "Calculate")]);

        ApplicationEntity application = Assert.IsType<ApplicationEntity>(new EntityRequestParser().Parse(request));
        FunctionEntity function = Assert.IsType<FunctionEntity>(Assert.Single(application.Children));

        Assert.Equal("Calculate", function.Name);
    }

    [Fact]
    public void ParseWhenElementTypeIsNotSupportedThenThrowsArgumentException()
    {
        JudgeRequest request = CreateRequest(elements:
        [
            new ElementDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Total",
                Type = "variable"
            }
        ]);

        Assert.Throws<ArgumentException>(() => new EntityRequestParser().Parse(request));
    }

    [Fact]
    public void ParseWhenRelationshipEndpointIsNotAnElementThenThrowsArgumentException()
    {
        JudgeRequest request = CreateRequest(
            elements: [Function("caller-node", "Caller")],
            relationships:
            [
                new RelationshipDto
                {
                    SourceId = "caller-node",
                    TargetId = "missing-node",
                    Type = "Uses"
                }
            ]);

        Assert.Throws<ArgumentException>(() => new EntityRequestParser().Parse(request));
    }

    private static JudgeRequest CreateRequest(
        List<ElementDto>? elements = null,
        List<RelationshipDto>? relationships = null) => new()
        {
            AppId = "sample-app",
            DesignSchema = new DesignSchemaDto
            {
                Elements = elements ?? [],
                Relationships = relationships ?? []
            }
        };

    private static ElementDto Function(string id, string name) => new()
    {
        Id = id,
        Name = name,
        Type = "function"
    };

    private static void AssertParameter(FunctionParameter parameter, string expectedName, string expectedType)
    {
        Assert.Equal(expectedName, parameter.Name);
        Assert.Equal(expectedType, parameter.Type);
    }
}
