var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<MovieContext>(options =>
{
    // Environment variables are variables whose values are stored outside your application: on your PC or 
    // in Kubernetes, for example. Doing so helps to keep sensitive information (ex. login credentials) out of 
    // the code and out of our repos, and helps to improve application security.
    
    // In this example, we're going to use environment variables to form a connection string for a database.
    // Using the Shell script file (by running '. ./local-env.sh' in your terminal), we can set all of the variables
    // we need for our dev environment. You can set and display variables in your terminal directly by using Bash commands: 
    // 'export KEY=VALUE' to set a value and 'echo $KEY' to return a variable's value
    
    string dbConnectionString = new NpgsqlConnectionStringBuilder
    {
        Host = Environment.GetEnvironmentVariable("DB_HOST"),
        Port = Convert.ToInt32(Environment.GetEnvironmentVariable("DB_PORT")),
        Database = Environment.GetEnvironmentVariable("DB_NAME"),
        Username = Environment.GetEnvironmentVariable("DB_USER"),
        Password = Environment.GetEnvironmentVariable("DB_PASSWORD")
    }.ConnectionString;
    
    // Using a class such as NpgsqlConnectionStringBuilder isn't required at all: string interpolation 
    // works just fine. For example:

    // string DB_HOST = Environment.GetEnvironmentVariable("DB_HOST");
    // ...
    // string dBConnectionString = $"Host={DB_HOST} ... "; 
    
    // ======|| Important sidenote ||======
    // Environmental variables for your connection string should be retrieved within AddDbContext when using ASP.NET 
    // and Rider.Running your application outside of Rider (ex. running via dotnet run in iTerm) should work with no issue. 
    // However, when using the Run button within Rider, you'll find that none of the environment variables can be 
    // found by your application. Even workarounds such as hardcoding the variables to your .bash_profile are a limited 
    // solution (may be able to run the application but not tests in Rider)
    
    // The connection string is passed in for connection to the database
    options.UseNpgsql(dbConnectionString);
});
builder.Services.AddScoped<IMovieService, MovieService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();