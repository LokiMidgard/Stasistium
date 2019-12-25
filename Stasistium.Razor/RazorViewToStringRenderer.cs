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
using System.Text.Encodings.Web;
using Microsoft.Extensions.DependencyInjection;

namespace Stasistium.Razor
{
    // The type is used to instantiate it.
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
    internal class RazorViewToStringRenderer
    {
        private readonly IRazorViewEngine _viewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;
        private readonly RenderConfiguration configuration;

        public RazorViewToStringRenderer(
            IRazorViewEngine viewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider,
            RenderConfiguration configuration)
        {
            this._viewEngine = viewEngine;
            this._tempDataProvider = tempDataProvider;
            this._serviceProvider = serviceProvider;
            this.configuration = configuration;
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

                await view.RenderAsync(viewContext).ConfigureAwait(false);

                return output.ToString();
            }
        }

        private IView FindView(ActionContext actionContext, string viewName)
        {
            var razorPageFactoryProvider = this._serviceProvider.GetRequiredService<IRazorPageFactoryProvider>();
            var razorPageFactoryResult = razorPageFactoryProvider.CreateFactory(Path.Combine(this.configuration.ContentProviderId, viewName));
            var razorPage = razorPageFactoryResult.RazorPageFactory();

            var pageActivator = this._serviceProvider.GetRequiredService<IRazorPageActivator>();
            var htmlEncoder = this._serviceProvider.GetRequiredService<HtmlEncoder>();
            var diagnosticSource = this._serviceProvider.GetRequiredService<DiagnosticSource>();

            IReadOnlyList<IRazorPage>? viewStart = null;

            if (this.configuration.ViewStartId != null)
            {
                var result = razorPageFactoryProvider.CreateFactory(this.configuration.ViewStartId);
                if (result.Success)
                    viewStart = new IRazorPage[] { result.RazorPageFactory() };
            }

            if (viewStart is null)
                viewStart = Array.Empty<IRazorPage>();


            return new RazorView(this._viewEngine, pageActivator, viewStart, razorPage, htmlEncoder, diagnosticSource);
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

        internal static RazorViewToStringRenderer GetRenderer(IEnumerable<IFileProvider> fileProviders, RenderConfiguration renderConfiguration)
        {


            var services = new ServiceCollection();
            var applicationEnvironment = PlatformServices.Default.Application;
            services.AddSingleton(applicationEnvironment);

            var environment = new HostingEnvironment
            {
                ApplicationName = Assembly.GetEntryAssembly().GetName().Name
            };
            services.AddSingleton<IHostingEnvironment>(environment);

            //services.Configure<RazorViewEngineOptions>(options =>
            //{

            //});

            services.AddSingleton(renderConfiguration);

            services.AddSingleton<ObjectPoolProvider, DefaultObjectPoolProvider>();

            // We don't want to dispose something that is used as a singleton somewhere
#pragma warning disable CA2000 // Dispose objects before losing scope
            var diagnosticSource = new DiagnosticListener("Microsoft.AspNetCore");
#pragma warning restore CA2000 // Dispose objects before losing scope
            services.AddSingleton<DiagnosticSource>(diagnosticSource);

            //services.AddSingleton<IRazorViewEngineFileProviderAccessor, DefaultRazorViewEngineFileProviderAccessor>()


            services.AddLogging();
            services.AddMvc()
                .AddRazorOptions(options =>
                {
                    options.ViewLocationExpanders.Clear();
                    options.FileProviders.Clear();
                    options.ViewLocationFormats.Clear();
                    options.PageViewLocationFormats.Clear();

                    options.ViewLocationFormats.Clear();
                    options.ViewLocationExpanders.Clear();
                    options.PageViewLocationFormats.Clear();
                    options.AreaViewLocationFormats.Clear();
                    options.AreaPageViewLocationFormats.Clear();

                    options.ViewLocationFormats.Add("{0}");
                    //options.ViewLocationExpanders.Add("{0}");
                    options.PageViewLocationFormats.Add("{0}");
                    options.AreaViewLocationFormats.Add("{0}");
                    options.AreaPageViewLocationFormats.Add("{0}");



                    //options.ViewLocationFormats.Add($"/{contentId}/{{0}}");
                    //options.PageViewLocationFormats.Add($"/{contentId}/{{0}}");
                    foreach (var provider in fileProviders)
                        options.FileProviders.Add(provider);
                })
                .AddRazorPagesOptions(options =>
                {

                });

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

    public class RenderConfiguration
    {
        public RenderConfiguration(string contentProviderId)
        {
            this.ContentProviderId = contentProviderId ?? throw new ArgumentNullException(nameof(contentProviderId));
        }

        public string ContentProviderId { get; }

        public string? ViewStartId { get; set; }

    }
}
