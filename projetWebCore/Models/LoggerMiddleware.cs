using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Diagnostics;

namespace projetWebCore.Models
{
    public class LoggerMiddleware
    {
        private readonly RequestDelegate _next;
        public LoggerMiddleware(RequestDelegate next)
        {
            _next = next;
        }
        public async Task Invoke(HttpContext context)
        {
            using (MemoryStream requestBodyStream = new MemoryStream())
            {
                using (MemoryStream responseBodyStream = new MemoryStream())
                {
                    Stream originalRequestBody = context.Request.Body;
                    context.Request.EnableBuffering();
                    Stream originalResponseBody = context.Response.Body;
                    try
                    {
                        await context.Request.Body.CopyToAsync(requestBodyStream);
                        requestBodyStream.Seek(0, SeekOrigin.Begin);
                        string requestBodyText = new
                       StreamReader(requestBodyStream).ReadToEnd();
                        requestBodyStream.Seek(0, SeekOrigin.Begin);
                        context.Request.Body = requestBodyStream;
                        string responseBody = "";
                        context.Response.Body = responseBodyStream;
                        Stopwatch watch = Stopwatch.StartNew();
                        await _next(context);
                        watch.Stop();
                        responseBodyStream.Seek(0, SeekOrigin.Begin);
                        responseBody = new StreamReader(responseBodyStream).ReadToEnd();
                       // AuditLogger.LogToAudit(context.Request.Host.Host,context.Request.Path, context.Request.QueryString.ToString(),context.Connection.RemoteIpAddress.MapToIPv4().ToString(),string.Join(",", context.Request.Headers.Select(he => he.Key +":[" + he.Value + "]").ToList()),requestBodyText, responseBody, DateTime.Now,watch.ElapsedMilliseconds);
                        responseBodyStream.Seek(0, SeekOrigin.Begin);
                        await responseBodyStream.CopyToAsync(originalResponseBody);
                    }
                    catch (Exception ex)
                    {
                        
                        byte[] data = System.Text.Encoding.UTF8.GetBytes("Unhandled Error occured, the error has been logged and the persons concerned are notified!! Please, try again in a while. " + ex.Message);
                        originalResponseBody.Write(data, 0, data.Length);
                        }
                        finally
                        {
                            context.Request.Body = originalRequestBody;
                            context.Response.Body = originalResponseBody;
                        }
                    }
                }
            }
        }
    }
