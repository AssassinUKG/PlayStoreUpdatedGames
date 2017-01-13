using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayStoreUpdatedGames
{
    [ProtoContract]
     class AppStoreClass
    {
        [ProtoMember(1)]
        public string Name { get; set; }
        [ProtoMember(2)]
        public string AppStoreLink { get; set; }
        [ProtoMember(3)]
        public string PublishedDate { get; set; }
        [ProtoMember(4)]
        public string Description { get; set; }
        [ProtoMember(5)]
        public string ImageLink { get; set; }
        [ProtoMember(6)]
        public string CurrentVersion { get; set; }
        [ProtoMember(7)]
        public string Price { get; set; }



        public string ToFormatedSave()
        {
            return string.Format("\r\n{0}:|:{1}:|:{2}:|:{3}:|:{4}:|:{5}:|:{6}\r\n", Name, AppStoreLink, PublishedDate, Description, ImageLink, CurrentVersion, Price);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
