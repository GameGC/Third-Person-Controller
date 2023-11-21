using System;
using System.Reflection;
using FuzzySearch;
using ThirdPersonController.Core;
using TypeNamespaceTree;
using UnityEditor;
using UnityEngine;


public class FussiTester : MonoBehaviour
{
    [MenuItem("Tools/Test")]
    public static void Menu()
    {
        Debug.Log(EditorGUIUtility.LoadRequired("Library/unity editor resources").GetType());
        var activatorPosition = new Rect(new Vector2(Screen.width/2,Screen.height/2), new Vector2(200, 1));
        FuzzyDropdown(activatorPosition);
    }
    
    public static void FuzzyDropdown
    (
        Rect activatorPosition
    )
    {
        FuzzyWindowC2.Show(activatorPosition, typeof(BaseFeature));
    }
}


public partial class FuzzyWindowC2 : EditorWindow
{ 
    static FuzzyWindowC2()
    {
        ShowAsDropDownFitToScreen = typeof(EditorWindow).GetMethod("ShowAsDropDownFitToScreen", BindingFlags.Instance | BindingFlags.NonPublic);
    }
    
    private float maxHeight = 320;
    private float height = 320;
    private float minWidth = 200;
    private float minOptionWidth;
    private float footerHeight;
    private Rect activatorPosition;
    private bool scrollToSelected;
    private float initialY;
    private bool initialYSet;
    
    public static readonly float searchFieldHeight = 20;
    public static readonly float headerHeight = 25;
        private static Event e => Event.current;
    
        private FuzzySearchPanel _searchPanel;
        private FuzzyHeaderWithBackButtonPanel _headerWithBackButtonPanel;
        private FuzzyListPanel _list;

        private IOptionTree[] hierarchyTree;
        private IOptionTree[] typeTree1d;
            
        private void OnEnable()
        {
            _searchPanel = new FuzzySearchPanel();
            _searchPanel.QueryChanged += SearchPanelOnQueryChanged;
            
            _headerWithBackButtonPanel = new FuzzyHeaderWithBackButtonPanel();
            _headerWithBackButtonPanel.BackClicked += OnHeaderBackClicked;
            _list = new FuzzyListPanel();
            
            var types = GetNonAbstractTypesSubclassOf(typeof(BaseFeature));
            
            hierarchyTree = BuildTreeHierarchy(types).Childs.ToArray();
            _list.SetData(hierarchyTree);
            _list.NextClicked += EnterChild;

            typeTree1d = BuildTree1d(types);
        }

        private void SearchPanelOnQueryChanged(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                _list.SetData(hierarchyTree);
                _list.Repaint = true;
            }
            else
            {
                var queryChars = query.ToCharArray();
            
                Array.Sort(typeTree1d,Sort);
                _list.SetData(typeTree1d);
                int Sort(IOptionTree a, IOptionTree b)
                {
                    int score0 = 0;
                    int score1 = 0;

                    for (int i = 0; i < queryChars.Length; i++)
                    {
                        if (a.Content.text.Contains(queryChars[i],StringComparison.InvariantCultureIgnoreCase)) 
                            score0++;
                        if (b.Content.text.Contains(queryChars[i],StringComparison.InvariantCultureIgnoreCase)) 
                            score1++;
                    }
                    return score1.CompareTo(score0);
                }
            }
            
            animTarget = 1;
            lastRepaintTime = DateTime.Now;
        }

        private void OnHeaderBackClicked()
        {
            EnterParent();
            var newChilds = _list.Parent.Parent.Childs;
            _list.Parent = newChilds[0].Parent;
            _list.SetData(newChilds.ToArray());
        }

        private void Update()
        {
            if (_list.Repaint)
            {
                Repaint();
                _list.Repaint = false;
            }
        }

        private void OnLevelGUI(float anim)
        {
            anim = Mathf.Floor(anim) + Mathf.SmoothStep(0, 1, Mathf.Repeat(anim, 1));

            var levelPosition = new Rect
                (
                position.width * (1 - anim) + 1,
                30,
                position.width - 2,
                height - 31
                );
            GUILayout.BeginArea(levelPosition);

            if (_list.Parent.Parent != null)
            {
                var headerPosition = GUILayoutUtility.GetRect(16, headerHeight);

               
                _headerWithBackButtonPanel.OnGUI(_list.Parent, headerPosition);
            }

            OnOptionsGUI(levelPosition.height - headerHeight);

            GUILayout.EndArea();
        }

        private void OnOptionsGUI(float scrollViewHeight)
        {
            _list.OnGUI();
        }
        
        private static readonly MethodInfo ShowAsDropDownFitToScreen;
        private void OnPositioning()
        {
            if (Event.current.type != EventType.Repaint)
            {
                return;
            }

            if (!initialYSet)
            {
                initialY = this.position.y;
                initialYSet = true;
            }

            var totalWidth = Mathf.Max(minWidth, activatorPosition.width, minOptionWidth + 36);

            var totalHeight = Mathf.Min(height, maxHeight);

            var position = (Rect)ShowAsDropDownFitToScreen.Invoke(this, new object[] { activatorPosition, new Vector2(totalWidth, totalHeight), null });

            position.y = initialY;
            
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                var maxY = Screen.width;

                if (position.yMax > maxY)
                {
                    position.height -= (position.yMax - maxY);
                }
            }

            if (this.position != position || minSize != position.size)
            {
                minSize = maxSize = position.size;
                this.position = position;
            }

            GUIUtility.ExitGUI();
        }

        static FuzzyWindowC2 instance;

        public static void Show(Rect activatorPosition,Type type)
        {
            // Makes sure control exits DelayedTextFields before opening the window
            GUIUtility.keyboardControl = 0;

            if (instance != null)
            {
                try
                {
                    instance.Close();
                }
                catch (Exception exception)
                {
                }
            }
            {
                instance = CreateInstance<FuzzyWindowC2>();


                instance.CreateWindow(activatorPosition);
            }
        }

       
        private float anim = 1;
        private int animTarget = 1;
        private float animationSpeed = 4;
        
        private DateTime lastRepaintTime;

        private float repaintDeltaTime => (float)(DateTime.Now - lastRepaintTime).TotalSeconds;

        private bool isAnimating => anim != animTarget;

        //Be sure do not call any layout change while the UI is repainting or force a repaint
        //before the layout is completed.
        //This will probably break the UI

        private void OnGUI()
        {
            GUI.Label(new Rect(0, 0, position.width, position.height), GUIContent.none, GUIStyle.none);

            //if (tree.searchable)
            {
                GUILayout.Space(7);

                var searchFieldPosition = GUILayoutUtility.GetRect(10, searchFieldHeight);
                searchFieldPosition.x += 8;
                searchFieldPosition.width -= 16;

                _searchPanel.OnGUI(searchFieldPosition);
                //  var newQuery = OnSearchGUI(searchFieldPosition, "");
            }

            OnLevelGUI(anim);

            UpdateAnimation();
            
            OnPositioning();
        }
        
        
        private void CreateWindow(Rect activatorPosition)
        {
            // Port the activator position to screen space
            activatorPosition.position = new Vector2(Screen.width / 2, Screen.height / 2);//GUIUtility.GUIToScreenPoint(activatorPosition.position);
            this.activatorPosition = activatorPosition;

            // Show and focus the window
            wantsMouseMove = true;
            var initialSize = new Vector2(activatorPosition.width, height);
            
            ShowAsDropDown(this.activatorPosition,initialSize);
            //ShowAsDropDownFitToScreen.Invoke(this, new object[] { activatorPosition, initialSize, null });

            Focus();
        }
}