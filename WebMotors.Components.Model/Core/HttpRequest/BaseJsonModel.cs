using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using WebMotors.Components.Model.Core;

namespace WebMotors.Components.Model.Core.HttpRequest
{
	public class BaseJsonModel
	{
		protected static T Get<T>(string url, Action<WebRequest> configureRequestMethod)
		{
			var responseContent = RestRequest.sendRequestJson(url, configureRequestMethod);
			return responseContent.FromJson<T>();
		}
	}
}
