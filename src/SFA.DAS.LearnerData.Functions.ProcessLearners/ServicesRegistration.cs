using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace SFA.DAS.LearnerData.Functions.ProcessLearners;

public class ServicesRegistration(IServiceCollection services, IConfiguration configuration)
{
    public IServiceCollection Register()
    {
        services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), configuration));
        services.AddOptions();

        //services.Configure<ReservationsJobs>(configuration.GetSection("ReservationsJobs"));
        //services.AddSingleton(cfg => cfg.GetService<IOptions<ReservationsJobs>>().Value);

        //var config = configuration.GetSection("ReservationsJobs").Get<ReservationsJobs>();
        //services.AddDasLogging(typeof(Program).Namespace);

        //services.AddDatabaseRegistration(config, configuration["EnvironmentName"]);

        //services.AddTransient<IAzureQueueService, AzureQueueService>();
        //services.AddTransient<IAccountLegalEntitiesService, AccountLegalEntitiesService>();
        //services.AddTransient<IAccountsService, AccountsService>();

        //services.AddHttpClient<IOuterApiClient, OuterApiClient>();

        //services.AddTransient<IAccountLegalEntityRepository, AccountLegalEntityRepository>();
        //services.AddTransient<IAccountRepository, AccountRepository>();

        //services.AddTransient<IAddAccountLegalEntityHandler, AddAccountLegalEntityHandler>();
        //services.AddTransient<IRemoveLegalEntityHandler, RemoveLegalEntityHandler>();
        //services.AddTransient<ISignedLegalAgreementHandler, SignedLegalAgreementHandler>();
        //services.AddTransient<ILevyAddedToAccountHandler, LevyAddedToAccountHandler>();
        //services.AddTransient<IAddAccountHandler, AddAccountHandler>();
        //services.AddTransient<IAccountNameUpdatedHandler, AccountNameUpdatedHandler>();

        //services.AddSingleton<IValidator<AddedLegalEntityEvent>, AddAccountLegalEntityValidator>();

        return services;
    }
}