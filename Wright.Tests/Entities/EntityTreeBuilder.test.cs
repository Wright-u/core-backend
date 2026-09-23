using Wright.Entities;
using Wright.Tests.Shared;

namespace Wright.Tests.Entities;

public class EntityTreeBuilderTest
{
    [Fact]
    public void BuildWhenEmptyThenThrowException()
    {
        Assert.Throws<ArgumentNullException>(() => EntityTreeBuilder.Build([], Guid.NewGuid()));
    }

    [Fact]
    public void BuildWhenNotEmptyThenReturnList()
    {
        var id = Guid.NewGuid();
        var result = EntityTreeBuilder.Build([new StubEntity { Id = id, Name = "Test Entity" }], id);
        Assert.Equal(result.Id, id);
    }

    [Fact]
    public void BuildWhenNestedEntitiesThenReturnOnlyRoot()
    {
        var parentId = Guid.NewGuid();
        var result = EntityTreeBuilder.Build([
            new StubEntity { Id = parentId, Name = "Test Entity"},
            new StubEntity { Id = Guid.NewGuid(), Name = "Test Entity", ParentId = parentId }
            ], parentId);

        Assert.Equal(result.Id, parentId);
        Assert.Single(result.Children);
    }
}