using BuildingBlocks.Exceptions;

namespace Catalog.API.Exceptions;

public sealed class ProductNotFoundException(Guid id) : NotFoundException("Product", id);
