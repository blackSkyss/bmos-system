
using BMOS.BAL.DTOs.Meals;
using BMOS.BAL.DTOs.OrderDetails;
using BMOS.BAL.DTOs.Orders;
using BMOS.BAL.DTOs.ProductMeals;
using BMOS.BAL.DTOs.Products;
using BMOS.BAL.DTOs.Wallets;
using BMOS.BAL.DTOs.Accounts;
using BMOS.BAL.DTOs.Customers;
using BMOS.BAL.DTOs.FireBase;
using BMOS.BAL.DTOs.OrderTransactions;
using BMOS.BAL.DTOs.Staffs;
using BMOS.BAL.DTOs.WalletTransactions;
using BMOS.BAL.DTOs.WalletTransactions.Momo;
using BMOS.BAL.Profiles.Accounts;
using BMOS.BAL.Profiles.Customers;
using BMOS.BAL.Profiles.MealImages;
using BMOS.BAL.Profiles.Meals;
using BMOS.BAL.Profiles.OrderDetails;
using BMOS.BAL.Profiles.Orders;
using BMOS.BAL.Profiles.OrderTransactions;
using BMOS.BAL.Profiles.ProductImages;
using BMOS.BAL.Profiles.ProductMeals;
using BMOS.BAL.Profiles.Products;
using BMOS.BAL.Profiles.Roles;
using BMOS.BAL.Profiles.Staffs;
using BMOS.BAL.Profiles.Tokens;
using BMOS.BAL.Profiles.Wallets;
using BMOS.BAL.Profiles.WalletTransactions;
using BMOS.BAL.Repositories.Implementations;
using BMOS.BAL.Repositories.Interfaces;
using BMOS.BAL.ScheduleJob;
using BMOS.BAL.Validators.Accounts;
using BMOS.BAL.Validators.Customers;
using BMOS.BAL.Validators.Products;
using BMOS.BAL.Scheduling;
using BMOS.BAL.Validators.Orders;
using BMOS.BAL.Validators.WalletTransactions;
using BMOS.DAL.Infrastructures;
using BMOS.WebAPI.Extensions;
using BMOS.WebAPI.Middlewares;
using FluentValidation;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using BMOS.BAL.Validators.Staffs;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using BMOS.BAL.DTOs.JWT;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BMOS.BAL.DTOs.Authentications;
using BMOS.BAL.Validators.Authentication;
using BMOS.BAL.Validators.Meals;
using BMOS.BAL.DTOs;
using System.Reflection;
using BMOS.BAL.DTOs.DashBoard;
using BMOS.BAL.DTOs.ProductImages;
using BMOS.BAL.DTOs.MealImages;
using BMOS.BAL.DTOs.WalletTransactions.Zalopay;

