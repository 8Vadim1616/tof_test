using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using com.playGenesis.VkUnityPlugin.MiniJSON;

public class MiniJsonExample : MonoBehaviour {

	// Use this for initialization
	void Start () {

		var jsonString = "{ \"array\": [1.44,2,3], " +
							"\"object\": {\"key1\":\"value1\", \"key2\":256}, " +
							"\"string\": \"The quick brown fox \\\"jumps\\\" over the lazy dog \", " +
							"\"unicode\": \"\\u3041 Men\\u00fa sesi\\u00f3n\", " +
							"\"int\": 65536, " +
							"\"float\": 3.1415926, " +
							"\"bool\": true, " +
							"\"null\": null }";

		var dict = Json.Deserialize(jsonString) as Dictionary<string, object>;

		Debug.Log("deserialized: " + dict.GetType());
		Debug.Log("dict['array'][0]: " + ((dict["array"]) as List<object>)[0]);
		Debug.Log("dict['string']: " + dict["string"] as string);
		Debug.Log("dict['float']: " + dict["float"]); // floats come out as doubles
		Debug.Log("dict['int']: " + dict["int"]); // ints come out as longs
		Debug.Log("dict['unicode']: " + dict["unicode"] as string);

		var str = Json.Serialize(dict);

		Debug.Log("serialized: " + str);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
