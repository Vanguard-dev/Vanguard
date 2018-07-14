using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Vanguard.ServerManager.Core.Abstractions;

namespace Vanguard.ServerManager.Core.Extensions
{
    public static class ModelStateExtensions
    {
        public static void AddEntityTransactionErrors(this ModelStateDictionary modelState, IEnumerable<EntityTransactionError> errors)
        {
            foreach (var error in errors)
            {
                modelState.AddModelError(error.Code, error.Description);
            }
        }

        public static void AddIdentityErrors(this ModelStateDictionary modelState, IEnumerable<IdentityError> errors)
        {
            foreach (var error in errors)
            {
                modelState.AddModelError(error.Code, error.Description);
            }
        }
    }
}