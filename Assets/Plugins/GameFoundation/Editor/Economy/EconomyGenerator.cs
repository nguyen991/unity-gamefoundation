using UnityEngine;
using UnityEditor;
using System.IO;
using System.Linq;
using GameFoundation.Economy;
using System.Collections.Generic;

public class EconomyGenerator : EditorWindow
{
    [MenuItem("Game Foundation/Generator/Game Foundation Ids", false, 101)]
    private static void ShowWindow()
    {
        var window = GetWindow<EconomyGenerator>();
        window.titleContent = new GUIContent("EconomyGenerator");
        window.Show();
    }

    private string path = "Scripts";
    private string fileName = "GFIds";

    private void OnGUI()
    {
        path = EditorGUILayout.TextField("Path", path);
        fileName = EditorGUILayout.TextField("File name", fileName);
        if (GUILayout.Button("Generate"))
        {
            Generate();
        }
    }

    private void Generate()
    {
        var fullPath = Path.Join("Assets", path, fileName + ".cs");
        Debug.Log(fullPath);

        // load economy data
        var guids = AssetDatabase.FindAssets("t:EconomyData");
        if (guids.Length <= 0)
        {
            Debug.LogError("Economy Data not found");
            return;
        }
        var economy = AssetDatabase.LoadAssetAtPath<EconomyData>(AssetDatabase.GUIDToAssetPath(guids[0]));

        // open file
        var content = $"using System.Collections;\nusing System.Collections.Generic;\nusing UnityEngine;\n\npublic static class {fileName}\n{{";

        // read all
        var tags = new List<string>();
        var properties = new List<string>();
        economy.currencyCatalog.Items.ForEach(it => { if(tags != null) tags.AddRange(it.tags); if (properties != null) properties.AddRange(it.properties.Keys); });
        economy.itemCatalog.Items.ForEach(it => { tags.AddRange(it.tags); properties.AddRange(it.properties.Keys); });
        economy.transactionCatalog.Items.ForEach(it => { tags.AddRange(it.tags); properties.AddRange(it.properties.Keys); });
        economy.rewardCatalog.Items.ForEach(it => { tags.AddRange(it.tags); properties.AddRange(it.properties.Keys); });

        // distinct list
        tags = tags.Distinct().Where(it => !string.IsNullOrEmpty(it)).ToList();
        properties = properties.Distinct().Where(it => !string.IsNullOrEmpty(it)).ToList();

        // write all
        content += WriteClass("Tag", tags, 1);
        content += WriteClass("Property", properties, 1);
        content += WriteClass("Currency", economy.currencyCatalog.Items.Select(it => it.key).ToList(), 1);
        content += WriteClass("Item", economy.itemCatalog.Items.Select(it => it.key).ToList(), 1);
        content += WriteClass("Transaction", economy.transactionCatalog.Items.Select(it => it.key).ToList(), 1);
        content += WriteClass("Store", economy.storeCatalog.Items.Select(it => it.key).ToList(), 1);
        content += WriteClass("Reward", economy.rewardCatalog.Items.Select(it => it.key).ToList(), 1);

        // end file
        content += "\n}\n";

        File.WriteAllText(fullPath, content);
        AssetDatabase.Refresh();
    }

    private string WriteClass(string name, List<string> items, int indent)
    {
        var content = "\n" + AppendIndent(indent) + $"public static class {name}\n" + AppendIndent(indent) + "{";
        indent++;
        items.Where(it => !string.IsNullOrEmpty(it)).Distinct().ToList().ForEach(it => content += "\n" + AppendIndent(indent) + $"public const string {it} = \"{it}\";");
        content += "\n" + AppendIndent(--indent) + "}\n";
        return content;
    }

    private string AppendIndent(int indent)
    {
        var str = "";
        for (var i = 0; i < indent * 4; i++)
        {
            str += " ";
        }
        return str;
    }
}