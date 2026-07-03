using Serilog;
using Serilog.Enrichers.Span;

namespace PartnerIntegrationBFFService.API
{
    public static class DependencyInjection
    {
        public static WebApplicationBuilder AddCustomSerilog(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((context, services, configuration) => configuration
                .ReadFrom.Configuration(context.Configuration)
                .ReadFrom.Services(services)
                .Enrich.FromLogContext()
                .Enrich.WithSpan()
            );

            return builder;
        }
    }
}

