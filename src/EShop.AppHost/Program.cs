using Aspire.Hosting.ApplicationModel;

var builder = DistributedApplication.CreateBuilder(args);

var jwtSecret = builder.AddParameter("jwt-secret", secret: true);
var sqlPassword = builder.AddParameter("identitydb-password", secret: true);

var seq = builder.AddContainer("seq", "datalust/seq")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("SEQ_FIRSTRUN_NOAUTHENTICATION", "true")
    .WithHttpEndpoint(port: 5341, targetPort: 80, name: "http")
    .WithVolume("eshop-seq-data", "/data")
    .WithLifetime(ContainerLifetime.Persistent);

var catalogDb = builder.AddPostgres("catalogdb")
    .WithImageTag("17")
    .WithDataVolume("eshop-catalog-postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("catalog-database", "CatalogDb");

var basketDb = builder.AddPostgres("basketdb")
    .WithImageTag("17")
    .WithDataVolume("eshop-basket-postgres")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("basket-database", "BasketDb");

var redis = builder.AddRedis("redis")
    .WithDataVolume("eshop-redis-data")
    .WithLifetime(ContainerLifetime.Persistent);

var identityDb = builder.AddSqlServer("identitydb", sqlPassword)
    .WithDataVolume("eshop-identity-sqlserver")
    .WithLifetime(ContainerLifetime.Persistent)
    .AddDatabase("identity-database", "IdentityDb");

var identity = builder.AddProject<Projects.Identity_API>("identity-api")
    .WithReference(identityDb, "Database")
    .WaitFor(identityDb)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ASPNETCORE_URLS", "http://localhost:7003")
    .WithEnvironment("Jwt__Issuer", "EShop.Identity")
    .WithEnvironment("Jwt__Audience", "EShop.Services")
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("Seq__ServerUrl", seq.GetEndpoint("http"))
    .WaitFor(seq);

var catalog = builder.AddProject<Projects.Catalog_API>("catalog-api")
    .WithReference(catalogDb, "Database")
    .WaitFor(catalogDb)
    .WithReference(identity)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ASPNETCORE_URLS", "http://localhost:7000")
    .WithEnvironment("Jwt__Issuer", "EShop.Identity")
    .WithEnvironment("Jwt__Audience", "EShop.Services")
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("Seq__ServerUrl", seq.GetEndpoint("http"))
    .WaitFor(seq);

var discount = builder.AddProject<Projects.Discount_Grpc>("discount-grpc")
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ASPNETCORE_URLS", "http://localhost:7002")
    .WithEnvironment("ConnectionStrings__Database", "Data Source=discountdb")
    .WithEnvironment("Seq__ServerUrl", seq.GetEndpoint("http"))
    .WaitFor(seq);

builder.AddProject<Projects.Basket_API>("basket-api")
    .WithReference(basketDb, "Database")
    .WaitFor(basketDb)
    .WithReference(redis, "Redis")
    .WaitFor(redis)
    .WithReference(discount)
    .WaitFor(discount)
    .WithReference(identity)
    .WithEnvironment("ASPNETCORE_ENVIRONMENT", "Development")
    .WithEnvironment("ASPNETCORE_URLS", "http://localhost:7001")
    .WithEnvironment("GrpcSettings__DiscountUrl", discount.GetEndpoint("http"))
    .WithEnvironment("Jwt__Issuer", "EShop.Identity")
    .WithEnvironment("Jwt__Audience", "EShop.Services")
    .WithEnvironment("Jwt__Secret", jwtSecret)
    .WithEnvironment("Seq__ServerUrl", seq.GetEndpoint("http"))
    .WaitFor(seq);

builder.Build().Run();
