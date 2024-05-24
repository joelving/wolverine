// <auto-generated/>
#pragma warning disable
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Wolverine.Http;
using Wolverine.Runtime;

namespace Internal.Generated.WolverineHandlers
{
    // START: POST_ef_publish
    public class POST_ef_publish : Wolverine.Http.HttpHandler
    {
        private readonly Wolverine.Http.WolverineHttpOptions _wolverineHttpOptions;
        private readonly Wolverine.Runtime.IWolverineRuntime _wolverineRuntime;
        private readonly Microsoft.EntityFrameworkCore.DbContextOptions<WolverineWebApi.ItemsDbContext> _dbContextOptions;

        public POST_ef_publish(Wolverine.Http.WolverineHttpOptions wolverineHttpOptions, Wolverine.Runtime.IWolverineRuntime wolverineRuntime, Microsoft.EntityFrameworkCore.DbContextOptions<WolverineWebApi.ItemsDbContext> dbContextOptions) : base(wolverineHttpOptions)
        {
            _wolverineHttpOptions = wolverineHttpOptions;
            _wolverineRuntime = wolverineRuntime;
            _dbContextOptions = dbContextOptions;
        }



        public override async System.Threading.Tasks.Task Handle(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var efCoreEndpoints = new WolverineWebApi.EfCoreEndpoints();
            var messageContext = new Wolverine.Runtime.MessageContext(_wolverineRuntime);
            var itemsDbContext = new WolverineWebApi.ItemsDbContext(_dbContextOptions);
            Wolverine.Http.Runtime.RequestIdMiddleware.Apply(httpContext, messageContext);
            
            // Enroll the DbContext & IMessagingContext in the outgoing Wolverine outbox transaction
            var envelopeTransaction = Wolverine.EntityFrameworkCore.WolverineEntityCoreExtensions.BuildTransaction(itemsDbContext, messageContext);
            await messageContext.EnlistInOutboxAsync(envelopeTransaction);
            // Reading the request body via JSON deserialization
            var (command, jsonContinue) = await ReadJsonAsync<WolverineWebApi.CreateItemCommand>(httpContext);
            if (jsonContinue == Wolverine.HandlerContinuation.Stop) return;
            
            // The actual HTTP request handler execution
            await efCoreEndpoints.PublishItem(command, itemsDbContext, messageContext).ConfigureAwait(false);

            
            // Added by EF Core Transaction Middleware
            var result_of_SaveChangesAsync = await itemsDbContext.SaveChangesAsync(httpContext.RequestAborted).ConfigureAwait(false);

            // If we have separate context for outbox and application, then we need to manually commit the transaction
            if (envelopeTransaction is Wolverine.EntityFrameworkCore.Internals.RawDatabaseEnvelopeTransaction rawTx) { await rawTx.CommitAsync(); }
            await messageContext.FlushOutgoingMessagesAsync().ConfigureAwait(false);
            // Wolverine automatically sets the status code to 204 for empty responses
            if (!httpContext.Response.HasStarted) httpContext.Response.StatusCode = 204;
        }

    }

    // END: POST_ef_publish
    
    
}

