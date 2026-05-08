using BuildingBlocks.Exceptions;

namespace Catalog.API.Exceptions;

public sealed class ProductNotFoundException(string message) : NotFoundException(message);
