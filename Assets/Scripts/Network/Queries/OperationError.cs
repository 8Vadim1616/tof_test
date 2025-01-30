using System;

namespace Assets.Scripts.Network.Queries
{
	public class OperationException : Exception
	{
        public QueryErrorType ErrorType { get; private set; }

	    public OperationException(QueryErrorType errortype, string message) : base(message)
	    {
	        ErrorType = errortype;
	    }

	    public OperationException(QueryErrorType errortype) : this(errortype, "No Discription") { }

        public override string ToString()
	    {
	        return string.Format("OperationException {0}: {1}", ErrorType, Message);
	    }
	}
}
