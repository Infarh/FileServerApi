﻿using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FileServerApi.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;

namespace FileServerApi.Services;

public class JWTProvider : IJWTProvider
{
    private readonly IConfiguration _Configuration;

    public JWTProvider(IConfiguration Configuration) => _Configuration = Configuration;

    public (string Token, DateTime Expires) GetToken(string User, string Role, DateTime Time)
    {
        var key = _Configuration["JwtAuth:Key"];
        var issuer = _Configuration["JwtAuth:Issuer"];
        var audience = _Configuration["JwtAuth:Audience"];

        var security_key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(security_key, SecurityAlgorithms.HmacSha256);

        Claim[] claims =
        {
            new(JwtRegisteredClaimNames.Sub, User),
            new("roles", Role),
            new("Date", Time.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expires = DateTime.Now.AddMinutes(120);
        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expires,
            signingCredentials: credentials);

        var token_str = new JwtSecurityTokenHandler().WriteToken(token);

        return (token_str, expires);
    }
}