wa
==
A simple alternative to MVC and Web API for Owin. Uses SimpleInjector.

# Usage

You need an Owin provider, e.g. Microsoft.Owin.Host.HttpListener. First, create a Startup class:
```C#
public class Startup
{
    public void Configuration(IAppBuilder appBuilder)
    {
        var container = new Container();
        container.Register<IWaConfig, WaConfig>(Lifestyle.Singleton);
        container.Register<ICurrentContext, CurrentContext>(new LifetimeScopeLifestyle());
            
        var config = container.GetInstance<IWaConfig>();
        config.Configure(container);
        config.AddStaticDirectory("Content");
        config.AddStaticDirectory("js");
        config.AddStaticDirectory("Scripts");
        config.JsonSerializerSettings = new Newtonsoft.Json.JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() };
        appBuilder.UseWa(config);

        appBuilder.Run(context =>
        {
            context.Response.StatusCode = 404;
            context.Response.WriteAsync("Unable to find machintruc");
            return Task.Delay(0);
        });
    }
}
```

You can now declare controllers as classes that extends the Controller or ApiController classes. The first one should contain methods that returns Views, the second one should contain methods that returns Json objects.

The route to a method is declared using attributes that uses a subset of Web API attribute routing.

Example of a Controller (Views don't support any rendering engine, you must implement the IView interface):

```C#
[RoutePrefix("")]
public class HomeController: Controller
{
  [Route("")]
  public ActionResult Index()
  {
    return View("HomeIndex");
  }
}


public class HomeIndexView: IView
{
  public string Render(ICurrentContext context, object model)
  {
    return "Hello world!";
  }
}
```

Example of an ApiController:

```C#
[RoutePrefix("api/user")]
public class UserController: ApiController
{
  private Dictionnary<int, User> users;
  private int nextId;

  [Route("")]
  [HttpGet]
  public IEnumerable<User> GetAll([FromUri] offset, [FromUri] limit)
  {
    return users.Values.Skip(offset).Take(limit);
  }
  
  [Route("{id:int}", Name="GetUser")]
  [HttpGet]
  public string GetById(int id)
  {
    return users[id];
  }
  
  [Route("")]
  [HttpPost]
  public IActionResult<User> Post([FromBody]User user)
  {
    if (user == null)
      return BadRequest<User>();
    users.Add(nextId++, user);
    return Created("GetUser", nextId-1, user);
  }
}
```


