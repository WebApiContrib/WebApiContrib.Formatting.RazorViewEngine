﻿using System;
using System.Configuration;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Web.Http;
using WebApiContrib.Formatting.Html.Common;
using WebApiContrib.Formatting.Html.Configuration;
using WebApiContrib.Formatting.Html.Locators;
using WebApiContrib.Formatting.Html.ViewParsers;

namespace WebApiContrib.Formatting.Html.Formatters
{
    public class HtmlMediaTypeViewFormatter : MediaTypeFormatter
    {
        private readonly string _siteRootPath;
        private readonly IViewLocator _viewLocator;
        private readonly IViewParser _viewParser;

        public HtmlMediaTypeViewFormatter() : this(null, null, null)
        {
        }


        public HtmlMediaTypeViewFormatter(string siteRootPath, IViewLocator viewLocator = null, IViewParser viewParser = null)
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xhtml"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xhtml+xml"));

            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));

            if (viewLocator != null)
                _viewLocator = viewLocator;

            if (viewParser != null)
                _viewParser = viewParser;

            _siteRootPath = siteRootPath;
        }


        private IViewLocator ViewLocator
        {
            get
            {
                if (_viewLocator != null)
                    return _viewLocator;

                if (GlobalViews.DefaultViewLocator != null)
                    return GlobalViews.DefaultViewLocator;

                var viewLocator = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof (IViewLocator)) as IViewLocator;

                if (viewLocator != null)
                    return viewLocator;

                throw new ConfigurationErrorsException("No ViewLocator is specidied");
            }
        }


        private IViewParser ViewParser
        {
            get
            {
                if (_viewParser != null)
                    return _viewParser;

                if (GlobalViews.DefaultViewParser != null)
                    return GlobalViews.DefaultViewParser;

                var viewParser = GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IViewParser)) as IViewParser;

                if (viewParser != null)
                    return viewParser;

                throw new ConfigurationErrorsException("No ViewParser is specidied");
            }
        }


        public override bool CanWriteType(Type type)
        {
            return true;
        }


        public override bool CanReadType(Type type)
        {
            return false;
        }


        public override Task<object> ReadFromStreamAsync(
                                                        Type type,
                                                        Stream readStream,
                                                        HttpContent content,
                                                        IFormatterLogger formatterLogger)
        {
            throw new NotSupportedException();
        }


        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
            //Type can be of type HttpError, it will happen if the ApiController throws an exception, there is nothing in this code at the moment
            //that will handle that issue. This code will try to locate a HttpError.cshtml file, even if it exists, the header will be 500 and 
            //the HttpError page will not be displayed, only the default 500 error page.

            return TaskHelpers.RunSync(() =>
            {
                var encoding = SelectCharacterEncoding(content.Headers);

                var parsedView = ParseView(type, value, encoding);

                writeStream.Write(parsedView, 0, parsedView.Length);
                writeStream.Flush();
            });
        }


        private byte[] ParseView(Type type, object model, Encoding encoding)
        {
            var view = model as IView ?? new View(GetViewName(model), model, type);

            var viewTemplate = ViewLocator.GetView(_siteRootPath, view);

            return ViewParser.ParseView(view, viewTemplate, encoding);
        }


        private static string GetViewName(object model)
        {
            var modelType = model.GetType();

            var viewAttributes = modelType.GetCustomAttributes(typeof (ViewAttribute), true)
                                          .OfType<ViewAttribute>()
                                          .ToArray();

            if (viewAttributes.Any())
                return viewAttributes.First().ViewName;
            
            if (GlobalViews.Views.ContainsKey(modelType))
                return GlobalViews.Views[modelType];

            return modelType.Name;
        }
    }
}