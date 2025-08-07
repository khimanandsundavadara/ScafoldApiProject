using Microsoft.AspNetCore.Http;
using Serilog;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;

    public ExceptionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(httpContext, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        HttpStatusCode statusCode;
        string message;

        switch (exception)
        {
            case ArgumentNullException:
            case ArgumentException:
                statusCode = HttpStatusCode.BadRequest;
                message = exception.Message;
                break;

            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Unauthorized access.";
                break;

            case KeyNotFoundException:
                statusCode = HttpStatusCode.NotFound;
                message = "Resource not found.";
                break;

            default:
                statusCode = HttpStatusCode.InternalServerError;
                message = "Internal Server Error. Please contact support.";
                break;
        }

        context.Response.StatusCode = (int)statusCode;

        var result = System.Text.Json.JsonSerializer.Serialize(new
        {
            StatusCode = context.Response.StatusCode,
            Message = message
        });

        return context.Response.WriteAsync(result);
    }

}
