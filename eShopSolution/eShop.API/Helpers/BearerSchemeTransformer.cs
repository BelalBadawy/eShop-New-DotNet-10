using Microsoft.AspNetCore.JsonPatch.Operations;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace eShop.API.Helpers
{
    //public class BearerSchemeTransformer : IOpenApiDocumentTransformer
    //{
    //    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    //    {
    //        // Add security scheme to components
    //        document.Components ??= new OpenApiComponents();

    //        if (document.Components.SecuritySchemes == null)
    //        {
    //            document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>();
    //        }

    //        document.Components.SecuritySchemes["Bearer"] = new OpenApiSecurityScheme
    //        {
    //            Type = SecuritySchemeType.Http,
    //            Scheme = "bearer",
    //            BearerFormat = "JWT",
    //            Description = "Enter JWT Bearer token **_only_**",
    //            In = ParameterLocation.Header,
    //            Name = "Authorization"
    //        };

    //        // Add security requirement to all operations
    //        foreach (var path in document.Paths.Values)
    //        {
    //            // Convert the dictionary key type
    //            if (path.Operations is Dictionary<System.Net.Http.HttpMethod, OpenApiOperation> httpOps)
    //            {
    //                // Map HttpMethod to OperationType if possible
    //                var mapped = new Dictionary<Microsoft.AspNetCore.JsonPatch.Operations.OperationType, OpenApiOperation>();
    //                foreach (var kvp in httpOps)
    //                {
    //                    if (Enum.TryParse<Microsoft.AspNetCore.JsonPatch.Operations.OperationType>(kvp.Key.Method, true, out var opType))
    //                    {
    //                        mapped[opType] = kvp.Value;
    //                    }
    //                }
    //                AddSecurityToOperations(mapped);
    //            }
    //            else
    //            {
    //                // If already correct type, just pass through
    //                AddSecurityToOperations(path.Operations as IDictionary<Microsoft.AspNetCore.JsonPatch.Operations.OperationType, OpenApiOperation>);
    //            }
    //        }

    //        // Also check for webhooks if present
    //        if (document.Webhooks?.Count > 0)
    //        {
    //            foreach (var webhook in document.Webhooks.Values)
    //            {
    //                // Convert the dictionary key type
    //                if (webhook.Operations is Dictionary<System.Net.Http.HttpMethod, OpenApiOperation> httpOps)
    //                {
    //                    var mapped = new Dictionary<Microsoft.AspNetCore.JsonPatch.Operations.OperationType, OpenApiOperation>();
    //                    foreach (var kvp in httpOps)
    //                    {
    //                        if (Enum.TryParse<Microsoft.AspNetCore.JsonPatch.Operations.OperationType>(kvp.Key.Method, true, out var opType))
    //                        {
    //                            mapped[opType] = kvp.Value;
    //                        }
    //                    }
    //                    AddSecurityToOperations(mapped);
    //                }
    //                else
    //                {
    //                    AddSecurityToOperations(webhook.Operations as IDictionary<Microsoft.AspNetCore.JsonPatch.Operations.OperationType, OpenApiOperation>);
    //                }
    //            }
    //        }

    //        return Task.CompletedTask;
    //    }

    //    private static void AddSecurityToOperations(IDictionary<OperationType, OpenApiOperation> operations)
    //    {
    //        if (operations == null) return;

    //        foreach (var operation in operations.Values)
    //        {
    //            operation.Security ??= new List<OpenApiSecurityRequirement>();

    //            // Check if Bearer security requirement already exists
    //            var hasBearerSecurity = operation.Security.Any(requirement =>
    //                requirement.Any(pair => pair.Key.Reference != null && pair.Key.Reference.Id == "Bearer"));

    //            if (!hasBearerSecurity)
    //            {
    //                operation.Security.Add(new OpenApiSecurityRequirement
    //                {
    //                    {
    //                        new OpenApiSecurityScheme
    //                        {
    //                            Reference = new OpenApiReference
    //                            {
    //                                Type = ReferenceType.SecurityScheme,
    //                                Id = "Bearer"
    //                            }
    //                        },
    //                        Array.Empty<string>()
    //                    }
    //                });
    //            }
    //        }
    //    }
    //}


    public class BearerSchemeTransformer : IOpenApiDocumentTransformer
    {
        public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
        {
            // Initialize components if needed
            if (document.Components == null)
            {
                document.Components = new OpenApiComponents();
            }

            // Initialize security schemes with the correct type
            if (document.Components.SecuritySchemes == null)
            {
                document.Components.SecuritySchemes = new Dictionary<string, IOpenApiSecurityScheme>();
            }

            // Add Bearer security scheme if it doesn't exist
            if (!document.Components.SecuritySchemes.ContainsKey("Bearer"))
            {
                document.Components.SecuritySchemes.Add("Bearer", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http, // Use enum instead of string
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = "Enter JWT Bearer token"
                });
            }

            // Add security requirement to all operations
            foreach (var path in document.Paths.Values)
            {
                // Convert HttpMethod dictionary to OperationType dictionary if needed
                if (path.Operations is Dictionary<System.Net.Http.HttpMethod, OpenApiOperation> httpOps)
                {
                    var mapped = new Dictionary<OperationType, OpenApiOperation>();
                    foreach (var kvp in httpOps)
                    {
                        if (Enum.TryParse<OperationType>(kvp.Key.Method, true, out var opType))
                        {
                            mapped[opType] = kvp.Value;
                        }
                    }
                    AddSecurityToOperations(mapped);
                }
                else
                {
                    AddSecurityToOperations(path.Operations as IDictionary<OperationType, OpenApiOperation>);
                }
            }

            return Task.CompletedTask;
        }

        private static void AddSecurityToOperations(IDictionary<OperationType, OpenApiOperation> operations)
        {
            if (operations == null) return;

            foreach (var operation in operations.Values)
            {
                if (operation.Security == null)
                {
                    operation.Security = new List<OpenApiSecurityRequirement>();
                }

                // Check if Bearer security requirement already exists
                bool hasBearerSecurity = operation.Security.Any(requirement =>
                    requirement.Any(pair => pair.Key is OpenApiSecuritySchemeReference reference &&
                        GetReferenceId(reference) == "Bearer"));

                if (!hasBearerSecurity)
                {
                    var securityRequirement = new OpenApiSecurityRequirement();
                    var bearerReference = new OpenApiSecuritySchemeReference("Bearer");
                    securityRequirement.Add(bearerReference, new List<string>());
                    operation.Security.Add(securityRequirement);
                }
            }
        }

        private static string GetReferenceId(OpenApiSecuritySchemeReference reference)
        {
            // Use reflection or other method to get the reference ID if property is not exposed
            // This is a workaround - adjust based on actual API
            return reference.ToString().Split('/').LastOrDefault() ?? string.Empty;
        }
    }
}

