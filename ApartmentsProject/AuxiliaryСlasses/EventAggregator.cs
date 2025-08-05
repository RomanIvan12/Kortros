using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApartmentsProject.AuxiliaryСlasses
{
    public class EventAggregator
    {
        private static EventAggregator _instance;
        public static EventAggregator Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new EventAggregator();
                return _instance;
            }
        }
        private EventAggregator() { }



        public event EventHandler AllCorrect;
        public void PublishAllCorrect()
        {
            AllCorrect?.Invoke(this, EventArgs.Empty);
        }
        public void SubscribeAllCorrect(EventHandler handler)
        {
            AllCorrect += handler;
        }
        public void UnsubscribeAllCorrect(EventHandler handler)
        {
            AllCorrect -= handler;
        }



    //    public event EventHandler RoomMatrixEntriesChanged;
    //    public void PublishRoomMatrixEntriesChanged()
    //    {
    //        RoomMatrixEntriesChanged?.Invoke(this, EventArgs.Empty);
    //    }
    //    public void SubscribeRoomMatrixEntriesChanged(EventHandler handler)
    //    {
    //        RoomMatrixEntriesChanged += handler;
    //    }
    //    public void UnsubscribeRoomMatrixEntriesChanged(EventHandler handler)
    //    {
    //        RoomMatrixEntriesChanged -= handler;
    //    }
    }
}
