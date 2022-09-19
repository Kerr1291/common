using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game;
using System;
using UnityEngine.UI;
using System.Linq;

namespace nv
{
    public class GameDemoMenu : MonoBehaviour, IEnumerable<DemoTab>
    {
        [Tooltip("Check this to populate the demo menu with tab information from the GDK")]
        public bool enableLegacyBehaviour = false;

        [Tooltip( "Check this to populate the demo menu with tab information from the inspector" )]
        public bool enableCustomEditorTabs = false;

        [Tooltip("Check this cause the demo menu to hide when an option is selected")]
        public bool hideMenuOnOptionSelected = true;

        [SerializeField]
        protected CommunicationNode comms;

        [Tooltip( "The root game object for the menu" )]
        public GameObject demoMenuRoot;
        [Tooltip( "The root game object for button that activates the menu" )]
        public GameObject demoMenuActivator;
        [Tooltip( "The button that activates the menu" )]
        public Button demoMenuActivatorButton;

        [Tooltip("The root game object arranging tabs in UI")]
        public RectTransform demoMenuTabRoot;

        [Tooltip( "The root game object arranging tab pages in UI" )]
        public RectTransform demoMenuTabPagesRoot;

        [Tooltip( "The prefab for generating tabs" )]
        public Button demoMenuTabPrefab;

        [Tooltip( "The prefab for generating pages to hold selectable options" )]
        public ScrollRect demoMenuTabPagePrefab;

        [Tooltip( "The prefab for generating selectable options" )]
        public Button demoMenuOptionPrefab;

        public Color tabSelectedColor = Color.white;
        public Color tabUnselectedColor = Color.gray;

        Dictionary<string, Button> demoMenuTabs;
        Dictionary<string, ScrollRect> demoMenuTabPages;
        Dictionary<string, Dictionary<string, Button>> demoMenuOptions;

        public string demoLabelText = "DEMO";
        protected string legacyExtraLabelText = string.Empty;
        protected bool enabledOnSuspend = false;


        public string DemoLabelText {
            get {
                return demoLabelText + legacyExtraLabelText;
            }
        }

        [SerializeField]
        protected DemoOptions demoOptions;

        public virtual void Awake()
        {
            if( demoOptions == null )
                demoOptions = new DemoOptions();

            if( demoOptions.TabList == null )
                demoOptions.TabList = new List<DemoTab>();

            demoMenuActivatorButton.onClick.RemovePersistentListener( HideDemoMenu );
            demoMenuActivatorButton.onClick.AddPersistentListener( DisplayDemoMenu );

            demoMenuTabs = new Dictionary<string, Button>();
            demoMenuTabPages = new Dictionary<string, ScrollRect>();
            demoMenuOptions = new Dictionary<string, Dictionary<string, Button>>();

            if( enableCustomEditorTabs )
                GenerateMenuObjects( demoOptions );

            comms.EnableNode(this);
            HideDemoMenu();
        }

        public virtual void OnEnable()
        {
            if( demoMenuActivator == null || demoMenuRoot == null )
                gameObject.SetActive( false );
        }

        public virtual void OnDestroy()
        {
            comms.DisableNode();
        }

        public virtual void EnableDemoMenu()
        {
            Show();
        }

        public virtual void DisableDemoMenu()
        {
            Hide();
        }


        //show the button that opens the demo menu
        public virtual void Show()
        {
            demoMenuActivator.SafeSetActive( true );
        }

        //hide the button that opens the demo menu
        public virtual void Hide()
        {
            demoMenuActivator.SafeSetActive( false );
        }


        //show the demo menu
        public virtual void DisplayDemoMenu()
        {
            demoMenuRoot.SafeSetActive( true );
        }

        //hide the demo menu
        public virtual void HideDemoMenu()
        {
            demoMenuRoot.SafeSetActive( false );
        }

        public virtual bool IsDemoMenuOpen()
        {
            return demoMenuRoot.SafeIsActive();
        }

        protected void SetButtonText(Button b, string text)
        {
            (b.GetComponentInChildren<Text>()).text = text;
        }

        Button InstantiateTab(string tab)
        {
            var newTab = Instantiate( demoMenuTabPrefab, demoMenuTabRoot );
            newTab.name = tab;
            SetButtonText( newTab, tab );
            newTab.onClick.AddListener( () => OnTabSelected( tab ) );
            newTab.gameObject.SafeSetActive( true );
            return newTab;
        }

        ScrollRect InstantiateTabPage( string tab )
        {
            var newTabPage = Instantiate( demoMenuTabPagePrefab, demoMenuTabPagesRoot );
            newTabPage.GetComponent<RectTransform>().SetLeft( 0f );
            newTabPage.GetComponent<RectTransform>().SetRight( 0f );
            newTabPage.GetComponent<RectTransform>().SetTop( 0f );
            newTabPage.GetComponent<RectTransform>().SetBottom( 0f );
            newTabPage.name = tab + " Tab Page";
            newTabPage.gameObject.SafeSetActive( false );
            return newTabPage;
        }