namespace BMOS.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            #region JWT 
            builder.Services.AddSwaggerGen(options =>
            {
                // using System.Reflection;
                var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "FU BMOS Shop Application API",
                    Description = "JWT Authentication API"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 1safsfsdfdfd\"",
                });
                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        new string[]{}
                    }
                });
            });

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,

                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["JwtAuth:Key"])),
                    ValidateLifetime = false,
                    ClockSkew = TimeSpan.Zero
                };
            });
            #endregion

            //ODATA
            var modelBuilder = new ODataConventionModelBuilder();
            
            modelBuilder.EntitySet<GetMealResponse>("Meals");
            modelBuilder.EntitySet<GetWalletsResponse>("Wallets");
            modelBuilder.EntitySet<GetCustomerResponse>("Customers");
            modelBuilder.EntitySet<GetAccountResponse>("Accounts");
            modelBuilder.EntitySet<GetProductResponse>("Products");
            modelBuilder.EntitySet<GetOrderResponse>("Orders");
            modelBuilder.EntitySet<GetStaffDashBoardResponse>("StaffDashBoards");
            modelBuilder.EntitySet<GetStoreOwnerDashBoardResponse>("StoreOwnerDashBoards");
            modelBuilder.EntitySet<GetWalletTransactionResponse>("WalletTransactions");
            modelBuilder.EntitySet<GetStaffResponse>("Staffs");
            modelBuilder.EntitySet<PostLoginResponse>("Authentications");
            modelBuilder.EntityType<GetProductByProductMealsResponse>();

            modelBuilder.ComplexType<GetMealImageResponse>();
            modelBuilder.ComplexType<GetMealFromProduct>();
            modelBuilder.ComplexType<GetProductImageResponse>();

            builder.Services.AddControllers().AddOData(options => options.Select()
                                                                         .Filter()
                                                                         .OrderBy()
                                                                         .Expand()
                                                                         .Count()
                                                                         .SetMaxTop(null)
                                                                         .AddRouteComponents("odata", modelBuilder.GetEdmModel()));

            //Dependency Injections
            builder.Services.Configure<JwtAuth>(builder.Configuration.GetSection("JwtAuth"));
            builder.Services.AddScoped<IDbFactory, DbFactory>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IAccountRepository, AccountRepository>();
            builder.Services.AddScoped<IAuthenticationRepository, AuthenticationRepository>();
            builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
            builder.Services.AddScoped<IMealImageRepository, MealImageRepository>();
            builder.Services.AddScoped<IMealRepository, MealRepository>();
            builder.Services.AddScoped<IWalletRepository, WalletRepository>();
            builder.Services.AddScoped<IWalletTransactionRepository, WalletTransactionRepository>();
            builder.Services.AddScoped<IOrderRepository, OrderRepository>();
            builder.Services.AddScoped<IOrderDetailRepository, OrderDetailRepository>();
            builder.Services.AddScoped<IOrderTransactionRepository, OrderTransactionRepository>();
            builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
            builder.Services.AddScoped<IProductRepository, ProductRepository>();
            builder.Services.AddScoped<IProductMealRepository, ProductMealRepository>();
            builder.Services.AddScoped<IRoleRepository, RoleRepository>();
            builder.Services.AddScoped<IStaffRepository, StaffRepository>();
            builder.Services.AddScoped<ITokenRepository, TokenRepository>();
            builder.Services.AddScoped<IDashBoardRepository, DashBoardRepository>();

            builder.Services.Configure<FireBaseImage>(builder.Configuration.GetSection("FireBaseImage"));

            //DI Validator
            builder.Services.AddScoped<IValidator<ChangePasswordRequest>, ChangePasswordValidation>();
            builder.Services.AddScoped<IValidator<UpdateCustomerRequest>, CustomerValidation>();
            builder.Services.AddScoped<IValidator<PostProductRequest>, PostProductValidation>();
            builder.Services.AddScoped<IValidator<PostMealRequest>, PostMealRequestValidation>();
            builder.Services.AddScoped<IValidator<UpdateMealRequest>, UpdateMealRequestValidation>();
            builder.Services.AddScoped<IValidator<UpdateProductRequest>, UpdateProductValidation>();
            builder.Services.AddScoped<IValidator<RegisterRequest>, RegisterValidation>();
            builder.Services.AddScoped<IValidator<PostStaffRequest>, PostStaffRequestValidation>();
            builder.Services.AddScoped<IValidator<PostAccountRequest>, PostAccountValidation>();
            builder.Services.AddScoped<IValidator<PostRecreateTokenRequest>, PostRecreateTokenValidation>();
            builder.Services.AddScoped<IValidator<PostAccountRequest>, LoginValidation>();
            builder.Services.AddScoped<IValidator<UpdateStaffRequest>, PutStaffRequestValidation>();


            // Momo config
            builder.Services.Configure<MomoConfigModel>(builder.Configuration.GetSection("MomoAPI"));
            // Zalo config
            builder.Services.Configure<ZaloConfigModel>(builder.Configuration.GetSection("ZaloAPI"));

            // Quartz config
            builder.Services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                var jobKeyWallet = new JobKey("WalletTransaction");
                q.AddJob<ProcessingWalletTransaction>(opts => opts.WithIdentity(jobKeyWallet));
                q.AddTrigger(opts => opts
                             .ForJob(jobKeyWallet)
                             .WithIdentity("WalletTransaction-trigger")
                             .WithCronSchedule("0/5 * * * * ?")); // Run every 5 seconds 

                var jobKeyMeal = new JobKey("Meal-update");
                q.AddJob<MealStatusUpdateJob>(opts => opts.WithIdentity(jobKeyMeal));
                q.AddTrigger(opts => opts
                             .ForJob(jobKeyMeal)
                             .WithIdentity("Meal-update-trigger")
                             .WithCronSchedule("0/10 * * * * ?")); // Run every 10 seconds
                
                var jobKeyProduct = new JobKey("product-update");
                q.AddJob<ProductStatusUpdateJob>(opts => opts.WithIdentity(jobKeyProduct));
                q.AddTrigger(opts => opts
                             .ForJob(jobKeyProduct)
                             .WithIdentity("Product-update-trigger")
                             .WithCronSchedule("0/10 * * * * ?")); // Run every 10 seconds
                                                                   //
                var jobKeyOrder = new JobKey("Order-update");
                q.AddJob<OrderProcessingStatusJob>(opts => opts.WithIdentity(jobKeyOrder));
                q.AddTrigger(opts => opts
                             .ForJob(jobKeyOrder)
                             .WithIdentity("Order-update-trigger")
                             .WithCronSchedule("0/10 * * * * ?")); // Run every 10 seconds 
            });
            builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);

            // Valid
            builder.Services.AddScoped<IValidator<PostOrderRequest>, PostOrderRequestValidation>();
            builder.Services.AddScoped<IValidator<PostWalletTransactionRequest>, PostWalletTransactionValidation>();

            // Auto mapper config
            builder.Services.AddAutoMapper(typeof(AccountProfile),
                                            typeof(CustomerProfile),
                                            typeof(MealImageProfile),
                                            typeof(MealProfile),
                                            typeof(WalletProfile),
                                            typeof(WalletTransactionProfile),
                                            typeof(OrderDetailProfile),
                                            typeof(OrderProfile),
                                            typeof(OrderLogProfile),
                                            typeof(PostProductImageProfile),
                                            typeof(ProductMealProfile),
                                            typeof(ProductProfile),
                                            typeof(RoleProfile),
                                            typeof(StaffProfile),
                                            typeof(TokenProfile));

            //Add middleware extentions
            builder.Services.AddTransient<ExceptionMiddleware>();

            //add CORS
            builder.Services.AddCors(cors => cors.AddPolicy(
                                        name: "WebPolicy",
                                        build =>
                                        {
                                            build.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
                                        }
                                    ));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("WebPolicy");

            app.UseAuthentication();

            app.UseAuthorization();

            //Add middleware extentions
            app.ConfigureExceptionMiddleware();

            app.MapControllers();

            app.Run();


        }
    }
}