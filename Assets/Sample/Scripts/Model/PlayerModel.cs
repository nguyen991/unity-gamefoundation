using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Model;

[System.Serializable]
public class PlayerModel : UniRxModel
{
    public int score;
    public int hp;
    public ReactiveProperty<int> hpRx;
    public ReactiveCollection<int> hpRxCollection;

    public PlayerModel(string name = "") : base()
    {
        score = 0;
        hp = 0;
        hpRx = new ReactiveProperty<int>();
        hpRxCollection = new ReactiveCollection<int>();
    }

    public override void Load()
    {
        hpRxCollection.Clear();
        base.Load();
    }
}
