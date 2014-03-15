using System;
using System.Web;
using System.Diagnostics;
using log4net;

namespace PaynearmeCallbacks
{
    public class PnmHttpHandler : IHttpHandler
    {
        private Stopwatch stopWatch;
        private ILog logger;

        public PnmHttpHandler()
        {
             stopWatch = new Stopwatch();
             logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        }

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            stopWatch.Start();
            logger.Info("Incoming request begin.");
            string req = context.Request.CurrentExecutionFilePath;

            try
            {
                if (req.Equals("/authorize"))
                {
                    IAuthorizationHandler handler = new ExampleAuthorizationHandler();
                    handler.HandleAuthorizationRequest(context.Request.QueryString, context.Response);
                }
                else if (req.Equals("/confirm"))
                {
                    string signature = SignatureUtils.Signature(context.Request.QueryString, Constants.SECRET);
                    if (signature.Equals(context.Request.QueryString["signature"]))
                    {
                        IConfirmationHandler handler = new ExampleConfirmationHandler();
                        handler.HandleConfirmationRequest(context.Request.QueryString, context.Response);
                    }
                    else
                    {
                        logger.Warn("Invalid signature for /confirm");
                        logger.Warn("  Got: " + context.Request.QueryString["signature"] + ", expected: " + signature);
                        throw new RequestException("Invalid signature for /confirm", 400);
                    }
                }
                else
                {
                    throw new RequestException("Callback request not found!", 404);
                }
            }
            catch (RequestException e)
            {
                context.Response.ContentType = "text/plain";
                context.Response.StatusCode = e.StatusCode;
                context.Response.Output.WriteLine(e.Message);
                context.Response.Output.Flush();
            }

            stopWatch.Stop();
            logger.Info("Request " + req + " handled in " + stopWatch.ElapsedMilliseconds + "ms");
            if (stopWatch.Elapsed.Seconds >= 6)
            {
                logger.Warn("Request was longer than 6 seconds!");
            }
            logger.Info("End Incoming Request.");
        }
    }
}