        Button InstantiateOption( string tab, string option )
        {
            var newOption = Instantiate( demoMenuOptionPrefab, demoMenuTabPages[tab].content );
            newOption.name = tab + " " + option;
            newOption.onClick.AddListener( () => OnOptionSelected( tab, option ) );
            SetButtonText( newOption, option );
            newOption.gameObject.SafeSetActive( false );
            return newOption;
        }

        protected virtual void AddDemoTab(string tab)
        {
            if( !demoOptions.Contains(tab) )
                demoOptions.AddDemoTab( tab );

            if( demoMenuTabs == null )
                demoMenuTabs = new Dictionary<string, Button>();
            if( demoMenuTabPages == null )
                demoMenuTabPages = new Dictionary<string, ScrollRect>();
            if( demoMenuOptions == null )
                demoMenuOptions = new Dictionary<string, Dictionary<string, Button>>();
            if( !demoMenuOptions.ContainsKey( tab ) )
                demoMenuOptions.Add( tab, new Dictionary<string, Button>() );

            if(!demoMenuTabs.ContainsKey( tab ) )
                demoMenuTabs.Add( tab, InstantiateTab( tab ) );
            if(!demoMenuTabPages.ContainsKey( tab ) )
                demoMenuTabPages.Add( tab, InstantiateTabPage( tab ) );
        }

        protected virtual void AddDemoTabOption( string tab, string option )
        {
            if( !demoOptions.Contains( tab ) )
                AddDemoTab( tab );
            var optionTab = demoOptions.TabList.FirstOrDefault( x => x.Name == tab );
            if( !optionTab.Contains( option ) )
                optionTab.AddDemoOption( option );
            if( demoMenuOptions[ tab ].ContainsKey( option ) && demoMenuOptions[ tab ][ option ] != null )
            {
                Destroy( demoMenuOptions[ tab ][ option ].gameObject );
                demoMenuOptions[ tab ][ option ] = InstantiateOption( tab, option );
            }
            else
            {
                demoMenuOptions[ tab ][ option ] = InstantiateOption( tab, option );
            }
        }

        protected virtual void AddDemoTabOptionData( string tab, string option, string data, string dataName = "" )
        {
            if( !demoOptions.Contains( tab ) )
                AddDemoTab( tab );
            var optionTab = demoOptions.TabList.FirstOrDefault( x => x.Name == tab );
            if( !optionTab.Contains( option ) )
                optionTab.AddDemoOption( option );
            optionTab.OptionList.FirstOrDefault(y => y.Name == option ).AddData(dataName,data);
            //TODO: display data elements
        }

        protected virtual void RemoveDemoTab( string tab )
        {
            var foundTab = demoOptions.TabList.FirstOrDefault( x => x.Name == tab );
            if(foundTab != null)
            {
                int index = demoOptions.TabList.IndexOf( foundTab );
                demoOptions.TabList.RemoveAt( index );
            }

            Destroy( demoMenuTabs[ tab ].gameObject );
            demoMenuTabs.Remove( tab );
        }

        protected virtual void RemoveDemoTabOption( string tab, string option )
        {
            var foundTab = demoOptions.TabList.FirstOrDefault( x => x.Name == tab );
            if( foundTab != null )
            {
                var foundOption = foundTab.OptionList.FirstOrDefault( x => x.Name == option );
                if( foundOption != null )
                {
                    int index = foundTab.OptionList.IndexOf( foundOption );
                    foundTab.OptionList.RemoveAt( index );
                }

                Destroy( demoMenuOptions[ tab ][ option ].gameObject );
                demoMenuOptions[ tab ].Remove( option );
            }
        }

        protected virtual void RemoveDemoTabOptionData( string tab, string option, string dataName )
        {
            var foundTab = demoOptions.TabList.FirstOrDefault( x => x.Name == tab );
            if( foundTab != null )
            {
                var foundOption = foundTab.OptionList.FirstOrDefault( x => x.Name == option );
                if( foundOption != null )
                {
                    var foundData = foundOption.DataList.FirstOrDefault( x => x.Name == dataName );
                    if( foundOption != null )
                    {
                        int index = foundOption.DataList.IndexOf( foundData );
                        foundOption.DataList.RemoveAt( index );
                        //TODO: remove the associated data display game objects
                    }
                }
            }
        }


        public virtual void DemoMenuChoiceMade( string whichTab, string legacy_triggerClassification, string demoChoice, bool legacy_handleGenerically )
        {
            if( enableLegacyBehaviour )
            {
                HideDemoMenu();
                //GDKBroadcastHidingDemoMenu();
                PublishDemoChoice( legacy_triggerClassification, demoChoice );
            }
        }

        public virtual void Initialize()
        {
            HideDemoMenu();
        }

        //public virtual void AddDemoTriggers( DemoFeatureData[] demoTriggers )
        //{
        //    if( enableLegacyBehaviour )
        //    {
        //        // Register a single point of callback for each demo trigger
        //        foreach(DemoOption trigger in demoTriggers )
        //        {
        //            AddDemoTabOption( trigger.Name, trigger. );                    
        //        }
        //    }
        //}

