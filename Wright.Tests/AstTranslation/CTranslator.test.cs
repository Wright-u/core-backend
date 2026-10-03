using Wright.AstTranslation.Translators;
using Wright.CodeAnatomy.Utils;
using Wright.Entities.Features.Function;

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
}
