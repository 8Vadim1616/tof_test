using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
public class NotificationLayout : MonoBehaviour {
    public float desiredWithd;
    public float desiredHeight;
    public float xMargins;
    public float yMargins;
    private RectTransform notification;
	// Use this for initialization
	void Start () {
        notification = GetComponent<RectTransform>();
	}
	
	// Update is called once per frame
	void Update () {
        var _withd = Screen.width + 2 * xMargins > desiredWithd ? desiredWithd : Screen.width+ 2 * xMargins;
        var _height = Screen.height +2*yMargins > desiredHeight ? desiredHeight : Screen.height + 2 * yMargins;
        notification.sizeDelta = new Vector2(_withd, _height);
        notification.anchoredPosition = new Vector2(0, yMargins);
	}
}