        public virtual void AddTabs( DemoTab[] tabs )
        {
            if( enableLegacyBehaviour )
            {
                tabs.ToList().ForEach( x =>
                {
                    AddDemoTab(x.Name);
                    x.OptionList.ForEach(y => AddDemoTabOption(x.Name, y.Name));
                }
                    );
            }
        }


        public virtual void AddSpecificChoice( string tabName, string choiceName, bool allowInShowBuild )
        {
            if( enableLegacyBehaviour )
            {
                AddDemoTabOption( tabName, choiceName );
            }
        }

        public virtual bool HasTab( string tabName )
        {
            return demoOptions.Contains( tabName );
        }

        public virtual void AddDemoTextHint( string demoHint )
        {
            legacyExtraLabelText += " " + demoHint;            
        }

        public virtual void RemoveDemoTextHint( string demoHint )
        {
            if( enableLegacyBehaviour )
            {
                legacyExtraLabelText = legacyExtraLabelText.Remove(demoHint).Trim();
            }
        }

        public virtual void RemoveTab( string tabName )
        {
            if( enableLegacyBehaviour )
            {
                RemoveDemoTab( tabName );
            }
        }

        protected virtual void SetDemoOptions( DemoOptions options )
        {
            demoOptions = options;
        }

        public virtual void PublishDemoChoice(string tab, string option, List<DemoData> data = null)
        {
            if( data == null )
                data = new List<DemoData>();

            comms.Publish( new GameDemoMsg() { Tab = tab, Option = option, Data = data.ToList() } );
        }

        [CommunicationCallback]
        public virtual void HandleDemoOptionsMsg( DemoOptionsMsg msg )
        {
            GenerateMenuObjects( msg.DemoOptions );
        }

        public virtual void GenerateMenuObjects(DemoOptions options)
        {
            options.TabList.ForEach( x => AddDemoTab( x.Name ) );
            options.TabList.ForEach( x => x.OptionList.ForEach( y => AddDemoTabOption( x.Name, y.Name ) ) );
            options.TabList.ForEach( x => x.OptionList.ForEach( y => y.DataList.ForEach( z => AddDemoTabOptionData( x.Name, y.Name, z.Value, z.Name ) ) ) );
        }

        public virtual void DestroyMenuObjects( DemoOptions options )
        {
            options.TabList.ForEach( x => RemoveDemoTab( x.Name ) );
            options.TabList.ForEach( x => x.OptionList.ForEach( y => RemoveDemoTabOption( x.Name, y.Name ) ) );
            options.TabList.ForEach( x => x.OptionList.ForEach( y => y.DataList.ForEach( z => RemoveDemoTabOptionData( x.Name, y.Name, z.Name ) ) ) );
        }

        //protected virtual void GDKBroadcastShowingDemoMenu()
        //{
        //    comms.Publish( new ButtonMsg() { messageType = DemoMenuCommonChoices.m_DemoMenuShowingEventName } );
        //}

        //protected virtual void GDKBroadcastHidingDemoMenu()
        //{
        //    comms.Publish( new ButtonMsg() { messageType = DemoMenuCommonChoices.m_DemoMenuHidingEventName } );
        //}

        public IEnumerator<DemoTab> GetEnumerator()
        {
            return demoOptions.TabList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        protected virtual void OnTabSelected( string tab )
        {
            demoMenuTabs.Where( x => x.Key != tab ).ToList().ForEach( x => { x.Value.GetComponent<Image>().color = tabUnselectedColor; } );
            demoMenuTabs.Where( x => x.Key == tab ).ToList().ForEach( x => { x.Value.GetComponent<Image>().color = tabSelectedColor; } );
            ShowTab( tab );
        }

        protected virtual void OnOptionSelected( string tab, string option )
        {
            //HideDemoMenu();
            //GDKBroadcastHidingDemoMenu();
            PublishDemoChoice( tab, option, demoOptions.TabList.First( x => x.Name == tab ).OptionList.First( y => y.Name == option ).DataList );

            if(hideMenuOnOptionSelected)
                HideDemoMenu();
        }

        protected virtual void ShowTab( string tab )
        {            
            demoMenuOptions.Where( x => x.Key != tab ).ToList().ForEach( x => x.Value.ToList().ForEach( y => y.Value.gameObject.SafeSetActive( false ) ) );
            demoMenuOptions.Where( x => x.Key == tab ).ToList().ForEach( x => x.Value.ToList().ForEach( y => y.Value.gameObject.SafeSetActive( true ) ) );

            demoMenuTabPages.Where( x => x.Key != tab ).ToList().ForEach( x => x.Value.gameObject.SafeSetActive( false ) );
            demoMenuTabPages.Where( x => x.Key == tab ).ToList().ForEach( x => x.Value.gameObject.SafeSetActive( true ) );
        }
    }
}
