# Skeleton


# EF Migrations
### Add
```
Add-Migration Init -c DataContext -s Skeleton.API -p Skeleton.Data -Args '--environment Local'
```
### Update
```
Update-Database -context DataContext -s Skeleton.API -p Skeleton.Data -Args '--environment Local'
```

# Emails
Easy template system if you make your own emails
Papercut is nice for local development -> https://github.com/ChangemakerStudios/Papercut-SMTP

# Snippets
## Query Timeout with Dapper
```
const int QueryTimeout = 10000;
IEnumerable<SomeClass> result;
try
{
    CancellationTokenSource cts = new(QueryTimeout);
    CommandDefinition cmd = new(commandText: sql, parameters: parameters, cancellationToken: cts.Token);
    using (IDbConnection connection = new SqlConnection(_connectionStringOptions.Foundation))
    {
        result = await connection.QueryAsync<SomeClass>(cmd);
    }
}
catch (TaskCanceledException e)
{
    _logger.LogError(e, "Command timed out and was cancelled");
}
```

## Json Parse
```
T? result;
using (HttpResponseMessage httpResponse = await _httpClient.GetAsync($"foo"))
{
	string responseBody = await httpResponse.Content.ReadAsStringAsync();
	if (!httpResponse.IsSuccessStatusCode)
	{
		throw new HttpRequestException($"Unsuccessful status code, response: {responseBody}");
	}
	try
	{
		JsonSerializerOptions serializerOptions = new()
		{
			UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow			
		};
		result = JsonSerializer.Deserialize<T>(responseBody, serializerOptions);
	}
	catch (JsonException je)
	{
		throw new JsonException($"Unexpected response: '{responseBody}'", je);
	}
}
return result is null ? throw new JsonException("Unexpected response: 'null'") : result;
```