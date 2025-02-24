using UnityEngine;
using System.Collections;
using com.playGenesis.VkUnityPlugin;
//using JsonFx.Json;
using System.Collections.Generic;
using System;
using com.playGenesis.VkUnityPlugin.MiniJSON;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;

public class Controller : MonoBehaviour
{
    public VkApi vkapi;
    public Downloader d;
    public List<VKUser> friends = new List<VKUser>();
    public VkSettings sets;
    // Use this for initialization

    void Start()
    {
        sets = VkApi.VkSetts;
        vkapi = VkApi.VkApiInstance;
        d = vkapi.gameObject.GetComponent<Downloader>();
        if (vkapi.IsUserLoggedIn)
        {
            startWorkingWithVk();
        }
        else
        {
            vkapi.LoggedIn += startWorkingWithVk;
            vkapi.Login();
        }
    }

    public void Login()
    {
        vkapi.Login();
    }
    public void LogOut()
    {
        vkapi.Logout();
        sets.forceOAuth = true;
        sets.revoke = true;

    }

    public void startWorkingWithVk()
    {
        if (VkApi.CurrentToken.TokenValidFor() < 120)
            Login();
        Get3FriendsDataFromVk();
    }

    public void Get3FriendsDataFromVk()
    {
        var request = new VKRequest()
        {
            url = "friends.get?user_id=" + VkApi.CurrentToken.user_id + "&count=3&fields=photo_200",
            CallBackFunction = OnGet5FriendsCompleted,
        };
        vkapi.Call(request);
        //vkapi.call принимает 3 параметра строку запроса, функцию обработчика запроса,
        //и массив обеъктов(можно передать любые объекты, их пожно использовать в обработчике
    }
    void OnGet5FriendsCompleted(VKRequest arg1)
    {
        //проверяем на ошибки
        if (arg1.error != null)
        {
            if (arg1.error.error_code == "5")
            {
                SceneManager.LoadScene("LoginScene");
            }
            else
                FindObjectOfType<GlobalErrorHandler>().Notification.Notify(arg1);
            return;
        }

        var dict = Json.Deserialize(arg1.response) as Dictionary<string, object>;
        var resp = (Dictionary<string, object>)dict["response"];
        var items = (List<object>)resp["items"];

        foreach (var item in items)
        {
            friends.Add(VKUser.Deserialize(item));
        }
        for (var i = 0; i < friends.Count; i++)
        {
            var friendsOnScene = GameObject.FindObjectsOfType<FriendManager>();
            friendsOnScene[i].t.text = friends[i].first_name + " " + friends[i].last_name;
            friendsOnScene[i].friend = friends[i];

        }
    }
    public void Back()
    {
        SceneManager.LoadScene("StarterScene");
    }
}
