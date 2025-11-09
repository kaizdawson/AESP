using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace AESP.API.Helpers
{
    public class FileUploadOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var controllerName = context.ApiDescription.ActionDescriptor.RouteValues["controller"];
            if (controllerName == null || !controllerName.Contains("Certificate"))
                return;
            // Kiểm tra xem endpoint có dùng IFormFile không
            var hasFileParam = context.ApiDescription.ParameterDescriptions
                .Any(p => p.ModelMetadata?.ModelType == typeof(IFormFile));

            if (hasFileParam)
            {
                // ✅ Bổ sung cả "file" và "name" vào form
                operation.RequestBody = new OpenApiRequestBody
                {
                    Content = new Dictionary<string, OpenApiMediaType>
                    {
                        ["multipart/form-data"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema
                            {
                                Type = "object",
                                Properties = new Dictionary<string, OpenApiSchema>
                                {
                                    ["name"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Description = "Tên chứng chỉ (VD: TESOL, IELTS, ...)"
                                    },
                                    ["file"] = new OpenApiSchema
                                    {
                                        Type = "string",
                                        Format = "binary",
                                        Description = "File chứng chỉ (ảnh hoặc PDF)"
                                    }
                                },
                                Required = new HashSet<string> { "name", "file" }
                            }
                        }
                    }
                };
            }
        }
    }

}
