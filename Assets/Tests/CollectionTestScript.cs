using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using GameFoundation.Utilities;

public class CollectionTestScript
{
    // A Test behaves as an ordinary method
    [Test]
    public void Shuffle()
    {
        var list = new List<int>() { 1, 2, 3, 4, 5 };
        Debug.Log($"Before shuffle: {string.Join(",", list)}");
        Debug.Log($"After shuffle: {string.Join(",", list.Shuffle())}");
        Assert.AreNotEqual(list, new List<int>() { 1, 2, 3, 4, 5 });
    }

    [Test]
    public void Sample()
    {
        var list = new List<int>() { 1, 2, 3, 4, 5 };
        Debug.Log($"List: {string.Join(",", list)}");
        Debug.Log($"Sample: {list.Sample()}");
    }

    [Test]
    public void SampleSize()
    {
        var list = new List<int>() { 1, 2, 3, 4, 5 };
        Debug.Log($"List: {string.Join(",", list)}");
        Debug.Log($"Sample: {string.Join(",", list.SampleSize(3))}");
    }
}
