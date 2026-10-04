using Wright.AstTranslation.Translators;
using Wright.CodeAnatomy.Utils;
using Wright.Entities.Features.Function;
using Wright.Entities.Features.Relation;

namespace Wright.Tests.AstTranslation;

public class CTranslatorTest
{
    [Fact]
    public void TranslateWhenFunctionDefinitionThenReturnFunctionEntity()
    {
        var tree = new TreeCodeParser().Parse("int sum(int left, int right) { return left + right; }", "C");

        var entity = new CTranslator().Translate(tree.RootNode.NamedChildren.Single());

        var function = Assert.IsType<FunctionEntity>(entity);
        Assert.Equal("sum", function.Name);
        Assert.Collection(
            function.Parameters,
            parameter =>
            {
                Assert.Equal("left", parameter.Name);
                Assert.Equal("int", parameter.Type);
            },
            parameter =>
            {
                Assert.Equal("right", parameter.Name);
                Assert.Equal("int", parameter.Type);
            });
    }

    [Fact]
    public void TranslateWhenNodeTypeIsNotSupportedThenThrow()
    {
        var tree = new TreeCodeParser().Parse("int value;", "C");

        Assert.Throws<NotSupportedException>(() =>
            new CTranslator().Translate(tree.RootNode.NamedChildren.Single()));
    }

    [Fact]
    public void TranslateWhenFunctionHasVoidParameterThenReturnNoParameters()
    {
        var tree = new TreeCodeParser().Parse("int main(void) { return 0; }", "C");

        var entity = new CTranslator().Translate(tree.RootNode.NamedChildren.Single());

        Assert.Empty(Assert.IsType<FunctionEntity>(entity).Parameters);
    }

    [Fact]
    public void TranslateWhenFunctionCallsOtherFunctionsThenAddUsesRelations()
    {
        var tree = new TreeCodeParser().Parse(
            "void caller(void) { first(); second(42); first(); }", "C");

        var function = Assert.IsType<FunctionEntity>(
            new CTranslator().Translate(tree.RootNode.NamedChildren.Single()));
        var relations = function.Children.Cast<RelationEntity>().OrderBy(relation => relation.Name).ToList();

        Assert.Collection(
            relations,
            relation => Assert.Equal("first", relation.Name),
            relation => Assert.Equal("second", relation.Name));
        Assert.All(relations, relation =>
        {
            Assert.Equal(RelationTypes.Uses, relation.Type);
            Assert.Same(function, relation.Source);
            Assert.Equal(function.Id, relation.SourceId);
            Assert.Same(function, relation.Parent);
            Assert.Equal(function.Id, relation.ParentId);
            Assert.NotNull(relation.Target);
            Assert.Equal(relation.Name, relation.Target.Name);
            Assert.Equal(relation.Target.Id, relation.TargetId);
        });
    }
}
