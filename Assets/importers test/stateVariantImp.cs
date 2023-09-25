using System;
using UnityEngine;
using UnityEditor.AssetImporters;
using System.IO;
using Newtonsoft.Json;
using UnityEditor;

[ScriptedImporter(1, "jssvar")]
public class stateVariantImp : ScriptedImporter
{
    public override void OnImportAsset(AssetImportContext ctx)
    {
        var definition = new {
            Type = "",
            Variants = new string[0],
            CombinedVariants = new string[0]
        };

        var customer1 = JsonConvert.DeserializeAnonymousType(File.ReadAllText(ctx.assetPath), definition);

        var asset = ScriptableObject.CreateInstance(customer1.Type);
        ctx.AddObjectToAsset("main", asset); 
        ctx.SetMainObject(asset);
    }
}



