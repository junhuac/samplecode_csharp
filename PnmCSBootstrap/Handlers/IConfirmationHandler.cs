using System;
using System.Collections.Specialized;
using System.Web;

namespace PaynearmeCallbacks
{
	public interface IConfirmationHandler
	{
		void HandleConfirmationRequest(NameValueCollection parameters, HttpResponse response);
	}
}

