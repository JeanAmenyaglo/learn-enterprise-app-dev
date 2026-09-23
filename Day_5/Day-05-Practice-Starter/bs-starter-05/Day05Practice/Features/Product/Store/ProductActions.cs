namespace Day05Practice.Features.Product.Store;

public record LoadProductsAction;
public record LoadProductsSuccessAction(List<Services.Product> Products);
public record LoadProductsFailureAction(string Error);