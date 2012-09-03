using System;
using RazorEngine.Configuration;
using RazorEngine.Templating;
using WebApiContrib.Formatting.Html;
using WebApiContrib.Formatting.Html.ViewParsers;


namespace WebApiContrib.Formatting.Razor
{
    public class RazorViewParser : IViewParser
    {
        private ITemplateService _templateService;


        public RazorViewParser()
        {
            //var config = new TemplateServiceConfiguration { Resolver = new TemplateResolver() };
            //_templateService = new TemplateService(config);
        }

        
        public RazorViewParser(ITemplateService templateService)
		{
			if (templateService == null)
				throw new ArgumentNullException("templateService");

			_templateService = templateService;
		}


        public RazorViewParser(ITemplateResolver resolver)
		{
			if (resolver == null)
				throw new ArgumentNullException("resolver");

			var config = new TemplateServiceConfiguration { Resolver = resolver };
			_templateService = new TemplateService(config);
		}
        
        
        public byte[] ParseView(IView view, string viewTemplate, System.Text.Encoding encoding)
        {
            var parsedView = GetParsedView(view, viewTemplate);

            return encoding.GetBytes(parsedView);
        }

        private string GetParsedView(IView view, string viewTemplate)
        {
            var implicitSetWorkaround = false;
            
            // 1* THIS IF STATEMENT IS ONLY ADDED AS A TEMPORARY WORKAROUND
            // There is probably a bug in the RazorEngine when trying to set the Model property of a template
            // next time this code runs.
            // A new instance need to be created, I think it has to do with a cache the TemplateService uses.
            if (_templateService == null)
            {
                var config = new TemplateServiceConfiguration {Resolver = new TemplateResolver()};
                _templateService = new TemplateService(config);

                implicitSetWorkaround = true;
            }



            string result = string.Empty;

            if (view.ModelType == null)
            {
                _templateService.Compile(viewTemplate, view.ViewName);
                result = _templateService.Run(view.ViewName);
            }
            else
            {
                _templateService.Compile(viewTemplate, view.ModelType, view.ViewName);
                result = _templateService.Run(view.ViewName, view.Model);
            }

            // Workaround to solve a bug, see comment 1* above.
            if (implicitSetWorkaround)
                _templateService = null;

            return result;
        }
    }
}
