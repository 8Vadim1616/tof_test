﻿#if UNITY_EDITOR || !UNITY_WEBGL && !UNITY_WSA
using HuaweiMobileServices.AppMessaging;
using HuaweiMobileServices.Base;
using HuaweiMobileServices.Id;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class HMSAppMessagingManager : HMSSingleton<HMSAppMessagingManager>
{
    public Action<AppMessage> OnMessageClicked { get; set; }
    public Action<AppMessage> OnMessageDisplay { get; set; }

    public Action<AppMessage, DismissType> OnMessageDissmiss { get; set; }
    public Action<AAIDResult> AAIDResultAction { get; set; }

    void Start()
    {
        Debug.Log("AppMessaging: Start");

        HmsInstanceId inst = HmsInstanceId.GetInstance();
        ITask<AAIDResult> idResult = inst.AAID;
        idResult.AddOnSuccessListener((result) =>
        {
            AAIDResult AAIDResult = result;
            Debug.Log("AppMessaging: " + result.Id);
            AAIDResultAction?.Invoke(result);
        }).AddOnFailureListener((exception) =>
        {

        });
        OnMessageClicked = OnMessageClickFunction;
        OnMessageDisplay = OnMessageDisplayFunction;
        OnMessageDissmiss = OnMessageDissmissFunction;
        AGConnectAppMessaging appMessaging = AGConnectAppMessaging.Instance;
        appMessaging.AddOnClickListener(OnMessageClicked);
        appMessaging.AddOnDisplayListener(OnMessageDisplay);
        appMessaging.AddOnDismissListener(OnMessageDissmiss);
        appMessaging.SetForceFetch();
    }

    private void OnMessageClickFunction(AppMessage obj)
    {
        Debug.Log("AppMessaging  OnMessageClickFunction");
    }
    private void OnMessageDisplayFunction(AppMessage obj)
    {
        Debug.Log("AppMessaging OnMessageDisplayFunction" + obj.MessageType);
    }

    private void OnMessageDissmissFunction(AppMessage obj, DismissType dismissType)
    {
        Debug.Log("AppMessaging OnMessageDissmissFunction" + obj.MessageType);
    }
}
#endif
