using System;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Ipc;

namespace Fomm
{
  internal delegate void RecieveMessageDelegate(string msg);

  internal static class Messaging
  {
    private class MessagePasser : MarshalByRefObject
    {
      public void SendMessage(string msg)
      {
        del(msg);
      }
    }

    private static RecieveMessageDelegate del;
    //private static MessagePasser messagePasser;

    public static void ServerSetup(RecieveMessageDelegate del)
    {
      Messaging.del = del;
      var serverChannel = new IpcChannel("localhost:9090");
      ChannelServices.RegisterChannel(serverChannel, false);
      RemotingConfiguration.RegisterWellKnownServiceType(typeof (MessagePasser), "MessagePasser.rem",
                                                         WellKnownObjectMode.Singleton);
    }

    public static void TransmitMessage(string s)
    {
      var channel = new IpcChannel();
      ChannelServices.RegisterChannel(channel, false);
      var remoteType = new WellKnownClientTypeEntry(typeof (MessagePasser),
                                                                         "ipc://localhost:9090/MessagePasser.rem");
      RemotingConfiguration.RegisterWellKnownClientType(remoteType);
      var passer = new MessagePasser();
      passer.SendMessage(s);
    }
  }
}