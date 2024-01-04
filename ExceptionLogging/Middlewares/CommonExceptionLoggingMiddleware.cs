// CommonExceptionLoggingMiddleware.cs

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace SharedMiddlewares
{
    public class CommonExceptionLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _ServiceName;

        public CommonExceptionLoggingMiddleware(RequestDelegate next,string ServiceName)
        {
            _next = next;
            _ServiceName = ServiceName;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                LogExceptionToDatabase(ex, context.Request.Path);
                throw;
            }
        }

        private void LogExceptionToDatabase(Exception ex, string path)
        {
            using (SqlConnection cn = new SqlConnection("Data Source=localhost\\SQLEXPRESS;Initial Catalog=InventoryCore;Integrated Security=True;TrustServerCertificate=True"))
            {
                cn.Open();

                SqlCommand cmd = new SqlCommand("Insert Into ServiceExceptionLogs values('"+DateTime.Now+"','"+path+"','"+ex.Message+"','"+ex.StackTrace+"','"+_ServiceName+"')", cn);
                int x = cmd.ExecuteNonQuery();
                
            }
        }
    }

    public static class CommonExceptionLoggingMiddlewareExtensions
    {
        public static IApplicationBuilder UseCommonExceptionLoggingMiddleware(this IApplicationBuilder builder,string serviceName)
        {
            return builder.UseMiddleware<CommonExceptionLoggingMiddleware>(serviceName);
        }
    }
}
