using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditorInternal;
using UnityEngine;

public class Installer : EditorWindow
{
    public Texture2D startTex;
    public Texture2D otherTex;
    
    public TextAsset manifestFile;
    private const string Version = "0.01_PRO";


    [MenuItem("Tools/Installer")]
    public static void Create()
    {
        var size = new Vector2(Screen.currentResolution.width /8f, Screen.currentResolution.height /4f);
        GetWindowWithRect<Installer>(new Rect(Vector2.zero, size)
            , true, "Installer", true);
    }
    private bool IsSetuped
    {
        get => EditorPrefs.GetString("MTPS_Setup") == Version;
        set
        {
            if(value)
                EditorPrefs.SetString("MTPS_Setup", Version);
        }
    }

    private string invoice
    {
        get => EditorPrefs.GetString("MTPS_invoice");
        set => EditorPrefs.SetString("MTPS_invoice", value);
    }

    private int stage = 0;

    
    
    private bool _multipleDownloads = true;
    private DownloaderItem[] _downloaderItems;
    private ReorderableList _downloadsDisplay;

    private void OnEnable()
    {
        var packageList = JsonConvert.DeserializeObject<Dictionary<string, string>>(manifestFile.text);
        
        
        _downloaderItems = new DownloaderItem[packageList.Count];
        
        int i = 0;
        foreach (var package in packageList)
        {
            _downloaderItems[i] = new DownloaderItem(package.Key,package.Value);
            i++;
        }

        _downloadsDisplay = new ReorderableList(_downloaderItems, typeof(DownloaderItem), false, true, false, false);
        _downloadsDisplay.drawHeaderCallback += rect =>
        {
            rect.width /= 2;
            EditorGUI.LabelField(rect, EditorGUIUtility.TrTextContent("Package"));
            rect.x += rect.width;
            var editorLabel = new GUIStyle(EditorStyles.label);
            editorLabel.alignment = TextAnchor.MiddleRight;
            EditorGUI.LabelField(rect, EditorGUIUtility.TrTextContent("Progress"),editorLabel);
        };
        _downloadsDisplay.drawElementCallback += (rect, index, active, focused) =>
        {
            var tempRectWidth = rect.width;
            rect.width *= 0.75F; 
            EditorGUI.LabelField(rect, _downloaderItems[index].PackageName);
            rect.x += rect.width;
            rect.width = tempRectWidth - rect.width;
            var editorLabel = new GUIStyle(EditorStyles.label);
            editorLabel.alignment = TextAnchor.MiddleRight;
            EditorGUI.LabelField(rect,$"{_downloaderItems[index].Progress}%",editorLabel);
        };
    }

    private void OnGUI()
    {
        if (stage == 0)
        {
            var labelStyle = new GUIStyle(EditorStyles.centeredGreyMiniLabel);
            labelStyle.normal.textColor = Color.white;
            labelStyle.hover.textColor = Color.white;
            labelStyle.active.textColor = Color.white;

            labelStyle.fontSize = 55;
            
            var width =GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true)).width;

            EditorGUI.DrawPreviewTexture(new Rect(0,0,width,position.height),startTex,null,ScaleMode.StretchToFill);
            GUI.Label(new Rect(width/2.5f,position.height/6f,width - width/2.5f,position.height*0.15f),Version,labelStyle);
          //  EditorGUI.DrawRect(new Rect(width/2.5f,position.height/6f,Mathf.Clamp(width - (width/2.5f),0,Screen.width/3.5f),position.height*0.15f),Color.red);



          var smallWhiteLabel = new GUIStyle(EditorStyles.label);
          smallWhiteLabel.normal.textColor = Color.white;
          smallWhiteLabel.hover.textColor =new Color(0.2f, 0.2f, 1f);
          smallWhiteLabel.onHover.textColor = new Color(0.2f, 0.2f, 1f);

          smallWhiteLabel.active.textColor = Color.white;
            if (GUI.Button(new Rect(0, position.height / 3, EditorGUIUtility.labelWidth, 18f),new GUIContent("Type invoice number:","CLICK ME TO FIND YOUR INVOICE\nTYPE INVOICE NUMBER WITH BEGINNING \"IN\""),smallWhiteLabel))
            {
                Application.OpenURL("https://assetstore.unity.com/orders");
            }
            EditorGUI.BeginChangeCheck();
            var newInvoice = GUI.TextField(new Rect(EditorGUIUtility.labelWidth, position.height / 3, EditorGUIUtility.fieldWidth * 2.2f, 18f), invoice,22);
            if (EditorGUI.EndChangeCheck())
            {
                invoice = newInvoice.Trim();
            }
            
            var button = new GUIStyle(GUI.skin.button);
            button.fontStyle = FontStyle.Bold;
            button.fontSize *= 2;
            button.normal.textColor = Color.white;
            button.active.textColor = Color.white;

