using BasketAPI.Services;
using Microsoft.Extensions.Caching.Memory;
using System.Net.Http.Headers;

namespace CodeChallengeApiClientNamespace
{
    public partial class CodeChallengeApiClient
    {

        //This partial method is given by the NSwag toolchain generated code in order to modify the requests
        //In this case we use it to add authentication
        partial void PrepareRequest(System.Net.Http.HttpClient client, System.Net.Http.HttpRequestMessage request, string url)
        {

            //If the request is a Login request, there is no need for authentication
            if (request.RequestUri != null && request.RequestUri.LocalPath == "/api/Login")
            {
                return;
            }

            string token = string.Empty;
            _storageService.GetToken();

            if (!(token is null || token == string.Empty))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }else
            {
                LoginRequest loginrequest = new LoginRequest()
                {
                    Email = _configuration["LoginEmail"]
                };

                LoginResponse result;
                try
                {
                    var tokenTask = LoginAsync(loginrequest);
                    tokenTask.Wait();
                    LoginResponse response = tokenTask.Result;
                    _storageService.StoreToken(response.Token);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", response.Token);
                }
                catch (Exception ex)
                {
                    string message = ex.Message;
                    Console.WriteLine(message);
                }
            }
            
        }

        
    }
}

