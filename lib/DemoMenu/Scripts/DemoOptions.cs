using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Game;

namespace nv
{
    [Serializable]
    [XmlRoot( "DemoOptions" )]
    public class DemoOptions
    {
        [XmlIgnore]
        public const string DemoOptionsFilename = "GameDemoFeatures.xml";

        [XmlElement( "Tab" )]
        public List<DemoTab> TabList;

        public virtual void Add( DemoTab obj )
        {
            AddDemoTab( obj );
        }

        public virtual bool Contains( string tabName )
        {
            return TabList.FirstOrDefault( x => x.Name == tabName ) != null;
        }

        public virtual void AddDemoTab( DemoTab tab )
        {
            TabList.Add( tab );
        }

        public virtual void AddDemoTab( string tab )
        {
            AddDemoTab( new DemoTab() { Name = tab, OptionList = new List<DemoOption>() } );
        }
    }

    [Serializable]
    [XmlType( "Tab" )]
    public class DemoTab
    {
        [XmlAttribute( "Name" )]
        public string Name;

        //[XmlArray( "OptionList" )]
        //[XmlArrayItem( "Option", typeof( DemoOption ) )]
        [XmlElement( "Option" )]
        public List<DemoOption> OptionList;

        public virtual bool Contains( string optionName )
        {
            return OptionList.FirstOrDefault( x => x.Name == optionName ) != null;
        }

        public virtual void Add( DemoOption obj )
        {
            AddDemoOption( obj );
        }

        public virtual void AddDemoOption( DemoOption option )
        {
            OptionList.Add( option );
        }

        public virtual void AddDemoOption( string option )
        {
            AddDemoOption( new DemoOption() { Name = option, DataList = new List<DemoData>() } );
        }
    }

    [Serializable]
    [XmlType( "Option" )]
    public class DemoOption
    {
        [XmlAttribute( "Name" )]
        public string Name;

        //[XmlArray( "DataList" )]
        //[XmlArrayItem( "Data", typeof( DemoData ) )]
        [XmlElement( "Data" )]
        public List<DemoData> DataList;

        public virtual bool Contains( string dataName )
        {
            return DataList.FirstOrDefault( x => x.Name == dataName ) != null;
        }

        public virtual void AddData( DemoData data )
        {
            DataList.Add( data );
        }

        public virtual void Add( DemoData obj )
        {
            AddData( obj );
        }

        public virtual void AddData( string name, string value )
        {
            AddData( new DemoData() { Name = name, Value = value } );
        }
    }

    [Serializable]
    //[XmlRoot( "Data" )]
    [XmlType( "Data" )]
    public class DemoData
    {
        [XmlAttribute( "Name" )]
        public string Name;

        [XmlAttribute( "Value" )]
        public string Value;
    }

    public static class DemoDataExtensions
    {
        public static bool ContainsKey( this List<DemoData> data, string key )
        {
            return data.Any( x => string.Compare( x.Name, key ) == 0 );
        }

        public static string GetValue( this List<DemoData> data, string key )
        {
            return data.First( x => string.Compare( x.Name, key ) == 0 ).Value;
        }

        public static T GetValue<T>( this List<DemoData> data, string key, Func<string,T> parseFunc )
        {
            return parseFunc( data.GetValue( key ) );
        }

        public static bool TryGetValue( this List<DemoData> data, string key, out string value )
        {
            var pair = data.FirstOrDefault( x => string.Compare( x.Name, key ) == 0 );
            value = string.Empty;
            if( pair == default( DemoData ) )
                return false;
            value = pair.Value;
            return true;
        }
    }
}