            if (!string.IsNullOrEmpty(invoice) && invoice.StartsWith("IN"))
            {
                GUI.backgroundColor = new Color32(210, 88, 247, 255);
                if (GUI.Button(
                        new Rect(width * 0.25f, position.height - position.height * 0.05f, width * 0.5f,
                            position.height * 0.05f), "Next", button))
                {
                    stage++;
                    SendValidateRequest();
                }
            }
        }

        if (stage == 1)
        {
            EditorGUI.DrawPreviewTexture(new Rect(0,0,position.width,position.height),otherTex,null,ScaleMode.StretchToFill);
            long time = (long)EditorApplication.timeSinceStartup;
            string loadingText = "Loading"+(time % 2 == 0? "..." : time % 3 == 0 ? ".": "..");
            var customLabel = new GUIStyle(EditorStyles.whiteLabel);
            customLabel.fontSize = GetFontSize(position.width * 0.1f);


            var rect = new Rect(position.width *0.25f, position.height / 2.5f, position.width * 0.5f, position.height * 0.1f);
            EditorGUI.LabelField(rect,loadingText,customLabel);
        }

        if (stage == -1)
        {
            EditorGUI.DrawPreviewTexture(new Rect(0,0,position.width,position.height),otherTex,null,ScaleMode.StretchToFill);
            var customLabel = new GUIStyle(EditorStyles.helpBox);
            customLabel.fontSize = GetFontSize(25);


            var rect = new Rect(position.width *0.25f, position.height / 2.5f, position.width * 0.5f, position.height * 0.1f);
           
            GUI.Label(rect,
                EditorGUIUtility.TrTextContentWithIcon("Failed to verify invoice", EditorGUIUtility.Load("console.erroricon") as Texture),
                customLabel);
            
            var button = new GUIStyle(GUI.skin.button);
            button.fontStyle = FontStyle.Bold;
            button.fontSize *= 2;
            button.normal.textColor = Color.white;
            button.active.textColor = Color.white;

            GUI.backgroundColor = new Color32(210, 88, 247, 255);
            if (GUI.Button(
                    new Rect(position.width * 0.25f, position.height - position.height * 0.05f,
                        position.width * 0.5f,
                        position.height * 0.05f), "Back", button))
            {
                stage = 0;
            }
        }
        if (stage == 2)
        {
            EditorGUI.DrawPreviewTexture(new Rect(0,0,position.width,position.height),otherTex,null,ScaleMode.StretchToFill);
            _multipleDownloads = ToggleDrawer.CustomToggle(new Rect(position.width *0.25f, position.height / 2.5f, position.width * 0.5f, position.height * 0.1f), _multipleDownloads, new GUIContent("Multiple Downloads?"));

            
            var button = new GUIStyle(GUI.skin.button);
            button.fontStyle = FontStyle.Bold;
            button.fontSize *= 2;
            button.normal.textColor = Color.white;
            button.active.textColor = Color.white;
            button.hover.textColor = new Color(0.85f, 0.85f, 0.85f);

            GUI.backgroundColor = new Color32(210, 88, 247, 255);

            var rect = new Rect(position.width * 0.2f, position.height - position.height * 0.05f, position.width * 0.6f,
                position.height * 0.05f);

            _downloadsDisplay.DoList(new Rect(position.width *0.1f, position.height / 2f, position.width * 0.8f, position.height * 0.5f));
            
            if (GUI.Button(rect, "Download packages", button))
            {
                if (_multipleDownloads)
                {
                    for (var i = 0; i < _downloaderItems.Length; i++)
                    {
                        SendPackageRequest(i);
                    }
                }
                else
                {
                    DownloadSynchronous(0);
                }

            }
            if(_downloaderItems.All(d=>d.Progress == 100))
                stage = 3;
        }

        if (stage == 3)
        {
            EditorGUI.DrawPreviewTexture(new Rect(0,0,position.width,position.height),otherTex,null,ScaleMode.StretchToFill);
            var button = new GUIStyle(GUI.skin.button);
            button.fontStyle = FontStyle.Bold;
            button.fontSize *= 2;
            button.normal.textColor = Color.white;
            button.active.textColor = Color.white;
            button.hover.textColor = new Color(0.85f, 0.85f, 0.85f);
            
            GUI.backgroundColor = new Color32(210, 88, 247, 255);
            
            var rect = new Rect(position.width *0.25f, position.height / 2.5f, position.width * 0.5f, position.height * 0.1f);


            if (GUI.Button(rect, "Install packages", button))
            {
                var path = Application.dataPath.Replace("/Assets","") + "/Packages/Local/GameGC/com.gamegc.surfacesystem/com.gamegc.surfacesystem-1.0.0.tgz";

                path =
                    "/Users/user/unityProjects/CustomCharacterController/Packages/Local/GameGC/com.gamegc.surfacesystemf/com.gamegc.surfacesystem-1.0.0.tgz";
                
                path = path.Substring(path.IndexOf("/Local")+1); Client.Add("file:"+path);
            }
        }
    }

    private void DownloadSynchronous(int index)
    {
        _downloaderItems[index].DownloadClient.DownloadFileCompleted += (sender, args) => DownloadSynchronous(index);
        SendPackageRequest(index);
        index++;
    }
    
    private async void SendValidateRequest()
    {
        var result = await CustomWebClient.Get("http://cvhelpers.byethost7.com/Backend/validator.php?invoice=" + invoice);
        if (result == "true")
        {
            stage=2;
        }
        else
        {
            Debug.LogError(result);
            stage = -1;
        }
    }


    private async void SendPackageRequest(int i)
    {
        var package = _downloaderItems[i].PackageName;
        var result = await CustomWebClient.Get("http://cvhelpers.byethost7.com/Backend/getfile.php?key=" + invoice+"&url="+package);

        //hacker protection 
        if (result.Contains("false"))
        {
            EditorApplication.Exit(0);
        }
        
        var path = Application.dataPath.Replace("/Assets","") + "/Packages/Local/GameGC/"+package+"f";
        if (!Directory.Exists(path))
            Directory.CreateDirectory(path);
        
        _downloaderItems[i].DownloadClient.DownloadFile(result,path);
    }

    private void Update()
    {
        if (stage == 1)
        {
            Repaint();
        }

        if (stage == 2)
        {
            Repaint();
        } 
    }

    private int GetFontSize(float fontSize) => Mathf.Min(Mathf.FloorToInt(Screen.width * fontSize/1000), Mathf.FloorToInt(Screen.height * fontSize/1000));
}