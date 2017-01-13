using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayStoreUpdatedGames
{
    class MessageArgsUI : EventArgs
    {


        private string _message;
        public MessageArgsUI(string message)
        {
            this._message = message;
        }
        public string Message
        {
            get { return _message; }
        }



    }
}
