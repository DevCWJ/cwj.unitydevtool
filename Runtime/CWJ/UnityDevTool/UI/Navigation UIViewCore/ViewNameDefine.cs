using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CWJ
{
    public static class ViewNameDefine
    {
        public const ViewName RootName_BeforeLogin = ViewName.StartView_BeforeLogIn;
        public const ViewName RootName_AfterLogin = ViewName.MainView_AfterLogIn;

        public enum ViewName
        {
            StartView_BeforeLogIn = 0,
            LogIn = 1,
            CalendarView = 2,


            MainView_AfterLogIn = 10,
            DataView_heartbeat = 11,
            DataView_SpO2 = 12,
            DataView_stress = 13,
            DataView_motion = 14,

            DataDisplay = 20,
            DataHistory = 21,
            DataChart = 22,
        }


    }
}
