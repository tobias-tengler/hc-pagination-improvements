While Hot Chocolate's `UsePaging` is amazing ✨, there's one performance flaw: Even if you do not select `nodes` or `edges` when querying a Connection type, the resolver for the pagination items is executed. This is unfortunate, since you might want to specifically _only_ query for a `totalCount` (which might be cheaply resolved), without performing the costly resolution and pagination of nodes.

One idea I had was to create an `IOperationOptimizer` that traverses the current operation and finds all selection sets on Connections. If the selection set doesn't contain `nodes` or `edges` the resolver pipeline of the field returning the Connection will be stubbed out. This way we skip the potentially costly computation of nodes and only evaluate the extensions on the Connection. Since a Connection type itself shouldn't contain any state, we can safely skip its creating based on the nodes.

An example implementation fo the `IOperationOptimizer` can be found [here](./Program.cs#L19)

One "downside" is that we now need to add extension for totalCount ourselves, since `[UsePaging(IncludeTotalCount = true)]` and the `Func` passed to `Connection<T>` both depend on the pagination resolver to run. I don't think of this as a loss though, since I'm not particularly fond of those approaches and would rather have a dedicated field responsible for the computation of the `totalCount` 😝
