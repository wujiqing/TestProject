using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Test.Extensions.Http;

namespace WebApplication2.ClientApi
{
    public class TypedOrderServiceClient
    {
        HttpClient _httpClient;
        public TypedOrderServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> Get()
        { 
            return await _httpClient.GetStringAsync("http://wwww.baidu.com"); //这里使用相对路径来访问
        }

    }
}
