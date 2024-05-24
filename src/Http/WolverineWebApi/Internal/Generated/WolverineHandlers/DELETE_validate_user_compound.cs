// <auto-generated/>
#pragma warning disable
using Microsoft.AspNetCore.Routing;
using System;
using System.Linq;
using Wolverine.Http;
using Wolverine.Http.FluentValidation;

namespace Internal.Generated.WolverineHandlers
{
    // START: DELETE_validate_user_compound
    public class DELETE_validate_user_compound : Wolverine.Http.HttpHandler
    {
        private readonly Wolverine.Http.WolverineHttpOptions _wolverineHttpOptions;
        private readonly Wolverine.Http.FluentValidation.IProblemDetailSource<WolverineWebApi.Validation.BlockUser> _problemDetailSource;

        public DELETE_validate_user_compound(Wolverine.Http.WolverineHttpOptions wolverineHttpOptions, Wolverine.Http.FluentValidation.IProblemDetailSource<WolverineWebApi.Validation.BlockUser> problemDetailSource) : base(wolverineHttpOptions)
        {
            _wolverineHttpOptions = wolverineHttpOptions;
            _problemDetailSource = problemDetailSource;
        }



        public override async System.Threading.Tasks.Task Handle(Microsoft.AspNetCore.Http.HttpContext httpContext)
        {
            var blockUserValidator = new WolverineWebApi.Validation.BlockUserValidator();
            // Reading the request body via JSON deserialization
            var (cmd, jsonContinue) = await ReadJsonAsync<WolverineWebApi.Validation.BlockUser>(httpContext);
            if (jsonContinue == Wolverine.HandlerContinuation.Stop) return;
            
            // Execute FluentValidation validators
            var result1 = await Wolverine.Http.FluentValidation.Internals.FluentValidationHttpExecutor.ExecuteOne<WolverineWebApi.Validation.BlockUser>(blockUserValidator, _problemDetailSource, cmd).ConfigureAwait(false);

            // Evaluate whether or not the execution should be stopped based on the IResult value
            if (!(result1 is Wolverine.Http.WolverineContinue))
            {
                await result1.ExecuteAsync(httpContext).ConfigureAwait(false);
                return;
            }


            var user = WolverineWebApi.Validation.ValidatedCompoundEndpoint.Load(cmd);
            var result2 = WolverineWebApi.Validation.ValidatedCompoundEndpoint.Validate(user);
            // Evaluate whether or not the execution should be stopped based on the IResult value
            if (!(result2 is Wolverine.Http.WolverineContinue))
            {
                await result2.ExecuteAsync(httpContext).ConfigureAwait(false);
                return;
            }


            
            // The actual HTTP request handler execution
            var result_of_Handle = WolverineWebApi.Validation.ValidatedCompoundEndpoint.Handle(cmd, user);

            await WriteString(httpContext, result_of_Handle);
        }

    }

    // END: DELETE_validate_user_compound
    
    
}

