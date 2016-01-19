using System.Collections.Generic;
using System.Globalization;
using System.IO;
using LoLTournament.Models.TournamentApi;
using Newtonsoft.Json;

namespace System.Web.Mvc
{
    public sealed class JsonDotNetValueProviderFactory : ValueProviderFactory
    {
        public override IValueProvider GetValueProvider(ControllerContext controllerContext)
        {
            if (controllerContext == null)
                throw new ArgumentNullException(nameof(controllerContext));

            if (!controllerContext.HttpContext.Request.ContentType.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
                return null;

            var reader = new StreamReader(controllerContext.HttpContext.Request.InputStream);
            var bodyText = reader.ReadToEnd();

            if (string.IsNullOrEmpty(bodyText))
                return null;

            if (controllerContext.HttpContext.Request.FilePath == "/Match/Callback")
            {
                var jsonObject = JsonConvert.DeserializeObject<CallbackResult>(bodyText);

                return new DictionaryValueProvider<CallbackResult>(new Dictionary<string, CallbackResult> { { "obj", jsonObject } }, CultureInfo.CurrentCulture);
            }

            return null;
        }
    }
}
