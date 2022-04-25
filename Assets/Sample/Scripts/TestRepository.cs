using System.Collections;
using System.Collections.Generic;
using GameFoundation.State;
using UnityEngine;
using UnityEngine.UI;

public class TestRepository : MonoBehaviour
{
    public InputField hpInput;
    public Text logText;

    private PlayerModel player;

    void Start()
    {
        // register player model
        player = new PlayerModel();
        player.Register();
    }

    public void UpdateHp()
    {
        player.hp = int.Parse(hpInput.text);
        player.hpRx.Value = player.hp;
        player.hpRxCollection.Clear();
        for (var i = 0; i < 5; i++)
        {
            player.hpRxCollection.Add(player.hp);
        }
        logText.text = "Updated";
    }

    public void LogHp()
    {
        Debug.Log(player.hp);
        Debug.Log(player.hpRx.Value);
        Debug.Log(string.Join(",", player.hpRxCollection));

        logText.text = string.Format("hp: {0}\nhpRx: {1}\nhpRxCollection: {2}", player.hp, player.hpRx.Value, string.Join(",", player.hpRxCollection));
    }

    public void Clear()
    {
        logText.text = "";
    }

    public void Save()
    {
        player.Save();
        logText.text = "Saved";
    }

    public void Load()
    {
        player.Load();
        logText.text = "Loaded";
    }

    public void Delete()
    {
        Repository.Instance.DeleteSaveFile(player.FileName);
    }

    public void Exists()
    {
        logText.text = Repository.Instance.IsHaveSaveFile(player.FileName).ToString();
    }
}
