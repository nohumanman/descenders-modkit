using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using ModLoaderSolution;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Collections;

namespace ModLoaderSolution
{
    public struct ChatMessage
    {
        public string message;
        public string user;
        public string map;
    }
    public class Chat : MonoBehaviour
    {
        public static Chat instance;
        public string currentMessage = "";
        public List<ChatMessage> ChatList = new List<ChatMessage>();
        public void Start()
        {
            instance = this;
        }
        public void SendMessage()
        {
            NetClient.Instance.SendData("CHAT_MESSAGE", currentMessage);
            currentMessage = "";
        }
        public string GetMessages()
        {
            string x = "";
            foreach(ChatMessage chatMessage in ChatList)
                x += chatMessage.map + ":" + chatMessage.user + " - " + chatMessage.message + "\n";
            return x;
        }
        public void GetMessage(string user, string map, string mess)
        {
            ChatList.Add(new ChatMessage
                {
                    user = user,
                    map = map,
                    message = mess
                }
            );
        }
    }
}
