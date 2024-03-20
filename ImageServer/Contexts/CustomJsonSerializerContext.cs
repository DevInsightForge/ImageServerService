using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace ImageServer.Contexts;


[JsonSerializable(typeof(ProblemDetails))]
public partial class CustomJsonSerializerContext : JsonSerializerContext
{
}