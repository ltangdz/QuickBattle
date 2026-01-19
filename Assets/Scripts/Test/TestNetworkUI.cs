using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TestNetworkUI : BaseUI
{
    public Button startServerButton;
    public Button startClientButton;

    private void Start()
    {
        startServerButton.onClick.AddListener(OnStartServerButtonClicked);
        startClientButton.onClick.AddListener(OnStartClientButtonClicked);
    }

    private void OnStartServerButtonClicked()
    {
        NetworkManager.Singleton.StartServer();
        Hide();
    }

    private void OnStartClientButtonClicked()
    {
        NetworkManager.Singleton.StartClient();
        Hide();
    }
}
