using JWT;
using JWT.Algorithms;
using JWT.Exceptions;
using JWT.Serializers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HrcApi
{
    public class JwtTools
    {
        public static string Key { get; set; } = "CSHISYOUDIE";
        public static string Encoder(Dictionary<string, object> payload, string key = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                key = Key;
            }
            IJwtAlgorithm jwtAlgorithm = new HMACSHA256Algorithm();
            IJsonSerializer jsonSerializer = new JsonNetSerializer();
            IBase64UrlEncoder base64UrlEncoder = new JwtBase64UrlEncoder();
            IJwtEncoder jwtEncoder = new JwtEncoder(jwtAlgorithm, jsonSerializer, base64UrlEncoder);
            payload.Add("timeout", DateTime.Now.AddHours(1));
            var token = jwtEncoder.Encode(payload, key);
            return token;
        }
        public static Dictionary<string, object> Decode(string jwtStr, string key = null)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                key = Key;
            }
            try
            {
                IJsonSerializer jsonSerializer = new JsonNetSerializer();
                IDateTimeProvider dateTimeProvider = new UtcDateTimeProvider();
                IJwtValidator jwtValidator = new JwtValidator(jsonSerializer, dateTimeProvider);
                IAlgorithmFactory algorithmFactory = new HMACSHAAlgorithmFactory();
                IJwtAlgorithm jwtAlgorithm = new HMACSHA256Algorithm();
                IBase64UrlEncoder base64UrlEncoder = new JwtBase64UrlEncoder();
                IJwtDecoder jwtDecoder = new JwtDecoder(jsonSerializer, jwtValidator, base64UrlEncoder, algorithmFactory);
                var json = jwtDecoder.Decode(token: jwtStr, key, verify: true);
                var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(json);
                if (Convert.ToDateTime(result["timeout"]) < DateTime.Now)
                {
                    throw new Exception(message: "token已过期请重新登录");
                }
                else
                {
                    result.Remove(key: "timeout");
                }
                return result;
            }
            catch (TokenExpiredException)
            {

                throw;
            }
        }
    }
}