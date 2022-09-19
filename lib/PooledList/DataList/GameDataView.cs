using UnityEngine;
using System.Collections;

namespace nv
{
    public class GameDataView : ListDataView<GameData>
    {
        //Called each time an object comes into view
        public override void BindDataToView(GameData data)
        {
            //before this call this object (the view) does not have an instance assigned
            //so we bind our data to this view and then do any additional setup required
            //example: we could set the text used by a ui text object by reading some value from the data
            base.BindDataToView(data);
        }
    }
}


