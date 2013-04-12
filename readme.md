WebApiContrib.Formatting.Razor
=============

A 'MediaTypeFormatter' for generating HTML markup for [ASP.NET Web API](http://asp.net/web-api) applications.

Overview

Before we dive into the details, here is a simple example of using the [`HtmlMediaTypeViewFormatter`](https://github.com/WebApiContrib/WebApiContrib.Formatting.Html/blob/master/src/WebApiContrib.Formatting.Html/Formatters/HtmlMediaTypeViewFormatter.cs) with Razor:

    public class CustomerController : ApiController
    {
        public Customer Get()
        {
            return new Customer { Name = "John Doe", Country = "Country" };
        }
    }

This controller will simply return a Customer as JSON or XAML based on the "Accept" or "content-type" header.

By adding the HtmlMediaTypeViewFormatter to the global.asax and using the [`RazorViewLocator`](https://github.com/WebApiContrib/WebApiContrib.Formatting.Razor/blob/master/src/WebApiContrib.Formatting.Razor/RazorViewLocator.cs) and a [`RazorViewParser`](https://github.com/WebApiContrib/WebApiContrib.Formatting.Razor/blob/master/src/WebApiContrib.Formatting.Razor/RazorViewParser.cs) the code can also render the model as  HTML by using a Razor view. By default the view will be located by convention, using the name of the returned model in this case "Customer.cshtml" or "Customer.vbhtml".
    
    protected void Application_Start()
    {
        //...

        GlobalConfiguration.Configuration.Formatters.Add(new HtmlMediaTypeViewFormatter());
        
        GlobalViews.DefaultViewParser = new RazorViewParser();
        GlobalViews.DefaultViewLocator = new RazorViewLocator();
        
        //...
    }

The [`GlobalViews`](https://github.com/WebApiContrib/WebApiContrib.Formatting.Html/blob/master/src/WebApiContrib.Formatting.Html/Configuration/GlobalViews.cs) and [`HtmlMediaTypeViewFormatter`](https://github.com/WebApiContrib/WebApiContrib.Formatting.Html/blob/master/src/WebApiContrib.Formatting.Html/Formatters/HtmlMediaTypeViewFormatter.cs) comes from the [WebApiContrib.Formatting.Html](https://github.com/WebApiContrib/WebApiContrib.Formatting.Html) project.

Views should be added to the web api projects ["~/Views"](https://github.com/WebApiContrib/WebApiContrib.Formatting.Razor/tree/master/samples/MvcWebApiSiteTest/Views) folder. Here is an example of a View:

    <html>Hello @Model.Name! Welcome to Razor!</html>

2 Install packages

Get it from [NuGet](http://nuget.org/packages/WebApiContrib.Formatting.Razor):

    Install-Package WebApiContrib.Formatting.Razor

3 How the View locator works

The razor view locator [`RazorViewLocator`](https://github.com/WebApiContrib/WebApiContrib.Formatting.Razor/blob/master/src/WebApiContrib.Formatting.Razor/RazorViewLocator.cs) vill try to locate a view by using either, convention, annotation, configuration or by returning a View. The views must be located in the follwoing folders:

    ~\Views
    ~\Views\Shared

The "~" is the root folder of the web application. Both .cshtml and .vbhtml are supported.

Note: There is no way to specify another paths at this moment, it's easy to implement a view locator. It's done by implementing the [`IViewLocator`](https://github.com/WebApiContrib/WebApiContrib.Formatting.Html/blob/master/src/WebApiContrib.Formatting.Html/Locators/IViewLocator.cs) interface located in the [WebApiContrib.Formatting.Html](https://github.com/WebApiContrib/WebApiContrib.Formatting.Html) assembly.

3.1 Using convention

By default the RazorViewLocator will locate a view by convention, it will try to find a view by the name of the model returned by the ApiController's methods. The following code will try to find a view with the name "Customer" in the "Views" or "Views\Shared" folder and use it to render the returnd 

Customer model:


    public class CustomerController : ApiController
    {
        public Customer Get()
        {
            return new Customer { Name = "John Doe", Country = "Country" };
        }
    }

To use the ApiController to render the Customer model with the Customer view, just use the following path in browser:

http://localhost/mysite/customer

3.2 Using annotation

A view could be specified for a specific mode by adding the [`ViewAttribute`](https://github.com/WebApiContrib/WebApiContrib.Formatting.Razor/blob/master/samples/MvcWebApiSiteTest/Controllers/HomeController.cs#L10) to the model returned by a ApiController method.


    public class CustomerController : ApiController
    {
        public Customer Get()
        {
            return new Customer { Name = "John Doe", Country = "Country" };
        }
    }
        
    [View("CustomerViaAttrib")]
    public class Customer
    {
        public string Name { get; set; }

        public string Country { get; set; }
    }

This code will try to locate a view with the name "CustomerViaAttrib".
