﻿using AutoMapper;
using CoreCodeCamp.Controllers;
using CoreCodeCamp.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.AspNetCore.Mvc.Versioning.Conventions;
using Microsoft.Extensions.DependencyInjection;

namespace CoreCodeCamp
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<CampContext>();
            services.AddScoped<ICampRepository, CampRepository>();

            //services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies()); //work

            //services.AddAutoMapper(typeof(CampProfile));
#pragma warning disable CS0618 // Type or member is obsolete
            services.AddAutoMapper();
#pragma warning restore CS0618 // Type or member is obsolete

            services.AddApiVersioning(opt =>
            {
                opt.AssumeDefaultVersionWhenUnspecified = true;
                opt.DefaultApiVersion = new ApiVersion(1, 1);
                opt.ReportApiVersions = true;
                opt.ApiVersionReader = new UrlSegmentApiVersionReader();
                //opt.ApiVersionReader = ApiVersionReader.Combine(
                //    new HeaderApiVersionReader("X-Version"),
                //    new QueryStringApiVersionReader("ver"));
                opt.Conventions.Controller<TalksController>()
                    .HasApiVersion(new ApiVersion(1, 0))
                    .HasApiVersion(new ApiVersion(1, 1))
                    .Action(c => c.Delete(default, default))
                    .MapToApiVersion(1, 1);


            });

            services.AddMvc(opt => opt.EnableEndpointRouting = false)
              .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
