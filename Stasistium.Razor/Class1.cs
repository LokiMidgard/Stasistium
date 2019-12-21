using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.ObjectPool;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.Extensions.PlatformAbstractions;
using Microsoft.Extensions.FileProviders;
using System.Collections.Generic;

namespace Stasistium.Razor
{

    public class RazorProvider
    {
        internal RazorProvider(RazorViewToStringRenderer renderer)
        {
            this.Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
        }

        internal RazorViewToStringRenderer Renderer { get; }
    }

    internal class RazorViewToStringRenderer
    {
        private IRazorViewEngine _viewEngine;
        private ITempDataProvider _tempDataProvider;
        private IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            this._viewEngine = viewEngine;
            this._tempDataProvider = tempDataProvider;
            this._serviceProvider = serviceProvider;
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            var actionContext = this.GetActionContext();
            var view = this.FindView(actionContext, viewName);

            using (var output = new StringWriter())
            {
                var viewContext = new ViewContext(
                    actionContext,
                    view,
                    new ViewDataDictionary<TModel>(
                        metadataProvider: new EmptyModelMetadataProvider(),
                        modelState: new ModelStateDictionary())
                    {
                        Model = model
                    },
                    new TempDataDictionary(
                        actionContext.HttpContext,
                        this._tempDataProvider),
                    output,
                    new HtmlHelperOptions());

                await view.RenderAsync(viewContext);

                return output.ToString();
            }
        }

        private IView FindView(ActionContext actionContext, string viewName)
        {
            var getViewResult = this._viewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: true);
            if (getViewResult.Success)
            {
                return getViewResult.View;
            }

            var findViewResult = this._viewEngine.FindView(actionContext, viewName, isMainPage: true);
            if (findViewResult.Success)
            {
                return findViewResult.View;
            }

            var searchedLocations = getViewResult.SearchedLocations.Concat(findViewResult.SearchedLocations);
            var errorMessage = string.Join(
                Environment.NewLine,
                new[] { $"Unable to find view '{viewName}'. The following locations were searched:" }.Concat(searchedLocations)); ;

            throw new InvalidOperationException(errorMessage);
        }

        internal static RazorViewToStringRenderer GetRenderer(IEnumerable<IFileProvider> fileProviders, string contentId)
        {
            var services = new ServiceCollection();
            var applicationEnvironment = PlatformServices.Default.Application;
            services.AddSingleton(applicationEnvironment);
            
            var environment = new HostingEnvironment
            { 
                ApplicationName =  Assembly.GetEntryAssembly().GetName().Name
            };
            services.AddSingleton<IHostingEnvironment>(environment);

            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.FileProviders.Clear();
                options.ViewLocationFormats.Clear();
                options.PageViewLocationFormats.Clear();
                options.ViewLocationFormats.Add($"/{contentId}/{{0}}");
                options.PageViewLocationFormats.Add($"/{contentId}/{{0}}");
                foreach (var provider in fileProviders)
                    options.FileProviders.Add(provider);
                    options.FileProviders.Add(new PhysicalFileProvider(Directory.GetCurrentDirectory()));
            });

            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
            services.AddSingleton<DiagnosticSource>(diagnosticSource);

            services.AddLogging();
            services.AddMvc();
            services.AddSingleton<RazorViewToStringRenderer>();
            var provider = services.BuildServiceProvider();
            return provider.GetRequiredService<RazorViewToStringRenderer>();
        }


        private ActionContext GetActionContext()
        {
            var httpContext = new DefaultHttpContext
            {
                RequestServices = _serviceProvider
            };
            return new ActionContext(httpContext, new RouteData(), new ActionDescriptor());
        }
    }
}
