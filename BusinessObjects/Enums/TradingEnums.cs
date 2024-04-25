using System;
namespace BusinessObjects.Enums
{
	public enum TradeStatus
	{
        //Values: submited, on delivery, verified, successful, cancel
        NotSubmitted, //0
        Submitted, //1
        OnDeliveryToMiddle, //2
        MiddleReceived, //3
        WaitFoeChecklistConfirm,//4
        Cancel, //5
        OnDevliveryToTrader, //6
        Successful, //7
    }

    public enum TraderType
    {
        PostOwner, //0 
        Interester //1
    }
}

