using ArchUnitNET.Domain;
using ArchUnitNET.Loader;
using ArchUnitNET.Fluent;

using static ArchUnitNET.Fluent.ArchRuleDefinition;
using ArchUnitNET.xUnit;
using Budget.Core.DataAccess;
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
    private readonly IObjectProvider<IType> _presentationLayer =
        Types().That().ResideInAssembly("Budget.Pages.Pages.*", true).As("Presentation Layer");

    private readonly IObjectProvider<Class> _pageModels =
        Classes().That().ImplementInterface(nameof(PageModel)).As(nameof(PageModel));

    private readonly IObjectProvider<Class> _useCases =
        Classes().That().HaveNameEndingWith("UseCase").As("Use cases");
    
    private readonly IObjectProvider<IType> _migrations =
        Types().That().ResideInNamespace("Budget.Pages.Migrations")
            .Or().ResideInNamespace("Budget.Core.Migrations")
            .As("Migrations");

    private readonly IObjectProvider<IType> _coreLayer =
        Types().That().ResideInNamespace("Budget.Core.UseCases.*", true).As("Core Layer");

    private readonly IObjectProvider<Interface> _forbiddenInterfaces =
        Interfaces().That().HaveFullNameContaining("forbidden").As("Forbidden Interfaces");


    [Fact]
    public void Types_should_be_in_correct_layer()
    {
        var useCaseRule = Classes().That().Are(_useCases).Should().Be(_coreLayer);
        var tableClientRule = Classes().That().Are(_pageModels).Should().Be(_presentationLayer);

        IArchRule rules =  useCaseRule.And(tableClientRule);

        rules.Check(Architecture);
    }

    [Fact]
    public void Use_cases_should_only_use_the_table_client()
    {
        var rule = Types().That()
            .AreNot(_useCases)
            .And()
            .AreNot(_migrations)
            .And()
            .AreNot(typeof(BudgetContext))
            .Should().NotDependOnAny(typeof(BudgetContext))
            .Because("Only use cases should use the table client to query");

        rule.Check(Architecture);
    }
}
