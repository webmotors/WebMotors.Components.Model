using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace WebMotors.Components.Model.Core.HttpRequest
{
	internal class RestRequest
	{
		internal static string sendRequestJson(string url, Action<WebRequest> configureRequestMethod)
		{
			WebRequest request = WebRequest.Create(url);
			configureRequestMethod(request);

			using (var response = request.GetResponse() as System.Net.HttpWebResponse)
			{
				using (var reader = new System.IO.StreamReader(response.GetResponseStream()))
				{
					return reader.ReadToEnd();
				}
			}
		}
	}
}
