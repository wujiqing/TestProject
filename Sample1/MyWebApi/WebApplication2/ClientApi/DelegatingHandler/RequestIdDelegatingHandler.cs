using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace WebApplication2.ClientApi.DelegatingHandler
{
    public class RequestIdDelegatingHandler : System.Net.Http.DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("x-guid", Guid.NewGuid().ToString());
            var result = await base.SendAsync(request, cancellationToken); //调用内部handler

            return result;
        }
    }
}
