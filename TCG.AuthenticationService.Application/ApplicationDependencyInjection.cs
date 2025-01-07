using System.Reflection;
using GreenPipes;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TCG.AuthenticationService.Application.Consumer;
using TCG.AuthenticationService.Application.Consumer.Messages;
using TCG.Common.MassTransit.Messages;
using TCG.Common.Settings;

namespace TCG.AuthenticationService.Application;

public static class ApplicationDependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        return services.AddMediatR(assembly);
    }

    public static IServiceCollection AddMassTransitWithRabbitMQ(this IServiceCollection serviceCollection)
    {
        //Config masstransit to rabbitmq
        serviceCollection.AddMassTransit(configure =>
        {
            configure.AddConsumer<UserByIdConsumer>();
            configure.AddConsumer<UserByTokenConsumer>();
            configure.AddConsumer<AddFidelityPointMessageConsumer>();
            configure.UsingRabbitMq((context, configurator) =>
            {
                var config = context.GetService<IConfiguration>();
                //On récupère le nom de la table Catalog
                ////On recupère la config de seeting json pour rabbitMQ
                var rabbitMQSettings = config.GetSection(nameof(RabbitMQSettings)).Get<RabbitMQSettings>();
                configurator.Host(new Uri(rabbitMQSettings.Host));
                //Defnir comment les queues sont crées dans rabbit
                configurator.ReceiveEndpoint("authservice", e =>
                {
                    e.UseMessageRetry(r => r.Interval(2, 3000));
                    e.ConfigureConsumer<UserByIdConsumer>(context);
                });
                configurator.ReceiveEndpoint("authservice2", e =>
                {
                    e.UseMessageRetry(r => r.Interval(2, 3000));
                    e.ConfigureConsumer<UserByTokenConsumer>(context);
                });
                configurator.ReceiveEndpoint("added-point-message-queue", e =>
                {
                    e.ConfigureConsumer<AddFidelityPointMessageConsumer>(context);
                });
            });
        });
        //Start rabbitmq bus pour exanges
        serviceCollection.AddMassTransitHostedService();
        return serviceCollection;
    }
}