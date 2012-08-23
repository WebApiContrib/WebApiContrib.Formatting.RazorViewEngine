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

        public HtmlMediaTypeViewFormatter(string siteRootPath = null, IViewLocator viewLocator = null, IViewParser viewParser = null)
        {
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/html"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xhtml"));
            SupportedMediaTypes.Add(new MediaTypeHeaderValue("application/xhtml+xml"));

            SupportedEncodings.Add(new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true));
            SupportedEncodings.Add(new UnicodeEncoding(bigEndian: false, byteOrderMark: true, throwOnInvalidBytes: true));

            _viewLocator = viewLocator;
            _viewParser = viewParser;
            _siteRootPath = siteRootPath;
        }

  
        public override bool CanWriteType(Type type)
        {
            return true;
        }

        public override bool CanReadType(Type type)
        {
            return false;
        }

        public override Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            throw new NotSupportedException();
        }

        public override MediaTypeFormatter GetPerRequestFormatterInstance(Type type, HttpRequestMessage request, MediaTypeHeaderValue mediaType)
        {
            if (_viewLocator == null || _viewParser == null)
            {
                var config = request.GetConfiguration();

                if (config != null)
                {
                    IViewLocator viewLocator = null;
                    IViewParser viewParser = null;

                    var resolver = config.DependencyResolver;

                    if (_viewLocator == null)
                        viewLocator = (IViewLocator) resolver.GetService(typeof (IViewLocator));

                    if (_viewParser == null)
                        viewParser = (IViewParser) resolver.GetService(typeof (IViewParser));

                    return new HtmlMediaTypeViewFormatter(_siteRootPath, viewLocator, viewParser);
                }
            }

            return base.GetPerRequestFormatterInstance(type, request, mediaType);
        }

        public override Task WriteToStreamAsync(Type type, object value, Stream writeStream, HttpContent content, TransportContext transportContext)
        {
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

        private IViewLocator ViewLocator
        {
            get
            {
                if (_viewLocator != null)
                    return _viewLocator;

                if (GlobalViews.DefaultViewLocator != null)
                    return GlobalViews.DefaultViewLocator;

                throw new ConfigurationErrorsException("No ViewLocator is specified");
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

                throw new ConfigurationErrorsException("No ViewParser is specified");
            }
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
