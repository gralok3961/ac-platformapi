// System
global using System.Net;
global using System.Text;
global using Microsoft.AspNetCore.Authentication;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Options;

// Microsoft
global using Microsoft.AspNetCore.Authentication.JwtBearer;
global using Microsoft.AspNetCore.Authorization;
global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.IdentityModel.Tokens;

// Azure
global using Azure.Identity;
global using Azure.Security.KeyVault.Secrets;

// API
global using API.Extensions;
global using Microsoft.Extensions.Logging.AzureAppServices;

// Base
global using Base.Controllers;
global using Base.Custom.HashCross.Helpers;
global using Base.Errors;
global using Base.Extensions;
global using Base.Helpers;
global using Base.Helpers.Dtos;
global using Base.Middlewares;
global using Base.Services.JWTToken;
global using Base.Custom.HashCross.Dtos;
// Core
global using Core.Dtos;
global using Core.Entities;
global using Core.Interfaces.Repositories;
global using Core.Interfaces.Services;
global using Core.Interfaces.Services.UCP;
global using Core.Helpers;
// Infrastructure
global using Infrastructure.Data;
global using Infrastructure.Data.Repositories;
global using Infrastructure.Helpers;
global using Infrastructure.Services;
global using Infrastructure.Services.Auth;
global using Infrastructure.Services.UCP;
// Static
global using static Base.Custom.HashCross.Helpers.HashCrossEnum;
