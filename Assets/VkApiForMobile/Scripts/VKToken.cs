using UnityEngine;
using System.Collections;
using System;

namespace com.playGenesis.VkUnityPlugin
{
    public class VKToken : EventArgs
    {
        /// <summary>
        /// access token string
        /// </summary>
        public string access_token;
        
        /// <summary>
        /// second until token expires
        /// </summary>
        public int expires_in;
        public string user_id;

        /// <summary>
        /// time token was created
        /// </summary>
        public DateTime tokenRecievedTime;
        
        /// <summary>
        /// vk user emil
        /// </summary>
        public string email;

  
        /// <param name="message">
        /// Serialized token with formt: token#expires_in#user_id#email
        /// </param>
        public static VKToken ParseSerializeTokenFromNaviteSdk(string message)
        {
            //serialization format: token#expires_in#user_id

            string[] token = message.Split('#');
            VKToken ti = new VKToken();

            ti.access_token = token[0];
            ti.tokenRecievedTime = DateTime.Now;
            ti.expires_in = int.Parse(token[1]) == 0 ? 999999 : int.Parse(token[1]);
            ti.user_id = token[2];
            if(token.Length==4){
                ti.email = token[3].Replace("%40","@");
            }
            return ti;
        }

        public static bool IsTokenNotExpired(VKToken ti)
        {
            var currentToken = ti;
            var isvalid = currentToken.tokenRecievedTime.AddSeconds(currentToken.expires_in) > DateTime.Now
                ? true
                : false;
            return isvalid;
        }

        /// <summary>
        /// Checks current token expiration time left
        /// </summary>
        /// <returns>secons until token expires</returns>
        public int TokenValidFor()
        {
            if(VkApi.CurrentToken ==null)
                return 0;
            //checks whether datetime has defaul value;
            if(VkApi.CurrentToken.tokenRecievedTime == new DateTime())
                return 0;
            var currentToken = VkApi.CurrentToken;
            return
                (int) (currentToken.tokenRecievedTime.AddSeconds(currentToken.expires_in) - DateTime.Now).TotalSeconds;
        }
        /// <summary>
        /// resets current token 
        /// </summary>
        public static void ResetToken()
        {
			VkApi.CurrentToken = new VKToken{
				access_token="",
				tokenRecievedTime=DateTime.Parse("1/1/1992"),
				expires_in=1,
				user_id="",
			};
            PlayerPrefs.SetString("vkaccess_token", "");
            PlayerPrefs.SetInt("vkexpires_in", 0);
            PlayerPrefs.SetString("vkuser_id", "");
            PlayerPrefs.SetString("vktokenRecievedTime", "1/1/1992");
            PlayerPrefs.SetString("vkemail","");

        }
        /// <summary>
        /// persists token on device
        /// </summary>
        public void Save()
        {
            PlayerPrefs.SetString("vkaccess_token", access_token);
            PlayerPrefs.SetInt("vkexpires_in", expires_in);
            PlayerPrefs.SetString("vkuser_id", user_id);
            PlayerPrefs.SetString("vktokenRecievedTime", tokenRecievedTime.ToString());
            PlayerPrefs.SetString("vkemail",email);
        }

        /// <summary>
        /// Loads persisted token
        /// </summary>
        public static VKToken LoadPersistent()
        {

            DateTime recievedtokentime= DateTime.Parse("1/1/1990");
            DateTime.TryParse(PlayerPrefs.GetString("vktokenRecievedTime", ""), out recievedtokentime);
            return new VKToken
            {
                access_token = PlayerPrefs.GetString("vkaccess_token", ""),
                expires_in = PlayerPrefs.GetInt("vkexpires_in", 0),
                tokenRecievedTime = recievedtokentime,
                user_id = PlayerPrefs.GetString("vkuser_id", ""),
                email = PlayerPrefs.GetString("vkemail", "")
            };
        }
        /// <summary>
        /// creates serialized token object from url
        /// </summary>
        /// <param name="url">url to which user is redirected after succesful authorisation</param>
        /// <returns>serialized token with formt: token#expires_in#user_id#email</returns>
        public static string ParseFromAuthUrl(string url)
        {
            var prms = url.Split('#')[1].Split('&');
            string access_token="", expires_in="", user_id = "", email = "";
            
            foreach (var p in prms)
            {
                var pName = p.Split('=')[0];
                var pValue= p.Split('=')[1];

                if (pName == "access_token")
                {
                    access_token = pValue;
                }else if (pName == "expires_in")
                {
                    expires_in = pValue;
                }else if (pName == "user_id")
                {
                    user_id = pValue;
                }else if (pName == "email")
                {
                    email =pValue;
                }

            }
            var result = access_token + "#" + expires_in + "#" + user_id;
            result = email == ""? result: result + "#" + email;
            return result;
        }
    }
}
