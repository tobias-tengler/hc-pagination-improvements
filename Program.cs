using HotChocolate.Execution.Processing;
using HotChocolate.Resolvers;
using HotChocolate.Types.Pagination;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddGraphQLServer()
    .AddOperationCompilerOptimizer<PaginationOperationOptimizer>()
    .AddTypes();

var app = builder.Build();

app.MapGraphQL();

app.RunWithGraphQLCommands(args);


public class PaginationOperationOptimizer : IOperationOptimizer
{
    private static Type ConnectionType = typeof(Connection);
    private const string EdgesField = "edges";
    private const string NodesField = "nodes";

    public void OptimizeOperation(OperationOptimizerContext context)
    {
        var operation = context.CreateOperation();

        var rootSelections = operation.RootSelectionSet.Selections;
        foreach (var rootSelection in rootSelections)
        {
            ProcessSelection(operation, rootSelection, context);
        }
    }

    private void ProcessSelection(IOperation operation, ISelection selection, OperationOptimizerContext context)
    {
        if (selection.Field.IsIntrospectionField)
        {
            return;
        }

        var returnType = selection.Type.NamedType();

        if (!returnType.IsObjectType())
        {
            return;
        }

        var runtimeReturnType = returnType.ToRuntimeType();

        if (runtimeReturnType != ConnectionType)
        {
            return;
        }

        OptimizePagination(operation, selection, context);

        if (selection.SelectionSet is null)
        {
            return;
        }

        var possibleTypes = operation.GetPossibleTypes(selection);
        foreach (var type in possibleTypes)
        {
            var selections = operation.GetSelectionSet(selection, type).Selections;
            foreach (var childSelection in selections)
            {
                ProcessSelection(operation, childSelection, context);
            }
        }
    }

    private void OptimizePagination(IOperation operation, ISelection selection, OperationOptimizerContext context)
    {
        if (selection.SelectionSet is null || selection.Type is not IObjectType productType)
        {
            return;
        }

        var selectionSetOnType = operation.GetSelectionSet(selection, productType);

        var areNodesOrEdgesSelected = selectionSetOnType.Selections.Any(s => s.Field.Name == EdgesField || s.Field.Name == NodesField);

        if (areNodesOrEdgesSelected)
        {
            return;
        }

        context.SetResolver(selection, ShortCircuitPaginationPipeline());

        static FieldDelegate ShortCircuitPaginationPipeline() =>
            ctx =>
            {
                ctx.Result = new();
                return ValueTask.CompletedTask;
            };
    }
}