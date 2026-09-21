
using FluentValidation;

using Microsoft.AspNetCore.Http;

using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Nova.Application.Dto.Response;
using Nova.Domain.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Data.SqlClient;
using System.Net;
using static Nova.Domain.Exceptions.NotFoundException;

namespace BranchCardManagementSystem.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionMiddleware> _logger;
        private readonly IHostEnvironment _env;

        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _next = next;
            _logger = logger;
            _env = env;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            var (statusCode, message, errors) = MapException(exception);

            // Log at a severity that matches how bad it is
            if (statusCode >= HttpStatusCode.InternalServerError)
                _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
            else
                _logger.LogWarning(exception, "Handled client error: {Message}", exception.Message);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)statusCode;

            var response = ResponseModel<string>.Fail(
                message,
                errors,
                statusCode
            );

            // Never leak internal exception details outside Development
            if (_env.IsDevelopment() && statusCode == HttpStatusCode.InternalServerError)
            {
                response.ErrorMessage.Add(exception.ToString());
            }

            await context.Response.WriteAsync(
                JsonConvert.SerializeObject(response, Formatting.Indented));
        }

        private static (HttpStatusCode StatusCode, string Message, List<string> Errors) MapException(Exception exception)
        {
            return exception switch
            {
                        FluentValidation.ValidationException validationEx => (
                    HttpStatusCode.BadRequest,
                    "Validation failed.",
                    validationEx.Errors
                        .Select(e => e.ErrorMessage)
                        .ToList()
                ),
            


                NotFoundException notFoundEx => (
                    HttpStatusCode.NotFound,
                    notFoundEx.Message,
                    new List<string> { notFoundEx.Message }
                ),

                ForbiddenException forbiddenEx => (
                    HttpStatusCode.Forbidden,
                    forbiddenEx.Message,
                    new List<string> { forbiddenEx.Message }
                ),

                ConflictException conflictEx => (
                    HttpStatusCode.Conflict,
                    conflictEx.Message,
                    new List<string> { conflictEx.Message }
                ),

                UnauthorizedAccessException ex => (
                    HttpStatusCode.Unauthorized,
                    ex.Message,
                    new List<string> { ex.Message }
                ),

                OperationFailedException opEx => (
                    opEx.StatusCode,
                    opEx.Message,
                    opEx.Errors.Count > 0 ? opEx.Errors : new List<string> { opEx.Message }
                ),

                DbUpdateConcurrencyException => (
                    HttpStatusCode.Conflict,
                    "The record was modified by another process.",
                    new List<string> { "Concurrency conflict occurred." }
                ),

                DbUpdateException dbEx => (
                    HttpStatusCode.BadRequest,
                    "Database update failed.",
                    new List<string> { dbEx.InnerException?.Message ?? dbEx.Message }
                ),

                SqlException sqlEx => (
                    HttpStatusCode.InternalServerError,
                    "A database error occurred.",
                    new List<string> { "Please contact support if this persists." }
                ),

                ArgumentNullException or ArgumentException => (
                    HttpStatusCode.BadRequest,
                    "Invalid argument.",
                    new List<string> { exception.Message }
                ),

                OperationCanceledException => (
                    (HttpStatusCode)499, 
                    "The request was cancelled.",
                    new List<string> { "The operation was cancelled." }
                ),

                _ => (
                    HttpStatusCode.InternalServerError,
                    "An unexpected error occurred. Please try again later.",
                    new List<string> { "Internal Server Error." }
                )
            };
        }
    }

    public static class ExceptionMiddlewareExtensions
    {
        public static IApplicationBuilder ConfigureCustomExceptionMiddleware(this IApplicationBuilder app)
            => app.UseMiddleware<ExceptionMiddleware>();
    }
}