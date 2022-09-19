using Game;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;

namespace nv
{
    public class DemoMenuActions : MonoBehaviour
    {
        [SerializeField]
        protected CommunicationNode comms;

        public List<DemoActions> demoActions;

        public const string DefaultActionsTabName = "TESTS";
        public const string CreditsString = "Credits";
        public const string CreditsPerBetString = "CreditsPerBet";

        protected virtual void Reset()
        {
            CreateDefaultActions();
        }

        protected virtual void CreateDefaultActions()
        {
            if( demoActions == null )
                demoActions = new List<DemoActions>();

            var defaultActions = new DemoActions();
            defaultActions.actions = new List<DemoAction>();

            defaultActions.tab = DefaultActionsTabName;
            {
                DemoAction defaultAction = new DemoAction();
                defaultAction.option = "Add Demo Credits";
                defaultAction.dataForMenuOption = new List<DemoData>() { new DemoData() { Name = CreditsString, Value = "100000" } };
                defaultAction.action = new DemoEvent();
                defaultAction.action.AddPersistentListener<List<DemoData>>( AddDemoCredits );
                defaultActions.actions.Add( defaultAction );
            }
            {
                DemoAction defaultAction = new DemoAction();
                defaultAction.option = "Add Small Credits";
                defaultAction.dataForMenuOption = new List<DemoData>() { new DemoData() { Name = CreditsString, Value = "200" } };
                defaultAction.action = new DemoEvent();
                defaultAction.action.AddPersistentListener<List<DemoData>>( AddDemoCredits );
                defaultActions.actions.Add( defaultAction );
            }
            {
                DemoAction defaultAction = new DemoAction();
                defaultAction.option = "Add 1x Bet Credits";
                defaultAction.dataForMenuOption = new List<DemoData>() { new DemoData() { Name = CreditsPerBetString, Value = "1" } };
                defaultAction.action = new DemoEvent();
                defaultAction.action.AddPersistentListener<List<DemoData>>( AddDemoCredits );
                defaultActions.actions.Add( defaultAction );
            }
            {
                DemoAction defaultAction = new DemoAction();
                defaultAction.option = "Add 10x Bet Credits";
                defaultAction.dataForMenuOption = new List<DemoData>() { new DemoData() { Name = CreditsPerBetString, Value = "10" } };
                defaultAction.action = new DemoEvent();
                defaultAction.action.AddPersistentListener<List<DemoData>>( AddDemoCredits );
                defaultActions.actions.Add( defaultAction );
            }
            {
                DemoAction defaultAction = new DemoAction();
                defaultAction.option = "Add 100x Bet Credits";
                defaultAction.dataForMenuOption = new List<DemoData>() { new DemoData() { Name = CreditsPerBetString, Value = "100" } };
                defaultAction.action = new DemoEvent();
                defaultAction.action.AddPersistentListener<List<DemoData>>( AddDemoCredits );
                defaultActions.actions.Add( defaultAction );
            }
            //{
            //    DemoAction defaultAction = new DemoAction();
            //    defaultAction.option = "Play Autoplay";
            //    defaultAction.action = new DemoEvent();
            //    defaultAction.action.AddPersistentListener<List<DemoData>>( PlayAutoPlay );
            //    defaultActions.actions.Add( defaultAction );
            //}
            //{
            //    DemoAction defaultAction = new DemoAction();
            //    defaultAction.option = "Play Autocasino";
            //    defaultAction.action = new DemoEvent();
            //    defaultAction.action.AddPersistentListener<List<DemoData>>( PlayAutoCasino );
            //    defaultActions.actions.Add( defaultAction );
            //}

            demoActions.Add( defaultActions );
        }

        protected virtual IEnumerator Start()
        {
            yield return new WaitUntil( () => GameObject.FindObjectOfType<GameDemoMenu>() != null );
            comms.EnableNode( this );
            CreateDefaultOptionsFromActions();
        }

        protected virtual void OnDestroy()
        {
            comms.DisableNode();
        }

        protected virtual void CreateDefaultOptionsFromActions()
        {
            DemoOptions defaultOptions = new DemoOptions();
            defaultOptions.TabList = new List<DemoTab>();
            demoActions.ForEach( x => defaultOptions.AddDemoTab( x.tab ) );
            demoActions.ForEach( x => x.actions.ForEach(y =>
            {
                var tab = defaultOptions.TabList.First( z => z.Name == x.tab );
                if( tab.OptionList == null)
                    tab.OptionList = new List<DemoOption>();
                tab.AddDemoOption( y.option );
                if( y.dataForMenuOption != null )
                    y.dataForMenuOption.ForEach( w => tab.OptionList.First( z => z.Name == y.option ).AddData( w ) );                    
            } ));

            comms.Publish( new DemoOptionsMsg() { DemoOptions = defaultOptions } );
        }

        [CommunicationCallback]
        protected virtual void HandleGameDemoMsg( GameDemoMsg msg )
        {
            Dev.Where();
            var actionSet = demoActions.FirstOrDefault( x => x.tab == msg.Tab );
            if( string.IsNullOrEmpty( actionSet.tab ) || actionSet.actions == null )
                return;
            var option = actionSet.actions.FirstOrDefault( x => x.option == msg.Option );
            if( option.action != null )
                option.action.Invoke( msg.Data );
        }

        protected virtual void AddDemoCredits(List<DemoData> optionalCredits)
        {
            // Send this to the server
            //AddDemoCredits message = new AddDemoCredits();
            //if(optionalCredits.Count > 0)
            //{
            //    if( string.Compare( optionalCredits[ 0 ].Name, CreditsString ) == 0 )
            //    {
            //        message.mAmount = Money.Cents( int.Parse( optionalCredits[ 0 ].Value ) );
            //    }
            //    else
            //    {
            //        int multiplier = int.Parse( optionalCredits[ 0 ].Value );
            //        message.mAmount = GameState.GameBetController.TotalBetCost * multiplier;
            //    }
            //}
            //else
            //{
            //    message.mAmount = Money.Cents( 100000 );
            //}
            //comms.Publish( message, Tags.Server );            
            Dev.Log("Example pressed");
        }
    }

    [Serializable]
    public class DemoEvent : UnityEvent<List<DemoData>> { }

    [Serializable]
    public struct DemoActions
    {
        public string tab;
        public List<DemoAction> actions;
    }

    [Serializable]
    public struct DemoAction
    {
        public string option;
        public DemoEvent action;
        [Tooltip("This data will be added to the demo menu's option")]
        public List<DemoData> dataForMenuOption;
    }
}
