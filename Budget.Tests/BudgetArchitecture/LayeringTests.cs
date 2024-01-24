using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent;

using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitNET.xUnit;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace Budget.Tests.BudgetArchitecture;

public class LayeringTests
{
    private static readonly Architecture Architecture = new ArchLoader().LoadAssemblies(
            System.Reflection.Assembly.Load("Budget.Pages"),
            System.Reflection.Assembly.Load("Budget.Core")
        ).Build();

    //declare variables you'll use throughout your tests up here
    //use As() to give them a custom description
    private readonly IObjectProvider<IType> PresentationLayer =
        Types().That().ResideInAssembly("Budget.Pages.Pages.*", true).As("Presentation Layer");

    private readonly IObjectProvider<Class> PageModels =
        Classes().That().ImplementInterface(nameof(PageModel)).As(nameof(PageModel));

    private readonly IObjectProvider<Class> UseCases =
        Classes().That().HaveNameEndingWith("UseCase").As("Use cases");

    private readonly IObjectProvider<IType> CoreLayer =
        Types().That().ResideInNamespace("Budget.Core.UseCases.*", true).As("Core Layer");

    private readonly IObjectProvider<Interface> ForbiddenInterfaces =
        Interfaces().That().HaveFullNameContaining("forbidden").As("Forbidden Interfaces");


    [Fact]
    public void Types_should_be_in_correct_layer()
    {
        var useCaseRule = Classes().That().Are(UseCases).Should().Be(CoreLayer);
        var tableClientRule = Classes().That().Are(PageModels).Should().Be(PresentationLayer);

        IArchRule rules =  useCaseRule.And(tableClientRule);

        rules.Check(Architecture);
    }
}
